using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.NewtonsoftJson
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a JSON-formatted string into a dictionary.
        /// </summary>
        /// <param name="json">A JSON-formatted string.</param>
        /// <returns>Inputs.</returns>
        public static Inputs ToInputs(this string json)
            => json?.ToDictionary().ToInputs();

        /// <summary>
        /// Converts a dictionary into an <see cref="Inputs"/>.
        /// </summary>
        /// <param name="json">A dictionary.</param>
        /// <returns>Inputs.</returns>
        public static Inputs ToInputs(this Dictionary<string, object> dictionary)
            => dictionary == null ? new Inputs() : new Inputs(dictionary);

        /// <summary>
        /// Converts a JSON formatted string into a dictionary.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>Returns a <c>null</c> if the object cannot be converted into a dictionary.</returns>
        public static Dictionary<string, object> ToDictionary(this string json)
        {
            var values = JsonConvert.DeserializeObject(json,
                new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateParseHandling = DateParseHandling.None
                });
            return GetValue(values) as Dictionary<string, object>;
        }

        /// <summary>
        /// Gets the value contained in a JObject, JValue, JProperty or JArray.
        /// </summary>
        /// <param name="value">The object containing the value to extract.</param>
        /// <remarks>If the value is a recognized type, it is returned unaltered.</remarks>
        private static object GetValue(this object value)
        {
            if (value is JObject objectValue)
            {
                var output = new Dictionary<string, object>();
                foreach (var kvp in objectValue)
                {
                    output.Add(kvp.Key, GetValue(kvp.Value));
                }
                return output;
            }

            if (value is JProperty propertyValue)
            {
                return new Dictionary<string, object>
                {
                    { propertyValue.Name, GetValue(propertyValue.Value) }
                };
            }

            if (value is JArray arrayValue)
            {
                return arrayValue.Children().Aggregate(new List<object>(), (list, token) =>
                {
                    list.Add(GetValue(token));
                    return list;
                });
            }

            if (value is JValue rawValue)
            {
                var val = rawValue.Value;
                if (val is long l)
                {
                    if (l >= int.MinValue && l <= int.MaxValue)
                    {
                        return (int)l;
                    }
                }
                return val;
            }

            return value;
        }

        /// <summary>
        /// Converts a JSON-formatted string into a JObject.
        /// </summary>
        /// <param name="json">A JSON-formatted string.</param>
        /// <returns>A JObject</returns>
        private static JObject ToJObject(this string json) => JObject.Parse(json);

        /// <summary>
        /// Converts a JSON-formatted string into an object.
        /// </summary>
        /// <param name="json">A JSON-formatted string.</param>
        /// <returns>An object that can be set to <see cref="ExecutionResult.Data"/>.</returns>
        public static object ToResult(this string json) => json.ToJObject() as object;
    }
}
