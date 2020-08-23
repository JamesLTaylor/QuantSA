using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using QuantSA.Excel.Shared;
using QuantSA.Shared.Serialization;

namespace QuantSA.Excel.Addin.Functions
{
    public class ObjectViewer
    {
        /// <summary>
        /// Get the field and property names inside a QuantSA object.
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        [QuantSAExcelFunction(Description = "View the properties that make up an object.  WARNING: This is for " +
                                            "information only, do not rely on the object's internal structure " +
                                            "since this can change at any time.",
            Name = "QSA.ViewObjectPropertyNames",
            ExampleSheet = "ZARSwap.xlsx",
            Category = "QSA.General",
            IsMacroType = false,
            IsHidden = false,
        HelpTopic = "http://www.quantsa.org/ViewObjectPropertyNames.html")]
        public static string[] ViewObjectPropertyNames(
            [QuantSAExcelArgument(Description = "The object you wish to view.", Name = "Object")]
            object objectName)
        {
            var fields = objectName.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            var names = new List<string>();
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes(typeof(JsonIgnoreAttribute)).Any()) continue;
                var name = field.Name;
                if (name[0] == '_')
                    name = name.Substring(1);
                names.Add(name);
            }

            return names.ToArray();
        }

        [QuantSAExcelFunction(Description = "View the contents of one of the properties of a QuantSA object.",
            Name = "QSA.ViewObjectPropertyValue",
            Category = "QSA.General",
            ExampleSheet = "ZARSwap.xlsx",
            IsMacroType = false,
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/ViewObjectPropertyValue.html")]
        public static object[,] ViewObjectPropertyValue(
            [QuantSAExcelArgument(Description = "The object you wish to view.", Name = "Object")]
            object objectName,
            [QuantSAExcelArgument(Description = "The name of the property from the object that you wish to view.")]
            string propertyName,
            [QuantSAExcelArgument(Description = "If the property with propertyName is in turn an object then this" +
                                                "is the name of the property inside that which you wish to view.",
                Default = null)]
            string propertyNameL2)
        {
            if (propertyNameL2 != null)
            {
                var obj = GetObjectPropertyValue(objectName, propertyName);
                return ViewObjectPropertyValue(obj, propertyNameL2, null);
            }

            var output = GetObjectPropertyValue(objectName, propertyName);
            if (output is ISerializableViaName objWithName)
                return ExcelTypeConverter.ConvertOuput(typeof(string), objWithName.GetName(), propertyName);
            return ExcelTypeConverter.ConvertOuput(output.GetType(), output, propertyName);
        }

        private static object GetObjectPropertyValue(object instance, string propertyName)
        {
            var fields = instance.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes(typeof(JsonIgnoreAttribute)).Any()) continue;
                var name = field.Name;
                if (name[0] == '_')
                    name = name.Substring(1);
                if (name != propertyName) continue;
                return field.GetValue(instance);
            }

            throw new ArgumentException($"{propertyName} does not appear in the provided object");
        }
    }
}