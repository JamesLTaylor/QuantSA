using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Primitives.Dates;

namespace QuantSA.Primitives
{
    /// <summary>
    /// A general storage container to keep both final results and debug data in QuantSA objects.
    /// </summary>
    /// <details>
    /// While this object stores 2-d arrays of objects, it is intended for these objects to only be 
    /// doubles, strings, ints and similar.
    /// </details>
    [Serializable]
    public class ResultStore : IProvidesResultStore
    {
        private Dictionary<string, double[,]> data;
        private Dictionary<string, Date[,]> dataDates;
        private Dictionary<string, string[,]> dataStrings;
        

        /// <summary>
        /// Create an empty result store
        /// </summary>
        public ResultStore()
        {
            data = new Dictionary<string, double[,]>();
            dataDates = new Dictionary<string, Date[,]>();
            dataStrings = new Dictionary<string, string[,]>();
        }

        /// <summary>
        /// Add a string the ResultStore
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">A string value</param>
        public void Add(string name, string result)
        {
            dataStrings[name] = new string[,] { { result } };
        }

        /// <summary>
        /// Add a scalar value to the ResultStore
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">A scalar value.</param>
        public void Add(string name, double result)
        {
            data[name] = new double[,] { { result } };
        }

        /// <summary>
        /// Add a column or row vector to the result store.
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">An array of values</param>
        /// <param name="column">True if the input is to be stored as a column, False if it is to be stored as a row.</param>
        public void Add(string name, double[] result, bool column=true)
        {
            if (column)
            {
                double[,] fullSizeResult = new double[result.Length, 1];
                for (int i = 0; i<result.Length; i++)
                {
                    fullSizeResult[i, 0] = result[i];
                }
                data[name] = fullSizeResult;
            }
            else
            {
                double[,] fullSizeResult = new double[1, result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    fullSizeResult[0, i] = result[i];
                }
                data[name] = fullSizeResult;
            }
        }

        /// <summary>
        /// Add a 2d array of results
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">An array of values</param>
        public void Add(string name, double[,] result)
        {
            data[name] = result.Clone() as double[,];
        }

        public void Add(string name, Date result)
        {
            dataDates[name] = new Date[,] { { result } };
            
        }

        public void Add(string name, Date[] result, bool column = true)
        {
            if (column)
            {
                Date[,] fullSizeResult = new Date[result.Length, 1];
                for (int i = 0; i < result.Length; i++)
                {
                    fullSizeResult[i, 0] = result[i];
                }
                dataDates[name] = fullSizeResult;
            }
            else
            {
                Date[,] fullSizeResult = new Date[1, result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    fullSizeResult[0, i] = result[i];
                }
                dataDates[name] = fullSizeResult;
            }
        }

        /// <summary>
        /// Return the list of all results available in this store.
        /// </summary>
        /// <returns></returns>
        public string[] GetNames()
        {
            List<string> nameList = data.Keys.ToList();
            nameList.AddRange(dataDates.Keys);
            nameList.AddRange(dataStrings.Keys);
            nameList.Sort();
            return nameList.ToArray<string>();
        }

        /// <summary>
        /// Get the results stored with the provided name.
        /// </summary>
        /// <param name="name">The name of the required result.  Use <see cref="GetNames"/> to see all the available names.</param>
        /// <returns></returns>
        public double[,] Get(string name)
        {
            if (data.ContainsKey(name))
            {
                return data[name];
            }
            else throw new ArgumentException(name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Get the results stored with the provided name.
        /// </summary>
        /// <param name="name">The name of the required result.  Use <see cref="GetNames"/> to see all the available names.</param>
        /// <returns></returns>
        public double GetScalar(string name)
        {
            if (data.ContainsKey(name))
            {
                if (data[name].GetLength(0) > 1 || data[name].GetLength(1) > 1)
                    throw new ArgumentException(name + " is not a scalar value in this ResultStore");
                return data[name][0,0];
            }
            else throw new ArgumentException(name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Get the Date results stored with the provided name.
        /// </summary>
        /// <param name="name">The name of the required result.  Use <see cref="GetNames"/> to see all the available names.  
        /// Use <see cref="IsDate(string)"/> to check if the result is of type Date.        /// 
        /// <returns></returns>
        public Date[,] GetDates(string name)
        {
            if (dataDates.ContainsKey(name))
            {
                return dataDates[name];
            }
            else throw new ArgumentException(name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Get the Date results stored with the provided name.
        /// </summary>
        /// <param name="name">The name of the required result.  Use <see cref="GetNames"/> to see all the available names.  
        /// Use <see cref="IsDate(string)"/> to check if the result is of type Date.        /// 
        /// <returns></returns>
        public string[,] GetStrings(string name)
        {
            if (dataStrings.ContainsKey(name))
            {
                return dataStrings[name];
            }
            else throw new ArgumentException(name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Check if the required result set consists of <see cref="Date"/>s.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsDate(string name)
        {
            if (dataDates.ContainsKey(name)) return true;
            else if (data.ContainsKey(name)) return false;
            else if (dataStrings.ContainsKey(name)) return false;
            else throw new ArgumentException(name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Check if the required result set consists of strings.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsString(string name)
        {
            if (dataStrings.ContainsKey(name)) return true;
            else if (data.ContainsKey(name)) return false;
            else if (dataDates.ContainsKey(name)) return false;
            else throw new ArgumentException(name + " does not exist in this store.  Use GetNames to check all available names.");
        }



        /// <summary>
        /// Returns itself.
        /// </summary>
        /// <returns></returns>
        public ResultStore GetResultStore()
        {
            return this;
        }
        
        
    }
}
