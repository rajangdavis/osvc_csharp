using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OSvCCSharpTest
{
    [TestClass]
    public class OSvCCSharpClientTest
    {
        public string env(string envVariable)
        {
            return System.Environment.GetEnvironmentVariable(envVariable);
        }

        [TestMethod]
        public void ClientTest()
        {
            var rnClient = new OSvCCSharp.Client(
                username: "username",
                password: "password",
                interface_: "interface",

                // Optional Configuration Settings
                demo_site: true,                // Changes domain from 'custhelp' to 'rightnowdemo'
                version: "v1.4"                // Changes REST API version, default is 'v1.3'
            );

            Assert.AreEqual(rnClient.config.access_token,"");
            Assert.AreEqual(rnClient.config.version,"v1.4");
            Assert.AreEqual(rnClient.config.username,"username");
            Assert.AreEqual(rnClient.config.password,"password");
            Assert.AreEqual(rnClient.config.interface_, "interface");

        }

    }
}