using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatSolver
{
    class Resolver
    {
        #region Private Fields
        // person, and prefered neighbour (max 2)
        private Dictionary<string, List<string>> people;
        //variable for every possible neighbourhood id, name_1, name_2
        private Dictionary<int, Tuple<string, string>> variablesNeighborhood;
        //variable for every possible people position in a row
        private Dictionary<int, Tuple<string, int>> variablesPosition;

        int clauseCount = 0;
        int countVariable = 0;
        int numberOfPositions = 0;

        string _lastGeneratedCNF = "";
        #endregion

        #region Constructor
        public Resolver(Dictionary<string, List<string>> peoplePreferenceList)
        {
            people = peoplePreferenceList;
            numberOfPositions = people.Count;
        }
        #endregion

        #region Private Methods
        public Dictionary<int, Tuple<string, string>> getNeighborhoodVars
        {
            get { return variablesNeighborhood; }
        }

        public Dictionary<int, Tuple<string, int>> getVariablesPosition
        {
            get { return variablesPosition; }
        }
        private Dictionary<int, Tuple<string, string>> GenerateNeighborhoodVariables()
        {
            Dictionary<int, Tuple<string, string>> temp = new Dictionary<int, Tuple<string, string>>();


            foreach (string item in people.Keys)
            {
                temp.Add(++countVariable, new Tuple<string, string>(item, null));
            }

            foreach (string item in people.Keys)
            {
                foreach (string item2 in people.Keys)
                {
                    if (item != item2)
                    {
                        temp.Add(++countVariable, new Tuple<string, string>(item, item2));
                    }
                }
            }

            variablesNeighborhood = temp;
            return temp;
        }
        private Dictionary<int, Tuple<string, int>> GeneratePositionVariables()
        {
            Dictionary<int, Tuple<string, int>> temp = new Dictionary<int, Tuple<string, int>>();

            foreach (string item in people.Keys)
            {
                for (int i = 1; i <= numberOfPositions; i++)
                {
                    temp.Add(++countVariable, new Tuple<string, int>(item, i));
                }
            }
            variablesPosition = temp;
            return temp;
        }

        /// <summary>
        /// constraint: at most 2 person has one neighbor
        /// </summary>
        /// <returns></returns>
        private string FirstCondition()
        {
            string cnf = "\n c first condition \n";
            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = index + 1; index2 < variablesNeighborhood.Count; index2++)
                {
                    var item2 = variablesNeighborhood.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    for (int index3 = index2 + 1; index3 < variablesNeighborhood.Count; index3++)
                    {
                        var item3 = variablesNeighborhood.ElementAt(index3);
                        var itemKey3 = item3.Key;
                        var itemValue3 = item3.Value;

                        if (itemValue.Item2 == null && itemValue2.Item2 == null && itemValue3.Item2 == null)
                        {
                            // nie są możliwe ustawienia z 3 'pustymi sąsiadami'
                            cnf += " -" + itemKey + " -" + itemKey2 + " -" + itemKey3 + " 0 \n";
                            ++clauseCount;
                        }
                    }
                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: at least 2 person has one neighbor
        /// </summary>
        /// <returns></returns>
        private string SecondCondition()
        {
            string cnf = "\n c second condition \n";
            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = 0; index2 < variablesNeighborhood.Count; index2++)
                {
                    var item2 = variablesNeighborhood.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    if (itemValue.Item2 == null && itemValue2.Item2 == null && itemValue.Item1 != itemValue2.Item1)
                    {
                        // nie są możliwe ustawienia z 3 'pustymi sąsiadami'
                        cnf += " " + itemKey2;
                    }

                }
                if (itemValue.Item2 == null)
                {
                    cnf += " 0 \n";
                    ++clauseCount;
                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: every person can have max 2 neighbours (or empty neighbour)
        /// </summary>
        /// <returns></returns>
        private string ThirdCondition()
        {
            string cnf = "\n c third condition \n";
            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = index + 1; index2 < variablesNeighborhood.Count; index2++)
                {
                    var item2 = variablesNeighborhood.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    for (int index3 = index2 + 1; index3 < variablesNeighborhood.Count; index3++)
                    {
                        var item3 = variablesNeighborhood.ElementAt(index3);
                        var itemKey3 = item3.Key;
                        var itemValue3 = item3.Value;

                        if (itemValue.Item1 == itemValue2.Item1 && itemValue2.Item1 == itemValue3.Item1)
                        {
                            cnf += " -" + itemKey + " -" + itemKey2 + " -" + itemKey3 + " 0 \n";
                            ++clauseCount;
                        }

                    }

                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: every person can have exacly (at least?) 2 neighbours (or empty neighbour)
        /// </summary>
        /// <returns></returns>
        private string FourthCondition()
        {
            string cnf = "\n c fourth condition \n";
            Dictionary<string, List<int>> temp = new Dictionary<string, List<int>>();
            foreach (var item in people)
            {
                //tworzy listę ludzi z dopiętą listą wszystkich zmiennych/możliwych sąsiadów
                temp.Add(item.Key, new List<int>());
            }

            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                if (temp.ContainsKey(itemValue.Item1))
                {
                    temp[itemValue.Item1].Add(itemKey);
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }

            foreach (var item in temp)
            {
                foreach (var item2 in item.Value)
                {
                    foreach (var item3 in item.Value)
                    {
                        if (item2 != item3)
                        {
                            cnf += " " + item3;
                        }
                    }

                    cnf += " 0 \n";
                    ++clauseCount;
                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: person at ends can't be neighbours
        /// </summary>
        /// <returns></returns>
        private string FifthCondition()
        {
            string cnf = "\n c fifth condition \n";
            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = index + 1; index2 < variablesNeighborhood.Count; index2++)
                {
                    var item2 = variablesNeighborhood.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    for (int index3 = index2 + 1; index3 < variablesNeighborhood.Count; index3++)
                    {
                        var item3 = variablesNeighborhood.ElementAt(index3);
                        var itemKey3 = item3.Key;
                        var itemValue3 = item3.Value;

                        if (itemValue.Item2 == null && itemValue2.Item2 == null && ((itemValue2.Item1 == itemValue3.Item1 && itemValue3.Item1 == itemValue.Item2) || (itemValue2.Item1 == itemValue3.Item2 && itemValue3.Item1 == itemValue.Item1)))
                        {
                            cnf += " -" + itemKey + " -" + itemKey2 + " -" + itemKey3 + " 0 \n";
                            ++clauseCount;
                        }

                    }

                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: person implication for matching variables
        /// </summary>
        /// <returns></returns>
        private string SixthCondition()
        {
            string cnf = "\n c sixth condition \n";
            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = index + 1; index2 < variablesNeighborhood.Count; index2++)
                {
                    var item2 = variablesNeighborhood.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    if (itemValue.Item1 == itemValue2.Item2 && itemValue.Item2 == itemValue2.Item1)
                    {
                        cnf += " -" + itemKey + " " + itemKey2 + " 0 \n";
                        ++clauseCount;
                        cnf += " -" + itemKey2 + " " + itemKey + " 0 \n";
                        ++clauseCount;
                    }
                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: every person have at most one position
        /// </summary>
        /// <returns></returns>
        private string SeventhCondition()
        {
            string cnf = "\n c seventh condition \n";
            for (int index = 0; index < variablesPosition.Count; index++)
            {
                var item = variablesPosition.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = index + 1; index2 < variablesPosition.Count; index2++)
                {
                    var item2 = variablesPosition.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    if (itemValue.Item1 == itemValue2.Item1)
                    {
                        cnf += " -" + itemKey + " -" + itemKey2 + " 0 \n";
                        ++clauseCount;
                    }
                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: every person have at least one position
        /// </summary>
        /// <returns></returns>
        private string EighthCondition()
        {
            string cnf = "\n c eighth condition \n";
            for (int index = 0; index < variablesPosition.Count; index++)
            {
                var item = variablesPosition.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = 0; index2 < variablesPosition.Count; index2++)
                {
                    var item2 = variablesPosition.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    if (itemValue.Item1 == itemValue2.Item1 /*&& itemValue.Item2 != itemValue2.Item2*/)
                    {
                        cnf += " " + itemKey2;
                    }
                }
                cnf += " 0 \n";
                ++clauseCount;
            }
            return cnf;
        }

        /// <summary>
        /// constraint: every position have at most one person
        /// </summary>
        /// <returns></returns>
        private string NinthCondition()
        {
            string cnf = "\n c ninth condition \n";

            for (int index = 0; index < variablesPosition.Count; index++)
            {
                var item = variablesPosition.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = index + 1; index2 < variablesPosition.Count; index2++)
                {
                    var item2 = variablesPosition.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    if (itemValue.Item2 == itemValue2.Item2)
                    {
                        cnf += " -" + itemKey + " -" + itemKey2 + " 0 \n";
                        ++clauseCount;
                    }
                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: person at first and last position have only one neighbor
        /// </summary>
        /// <returns></returns>
        private string TenthCondition()
        {
            string cnf = "\n c tenth condition \n";
            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                if (itemValue.Item2 == null) // ma sąsiada pustego
                {

                    for (int index2 = 0; index2 < variablesPosition.Count; index2++)
                    {
                        var item2 = variablesPosition.ElementAt(index2);
                        var itemKey2 = item2.Key;
                        var itemValue2 = item2.Value;

                        if ((itemValue.Item1 == itemValue2.Item1) && (itemValue2.Item2 != 1) && (itemValue2.Item2 != numberOfPositions))
                        {
                            cnf += " -" + itemKey + " -" + itemKey2 + " 0 \n";
                            ++clauseCount;
                        }
                    }
                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: neighbors on position x, which is next to y, have to be neighbor of man on position y
        /// </summary>
        /// <returns></returns>
        private string EleventhCondition()
        {
            string cnf = "\n c eleventh condition \n";
            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                for (int index2 = index + 1; index2 < variablesNeighborhood.Count; index2++)
                {
                    var item2 = variablesNeighborhood.ElementAt(index2);
                    var itemKey2 = item2.Key;
                    var itemValue2 = item2.Value;

                    if (itemValue.Item1 == itemValue2.Item2 && itemValue.Item2 == itemValue2.Item1)
                    {
                        for (int index3 = 0; index3 < variablesPosition.Count; index3++)
                        {
                            var itemPos = variablesPosition.ElementAt(index3);
                            var itemPosKey = itemPos.Key;
                            var itemPosValue = itemPos.Value;

                            if (itemValue.Item1 == itemPosValue.Item1)
                            {
                                cnf += " -" + itemKey + " -" + itemPosKey;

                                for (int index4 = 0; index4 < variablesPosition.Count; index4++)
                                {
                                    var itemPos2 = variablesPosition.ElementAt(index4);
                                    var itemPosKey2 = itemPos2.Key;
                                    var itemPosValue2 = itemPos2.Value;

                                    if (itemValue2.Item1 == itemPosValue2.Item1 && ((itemPosValue.Item2 == (itemPosValue2.Item2 - 1)) || (itemPosValue.Item2 == (itemPosValue2.Item2 + 1))))
                                    {
                                        cnf += " " + itemPosKey2;
                                    }
                                }
                                cnf += " 0 \n";
                                ++clauseCount;
                            }
                        }
                    }
                }
            }
            return cnf;
        }

        /// <summary>
        /// constraint: preferences
        /// </summary>
        /// <returns></returns>
        private string TwelfthCondition()
        {
            string cnf = "\n c twelfth condition \n";
            for (int index = 0; index < variablesNeighborhood.Count; index++)
            {
                var item = variablesNeighborhood.ElementAt(index);
                var itemKey = item.Key;
                var itemValue = item.Value;

                //for (int index2 = 0; index2 < people.Count; index2++)
                //{
                if (people[itemValue.Item1] != null && people[itemValue.Item1].Contains(itemValue.Item2))
                {
                    cnf += itemKey + " 0 \n";
                    ++clauseCount;
                }
                //}
            }
            return cnf;
        }
        #endregion

        #region Public Methods
        public string GenerateCNF()
        {
            this.clauseCount = 0;
            this.countVariable = 0;
            GenerateNeighborhoodVariables();
            GeneratePositionVariables();

            string cnf = "";

            cnf += FirstCondition();
            cnf += SecondCondition();
            cnf += ThirdCondition();
            cnf += FourthCondition();
            cnf += FifthCondition();
            cnf += SixthCondition();
            cnf += SeventhCondition();
            cnf += EighthCondition();
            cnf += NinthCondition();
            cnf += TenthCondition();
            cnf += EleventhCondition();
            cnf += TwelfthCondition();

            _lastGeneratedCNF = "p cnf " + (variablesNeighborhood.Count + variablesPosition.Count) + " " + clauseCount + "\n" + cnf;


            //string temp = "";
            //foreach (var item in getNeighborhoodVars)
            //{
            //    temp += item.Key + ":  " + item.Value.Item1 + " " + item.Value.Item2 + "\n";
            //}
            //foreach (var item in getVariablesPosition)
            //{
            //    temp += item.Key + ":  " + item.Value.Item1 + " " + item.Value.Item2 + "\n";
            //}
            return _lastGeneratedCNF;
        }
        #endregion
    }
}
