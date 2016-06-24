using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SatSolver
{
    public class Rsat
    {
        #region Private fields

        //private List<string> _rsatReponse;
        public List<string> _rsatReponse;

        #endregion

        #region Properties

        public bool IsSatisfiable
        {
            get
            {
                return IsProblemSatisfiable();
            }
        }

        public List<int> ResultVariables
        {
            get { return ParseStringToIntList(GetRsatResultVariables()); }
        }

        #endregion

        #region Methods

        public void Run(string problemCNF)
        {
            _rsatReponse = new List<string>();
            File.WriteAllText("problem.cnf", problemCNF);

            try
            {
                var start = new ProcessStartInfo
                {
                    FileName = "rsat_2.01_win.exe",
                    Arguments = "problem.cnf -s",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (Process p = Process.Start(start))
                {
                    if (p != null)
                        using (StreamReader reader = p.StandardOutput)
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                _rsatReponse.Add(line);
                            }
                        }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private bool IsProblemSatisfiable()
        {
            bool problemSolved = false;
            foreach (var line in _rsatReponse)
            {
                if (line.Contains(" SATISFIABLE"))
                {
                    problemSolved = true;
                    break;
                }
            }
            return problemSolved;
        }

        private string GetRsatResultVariables()
        {
            string result = string.Empty;

            foreach (var line in _rsatReponse)
            {
                if (line.StartsWith("v"))
                {
                    result = line;
                    result = result.Replace('v', ' ').TrimStart();
                    result = result.Remove(result.Length - 2);
                    break;
                }
            }

            return result;
        }

        private List<int> ParseStringToIntList(string numbersInString)
        {
            List<int> numberInIntArray = new List<int>();
            string[] numbersInStringArray = numbersInString.Split(' ');

            foreach (var item in numbersInStringArray)
            {
                int temp;
                int.TryParse(item, out temp);
                numberInIntArray.Add(temp);
            }

            numberInIntArray.Sort();
            return numberInIntArray;
        }
        #endregion
    }
}
