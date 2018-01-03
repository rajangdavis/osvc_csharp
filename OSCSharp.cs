using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OSCCSharp
{
    // Handles the JSON Response
    internal partial class JsonResponse
    {
        [JsonProperty("items")]
        internal List<Item> Items { get; set; }
    }

    internal partial class Item
    {
        [JsonProperty("columnNames")]
        public List<string> ColumnNames { get; set; }

        [JsonProperty("rows")]
        public List<List<string>> Rows { get; set; }
    }

    internal partial class JsonError
    {
        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("instance")]
        public string Instance { get; set; }

        [JsonProperty("o:errorCode")]
        public string OErrorCode { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    internal partial class JsonError
    {
        internal static JsonError FromJson(string json) => JsonConvert.DeserializeObject<JsonError>(json, Converter.Settings);
    }

    internal partial class Item
    {
        internal static Item FromJson(string json) => JsonConvert.DeserializeObject<Item>(json, Converter.Settings);
    }

    internal partial class JsonResponse
    {
        internal static JsonResponse FromJson(string json) => JsonConvert.DeserializeObject<JsonResponse>(json, Converter.Settings);
    }
    internal static class Serialize
    {
        internal static string ToJson(this JsonResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal class Converter
    {
        internal static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };

        internal static JsonSerializerSettings Settings => settings;
    }
    // END JSON Response


    internal class Configuration
    {
        public string username;
        public string password;
        public string interfaceName;
        public string version = "v1.3";
        public bool ssl_verify = true;
        public bool rule_suppression = false;
        public bool demo_site = false;

        public void SetUsername(string un) => username = un ?? throw new ArgumentNullException(nameof(un));
        public void SetPassword(string pw) => password = pw ?? throw new ArgumentNullException(nameof(pw));
        public void SetInterface(string intf) => interfaceName = intf ?? throw new ArgumentNullException(nameof(intf));
        public void SetVersion(string ver) => version = ver ?? throw new ArgumentNullException(nameof(ver));
        public void ChangeSSL(bool sslVerify) => ssl_verify = sslVerify;
        public void SuppressRules(bool suppressRules) => rule_suppression = suppressRules;
        public void IsDemo(bool demoSite) => demo_site = demoSite;
    }

    public class Client
    {
        internal Configuration config;

        public Client(string username, string password, string interfaceName, bool sslVerify = true, bool suppressRules = false, bool demoSite = false)
        {
            Configuration configuration = new Configuration();
            configuration.SetUsername(username);
            configuration.SetPassword(password);
            configuration.SetInterface(interfaceName);
            configuration.ChangeSSL(sslVerify);
            configuration.IsDemo(demoSite);
            config = configuration;
        }

        public void ChangeVersion(string versionToChangeTo)
        {
            if (string.IsNullOrEmpty(versionToChangeTo))
            {
                throw new ArgumentException("message", nameof(versionToChangeTo));
            }

            config.SetVersion(versionToChangeTo);
        }

    }


    public class Connect
    {
        protected Client client;

        public Connect(Client clientVar) => client = clientVar ?? throw new ArgumentNullException(nameof(clientVar));

        public string Get(string url)
        {

            return WebRequestMethod(url, "GET");
            
        }

        public string Post(string url, object jsonData)
        {

            return WebRequestMethod(url, "POST", data: jsonData);

        }

        public string Patch(string url, object jsonData)
        {
            var headers = new Dictionary<string, string>() {
                {"X-HTTP-Method-Override","PATCH"}
            };
            return WebRequestMethod(url, "POST", data: jsonData, headers: headers);

        }

        public string Delete(string url)
        {

            return WebRequestMethod(url, "DELETE");

        }

        private string WebRequestMethod(string url, string method, object data = null, Dictionary<string, string> headers = null)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            string resourceUrl = UrlFormat(url);
            WebRequest req = WebRequest.Create(resourceUrl);
            req.Method = method;
            if(headers != null)
                foreach (KeyValuePair<string, string> header in headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }

            if (data!=null)
            {
                var jsonDataConverted = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] jsonData = encoding.GetBytes(jsonDataConverted);
                Stream newStream = req.GetRequestStream();
                newStream.Write(jsonData, 0, jsonDataConverted.Length);
                newStream.Close();
            }
            req.Credentials = new NetworkCredential(client.config.username, client.config.password);
            try
            {
                using (WebResponse res = req.GetResponse())
                {
                    StreamReader rd = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                    return rd.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (WebResponse res = e.Response)
                {
                    if(res == null)
                    {

                        return "{ 'type': '"+ resourceUrl +"','title': 'Invalid URL',  'status': 400, 'detail': 'The URL you are requesting is bad; please make sure you have the interface named spelt correctly','instance': '"+ resourceUrl + "','o:errorCode': 'OSCCSharp Generated Error'}";
                    }
                    else
                    {
                        StreamReader rd = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                        return rd.ReadToEnd();
                    }
                }
            }
        }

        private string UrlFormat(string resourceUrl)
        {
            string custOrDemo = client.config.demo_site == false ? "custhelp" : "rightnowdemo";
            return $"https://{client.config.interfaceName}.{custOrDemo}.com/services/rest/connect/{client.config.version}/{resourceUrl}";
        }

    }


    internal class NormalizeResults
    {
        public string Normalize(string responseData)
        {
            var error = JsonError.FromJson(responseData);
            var data = JsonResponse.FromJson(responseData);
            var finalList = new List<List<Dictionary<string, string>>>();
            if (data!= null && data.Items != null)
            {
                foreach(Item item in data.Items)
                {
                    List<Dictionary<string, string>> resultArray = IterateThroughRows(item);
                    finalList.Add(resultArray);
                }
                
                return JsonConvert.SerializeObject(finalList.SelectMany(x => x), Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                
            }
            else if (data != null && data.Items == null) {
                return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
            }
            else
            {
                return JsonConvert.SerializeObject(error, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
            }
        }

        public List<Dictionary<string,string>> IterateThroughRows(Item item)
        {

            var finalHash = new List<Dictionary<string, string>>();

            if(item.Rows != null)
            {
                foreach (var row in item.Rows)
                {
                    var objHash = new Dictionary<string, string>();
                    for (int j = 0; j < item.ColumnNames.Count; j++)
                    {
                        objHash.Add(item.ColumnNames[j], row[j]);
                    }
                    finalHash.Add(objHash);

                }

            }
            return finalHash;
        }

    }

    public class QueryResults
    {
        protected Client client;

        public QueryResults(Client clientVar) => client = clientVar ?? throw new ArgumentNullException(nameof(clientVar));

        public string Query(string query)
        {
            Connect request = new OSCCSharp.Connect(client);
            var results = new NormalizeResults();
            return results.Normalize(request.Get($"queryResults?query={query}"));
        }
    }



    public class QueryResultsSet
    {
        protected Client client;

        public QueryResultsSet(Client clientVar) => client = clientVar ?? throw new ArgumentNullException(nameof(clientVar));

        public Dictionary<string,string> QuerySet(List<Dictionary<string,string>> queries)
        {

            List<string> queryArr = new List<string>();
            List<string> keyMap = new List<string>();

            foreach(Dictionary<string, string> query in queries)
            {
                keyMap.Add(query["key"]);
                queryArr.Add(query["query"]);
            }

            var queryResultsSet = new Dictionary<string, string>();
            var querySearch = new QueryResults(client);

            string finalQueryString = String.Join("; ", queryArr);
            string finalResults = querySearch.Query(finalQueryString);
    
            try
            {
                var finalResultsToList = JsonConvert.DeserializeObject<List<List<Dictionary<string, string>>>>(finalResults);
                for (int i = 0; i < keyMap.Count; i++)
                {
                    queryResultsSet[keyMap[i]] = JsonConvert.SerializeObject(finalResultsToList[i], Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                }

                return queryResultsSet;
            }
            catch (Exception)
            {
                for (int i = 0; i < keyMap.Count; i++)
                {
                    queryResultsSet[keyMap[i]] = finalResults;
                }

                return queryResultsSet;

            }

        }

    }


    public class AnalyticsReportResults
    {
        protected Client client;
        public List<Dictionary<string,string>> filters;

        public AnalyticsReportResults(Client clientVar) => client = clientVar ?? throw new ArgumentNullException(nameof(clientVar));

        public string Run(int id = -1, string lookupName = "")
        {
            var jsonData = new Dictionary<string, object>();
            if (id!= -1) {
                jsonData.Add("id", id);
            }
            else if(lookupName.Length > 0)
            {
                jsonData.Add("lookupName", lookupName);
            }
            
            if(filters!=null && filters.Count > 0)
            {
                jsonData.Add("filters", filters);
            }
            

            Connect request = new OSCCSharp.Connect(client);
            var results = new NormalizeResults();
            var reportRequest = Item.FromJson(request.Post("analyticsReportResults", jsonData));
            return JsonConvert.SerializeObject(results.IterateThroughRows(reportRequest), Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
        }
    }

    // Utility functions
    public static class Utils
    {
        public static string DateToIsoString(string dateString, string culture) {
            IFormatProvider timeFormat = new System.Globalization.CultureInfo(culture, true);
            DateTime dateTime = Convert.ToDateTime(dateString);
            return dateTime.ToString("s");
        }

        public static Dictionary<string,object> Arrf()
        {
            return new Dictionary<string, object>();
        }
    }

}