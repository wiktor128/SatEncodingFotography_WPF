using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System;

namespace SatSolver
{
    public class Processor
    {
        #region Private Fields

        // person, and prefered neighbour (max 2)
        private Dictionary<string, List<string>> _peoplePreferenceList { get; set; }
        //variable for every possible neighbourhood id, name_1, name_2
        private Dictionary<int, Tuple<string, string>> _variablesNeighborhood { get; set; }
        //variable for every possible people position in a row
        private Dictionary<int, Tuple<string, int>> _variablesPosition { get; set; }
        private List<int> _resultVars = null;

        #endregion

        #region Constructor
        public Processor(Dictionary<string, List<string>> peoplePreferenceList)
        {
            _peoplePreferenceList = peoplePreferenceList;
        }

        #endregion

        #region Public Methods

        public bool Run()
        {
            var rsat = new Rsat();
            Resolver resolver = new Resolver(_peoplePreferenceList);

            rsat.Run(resolver.GenerateCNF());

            if (rsat.IsSatisfiable)
            {
                System.Console.Write(string.Join("\n", rsat._rsatReponse));
                _variablesNeighborhood = resolver.getNeighborhoodVars;
                _variablesPosition = resolver.getVariablesPosition;
                _resultVars = rsat.ResultVariables;

                return true;
            }
            else
            {
                return false;
            }
        }

        public SortedDictionary<int, string> getPeoplePositions()
        {
            SortedDictionary<int, string> peoplePositions = null;
            if (_resultVars != null)
            {
                peoplePositions = new SortedDictionary<int, string>();
                foreach (var resultVar in _resultVars)
                {
                    foreach (var positionVar in _variablesPosition)
                    {
                        if (resultVar == positionVar.Key)
                        {
                            peoplePositions.Add(positionVar.Value.Item2, positionVar.Value.Item1);
                        }
                    }
                }
            }

            return peoplePositions;
        }

        #endregion
    }
}
