using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;

namespace HrTetris.Helper
{
    /// <summary>
    /// Static class containing methods for Serialization of objects to JSON. (Object -> JSON),
    /// Methods for Deserialization of objects from JSON. (JSON -> Object),
    /// Methods for generating Json Schemas and validating Schemas and JSON data
    /// </summary>
    public static class JSON
    {
        // Dictionary with already processed Types and corresponding JsonSchemas for faster schema retrieval.
        private static Dictionary<Type, string> _schemaCache = new Dictionary<Type, string>();

        /// <summary>
        /// Settings used for serialization.
        /// </summary>
        public static JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
        };

        #region Object To JSON

        /// <summary>
        /// Serializes the object to a JSON string and returns it.
        /// </summary>
        /// <typeparam name="T">Type of the object that will be serialized.</typeparam>
        /// <param name="obj">Object that will be serialized.</param>
        /// <returns>Returns a formatted JSON string representing the serialized input object.</returns>
        public static string SerializeToJsonString<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, Settings);
        }

        /// <summary>
        /// Serializes the input object to a JObject object and returns it.
        /// </summary>
        /// <typeparam name="T">Type of the object that will be serialized</typeparam>
        /// <param name="obj">Object that will be serialized.</param>
        /// <returns>Returns a Newtonsoft.Json.Linq JObject containing the serialized input object.</returns>
        public static JObject GenerateJsonObject<T>(T obj)
        {
            return JObject.FromObject(obj);
        }

        /// <summary>
        /// Serializes the object to a JSON string and writes it to a file.
        /// </summary>
        /// <typeparam name="T">Type of the object that will be serialized.</typeparam>
        /// <param name="obj">Object that will be serialized.</param>
        /// <param name="path">Full path and filename to save the JSOn file to.</param>
        public static void SerializeToJsonFile<T>(T obj, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (StreamWriter sWriter = new StreamWriter(path))
            {
                sWriter.WriteLine(SerializeToJsonString(obj));
                sWriter.Close();
            }
        }

        #endregion Object To JSON

        #region JSON To Object

        /// <summary>
        /// Deserializes the input JSON string and returns a new object.
        /// Validates the JSON string against a schema generated for the selected class type.
        /// Throws a exception if the validation did not pass.
        /// </summary>
        /// <typeparam name="T">Type of the object that is to be returned. Schema of this type is created and used to validate the JSON.</typeparam>
        /// <param name="json">String containing the JSON data.</param>
        /// <param name="validate">Validate the JSON before serializing ?</param>
        /// <returns>Returns a new object of type T, deserialized from the input JSON.</returns>
        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        /// <summary>
        /// Deserializes and returns the object straight from the JSON file.
        /// Validates the JSON string against a schema generated for the selected class type.
        /// Throws a exception if the validation did not pass.
        /// </summary>
        /// <typeparam name="T">Type of the object that is to be returned. Schema of this type is created and used to validate the JSON.</typeparam>
        /// <param name="path">String containing the JSON data.</param>
        /// <param name="validate">Validate the JSON before serializing ?</param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string path)
        {
            if (File.Exists(path))
            {
                return DeserializeObject<T>(File.ReadAllText(path));
            }
            return default(T);
        }

        #endregion JSON To Object

    }
}
