using System;
using System.Collections.Generic;

namespace QuantSA.Excel.Addin
{
    public class ObjectMap
    {
        private static readonly object ThisLock = new object();
        private static ObjectMap _instance;
        private readonly Dictionary<string, ObjectEntry> _namesAndObjects;
        private long _objectCounter;

        public string LatestException;

        /// <summary>
        /// Only called by Instance
        /// </summary>
        private ObjectMap()
        {
            _objectCounter = 0;
            _namesAndObjects = new Dictionary<string, ObjectEntry>();
        }


        /// <summary>
        /// The singleton instance of the object map
        /// </summary>
        public static ObjectMap Instance
        {
            get
            {
                lock (ThisLock)
                {
                    return _instance ?? (_instance = new ObjectMap());
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
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (name.Length < 2) throw new ArgumentException("Specified name cannot have a '.'");
            if (name.IndexOf(' ') > 0) throw new ArgumentException("Specified name cannot have a space");
            string uniqueID;
            lock (ThisLock)
            {
                _objectCounter++;
                uniqueID = name + "." + DateTime.Now.ToString("HH:mm:ss") + "-" + _objectCounter;
                if (_namesAndObjects.ContainsKey(name))
                {
                    //TODO: Make sure this is the same object.  If not throw an error
                }

                _namesAndObjects[name] = new ObjectEntry(obj, uniqueID);
            }

            return uniqueID;
        }

        /// <summary>
        /// Checks that the string looks like an object name.  Checks that there is in the following order:
        /// '.', ':', ':', '-'. 
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        private bool IsObjectName(string objectName)
        {
            var ind = objectName.IndexOf('.');
            if (ind < 0) return false;
            ind = objectName.IndexOf(':', ind);
            if (ind < 0) return false;
            ind = objectName.IndexOf(':', ind);
            if (ind < 0) return false;
            ind = objectName.IndexOf('-', ind);
            if (ind < 0) return false;
            return true;
        }

        public bool TryGetObjectFromID(string objectName, out object element)
        {
            element = null;
            if (!IsObjectName(objectName)) return false;
            if (objectName == null) throw new ArgumentNullException(nameof(objectName));
            var idx = objectName.LastIndexOf('.');
            var name = objectName.Substring(0, idx);
            if (!_namesAndObjects.TryGetValue(name, out var entry)) return false;
            element = entry.Obj;
            return true;
        }

        private struct ObjectEntry
        {
            public ObjectEntry(object obj, string uniqueID)
            {
                UniqueID = uniqueID;
                Obj = obj;
            }

            public string UniqueID { get; }

            public object Obj { get; }
            //public string bookName;
            //public string sheetName;
            //public string cellName;
        }
    }
}