using System;
using System.Collections.Generic;

namespace Excel
{
    public class ObjectMap
    {
        private struct ObjectEntry
        {
            public string uniqueID;
            public object obj;
            public string bookName;
            public string sheetName;
            public string cellName;
        }

        private static Object thisLock = new Object();
        private static ObjectMap instance;
        private Dictionary<string, ObjectEntry> namesAndObjects;
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
        /// Add an object onto the map.  Objects with the same name will overwrite each other.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
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
                namesAndObjects[name] = new ObjectEntry { obj = obj, uniqueID = uniqueID };
            }
            return uniqueID;
        }


        public object GetObjectFromID(string uniqueID)
        {
            string[] nameParts = uniqueID.Split('.');            
            return GetObjectFromName(nameParts[0]);
        }


        public object GetObjectFromName(string name)
        {
            if (!namesAndObjects.ContainsKey(name))
            {
                throw new IndexOutOfRangeException("Object map does not contain an object with id: " + name);
            }
            return namesAndObjects[name].obj;
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
                    if (instance == null)
                    {
                        instance = new ObjectMap();
                    }
                    return instance;
                }
            }
        }


    }
}