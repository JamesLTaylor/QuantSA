using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace QuantSA.Excel
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
                namesAndObjects[name] = new ObjectEntry { obj = obj, uniqueID = uniqueID };
            }
            return uniqueID;
        }

        public T GetObjectFromID<T>(string objectName)
        {
            object obj = ObjectMap.Instance.GetObjectFromID(objectName);
            if (obj is T) { return (T)obj; }
            else throw new ArgumentException(objectName + " is not of required type: " + typeof(T).ToString());            
        }

        /// <summary>
        /// Get the object given the full id.  Strips everything to right of first dot and uses remaining as short name.
        /// </summary>
        /// <param name="uniqueID">Full id as returned by a call to <see cref="AddObject"/></param>
        /// <returns></returns>
        private object GetObjectFromID(string uniqueID)
        {
            string[] nameParts = uniqueID.Split('.');            
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


        /// <summary>
        /// Uses the Excel instance to add a serialized version of an object to the object map.  Allows different plugins to 
        /// interact with each other if they know the same object definitions.
        /// </summary>
        /// <param name="name">The short name of the object on the map.  A longer name/ID will be provided automatically.</param>
        /// <param name="obj">The object to be added.</param>
        /// <returns>A String.</returns>
        public static string AddObjectPlugIn(string name, object obj, bool serialize)
        {
            if (serialize)
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                new BinaryFormatter().Serialize(stream, obj);
                string serializedObject = Convert.ToBase64String(stream.ToArray());
                stream.Close();

                string objID = XlCall.Excel(XlCall.xlUDF, "QSA.PutOnMap", name, serializedObject, 1) as string;
                if (objID.StartsWith("ERROR")) throw new Exception(objID);
                return objID;
            } else
            {
                string objID = XlCall.Excel(XlCall.xlUDF, "QSA.PutOnMap", name, (string)obj, 0) as string;
                if (objID.StartsWith("ERROR")) throw new Exception(objID);
                return objID;
            }
        }

        /// <summary>
        /// Uses the Excel instance to retrieve a serialized version of an object on the object map.  Allows different plugins to 
        /// interact with each other.
        /// </summary>
        /// <param name="name">The short name of the object on the map.</param>
        /// <param name="serialize">Indicates whether the object has been strored directly or in serialized form.  
        /// You need to know how it was added in the first place.  Generally if is an object deinfed in QuantSA it
        /// will be added directly to objectMap while if it is defined in a plugin then it should be stored as a string.
        /// <para/>true - return the object.
        /// <para/>false - return the string for later deserialization.</param>
        /// <returns>An instance of the object or a string representing the serialized version of the object.</returns>
        public static object GetObjectPlugIn(string name, bool serialize)
        {
            int serializeValue = serialize ? 1 : 0;

            string serializedObject = XlCall.Excel(XlCall.xlUDF, "QSA.GetFromMap", name, serializeValue) as string;
            if (serializedObject.StartsWith("ERROR")) throw new Exception(serializedObject);

            if (serialize)
            {
                byte[] bytes = Convert.FromBase64String(serializedObject);
                MemoryStream stream = new MemoryStream(bytes);
                object obj = new BinaryFormatter().Deserialize(stream);
                stream.Close();

                return obj;
            }
            else
            {
                return serializedObject;
            }
        }


    }
}