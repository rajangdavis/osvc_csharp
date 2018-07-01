using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OSvCCSharp
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

    static class Converter
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
        public string interface_;
        public string session;
        public string oauth;
        public string version = "v1.3";
        public bool no_ssl_verify;
        public bool suppress_rules;
        public bool demo_site;
        public string access_token;

        public void SetUsername(string un) => username = un;
        public void SetPassword(string pw) => password = pw;
        public void SetInterface(string intf) => interface_ = intf ?? "";
        public void SetVersion(string ver) => version = ver ?? "v1.3";
        public void SetSession(string sessionVar) => session = sessionVar;
        public void SetOAuth(string oauthVar) => oauth = oauthVar;
        public void ChangeSSL(bool noSslVerifyVar) => no_ssl_verify = noSslVerifyVar;
        public void SuppressRules(bool suppressRulesVar) => suppress_rules= suppressRulesVar;
        public void IsDemo(bool demoSite) => demo_site = demoSite;
        public void SetAccessToken(string at) => access_token= at;
    }

    public class Client
    {
        internal Configuration config;

        public Client(string interface_, string username = "", string password = "", string version = "v1.3", string session ="", string oauth = "", string access_token = "", bool no_ssl_verify = false, bool suppress_rules = false, bool demo_site = false)
        {
            Configuration configuration = new Configuration();
            configuration.SetUsername(username);
            configuration.SetPassword(password);
            configuration.SetInterface(interface_);
            configuration.SetSession(session);
            configuration.SetVersion(version);
            configuration.SetOAuth(oauth);
            configuration.ChangeSSL(no_ssl_verify);
            configuration.IsDemo(demo_site);
            configuration.SetAccessToken(access_token);

            config = configuration;
        }

    }

    public static class Connect
    {

        public static string Get( Dictionary<string, object> options)
        {
            string url = (string)options["url"];
            Client client = (Client)options["client"];
            return WebRequestMethod(client, url);
            
        }

        public static string Post(Dictionary<string, object> options)
        {
            string url = (string)options["url"];
            Client client = (Client)options["client"];
            Dictionary<string, object> jsonData = (Dictionary<string, object>)options["json"];
            return WebRequestMethod(client, url, "POST", data: jsonData);

        }

        public static string Patch(Dictionary<string, object> options)
        {
            string url = (string)options["url"];
            Client client = (Client)options["client"];
            Dictionary<string, object> jsonData = (Dictionary<string, object>)options["json"];
            var headers = new Dictionary<string, string>{
                {"X-HTTP-Method-Override","PATCH"}
            };
            return WebRequestMethod(client, url, "POST", data: jsonData, headers: headers);

        }

        public static string Delete(Dictionary<string, object> options)
        {
            string url = (string)options["url"];
            Client client = (Client)options["client"];
            return WebRequestMethod(client, url, "DELETE");

        }

        public static string Options(Dictionary<string, object> optionsDictionary)
        {
            string url = (string)optionsDictionary["url"];
            Client client = (Client)optionsDictionary["client"];
            return WebRequestMethod(client, url, "OPTIONS");

        }

        private static string WebRequestMethod(Client client, string url = "", string method = "GET", object data = null, Dictionary<string, string> headers = null)
        {
            string resourceUrl = UrlFormat(url, client);
            WebRequest req = WebRequest.Create(resourceUrl);
            req.Method = method;
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }
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

                        return "{ 'type': '"+ resourceUrl +"','title': 'Invalid URL',  'status': 400, 'detail': 'The URL you are requesting is bad; please make sure you have the interface named spelt correctly','instance': '"+ resourceUrl + "','o:errorCode': 'OSvCCSharp Generated Error'}";
                    }
                    else
                    {
                        StreamReader rd = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                        return rd.ReadToEnd();
                    }
                }
            }
        }

        private static string UrlFormat(string resourceUrl, Client client)
        {
            string custOrDemo = client.config.demo_site == false ? "custhelp" : "rightnowdemo";
            return $"https://{client.config.interface_}.{custOrDemo}.com/services/rest/connect/{client.config.version}/{resourceUrl}";
        }

    }


    static internal class NormalizeResults
    {
        public static string Normalize(string responseData)
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

        public static List<Dictionary<string,string>> IterateThroughRows(Item item)
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

    public static class QueryResults
    {

        public static string Query(Dictionary<string,object> options)
        {
            string query = (string)options["query"];

            Client client = (Client)options["client"];

            var getOptions = new Dictionary<string, object>{
                { "url", $"queryResults?query={query}"},
                { "client", client }
            };


            return NormalizeResults.Normalize(OSvCCSharp.Connect.Get(getOptions));
        }
    }



    public static class QueryResultsSet
    {
        public static Dictionary<string, string> QuerySet(Dictionary<string,object> options)
        {
            var queries = (List<Dictionary<string, string>>)options["queries"];
            Client client = (Client)options["client"];
            List<string> queryArr = new List<string>();
            List<string> keyMap = new List<string>();

            foreach (Dictionary<string, string> query in queries)
            {
                keyMap.Add(query["key"]);
                queryArr.Add(query["query"]);
            }

            var queryResultsSet = new Dictionary<string, string>();
            string finalQueryString = String.Join("; ", queryArr);
            var querySetOptions = new Dictionary<string, object>
            {
                { "client", client},
                { "query", finalQueryString }
            };

            
            string finalResults = OSvCCSharp.QueryResults.Query(querySetOptions);

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


    public static class AnalyticsReportResults
    {
        public static string Run(Dictionary<string,object> options)
        {
            var jsonData = (Dictionary<string, object>)options["json"];
            var client = (Client)options["client"];

            Dictionary<string, object> arrOptions = new Dictionary<string, object>
            {
                {"url", "analyticsReportResults" },
                {"json", jsonData },
                { "client", client }
            };

            var reportRequest = Item.FromJson(OSvCCSharp.Connect.Post(arrOptions));
            return JsonConvert.SerializeObject(NormalizeResults.IterateThroughRows(reportRequest), Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
        }
    }
}