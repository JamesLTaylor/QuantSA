using System;
using System.Collections.Generic;
using QuantSA.Excel.Shared;

namespace QuantSA.Excel.Common
{
    public class ObjectMap
    {
        private static readonly object thisLock = new object();
        private static ObjectMap instance;
        private readonly Dictionary<string, ObjectEntry> namesAndObjects;
        private long objectCounter;


        /// <summary>
        /// Only called by Instance
        /// </summary>
        private ObjectMap()
        {
            objectCounter = 0;
            namesAndObjects = new Dictionary<string, ObjectEntry>();
        }


        /// <summary>
        /// The singleton instance of the object map
        /// </summary>
        public static ObjectMap Instance
        {
            get
            {
                lock (thisLock)
                {
                    if (instance == null) instance = new ObjectMap();
                    return instance;
                }
            }
        }

        /// <summary>
        /// Add an object onto the map.  Objects with the same name will overwrite each other.
        /// </summary>
        /// <param name="name">The short name of the object on the map.  A longer name/ID will be provided automatically.</param>
        /// <param name="obj">The object to be added.</param>
        /// <returns></returns>
        public string AddObject(string name, object obj)
        {
            if (obj == null) throw new Exception("Cannot add a null object");
            if (name.Length < 2) throw new Exception("Specified name must be at least two characters");
            if (name.IndexOf('.') > 0) throw new Exception("Specified name cannot have a '.'");
            if (name.IndexOf(' ') > 0) throw new Exception("Specified name cannot have a space");
            string uniqueID;
            lock (thisLock)
            {
                objectCounter++;
                uniqueID = name + "." + DateTime.Now.ToString("HH:mm:ss") + "-" + objectCounter;
                if (namesAndObjects.ContainsKey(name))
                {
                    //TODO: Make sure this is the same object.  If not throw an error
                }

                namesAndObjects[name] = new ObjectEntry {obj = obj, uniqueID = uniqueID};
            }

            return uniqueID;
        }

        public T GetObjectFromID<T>(string objectName)
        {
            if (objectName == null) throw new ArgumentNullException(nameof(objectName));
            var obj = Instance.GetObjectFromID(objectName);
            if (obj is T variable)
                return variable;
            throw new ArgumentException(objectName + " is not of required type: " + typeof(T));
        }

        /// <summary>
        /// Get the object given the full id.  Strips everything to right of first dot and uses remaining as short name.
        /// </summary>
        /// <param name="uniqueID">Full id as returned by a call to <see cref="AddObject"/></param>
        /// <returns></returns>
        private object GetObjectFromID(string uniqueID)
        {
            var nameParts = uniqueID.Split('.');
            return GetObjectFromName(nameParts[0]);
        }

        /// <summary>
        /// Get the object on the map with the name provided.
        /// </summary>
        /// <param name="name">The name used to add the object to the map.</param>
        /// <returns></returns>
        private object GetObjectFromName(string name)
        {
            if (!namesAndObjects.ContainsKey(name))
                throw new IndexOutOfRangeException("Object map does not contain an object with id: " + name);
            return namesAndObjects[name].obj;
        }

        private struct ObjectEntry
        {
            public string uniqueID;

            public object obj;
            //public string bookName;
            //public string sheetName;
            //public string cellName;
        }
    }
}