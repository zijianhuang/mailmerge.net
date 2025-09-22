using System.Text.Json.Nodes;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace BulkMailMerge
{
    /// <summary>
    /// Merge Json array with HTML template
    /// </summary>
    public class JsonMerger
    {
        readonly HandlebarsTemplate<object, object> handlebarsTemplate;
        readonly JsonArray jsonArray;
        readonly ILogger logger;
        readonly string keyField;

        public JsonMerger(string htmlTemplateText, string jsonArrayText, string keyField, ILogger logger)
        {
            this.logger = logger;
            this.keyField = keyField;
            handlebarsTemplate = Handlebars.Compile(htmlTemplateText);
            jsonArray = JsonArray.Parse(jsonArrayText) as JsonArray;
        }

        public JsonMerger(string htmlTemplateText, JsonArray jsonArray, string keyField, ILogger logger)
        {
            this.logger = logger;
            this.keyField = keyField;
            handlebarsTemplate = Handlebars.Compile(htmlTemplateText);
            this.jsonArray = jsonArray;
        }


        public string Merge(string id)
        {
            var found = jsonArray.SingleOrDefault(d =>
            {
                var obj = d as JsonObject;
                if (obj == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(keyField))
                {
                    KeyValuePair<string, JsonNode> emailAddressField = obj.FirstOrDefault();
                    if (emailAddressField.Equals(null))
                    {
                        return false;
                    }

                    var v = emailAddressField.Value.ToString();
                    return v == id;
                }
                else
                {
                    if (obj.TryGetPropertyValue(keyField, out var value))
                    {
                        var v = value.ToString();
                        return v == id;
                    }

                    return false;
                }
            });

            if (found == null)
            {
                logger.LogWarning($"Not found data for {id}");
                return null;
            }

            var bodyText = handlebarsTemplate(found);
            return bodyText;
        }

        public static Tuple<JsonArray, string[]> ConvertToJsonArrayAndContactList(string filePath, string keyField)
        {
            var jsonArrayText= File.ReadAllText(filePath);
            var jsonArray = JsonArray.Parse(jsonArrayText) as JsonArray;
            var contactList = new List<string>(jsonArray.Count);
            foreach (var item in jsonArray)
            {
                if (string.IsNullOrEmpty(keyField))
                {
                    contactList.Add((item as JsonObject).First().Value.ToString());
                }
                else
                {
                    contactList.Add((item as JsonObject)[keyField].ToString());
                }
            }

            return Tuple.Create(jsonArray, contactList.ToArray());
        }
    }
}
