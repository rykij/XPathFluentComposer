using Microsoft.VisualStudio.TestTools.UnitTesting;
using XPathFluentComposer;

namespace XPathFluentComposerTests
{
    [TestClass]
    public class XPathQueryBuilderTest
    {
        public string query;
        XPathQueryBuilder xPathQueryBuilder;

        [TestInitialize]
        public void init()
        {
            query = string.Empty;
            xPathQueryBuilder = new XPathQueryBuilder(); 
        }

        [TestCategory("unit"), TestMethod]
        public void SelectElement()
        {
            xPathQueryBuilder.SelectElement("element");
            Assert.AreEqual("/element", xPathQueryBuilder.Query);
        }

        [TestCategory("unit"), TestMethod]
        public void AddFilterNameAndValue()
        {
            xPathQueryBuilder.AddFilterName("element");
            xPathQueryBuilder.AddFilterValue("value");

            Assert.AreEqual("[@element='value']", xPathQueryBuilder.Query);
        }

        [TestCategory("unit"), TestMethod]
        public void SelectFirst()
        {
            xPathQueryBuilder.SelectElement("element");
            xPathQueryBuilder.SelectFirst();

            Assert.AreEqual("/element[1]", xPathQueryBuilder.Query);
        }

        [TestCategory("unit"), TestMethod]
        public void SelectLast()
        {
            xPathQueryBuilder.SelectElement("element");
            xPathQueryBuilder.SelectLast();

            Assert.AreEqual("/element[last()]", xPathQueryBuilder.Query);
        }

        [TestCategory("unit"), TestMethod]
        public void CreateEntireXPath()
        {
            xPathQueryBuilder.SelectElement("root");
            xPathQueryBuilder.SelectElement("element");
            xPathQueryBuilder.AddFilterName("filter");
            xPathQueryBuilder.AddFilterValue("value");
            xPathQueryBuilder.SelectElement("*");

            Assert.AreEqual("/root/element[@filter='value']/*", xPathQueryBuilder.Query);
        }

        [TestCategory("unit"), TestMethod]
        public void RemoveLastEntry()
        {
            xPathQueryBuilder.SelectElement("root");
            xPathQueryBuilder.SelectElement("*");

            Assert.AreEqual("/root/*", xPathQueryBuilder.Query);

            xPathQueryBuilder.RemoveLastEntry();

            Assert.AreEqual("/root", xPathQueryBuilder.Query);
        }

        [TestCategory("unit"), TestMethod]
        public void GetLastEntryNode()
        {
            xPathQueryBuilder.SelectElement("root");
            xPathQueryBuilder.SelectElement("element");
            xPathQueryBuilder.AddFilterName("filter");
            xPathQueryBuilder.AddFilterValue("value");

            string lastEntry = xPathQueryBuilder.GetLastEntryNode();

            Assert.AreEqual("element", lastEntry);
        }
    }
}
