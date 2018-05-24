# OSvCCSharp

An (under development) C# library for using the [Oracle Service Cloud REST API](https://docs.oracle.com/cloud/latest/servicecs_gs/CXSVC/) influenced by the [ConnectPHP API](http://documentation.custhelp.com/euf/assets/devdocs/november2016/Connect_PHP/Default.htm)

## Todo
I am looking to implement the following items soon:
1. Test suite
2. Documentation
3. Nuget Package

## Compatibility

The library is being tested against Oracle Service Cloud May 2017 using C# v4.0.30319.

All of the HTTP methods should work on any version of Oracle Service Cloud since version May 2015; however, there maybe some issues with querying items on any version before May 2016. This is because ROQL queries were not exposed via the REST API until May 2016.


## Use Cases
You can use this C# Library for basic scripting and microservices.

The main features that work to date are as follows:

1. [Simple configuration](#client-configuration)
2. Running ROQL queries [either 1 at a time](#osvccsharpqueryresults-example) or [multiple queries in a set](#osvccsharpqueryresultsset-example)
3. [Running Reports with filters](#osvccsharpanalyticsreportsresults)
4. Basic CRUD Operations via HTTP Methods
	1. [Create => Post](#create)
	2. [Read => Get](#read)
	3. [Update => Patch](#update)
	4. [Destroy => Delete](#delete)

<!-- ## Installing C# and the .NET runtime (for Windows)


## Installation -->

## Client Configuration

An OSvCCSharp.Client class lets the library know which credentials and interface to use for interacting with the Oracle Service Cloud REST API.
This is helpful if you need to interact with multiple interfaces or set different headers for different objects.

```C#

// Configuration is as simple as requiring the package
// and passing in an object

// Client Configuration
var rnClient = new OSvCCSharp.Client(
    username: AppSettings["OSC_ADMIN"],
    password: AppSettings["OSC_PASSWORD"],
    interfaceName: AppSettings["OSC_SITE"],

    // Optional Configuration Settings
    demo_site: false, 				// Changes domain from 'custhelp' to 'rightnowdemo'
    version: "v1.4", 				// Changes REST API version, default is 'v1.3'
    ssl_verify: false, 				// Turns off SSL verification
    rule_suppression: false         // Supresses Business Rules
);


```

## OSvCCSharp.QueryResults example

This is for running one ROQL query. Whatever is allowed by the REST API (limits and sorting) is allowed with this library.

OSvCCSharp.QueryResults only has one function: 'query', which takes an OSvCCSharp.Client object and string query (example below).

```C#

using static System.Console;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static OSvCCSharp.Utils;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                username: AppSettings["OSC_ADMIN"],    
                password: AppSettings["OSC_PASSWORD"], 
                interfaceName: AppSettings["OSC_SITE"]
            );

            var q = new OSvCCSharp.QueryResults(rnClient);
            var query = "SELECT * FROM Contacts C WHERE CreatedTime > '2005-01-10T04:00:00Z'";
            var queryResults = q.Query(query); // Run the query
            
            var queryObjects = JsonConvert.DeserializeObject<List<object>>(queryResults);
            foreach (object queryResult in queryObjects)
            {
                var queryResultString = JsonConvert.SerializeObject(queryResult);
                JToken token = JObject.Parse(queryResultString);
                var contactId = (int)token.SelectToken("id");
                WriteLine($"Here is the contact ID: { contactId }");
            }
        }
    }
}


```

## OSvCCSharp.QueryResultsSet example

This is for running multiple ROQL queries. Whatever is allowed by the REST API (limits and sorting) is allowed with this library.

OSvCCSharp.QueryResultsSet only has one function: 'query', which takes an OSvCCSharp.Client object and string query (example below).

```C#


using static System.Console;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static OSvCCSharp.Utils;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                username: AppSettings["OSC_ADMIN"],    
                password: AppSettings["OSC_PASSWORD"], 
                interfaceName: AppSettings["OSC_SITE"]
            );

            var mq = new OSvCCSharp.QueryResultsSet(rnClient);
            var queries = new List<Dictionary<string, string>>()
            {
                new Dictionary<string,string>()
                {
                    {"query","DESCRIBE answers"}
                    {"key","answersSchema"},
                },
                new Dictionary<string,string>()
                {
                    {"query","SELECT * FROM answers"}
                    {"key","answers"},
                },
                new Dictionary<string,string>()
                {
                    {"query","DESCRIBE serviceProducts"}
                    {"key","productsSchema"},
                },
                new Dictionary<string,string>()
                {
                    {"query","SELECT * FROM serviceProducts"}
                    {"key","products"},
                },
                new Dictionary<string,string>()
                {
                    {"query","DESCRIBE serviceCategories"}
                    {"key","categoriesSchema"},
                },
                new Dictionary<string,string>()
                {
                    {"query","SELECT * FROM serviceCategories"}
                    {"key","categories"},
                },
            };

            var mqResults = mq.QuerySet(queries);
            WriteLine(mqResults["products"]);
            WriteLine(mqResults["productsSchema"]);
            WriteLine(mqResults["categories"]);
            WriteLine(mqResults["categoriesSchema"]);
            WriteLine(mqResults["answers"]);
            WriteLine(mqResults["answersSchema"]);
        }
    }
}

```

## OSvCCSharp.AnalyticsReportResults

You can create a new instance either by the report 'id' or 'lookupName'.

OSvCCSharp.AnalyticsReportResults only has one function: 'run', which takes an OSvCRuby::Client object.

OSvCCSharp.AnalyticsReportResults have the following properties: 'id', 'lookupName', and 'filters'. More on filters and supported datetime methods are below this OSvCCSharp.AnalyticsReportResults example script.

```C#

using static System.Console;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static OSvCCSharp.Utils;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var rnClient = new OSvCCSharp.Client(
                username: AppSettings["OSC_ADMIN"],    
                password: AppSettings["OSC_PASSWORD"], 
                interfaceName: AppSettings["OSC_SITE"]
            );

            var arr = new OSvCCSharp.AnalyticsReportResults(rnClient);
            arr.filters = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>()
                {
                    { "name", "search_ex" },
                    { "values", "Maestro" }
                }
            };
            var arrResults = arr.Run(id: 176);
            var arrObjects = JsonConvert.DeserializeObject<List<object>>(arrResults);
            foreach (object arrResult in arrObjects)
            {
                WriteLine(JsonConvert.SerializeObject(arrResult));
            }
        }
    }
}

```

## Basic CRUD operations

### CREATE
```C#

using System;
using System.Collections.Generic;
using static System.Console;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            // Client Configuration
            var rnClient = new OSvCCSharp.Client(
                username: AppSettings["OSC_ADMIN"],         
                password: AppSettings["OSC_PASSWORD"],      
                interfaceName: AppSettings["OSC_SITE"],
            );

            //// Connect Object for HTTP Requests
            var connect = new OSvCCSharp.Connect(rnClient);

            //// POST Request
            var newProd = new Dictionary<string, object>()
            {
                {"adminVisibleInterfaces",new List<Dictionary<string, int>>()
                {
                    new Dictionary<string, int>()
                    {
                        {"id", 1 }
                    }
                }},
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

            var createdProduct = connect.Post("serviceProducts", newProd); // returns JSON body
        }
    }
}

```






### READ
```C#
//// OSvCCSharp.get(options, callback)
//// returns callback function
// Here's how you could get an instance of ServiceProducts

//// OSvCCSharp.Delete(url)
//// returns string
// Here's how you could delete a serviceProduct object
	
using System;
using System.Collections.Generic;
using static System.Console;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            // Client Configuration
            var rnClient = new OSvCCSharp.Client(
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                interfaceName: AppSettings["OSC_SITE"],
                demoSite: true,
                suppressRules: true
            );

            //// Connect Object for HTTP Requests
            var connect = new OSvCCSharp.Connect(rnClient);
            WriteLine(connect.Get("serviceProducts/174"));   // returns JSON object
        }
    }
}
```






### UPDATE
```C#
//// OSvCCSharp.Patch(url, json)
//// returns string
// Here's how you could update an Answer object
// using JSON objects
// to set field information


using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            // Client Configuration
            var rnClient = new OSvCCSharp.Client(
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                interfaceName: AppSettings["OSC_SITE"],
                suppressRules: true
            );

            //// Connect Object for HTTP Requests
            var connect = new OSvCCSharp.Connect(rnClient);

            // Run multiple queries
            // assign to keys

            var mq = new OSvCCSharp.QueryResultsSet(rnClient);
            var queries = new List<Dictionary<string, string>>()
            {
                new Dictionary<string,string>()
                {
                    {"query","SELECT COUNT() AS count FROM siteInterfaces"}
                    {"key","interfaceCount"},
                },
                new Dictionary<string,string>()
                {
                    {"query","SELECT serviceProducts.categoryLinks.serviceCategoryList.serviceCategory.id FROM serviceProducts WHERE id = 174"}
                    {"key","catLinks"},
                },
                new Dictionary<string,string>()
                {
                    {"query","SELECT serviceProducts.dispositionLinks.serviceDispositionList.serviceDisposition.id FROM serviceProducts WHERE id = 174"}
                    {"key","dispLinks"},
                },
            };

            var mqResults = mq.QuerySet(queries);

            var interFaceCount = mqResults["interFaceCount"][0]["count"];
            var catLinks = mqResults["catLinks"];
            var dispLinks = mqResults["dispLinks"];

            // Build the JSON object
            var updateJson = new Dictionary<string, object>()
            {
                {"descriptions",new List<Dictionary<string, object>>
                    {
                    new Dictionary<string, object>()
                        {
                            { "labelText","updating a ServiceProduct " },
                            { "language", new Dictionary<string, int>()
                                {
                                    {"id", 1}
                                }
                            }
                        }
                    }
                },
                {"displayOrder",2},
                {"names",new List<Dictionary<string, object>>
                    {
                    new Dictionary<string, object>()
                        {
                            { "labelText","updating the privious " },
                            { "language", new Dictionary<string, int>()
                                {
                                    {"id", 1}
                                }
                            }
                        }
                    }
                }
            };

            
            // if the adminvisibleinterfaces count = 0
            //  then set the first admin interface to ID = 1

            if(interFaceCount == 0)
            {
                updateJson["adminVisibleInterfaces"] = new List<Dictionary<string,object>>{
                    new Dictionary<string,object>(){
                           {"id", 1}
                    }
                }
            }


            foreach (Dictionary<string,object> cl in catLinks)
            {
                if ((int)cl["id"] == 1)
                {
                    cl["id"] = 2;

                    // Makes a copy of catLinks and returns an arry of these:
                    // { 'serviceCategory': { "id": catLink["id"]} }
                    updateJson["categoryLinks"] = catLinks.Select(catLink => new Dictionary<string, Dictionary<string, object>>() {
                        { "serviceCategory",new Dictionary<string, object>(){
                            {"id",catLink["id"] }
                        } }
                    }).ToArray();

                }
            }

            
            // Loop through dispLinks
            foreach (Dictionary<string,object> dl in dispLinks)
            {
                if ((int)dl["id"] == 1)
                {
                    dl["id"] = 2;

                    // Makes a copy of dispLinks and returns an arry of these:
                    // { 'serviceDisposition': { "id": dispLink["id"]} }
                    updateJson["dispositionLinks"] = dispLinks.Select(dispLink => new Dictionary<string, Dictionary<string, object>>() {
                        { "serviceDisposition",new Dictionary<string, object>(){
                            {"id",dispLink["id"] }
                        } }
                    }).ToArray();

                }
            }

            if(interFaceCount == 0)
            {
                updateJson["endUserVisibleInterfaces"] = new List<Dictionary<string,object>>{
                    new Dictionary<string,object>(){
                           {"id", 1}
                    }
                }
            }

            WriteLine(updateJson);

            var updatedProduct = connect.Post("serviceProducts/174", updateJson); // returns empty string
        }
    }
}


```


 
### DELETE
```C#
//// OSvCCSharp.Delete(url)
//// returns string
// Here's how you could delete a serviceProduct object
	
using System;
using System.Collections.Generic;
using static System.Console;
using static System.Configuration.ConfigurationManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            // Client Configuration
            var rnClient = new OSvCCSharp.Client(
                username: AppSettings["OSC_ADMIN"],
                password: AppSettings["OSC_PASSWORD"],
                interfaceName: AppSettings["OSC_SITE"],
                demoSite: true,
                suppressRules: true
            );

            //// Connect Object for HTTP Requests
            var connect = new OSvCCSharp.Connect(rnClient);
            WriteLine(connect.Delete("serviceProducts/174"));   // returns empty string
        }
    }
}

```