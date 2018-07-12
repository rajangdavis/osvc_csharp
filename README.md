# OSvCCSharp

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/9921a01e055640d2bb2ece19c95986ea)](https://app.codacy.com/app/rajangdavis/osvc_csharp?utm_source=github.com&utm_medium=referral&utm_content=rajangdavis/osvc_csharp&utm_campaign=badger)

An (under development) C# library for using the [Oracle Service Cloud REST API](https://docs.oracle.com/cloud/latest/servicecs_gs/CXSVC/) influenced by the [ConnectPHP API](http://documentation.custhelp.com/euf/assets/devdocs/november2016/Connect_PHP/Default.htm)

## Todo
I am looking to implement the following items soon:
1. Test suite
2. Documentation

## Compatibility

The library is being tested against Oracle Service Cloud 18A using C# v4.0.30319.

All of the HTTP methods should work on any version of Oracle Service Cloud since version May 2015; however, there maybe some issues with querying items on any version before May 2016. This is because ROQL queries were not exposed via the REST API until May 2016.

## Basic Usage
The features that work to date are as follows:

1. [HTTP Methods](#http-methods)
    1. For creating objects and [uploading one or more file attachments](#uploading-file-attachments), make a [POST request with the OSvCCSharp.Connect Object](#post)
    2. For reading objects and [downloading one or more file attachments](#downloading-file-attachments), make a [GET request with the OSvCCSharp.Connect Object](#get)
    3. For updating objects, make a [PATCH request with the OSvCCSharp.Connect Object](#patch)
    4. For deleting objects, make a [DELETE request with the OSvCCSharp.Connect Object](#delete)
    5. For looking up options for a given URL, make an [OPTIONS request with the OSvCCSharp.Connect Object](#options)
2. Running ROQL queries [either 1 at a time](#osvcnodequeryresults-example) or [multiple queries in a set](#osvcnodequeryresultsset-example)
3. [Running Reports](#osvcnodeanalyticsreportsresults)
4. [Optional Settings](#optional-settings)

Here are the _spicier_ (more advanced) features:

1. [Bulk Delete](#bulk-delete)
2. [Running multiple ROQL Queries in parallel](#running-multiple-roql-queries-in-parallel)
3. [Performing Session Authentication](#performing-session-authentication)

## Installing C# and the .NET runtime (for Windows)
[Try this link.](https://www.microsoft.com/net/download/dotnet-framework-runtime)

## Installation
Install with [nuget](https://www.nuget.org/packages/OSvCCSharp/) or use the dotnet commandline:

    $ dotnet add package OSvCCSharp

## Authentication

An OSvCCSharp.Client class lets the library know which credentials and interface to use for interacting with the Oracle Service Cloud REST API.
This is helpful if you need to interact with multiple interfaces or set different headers for different objects.

```C#

// Configuration is as simple as requiring the package
// and passing in values to create an OSvCCSharp.Client
using OSvCCSharp;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

        // Client Configuration
        var rnClient = new OSvCCSharp.Client(
            
            // Interface to connect with 
            interface_: AppSettings["OSC_SITE"],
            
            // Basic Authentication
            username: AppSettings["OSC_ADMIN"],
            password: AppSettings["OSC_PASSWORD"],

            // Session Authentication
            // session: <session_token>,
            
            // OAuth Token Authentication
            // oauth: <oauth_token>,

            // Optional Configuration Settings
            demo_site: true,                   // Changes domain from 'custhelp' to 'rightnowdemo'
            version: "v1.4",                    // Changes REST API version, default is 'v1.3'
            no_ssl_verify: true,                // Turns off SSL verification
            suppress_rules: true,               // Suppresses Business Rules
            access_token: "My access token"     // Adds an access token to ensure quality of service
        );
    }
}


```
## Optional Settings

In addition to a client to specify which credentials, interface, and CCOM version to use, you will need to create an options object to pass in the client as well as specify any additional parameters that you may wish to use.

Here is an example using the client object created in the previous section:
```C#
using OSvCCSharp;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

        var rnClient = new OSvCCSharp.Client(
            interface_: AppSettings["OSC_SITE"],
            username: AppSettings["OSC_ADMIN"],
            password: AppSettings["OSC_PASSWORD"],
        );

        // You will then create an options Dictionary that
        // you will pass the rnClient object to 

        // You may add optional settings
        // to modify certain aspects of
        // the HTTP request that you are making

        var options = new Dictionary<string, object>
        {
            // set the client for the request
            { "client" , rnClient },

            // Adds a custom header that adds an annotation (CCOM version must be set to "v1.4" or "latest"); limited to 40 characters
            { "annotation", "Custom annotation" },

            // Adds a custom header to excludes null from results; for use with GET requests only                    
            { "exclude_null", true },

            // Number of milliseconds before another HTTP request can be made; this is an anti-DDoS measure
            { "next_request", 500 },

            // Sets 'Accept' header to 'application/schema+json'
            { "schema", true },

            // Adds a custom header to return results using Coordinated Universal Time (UTC) format for time (Supported on November 2016+
            { "utc_time", true }
        };
    }
}
```


## HTTP Methods

To use various HTTP Methods to return raw response objects, use the "Connect" object

### POST
```C#
//// OSvCCSharp.Connect.Post(options)
//// returns a string

// Here's how you could create a new ServiceProduct object
// using C# variables, lists, and dictionaries (sort of like JSON)

using OSvCCSharp;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

        var rnClient = new OSvCCSharp.Client(
            interface_: AppSettings["OSC_SITE"],
            username: AppSettings["OSC_ADMIN"],
            password: AppSettings["OSC_PASSWORD"],
            version: "latest"
        );

        var newProd = new Dictionary<string, object>()
            {
                {"descriptions",new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>()
                    {
                        {"labelText", "creating a new ServiceProduct" },
                        {"language", new Dictionary<string,int>(){
                            { "id", 1}
                        } }
                    }
                }},
                {"displayOrder", 3 },
                {"dispositionLinks",new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>()
                    {
                        {"serviceDisposition", new Dictionary<string,string>(){
                            { "lookupName", "Agent: Knowledge"}
                        } }
                    }
                }},
                {"categoryLinks",new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>()
                    {
                        {"serviceCategory", new Dictionary<string,string>(){
                            { "lookupName", "Alerts"}
                        } }
                    }
                }},
                {"adminVisibleInterfaces",new List<Dictionary<string, int>>()
                {
                    new Dictionary<string, int>()
                    {
                        {"id", 1 }
                    }
                }},
                {"endUserVisibleInterfaces",new List<Dictionary<string, int>>()
                {
                    new Dictionary<string, int>()
                    {
                        {"id", 1 }
                    }
                }},
                {"names",new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>()
                    {
                        { "labelText","NEW_PRODUCT" },
                        { "language", new Dictionary<string, int>()
                            {
                                {"id", 1}
                            }
                        }
                    }
                }},
                {"parent",new List<Dictionary<string, int>>()
                {
                    new Dictionary<string, int>()
                    {
                        {"id", 172 }
                    }
                }},
            };

            Dictionary<string, object> options = new Dictionary<string, object>()
            {
                { "client", rnClient},
                { "url", "serviceProducts"},
                { "json", newProd},
                { "annotation", "Creating a product" }
            };

            var createdProduct = OSvCCSharp.Connect.Post(options); // returns JSON body
    }
}

```

### GET
```C#
//// OSvCCSharp.Connect.Get(options)
//// returns a string
// Here's how you could get an instance of ServiceProducts
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            var getProductOptions = new Dictionary<string, object>(){
                { "client", rnClient },
                { "url" , "serviceProducts/15" },
                { "annotation", "Fetching product with id of 15" }
            };

            WriteLine(OSvCCSharp.Connect.Get(getProductOptions)); // returns JSON body

        }
    }
}
```

### PATCH
```C#
//// OSvCCSharp.Connect.Patch(options)
//// returns a string
// Here's how you could update a serviceProduct object
// using dictionaries and lists
// to set field information
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            var jsonDataForUpdate = new Dictionary<string, object>()
            {
                {"names",new List<Dictionary<string, object>>
                    {
                    new Dictionary<string, object>()
                        {
                            { "labelText","PRODUCT-TEST-UPDATED" },
                            { "language", new Dictionary<string, int>()
                                {
                                    {"id", 1}
                                }
                            }
                        }
                    }
                }
            };

            var patchProductOptions = new Dictionary<string, object>(){
                { "client", rnClient },
                { "url" , "serviceProducts/15" },
                { "annotation", "Fetching product with id of 15" },
                { "json", jsonDataForUpdate }
            };

            WriteLine(OSvCCSharp.Connect.Patch(patchProductOptions)); // returns empty body

        }
    }
}


```

### DELETE
```C#
//// OSvCCSharp.Connect.Delete(options)
//// returns a string
// Here's how you could delete a serviceProduct object
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            var deleteProductOptions = new Dictionary<string, object>(){
                { "client", rnClient },
                { "url" , "serviceProducts/15" },
                { "annotation", "Deleting product with id of 15" }
            };

            WriteLine(OSvCCSharp.Connect.Delete(deleteProductOptions)); // returns empty body

        }
    }
}

```


## Uploading File Attachments
In order to upload a file attachment, add a "files" property to your options object with an list as it's value. In that list, input the file locations of the files that you wish to upload relative to where the script is ran.

```C#
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

        var rnClient = new OSvCCSharp.Client(
            interface_: AppSettings["OSC_SITE"],
            username: AppSettings["OSC_ADMIN"],
            password: AppSettings["OSC_PASSWORD"],
            version: "latest"
        );

        var newProd = new Dictionary<string, object>()
            {
                {"primaryContact",new List<Dictionary<string, object>>()
                {
                    new Dictionary<string,int>(){
                            { "id", 1}
                        } }
                }
            };

            Dictionary<string, object> options = new Dictionary<string, object>()
            {
                { "client", rnClient},
                { "url", "serviceProducts"},
                { "json", newProd},
                { "annotation", "Creating a product" },
                { "files", new List<string>{
                   "./haQE7EIDQVUyzoLDha2SRVsP415IYK8_ocmxgMfyZaw.png"
                 } }
            };

            var createdProduct = OSvCCSharp.Connect.Post(options); // returns JSON body
    }
}

```

## Downloading File Attachments
In order to download a file attachment, add a "?download" query parameter to the file attachment URL and send a get request using the OSvCCSharp.Connect.get method. The file will be downloaded to the same location that the script is ran.

```C#
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;
namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            var getProductOptions = new Dictionary<string, object>(){
                { "client", rnClient },
                { "url" , "incidents/24898/fileAttachments/245?download" }
                { "annotation", "Downloading a file attachment" }
            };

            WriteLine(OSvCCSharp.Connect.Get(getProductOptions)); // returns JSON body

        }
    }
}

```

In order to download multiple attachments for a given object, add a "?download" query parameter to the file attachments URL and send a get request using the OSvCCSharp.Connect.get method. 

All of the files for the specified object will be downloaded and archived in a .tgz file.

```C#
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            var getProductOptions = new Dictionary<string, object>(){
                { "client", rnClient },
                { "url" , "incidents/24898/fileAttachments?download" }
                { "annotation", "Downloading all attachments" }
            };

            WriteLine(OSvCCSharp.Connect.Get(getProductOptions)); // returns JSON body

        }
    }
}

```

You can extract the file using [tar](https://askubuntu.com/questions/499807/how-to-unzip-tgz-file-using-the-terminal/499809#499809)
    
    $ tar -xvzf ./downloadedAttachment.tgz

## OSvCCSharp.QueryResults example

This is for running one ROQL query. Whatever is allowed by the REST API (limits and sorting) is allowed with this library.

OSvCCSharp.QueryResults only has one function: 'Query', which takes an OSvCCSharp.Client object and string query (example below).

```C#
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            // QueryResults
            var queryOptions = new Dictionary<string, object>
            {
                { "client", rnClient },
                { "query", "SELECT count(ids) as id_count FROM CONTACTS " },
                { "annotation", "Running a single query" }
            };

            WriteLine(OSvCCSharp.QueryResults.Query(queryOptions)); // returns JSON body

        }
    }
}


```
## OSvCCSharp.QueryResultsSet example

This is for running multiple queries and assigning the results of each query to a key for further manipulation.

OSvCCSharp.QueryResultsSet only has one function: 'QuerySet', which takes an OSvCCSharp.Client object and multiple query dictionaries (example below).

```C#
// Pass in each query into a dictionary
// set query: to the query you want to execute
// set key: to the value you want the results to of the query to be referenced to
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            var querySetOptions = new Dictionary<string, object>
            {
                {"client", rnClient },
                {"queries",  new List<Dictionary<string, string>>()
                    {
                        new Dictionary<string,string>()
                        {
                            {"key","answersSchema"},
                            {"query","DESCRIBE answers"}
                        },
                        new Dictionary<string,string>()
                        {
                            {"key","answers"},
                            {"query","SELECT * FROM ANSWERS LIMIT 1"}
                        },
                        new Dictionary<string,string>()
                        {
                            {"key","serviceCategoriesSchema"},
                            {"query","DESCRIBE serviceCategories"}
                        },
                        new Dictionary<string,string>()
                        {
                            {"key","contacts"},
                            SELECT * FROM serviceCategories
                            {"query","SELECT * FROM serviceCategories"}
                        }
                    }},
                { "annotation", "Running Multiple queries" }
            };

            var mqResults = OSvCCSharp.QueryResultsSet.QuerySet(querySetOptions);
            
            WriteLine(mqResults["answersSchema"]);
            
            //  Results for "DESCRIBE ANSWERS"
            // 
            //  [
            //   {
            //     "Name": "id",
            //     "Type": "Integer",
            //     "Path": ""
            //   },
            //   {
            //     "Name": "lookupName",
            //     "Type": "String",
            //     "Path": ""
            //   },
            //   {
            //     "Name": "createdTime",
            //     "Type": "String",
            //     "Path": ""
            //   }
            //   ... everything else including customfields and objects...
            // ]
            WriteLine(mqResults["answers"]);

            //  Results for "SELECT * FROM ANSWERS LIMIT 1"
            // 
            //  [
            //   {
            //     "id": 1,
            //     "lookupName": 1,
            //     "createdTime": "2016-03-04T18:25:50Z",
            //     "updatedTime": "2016-09-12T17:12:14Z",
            //     "accessLevels": 1,
            //     "adminLastAccessTime": "2016-03-04T18:25:50Z",
            //     "answerType": 1,
            //     "expiresDate": null,
            //     "guidedAssistance": null,
            //     "keywords": null,
            //     "language": 1,
            //     "lastAccessTime": "2016-03-04T18:25:50Z",
            //     "lastNotificationTime": null,
            //     "name": 1,
            //     "nextNotificationTime": null,
            //     "originalReferenceNumber": null,
            //     "positionInList": 1,
            //     "publishOnDate": null,
            //     "question": null,
            //     "solution": "<HTML SOLUTION WITH INLINE CSS>",
            //     "summary": "SPRING IS ALMOST HERE!",
            //     "updatedByAccount": 16,
            //     "uRL": null
            //   }
            // ]

            WriteLine(mqResults["serviceCategoriesSchema"]);

            //  Results for "DESCRIBE SERVICECATEGORIES"
            //  
            // [
            // ... skipping the first few ... 
            //  {
            //     "Name": "adminVisibleInterfaces",
            //     "Type": "SubTable",
            //     "Path": "serviceCategories.adminVisibleInterfaces"
            //   },
            //   {
            //     "Name": "descriptions",
            //     "Type": "SubTable",
            //     "Path": "serviceCategories.descriptions"
            //   },
            //   {
            //     "Name": "displayOrder",
            //     "Type": "Integer",
            //     "Path": ""
            //   },
            //   {
            //     "Name": "endUserVisibleInterfaces",
            //     "Type": "SubTable",
            //     "Path": "serviceCategories.endUserVisibleInterfaces"
            //   },
            //   ... everything else include parents and children ...
            // ]
            
            WriteLine(mqResults["serviceCategories"]);

            //  Results for "SELECT * FROM serviceCategories"
            // 
            //  [
            //   {
            //     "id": 3,
            //     "lookupName": "Manuals",
            //     "createdTime": null,
            //     "updatedTime": null,
            //     "displayOrder": 3,
            //     "name": "Manuals",
            //     "parent": 60
            //   },
            //   {
            //     "id": 4,
            //     "lookupName": "Installations",
            //     "createdTime": null,
            //     "updatedTime": null,
            //     "displayOrder": 4,
            //     "name": "Installations",
            //     "parent": 60
            //   },
            //   {
            //     "id": 5,
            //     "lookupName": "Downloads",
            //     "createdTime": null,
            //     "updatedTime": null,
            //     "displayOrder": 2,
            //     "name": "Downloads",
            //     "parent": 60
            //   },
            //   ... you should get the idea by now ...
            // ]
        }
    }
}
                    


```
## OSvCCSharp.AnalyticsReportsResults

You can create a new instance either by the report 'id' or 'lookupName'.

OSvCCSharp.AnalyticsReportsResults only has one function: 'run', which takes an OSvCCSharp.Client object.

Pass in the 'id', 'lookupName', and 'filters' in the options data object to set the report and any filters. 
```C#
using OSvCCSharp;
using static System.Configuration.ConfigurationManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Console;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            //// AnalyticsReportResults
            var jsonData = new Dictionary<string, object>{
                { "id", 176 },
                { "limit", 2 },
                { "filters", new Dictionary<string, string>{
                    { "name", "search_ex" },
                    { "values", "Maestro" }
                }}
            };

            var arrOptions = new Dictionary<string, object>
            {
                { "client", rnClient},
                { "json", jsonData },
                { "annotation", "Running Reports" }
            };

            var arrResults = OSvCCSharp.AnalyticsReportResults.Run(arrOptions);
            var arrObjects = JsonConvert.DeserializeObject<List<object>>(arrResults);
            foreach (object arrResult in arrObjects)
            {
                WriteLine(JsonConvert.SerializeObject(arrResult));
            }

        }
    }
}

```

## Bulk Delete
This library makes it easy to use the Bulk Delete feature within the latest versions of the REST API. 

You can either use a QueryResults or QueryResultsSet object in order to run bulk delete queries.

Before you can use this feature, make sure that you have the [correct permissions set up for your profile](https://docs.oracle.com/en/cloud/saas/service/18b/cxsvc/c_osvc_bulk_delete.html#BulkDelete-10689704__concept-212-37785F91).

Here is an example of the how to use the Bulk Delete feature: 
```C#
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            // QueryResults
            var queryOptions = new Dictionary<string, object>
            {
                { "client", rnClient },
                { "query", "DELETE from incidents LIMIT 10" },
                { "annotation", "Running a single query" }
            };

            WriteLine(OSvCCSharp.QueryResults.Query(queryOptions)); // returns JSON body

        }
    }
}
```
## Performing Session Authentication

1. Create a custom script with the following code and place in the "Custom Scripts" folder in the File Manager:

```php
<?php

// Find our position in the file tree
if (!defined('DOCROOT')) {
$docroot = get_cfg_var('doc_root');
define('DOCROOT', $docroot);
}
 
/************* Agent Authentication ***************/
 
// Set up and call the AgentAuthenticator
require_once (DOCROOT . '/include/services/AgentAuthenticator.phph');

// get username and password
$username = $_GET['username'];
$password = $_GET['password'];
 
// On failure, this includes the Access Denied page and then exits,
// preventing the rest of the page from running.
echo json_encode(AgentAuthenticator::authenticateCredentials($username,$password));

```
2. Create a node script similar to the following and it should connect:

```C#
// Require necessary libraries
using static System.Console;
using static System.Configuration.ConfigurationManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            Task<string> sessionJson = GetSessionId();
            JToken token = JObject.Parse(sessionJson.Result);
            var sessionId = (string)token.SelectToken("session_id");

            var rnSessionClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                session: sessionId
            );

            Dictionary<string, object> sessionOptions = new Dictionary<string, object>(){
                { "client", rnSessionClient },
                { "url" , "incidents/24898/fileAttachments/245?download" }
            };

            ////// GET Request
            WriteLine(OSvCCSharp.Connect.Get(sessionOptions)); // returns JSON body

        }

        static async Task<string> GetSessionId()
        {
            var url = $"https://{AppSettings["OSC_SITE"]}.custhelp.com/cgi-bin/";
            // add the location of the above file
            url += $"{AppSettings["OSC_CONFIG"]}.cfg/php/custom/login_test.php";
            // add the credentials for getting a session ID
            url += $"?username={AppSettings["OSC_ADMIN"]}&password={AppSettings["OSC_PASSWORD"]}";

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            var contents = await response.Content.ReadAsStringAsync();

            return contents;
        }
    }
}
```

## Running multiple ROQL Queries in parallel
Instead of running multiple queries in with 1 GET request, you can run multiple GET requests and combine the results by adding a "parallel" property to the options object.

```C#
using static System.Console;
using static System.Configuration.ConfigurationManager;
using OSvCCSharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                interface_: AppSettings["OSC_SITE"],
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                version: "latest"
            );

            var querySetOptions = new Dictionary<string, object>
            {
                {"client", rnClient },
                {"parallel", true },
                {"queries",  new List<Dictionary<string, string>>()
                    {
                        new Dictionary<string,string>()
                        {
                            {"key","incidents"},
                            {"query","select id from incidents LIMIT 20000"}
                        },
                        new Dictionary<string,string>()
                        {
                            {"key","serviceProducts"},
                            {"query","select id, name from serviceProducts"}
                        },
                        new Dictionary<string,string>()
                        {
                            {"key","serviceCategories"},
                            {"query","select id, name from serviceCategories"}
                        },
                        new Dictionary<string,string>()
                        {
                            {"key","contacts"},
                            {"query","select id from contacts"}
                        }
                    }},
                { "annotation", "Running Multiple queries" }
            };

            var mqResults = OSvCCSharp.QueryResultsSet.QuerySet(querySetOptions);
            WriteLine(mqResults["incidents"]);
            WriteLine(mqResults["serviceProducts"]);
            WriteLine(mqResults["serviceCategories"]);
            WriteLine(mqResults["contacts"]);
```