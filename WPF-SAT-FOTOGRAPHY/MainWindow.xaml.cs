using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SatSolver;

namespace WPF_SAT_FOTOGRAPHY
{
    public partial class MainWindow : Window
    {
        List<StackPanel> peopleStackPanels = new List<StackPanel>();
        Dictionary<TextBox, string> peopleNamesMapper = new Dictionary<TextBox, string>();
        ViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new ViewModel();
            DataContext = viewModel;
        }

        private Dictionary<string, List<string>> ReadPeople()
        {
            Dictionary<string, List<string>> neighbourhood = new Dictionary<string, List<string>>();

            foreach (StackPanel item in peopleStackPanels)
            {
                if (((TextBox)item.Children[0]).Text != "" && ((TextBox)item.Children[0]).Text != null)
                {
                    List<string> neigboursList = new List<string>();
                    if (((ComboBox)item.Children[1]).Text != null && ((ComboBox)item.Children[1]).Text != "")
                    {
                        neigboursList.Add(((ComboBox)item.Children[1]).Text);
                    }
                    if (((ComboBox)item.Children[2]).Text != null && ((ComboBox)item.Children[2]).Text != "")
                    {
                        neigboursList.Add(((ComboBox)item.Children[2]).Text);
                    }
                    try
                    {
                        neighbourhood.Add(
                        ((TextBox)item.Children[0]).Text,
                        neigboursList
                    );
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                    
                }
            }

            return neighbourhood;
        }
        // check number of added rows (min == 3)
        private bool Validate()
        {
            bool result = true;
            int notEmptyCounter = 0;
            string message = "";

            // count number of added people
            if (peopleStackPanels.Count >= 3)
            {
                foreach (StackPanel item in peopleStackPanels)
                {
                    if (((TextBox)item.Children[0]).Text != "" && ((TextBox)item.Children[0]).Text != null)
                    {
                        notEmptyCounter++;
                    }
                }
            }
            if (notEmptyCounter < 3)
            {
                result = false;
                message += "Please, add at least 3 people.\n";
            }

            // look for duplicated names
            var duplicates = peopleNamesMapper
                    .Where(a => a.Value != "")
                    .GroupBy(i => i.Value)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);
            if (duplicates.Count() > 0)
            {
                result = false;
            }
            foreach (var name in duplicates)
            {
                message += "Duplicated name found: " + name + ".\n";
            }

            // look for person name inside neighbors list
            foreach (StackPanel item in peopleStackPanels)
            {
                if ((((TextBox)item.Children[0]).Text == ((ComboBox)item.Children[1]).Text || (((TextBox)item.Children[0]).Text == ((ComboBox)item.Children[2]).Text)) && ((TextBox)item.Children[0]).Text != "")
                {
                    message += "Person with name '" + ((TextBox)item.Children[0]).Text + "' have neighbor with same name.\n";
                    result = false;
                }
            }

            // compare neighbors for every person
            foreach (StackPanel item in peopleStackPanels)
            {
                if (((ComboBox)item.Children[1]).Text == ((ComboBox)item.Children[2]).Text && ((ComboBox)item.Children[2]).Text != "" && ((ComboBox)item.Children[2]).Text != null)
                {
                    message += "Person with name '" + ((TextBox)item.Children[0]).Text + "' have duplicated neighbor.\n";
                    result = false;
                }
            }


            // Show message if validation not passed
            if (!result)
            {
                MessageBox.Show(message, "Validation Message");
            }
            
            return result;
        }
        //show result in 'result' xaml section
        private void ShowResult(bool satisfability, SortedDictionary<int, string> peopleInOrder)
        {
            if (!satisfability)
            {
                resultSatisfability_textBlock.Text = "UNSATISFABLE";
                resultSatisfability_textBlock.Foreground = Brushes.OrangeRed;

                result_listBox.Items.Clear();
            }
            else
            {
                resultSatisfability_textBlock.Text = "SATISFABLE";
                resultSatisfability_textBlock.Foreground = Brushes.Green;

                result_listBox.Items.Clear();

                foreach (var item in peopleInOrder)
                {
                    result_listBox.Items.Add(item.Key + ": " + item.Value);
                }
            }
        }

        private void addPerson_button_Click(object sender, RoutedEventArgs e)
        {
            StackPanel childStackPanel = StackPanelRow.generateNewRowPanel(deletePerson_button_Click, person_textBox_textChanged, viewModel.PeopleNames);
            peopleNamesMapper.Add((TextBox)childStackPanel.Children[0], ""); // save reference to name field

            peopleStackPanels.Add(childStackPanel);
            this.addPerson_stackPanel.Children.Add(childStackPanel);
        }

        private void person_textBox_textChanged(object sender, TextChangedEventArgs e)
        {

            string newValue = ((TextBox)sender).Text.Trim();
            string oldValue = peopleNamesMapper[((TextBox)sender)].Trim();
            peopleNamesMapper[((TextBox)sender)] = newValue;

            // save changed name
            if ((oldValue == null || oldValue == "") && (newValue != null && newValue != ""))
            {
                viewModel.PeopleNames.Add(newValue);
            }
            else if (newValue != "" && newValue != null)
            {
                 if (viewModel.PeopleNames.Contains(oldValue))
                {
                    for (int i = 0; i < viewModel.PeopleNames.Count; i++)
                    {
                        if (viewModel.PeopleNames[i] == oldValue)
                        {
                            viewModel.PeopleNames[i] = newValue;
                        }
                    }
                }
            }
            else if((oldValue != null && oldValue != ""))
            {
                viewModel.PeopleNames.Remove(oldValue);
            }
        }

        private void deletePerson_button_Click(object sender, RoutedEventArgs e)
        {
            // znajdź obiekty interfejsu powiązane z osobą do usunięcia
            for (int i = 0; i < peopleStackPanels.Count; i++)
            {
                if ((peopleStackPanels[i].Children.Contains((Button)sender)))
                {
                    viewModel.PeopleNames.Remove(peopleNamesMapper[(TextBox)peopleStackPanels[i].Children[0]]);
                    peopleNamesMapper.Remove((TextBox)(peopleStackPanels[i].Children[0]));

                    addPerson_stackPanel.Children.Remove(peopleStackPanels[i]);
                    peopleStackPanels.RemoveAt(i);
                    return;
                }
                
            }
        }

        private void run_button_Click(object sender, RoutedEventArgs e)
        {

            if (Validate())
            {
                Processor satProcessor = new Processor(ReadPeople());

                if (satProcessor.Run())
                {
                    SortedDictionary<int, string> temp = satProcessor.getPeoplePositions();
                    ShowResult(true, temp);
                }
                else
                {
                    ShowResult(false, null);
                }
            }
        }
    }

    public static class StackPanelRow
    {
        // returns prepared TextBox for new person name
        private static TextBox generatePersonName_textBox(TextChangedEventHandler textChangedHandler)
        {
            Thickness margin = new Thickness { Left = 8, Top = 4, Right = 8, Bottom = 4 };
            TextBox textbox = new TextBox { Width = 150, Margin = margin };
            textbox.TextChanged += textChangedHandler;

            return textbox;
        }
        // return prepared ComboBox with possibility of real-time value list modifications
        private static ComboBox generatePeopleNames_comboBox(IEnumerable<string> nameCollection)
        {
            Thickness margin = new Thickness { Left = 8, Top = 4, Right = 8, Bottom = 4 };
            ComboBox comboBox = new ComboBox { Width = 150, Margin = margin };
            comboBox.ItemsSource = nameCollection;

            return comboBox;
        }
        // generated button allows to remove not necessary row in stackPanel
        private static Button generateButton(RoutedEventHandler deleteButtonClickHandler)
        {
            Thickness margin = new Thickness { Left = 8, Top = 4, Right = 8, Bottom = 4 };
            Thickness padding = new Thickness { Left = 4, Top = 0, Right = 4, Bottom = 0 };
            Button button = new Button { Margin = margin, Padding = padding, Content = "delete"};
            button.Click += deleteButtonClickHandler;

            return button;
        }

        public static StackPanel generateNewRowPanel(RoutedEventHandler deleteButtonClickHandler, TextChangedEventHandler textChangedHandler, IEnumerable<string> nameCollection)
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal};
            stackPanel.Children.Add(generatePersonName_textBox(textChangedHandler));
            stackPanel.Children.Add(generatePeopleNames_comboBox(nameCollection));
            stackPanel.Children.Add(generatePeopleNames_comboBox(nameCollection));
            stackPanel.Children.Add(generateButton(deleteButtonClickHandler));
            return stackPanel;
        }
    }

    public class ViewModel
    {
        // collection used for real-time binding combo-box
        public ObservableCollection<string> PeopleNames { get; set; }

        public ViewModel()
        {
            if (PeopleNames == null)
            {
                PeopleNames = new ObservableCollection<string> { "" };
            }        }
    }
}
