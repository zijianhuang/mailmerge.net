using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;

namespace BulkMailMerge
{
    public static class CsvToJson
    {
        /// <summary>
        /// Convert CSV file to JsonArray
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <returns></returns>
        public static JsonArray Convert(string csvFilePath)
        {
            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<dynamic>().ToArray();
            var jsonArray = new JsonArray();
            foreach (var r in records)
            {
                string bs = JsonSerializer.Serialize<dynamic>(r);
                var bb = JsonObject.Parse(bs);
                jsonArray.Add(bb);

            }
            return jsonArray;
        }

        public static Tuple<JsonArray, string[]> ConvertToJsonArrayAndContactList(string filePath, string keyField)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<dynamic>().ToArray();
            var jsonArray = new JsonArray();
            var contactList = new List<string>(records.Length);
            foreach (var r in records)
            {
               var bb = r as JsonObject;
                jsonArray.Add(bb);
                if (string.IsNullOrEmpty(keyField))
                {
                    contactList.Add((bb).First().Value.ToString());
                }
                else
                {
                    contactList.Add((bb)[keyField].ToString());
                }
            }

            return Tuple.Create(jsonArray, contactList.ToArray());
        }


    }
}
