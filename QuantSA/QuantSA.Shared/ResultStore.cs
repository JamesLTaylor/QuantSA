using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Shared.Dates;

namespace QuantSA.Shared
{
    /// <summary>
    /// A general storage container to keep both final results and debug data in QuantSA objects.
    /// </summary>
    /// <details>
    /// While this object stores 2-d arrays of objects, it is intended for these objects to only be 
    /// doubles, strings, ints and similar.
    /// </details>
    [Serializable]
    public class ResultStore
    {
        private readonly Dictionary<string, object[,]> _data;
        private readonly Dictionary<string, object[,]> _dataDates;
        private readonly Dictionary<string, object[,]> _dataStrings;


        /// <summary>
        /// Create an empty result store
        /// </summary>
        public ResultStore()
        {
            _data = new Dictionary<string, object[,]>();
            _dataDates = new Dictionary<string, object[,]>();
            _dataStrings = new Dictionary<string, object[,]>();
        }

        /// <summary>
        /// Add a string the ResultStore
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">A string value</param>
        public void Add(string name, string result)
        {
            _dataStrings[name] = new object[,] {{result}};
        }

        /// <summary>
        /// Add a scalar value to the ResultStore
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">A scalar value.</param>
        public void Add(string name, double result)
        {
            _data[name] = new object[,] {{result}};
        }

        /// <summary>
        /// Add a column or row vector to the result store.
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">An array of values</param>
        /// <param name="column">True if the input is to be stored as a column, False if it is to be stored as a row.</param>
        public void Add(string name, double[] result, bool column = true)
        {
            if (column)
            {
                var fullSizeResult = new object[result.Length, 1];
                for (var i = 0; i < result.Length; i++) fullSizeResult[i, 0] = result[i];
                _data[name] = fullSizeResult;
            }
            else
            {
                var fullSizeResult = new object[1, result.Length];
                for (var i = 0; i < result.Length; i++) fullSizeResult[0, i] = result[i];
                _data[name] = fullSizeResult;
            }
        }

        /// <summary>
        /// Add a 2d array of results
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">An array of values</param>
        public void Add(string name, double[,] result)
        {
            _data[name] = result.Clone() as object[,];
        }

        public void Add(string name, Date result)
        {
            _dataDates[name] = new object[,] {{result}};
        }

        public void Add(string name, Date[] result, bool column = true)
        {
            if (column)
            {
                var fullSizeResult = new object[result.Length, 1];
                for (var i = 0; i < result.Length; i++) fullSizeResult[i, 0] = result[i];
                _dataDates[name] = fullSizeResult;
            }
            else
            {
                var fullSizeResult = new object[1, result.Length];
                for (var i = 0; i < result.Length; i++) fullSizeResult[0, i] = result[i];
                _dataDates[name] = fullSizeResult;
            }
        }

        /// <summary>
        /// Return the list of all results available in this store.
        /// </summary>
        /// <returns></returns>
        public string[] GetNames()
        {
            var nameList = _data.Keys.ToList();
            nameList.AddRange(_dataDates.Keys);
            nameList.AddRange(_dataStrings.Keys);
            nameList.Sort();
            return nameList.ToArray<string>();
        }

        /// <summary>
        /// Get the results stored with the provided name.
        /// </summary>
        /// <param name="name">The name of the required result.  Use <see cref="GetNames"/> to see all the available names.</param>
        /// <returns></returns>
        public object[,] Get(string name)
        {
            if (_data.ContainsKey(name))
                return _data[name];
            throw new ArgumentException(
                name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Get the results stored with the provided name.
        /// </summary>
        /// <param name="name">The name of the required result.  Use <see cref="GetNames"/> to see all the available names.</param>
        /// <returns></returns>
        public object GetScalar(string name)
        {
            if (_data.ContainsKey(name))
            {
                if (_data[name].GetLength(0) > 1 || _data[name].GetLength(1) > 1)
                    throw new ArgumentException(name + " is not a scalar value in this ResultStore");
                return _data[name][0, 0];
            }

            throw new ArgumentException(
                name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Get the Date results stored with the provided name.
        /// </summary>
        /// <param name="name">The name of the required result.  Use <see cref="GetNames"/> to see all the available names.  
        /// Use <see cref="IsDate(string)"/> to check if the result is of type Date.</param>.
        /// <returns></returns>
        public object[,] GetDates(string name)
        {
            if (_dataDates.ContainsKey(name))
                return _dataDates[name];
            throw new ArgumentException(
                name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Get the Date results stored with the provided name.
        /// </summary>
        /// <param name="name">The name of the required result.  Use <see cref="GetNames"/> to see all the available names.  
        /// Use <see cref="IsDate(string)"/> to check if the result is of type Date.</param>.
        /// <returns></returns>
        public object[,] GetStrings(string name)
        {
            if (_dataStrings.ContainsKey(name))
                return _dataStrings[name];
            throw new ArgumentException(
                name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Check if the required result set consists of <see cref="Date"/>s.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsDate(string name)
        {
            if (_dataDates.ContainsKey(name)) return true;
            if (_data.ContainsKey(name)) return false;
            if (_dataStrings.ContainsKey(name)) return false;
            throw new ArgumentException(
                name + " does not exist in this store.  Use GetNames to check all available names.");
        }

        /// <summary>
        /// Check if the required result set consists of strings.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsString(string name)
        {
            if (_dataStrings.ContainsKey(name)) return true;
            if (_data.ContainsKey(name)) return false;
            if (_dataDates.ContainsKey(name)) return false;
            throw new ArgumentException(
                name + " does not exist in this store.  Use GetNames to check all available names.");
        }
    }
}