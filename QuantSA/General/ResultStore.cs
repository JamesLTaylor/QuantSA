using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
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
        //TODO: Possibly add string and datetime dictionaries

        /// <summary>
        /// Create an empty result store
        /// </summary>
        public ResultStore()
        {
            data = new Dictionary<string, double[,]>();
        }

        /// <summary>
        /// Add a scalar value to the ResultStore
        /// </summary>
        /// <param name="name">The name used by users to retrieve this piece of information</param>
        /// <param name="result">A scalar value, such as a string, int or double</param>
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

        /// <summary>
        /// Return the list of all results available in this store.
        /// </summary>
        /// <returns></returns>
        public string[] GetNames()
        {
            string[] names = data.Keys.ToArray<string>();
            Array.Sort<string>(names);
            return names;
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
        /// Returns itself.
        /// </summary>
        /// <returns></returns>
        public ResultStore GetResultStore()
        {
            return this;
        }
    }
}
