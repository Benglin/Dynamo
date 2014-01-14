using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class MigrationManagerTests
    {
        private XmlDocument xmlDocument = null;

        [SetUp]
        public void SetupTests()
        {
            xmlDocument = new XmlDocument();
        }

        [TearDown]
        public void TearDownTests()
        {
            xmlDocument = null;
        }

        [Test]
        public void DuplicateWithAttributes00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // First argument being null should throw an exception.
                MigrationManager.DuplicateWithAttributes(null, null);
            });
        }

        [Test]
        public void DuplicateWithAttributes01()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");

            Assert.Throws<ArgumentException>(() =>
            {
                // Second argument being null should throw an exception.
                MigrationManager.DuplicateWithAttributes(srcElement, null);
            });
        }

        [Test]
        public void DuplicateWithAttributes02()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");

            Assert.Throws<ArgumentException>(() =>
            {
                // Second argument being empty should throw an exception.
                MigrationManager.DuplicateWithAttributes(srcElement, new string[] { });
            });
        }

        [Test]
        public void DuplicateWithAttributes03()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");

            // Non-existence attribute will result in a same-name attribute 
            // in the resulting XmlElement with an empty value.
            XmlElement dstElement = MigrationManager.DuplicateWithAttributes(
                srcElement, new string[] { "dummy" });

            Assert.IsNotNull(dstElement);
            Assert.AreEqual(1, dstElement.Attributes.Count);
            Assert.AreEqual("", dstElement.Attributes["dummy"].Value);
        }

        [Test]
        public void DuplicateWithAttributes04()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            srcElement.SetAttribute("guid", "D514AA10-63F0-4479-BB9F-0FEBEB2274B0");
            srcElement.SetAttribute("isUpstreamVisible", "yeah");

            // Non-existence attribute will result in a same-name attribute 
            // in the resulting XmlElement with an empty value.
            XmlElement dstElement = MigrationManager.DuplicateWithAttributes(
                srcElement, new string[] { "guid", "dummy", "isUpstreamVisible" });

            Assert.IsNotNull(dstElement);
            Assert.AreEqual(3, dstElement.Attributes.Count);
            Assert.AreEqual("D514AA10-63F0-4479-BB9F-0FEBEB2274B0",
                dstElement.Attributes["guid"].Value);

            Assert.AreEqual("", dstElement.Attributes["dummy"].Value);
            Assert.AreEqual("yeah", dstElement.Attributes["isUpstreamVisible"].Value);
        }

        [Test]
        public void DuplicateWithAllAttributes00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.DuplicateWithAllAttributes(null);
            });
        }

        [Test]
        public void DuplicateWithAllAttributes01()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            XmlElement dstElement = MigrationManager.DuplicateWithAllAttributes(srcElement);

            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);
            Assert.AreEqual(0, dstElement.Attributes.Count);
        }

        [Test]
        public void DuplicateWithAllAttributes02()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            srcElement.SetAttribute("one", "1");
            srcElement.SetAttribute("two", "2");
            srcElement.SetAttribute("three", "3");

            XmlElement dstElement = MigrationManager.DuplicateWithAllAttributes(srcElement);

            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);
            Assert.AreEqual(3, dstElement.Attributes.Count);
            Assert.AreEqual("1", dstElement.Attributes["one"].Value);
            Assert.AreEqual("2", dstElement.Attributes["two"].Value);
            Assert.AreEqual("3", dstElement.Attributes["three"].Value);
        }
    }
}
