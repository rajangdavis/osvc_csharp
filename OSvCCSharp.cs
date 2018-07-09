using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

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
        public void SuppressRules(bool suppressRulesVar) => suppress_rules = suppressRulesVar;
        public void IsDemo(bool demoSite) => demo_site = demoSite;
        public void SetAccessToken(string at) => access_token = at;
    }

    public class Client
    {
        internal Configuration config;

        public Client(string interface_, string username = "", string password = "", string version = "v1.3", string session = "", string oauth = "", string access_token = "", bool no_ssl_verify = false, bool suppress_rules = false, bool demo_site = false)
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

        private static string DownloadFileCheck(Dictionary<string, object> downloadOptions)
        {
            string url = (string)downloadOptions["url"];

            if (url.IndexOf("?download", StringComparison.CurrentCulture) > -1)
            {
                Dictionary<string, object> optionsCopy = new Dictionary<string, object>(downloadOptions);
                optionsCopy["url"] = url.Replace("?download", "");
                string fileData = WebRequestMethod(optionsCopy);
                JToken token = JObject.Parse(fileData);
                string fileName = (string)token.SelectToken("fileName");
                if (String.IsNullOrEmpty(fileName))
                {
                    fileName = "downloadedAttachment.tgz";
                }
                downloadOptions.Add("downloadFileName", fileName);
                return WebRequestMethod(downloadOptions);
            }
            else
            {
                return WebRequestMethod(downloadOptions);
            }
        }

        public static string Get(Dictionary<string, object> options)
        {
            Dictionary<string, object> getOptions = new Dictionary<string, object>(options);
            getOptions.Add("method", "GET");
            return DownloadFileCheck(getOptions);
        }

        public static string Post(Dictionary<string, object> options)
        {
            Dictionary<string, object> postOptions = new Dictionary<string, object>(options);
            postOptions.Add("method", "POST");
            return WebRequestMethod(postOptions);
        }

        public static string Patch(Dictionary<string, object> options)
        {
            Dictionary<string, object> patchOptions = new Dictionary<string, object>(options);
            patchOptions.Add("headers", new Dictionary<string, string>{
                {"X-HTTP-Method-Override","PATCH"}
            });
            patchOptions.Add("method", "POST");
            return WebRequestMethod(patchOptions);
        }

        public static string Delete(Dictionary<string, object> options)
        {
            Dictionary<string, object> deleteOptions = new Dictionary<string, object>(options);
            deleteOptions.Add("method", "DELETE");
            return WebRequestMethod(deleteOptions);
        }

        public static string Options(Dictionary<string, object> optionsDictionary)
        {
            Dictionary<string, object> optionsOptions = new Dictionary<string, object>(optionsDictionary);
            optionsOptions.Add("method", "OPTIONS");
            return WebRequestMethod(optionsOptions);
        }

        private static string WebRequestMethod(Dictionary<string, object> optionsForWebRequest)
        {
            Client client = (Client)optionsForWebRequest["client"];
            string url = (string)optionsForWebRequest["url"];
            string resourceUrl = UrlFormat(url, client);
            string method = (string)optionsForWebRequest["method"];

            Dictionary<string, object> data = new Dictionary<string, object> { };

            if (optionsForWebRequest.ContainsKey("json"))
            {
                data = (Dictionary<string, object>)optionsForWebRequest["json"];
            }

            Dictionary<string, string> headers = new Dictionary<string, string> { };

            if (optionsForWebRequest.ContainsKey("headers"))
            {
                var clientHeaders = (Dictionary<string, string>)optionsForWebRequest["headers"];
                headers = OptionalHeadersCheck(clientHeaders, optionsForWebRequest);
            }
            else
            {
                headers = OptionalHeadersCheck(headers, optionsForWebRequest);
            }

            if (client.config.no_ssl_verify == true)
            {
                // Turns off SSL verification
                // https://dejanstojanovic.net/aspnet/2014/september/bypass-ssl-certificate-validation/
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            // We create the request and set the Accept Header
            // Because the Accept Header is not accessible for WebRequests
            // https://msdn.microsoft.com/library/system.net.httpwebrequest.accept.aspx/
            HttpWebRequest req = JsonSchemaCheck(optionsForWebRequest);

            req.Method = method;
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }
            }

            if (method == "POST")
            {
                data = UploadFileCheck(data, optionsForWebRequest);
                var jsonDataConverted = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] jsonData = encoding.GetBytes(jsonDataConverted);
                Stream newStream = req.GetRequestStream();
                newStream.Write(jsonData, 0, jsonDataConverted.Length);
                newStream.Close();
            }

            try
            {

                if (optionsForWebRequest.ContainsKey("downloadFileName"))
                {

                    using (WebResponse res = req.GetResponse() as HttpWebResponse)
                    {
                        string fileName = (string)optionsForWebRequest["downloadFileName"];
                        Stream httpResponseStream = res.GetResponseStream();

                        int bufferSize = 1024;
                        byte[] buffer = new byte[bufferSize];
                        int bytesRead = 0;

                        // Read from response and write to file
                        FileStream fileStream = File.Create(fileName);
                        while ((bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                        }

                        fileStream.Dispose();

                        return $"Downloaded {fileName}";
                    }
                }
                else
                {
                    using (WebResponse res = req.GetResponse() as HttpWebResponse)
                    {
                        StreamReader rd = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                        return rd.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                // For the 301 redirects
                var hasChanged = (req.RequestUri != req.Address);
                if (hasChanged)
                {
                    var forwardedUrl = req.RequestUri.MakeRelativeUri(req.Address);
                    optionsForWebRequest["url"] = forwardedUrl.ToString();
                    return WebRequestMethod(optionsForWebRequest);
                }

                using (WebResponse res = e.Response)
                {
                    if (res == null)
                    {

                        return "{ 'type': '" + resourceUrl + "','title': 'Invalid URL',  'status': 400, 'detail': 'The URL you are requesting is bad; please make sure you have the interface named spelt correctly','instance': '" + resourceUrl + "','o:errorCode': 'OSvCCSharp Generated Error'}";
                    }
                    else
                    {
                        StreamReader rd = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                        return rd.ReadToEnd();
                    }
                }
            }
        }

        private static Dictionary<string,object> UploadFileCheck(Dictionary<string, object> data, Dictionary<string, object> options)
        {
            if (options.ContainsKey("files"))
            {
                var filesToUpload = (List<string>)options["files"];
                var fileAttachmentList = new List<Dictionary<string, object>>();
                bool noFileError = false;

                foreach (var file in filesToUpload)
                {
                    try
                    {
                        Dictionary<string, object> fileHash = new Dictionary<string, object> { };
                        string fileName = Path.GetFileName(file);
                        Byte[] bytes = File.ReadAllBytes(file);
                        String fileData = Convert.ToBase64String(bytes);
                        fileHash.Add("fileName", fileName);
                        fileHash.Add("data", fileData);
                        fileAttachmentList.Add(fileHash);
                    }
                    catch
                    {
                        Console.WriteLine($"There was an error with uploading file from '{file}'");
                        noFileError = true;
                    }
                }

                if (noFileError == false)
                {
                    data.Add("fileAttachments", fileAttachmentList);
                }
            }
            return data;
        }

        private static HttpWebRequest JsonSchemaCheck(Dictionary<string, object> options)
        {
            Client client = (Client)options["client"];
            string url = (string)options["url"];
            string resourceUrl = UrlFormat(url, client);
            var httpReq = (HttpWebRequest)WebRequest.Create(resourceUrl);
            httpReq = SetAuthentication(httpReq, client);
            if (options.ContainsKey("schema") && (bool)options["schema"] == true)
            {
                httpReq.Accept = "application/schema+json";
            }

            return httpReq;
        }

        private static HttpWebRequest SetAuthentication(HttpWebRequest req, Client client)
        {
            string credUrl = UrlFormat("", client);
            CredentialCache cache = new CredentialCache();

            if (client.config.username != "")
            {
                var creds = new NetworkCredential(client.config.username, client.config.password);
                cache.Add(new Uri(credUrl), "Basic", creds);
            }

            if (client.config.session != "")
            {
                req.Headers.Add("Authorization", $"Session {client.config.session}");
            }

            if (client.config.oauth != "")
            {
                req.Headers.Add("Authorization", $"Bearer {client.config.oauth}");
            }

            req.Credentials = cache;

            return req;
        }

        private static string UrlFormat(string resourceUrl, Client client)
        {
            string custOrDemo = client.config.demo_site == false ? "custhelp" : "rightnowdemo";
            return $"https://{client.config.interface_}.{custOrDemo}.com/services/rest/connect/{client.config.version}/{resourceUrl}";
        }

        private static Dictionary<string, string> SuppressRulesCheck(Dictionary<string, string> headers, Client client)
        {
            if (client.config.suppress_rules == true)
            {
                headers.Add("OSvC-CREST-Suppress-All", "true");
            }
            return headers;
        }

        private static Dictionary<string, string> AccessTokenCheck(Dictionary<string, string> headers, Client client)
        {
            if (client.config.access_token != "")
            {
                headers.Add("osvc-crest-api-access-token", client.config.access_token);
            }
            return headers;
        }

        private static Dictionary<string, string> OptionalHeadersCheck(Dictionary<string, string> headers, Dictionary<string, object> optionsToCheck)
        {

            Client rnClient = (Client)optionsToCheck["client"];

            var headersWithSuppression = SuppressRulesCheck(headers, rnClient);
            var headersWithAccessToken = AccessTokenCheck(headersWithSuppression, rnClient);

            if (optionsToCheck.ContainsKey("exclude_null") && (bool)optionsToCheck["exclude_null"] == true)
            {
                headersWithAccessToken.Add("prefer", "exclude-null");
            }

            if (optionsToCheck.ContainsKey("next_request") && (int)optionsToCheck["next_request"] > 0)
            {
                headersWithAccessToken.Add("osvc-crest-next-request-after", optionsToCheck["next_request"].ToString());
            }

            if (optionsToCheck.ContainsKey("utc_time") && (bool)optionsToCheck["utc_time"] == true)
            {
                headersWithAccessToken.Add("OSvC-CREST-Time-UTC", "yes");
            }

            if (optionsToCheck.ContainsKey("annotation"))
            {
                // Check the annotation length
                headersWithAccessToken.Add("OSvC-CREST-Application-Context", (string)optionsToCheck["annotation"]);
            }
            return headersWithAccessToken;
        }

    }

    static internal class NormalizeResults
    {
        public static string Normalize(string responseData)
        {
            var error = JsonError.FromJson(responseData);
            var data = JsonResponse.FromJson(responseData);

            var finalList = new List<List<Dictionary<string, string>>>();
            if (data != null && data.Items != null)
            {
                foreach (Item item in data.Items)
                {
                    List<Dictionary<string, string>> resultArray = IterateThroughRows(item);
                    finalList.Add(resultArray);
                }

                // 
                if (finalList.Count == 1)
                {
                    return JsonConvert.SerializeObject(finalList.SelectMany(x => x), Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                }
                else
                {
                    return JsonConvert.SerializeObject(finalList, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                }

            }
            else if (data != null && data.Items == null)
            {
                return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
            }
            else
            {
                return JsonConvert.SerializeObject(error, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
            }
        }

        public static List<Dictionary<string, string>> IterateThroughRows(Item item)
        {

            var finalHash = new List<Dictionary<string, string>>();

            if (item.Rows != null)
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

        public static string Query(Dictionary<string, object> options)
        {
            string query = (string)options["query"];
            var queryOptions = new Dictionary<string, object>(options);
            queryOptions.Add("url", $"queryResults?query={query}");
            return NormalizeResults.Normalize(OSvCCSharp.Connect.Get(queryOptions));
        }
    }

    public static class QueryResultsSet
    {
        public static Dictionary<string, string> QuerySet(Dictionary<string, object> options)
        {
            var queries = (List<Dictionary<string, string>>)options["queries"];
            List<string> queryArr = new List<string>();
            List<string> keyMap = new List<string>();

            foreach (Dictionary<string, string> query in queries)
            {
                keyMap.Add(query["key"]);
                queryArr.Add(query["query"]);
            }

            var queryResultsSet = new Dictionary<string, string>();
            var querySetOptions = new Dictionary<string, object>(options);

            //if (options.ContainsKey("parallel") && (bool)options["parallel"] == true)
            //{

            //    foreach (var query in queryArr)
            //    {
            //        querySetOptions.Add("query", query);
            //        queryResultsSet.Add("key", OSvCCSharp.QueryResults.Query(querySetOptions));
            //    }

            //    return queryResultsSet;
            //}


            
            string finalQueryString = String.Join("; ", queryArr);

            querySetOptions.Add("query", finalQueryString);

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
        public static string Run(Dictionary<string, object> options)
        {
            Dictionary<string, object> arrOptions = new Dictionary<string, object>(options);
            arrOptions.Add("url", "analyticsReportResults");

            var reportRequest = Item.FromJson(OSvCCSharp.Connect.Post(arrOptions));
            return JsonConvert.SerializeObject(NormalizeResults.IterateThroughRows(reportRequest), Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
        }
    }
}