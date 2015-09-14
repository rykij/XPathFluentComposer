using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XPathFluentComposer;

namespace XPathFluentComposerTests
{
    [TestClass]
    [DeploymentItem(@"UnitTests\XPathFluentComposerTests\TestData\", "TestData")]
    public class XPathFluentComposerTests
    {
        string testFile1 = string.Empty;
        string testfile2 = string.Empty;
        string newTestFile = string.Empty;

        private TestContext testContextInstance;
        public TestContext TestContext { get { return testContextInstance; } set { testContextInstance = value; } }

        [TestInitialize]
        public void Init()
        {
            Directory.CreateDirectory(TestContext.DeploymentDirectory + "\\temp");

            File.Copy(TestContext.DeploymentDirectory + "\\TestData\\testfile1.xml", TestContext.DeploymentDirectory + "\\temp\\testfile1.xml",true);
            File.Copy(TestContext.DeploymentDirectory + "\\TestData\\testfile2.xml", TestContext.DeploymentDirectory + "\\temp\\testfile2.xml", true);
           
            testFile1 = TestContext.DeploymentDirectory + "\\temp\\testfile1.xml";
            testfile2 = TestContext.DeploymentDirectory + "\\temp\\testfile2.xml";
        }

        [TestCleanup]
        public void CleanUp()
        {
            Directory.Delete(TestContext.DeploymentDirectory + "\\temp", true);
        }

        [TestCategory("integration"), TestMethod]
        public void HowToUseLibraryExample_CreateCompleteXmlFile_FileAreEqualToStub()
        {
            XPathComposer composer = new XPathComposer();
            string xmlPath = TestContext.DeploymentDirectory + "\\temp\\" + "test.xml";

            IList<KeyValuePair<string, string>> nodeList = new List<KeyValuePair<string, string>>();

            //create parent nodes
            CD cd1 = new CD("LifeForms", "Future Sound Of London", "1995");
            CD cd2 = new CD("Insides - LP", "Orbital", "1996");
            
            //create xml file, adding a parent node,an attribute and a list of child nodes
            composer.CreateXmlFile(xmlPath, "root").AddParentNode("cd").AddAttribute("id", "bestprice").AddNodeList<string, string>(cd1.GetList());

            //add attribute to a child node
            composer.RootNode("root").Select("cd").Where("id").Equals("bestprice").Select("artist").AddAttribute("country", "uk");

            //creatre other parent node adding an attribute and a list of child nodes
            composer.RootNode("root").AddParentNode("cd").AddAttribute("id", "limited ed").AddNodeList<string, string>(cd2.GetList());

            //add attribute to a child node
            composer.RootNode("root").Select("cd").Where("id").Equals("limited ed").Select("artist").AddAttribute("country", "uk");

            string fileContent = Regex.Replace(string.Join("", File.ReadAllText(xmlPath)), @"[\n\t] + ", "").Trim();

            Assert.AreEqual(stubXmlFile, fileContent);
        }

        [TestCategory("unit"), TestMethod]
        //XPath ->  "/catalog/cd[@country = 'UK']/title"
        public void SelectValue_TestFile1_ValueIsTheSame()
        {
            XPathComposer composer = new XPathComposer();
            string value = composer.FileName(testFile1).
                                    RootNode("catalog").
                                    Select("cd").
                                    Where("country").
                                    Equals("UK").
                                    GetValueOf<string>("title");

            Assert.AreEqual("Hide your heart", value);
        }

        [TestCategory("unit"), TestMethod]
        //XPath ->  "/catalog/cd[@country = 'UK']/*"
        public void SelectValuesList_TestFile1__ValuesAreTheSame()
        {
            XPathComposer composer = new XPathComposer();
            IList<KeyValuePair<string,string>> valueList = composer.FileName(testFile1).
                                                                 RootNode("catalog").
                                                                 Select("cd").
                                                                 Where("country").
                                                                 Equals("UK").
                                                                 SelectAllChildNodes().
                                                                 GetList<string,string>();

            Assert.IsTrue(valueList.Count != 0);

            Assert.AreEqual("Hide your heart", valueList[0].Value);
            Assert.AreEqual("Bonnie Tyler", valueList[1].Value);
            Assert.AreEqual("10.0", valueList[2].Value);
        }

        [TestCategory("unit"), TestMethod]
        //XPath ->  "/catalog/cd[1]"
        public void SelectFirstNode_SelectAllChildNodes_TestFile1_ValuesAreTheSame()
        {
            XPathComposer composer = new XPathComposer();
            IList<KeyValuePair<string, string>> valueList = composer.FileName(testFile1).
                                                                     RootNode("catalog").
                                                                     Select("cd").
                                                                     First<string,string>();
 
            Assert.IsTrue(valueList.Count != 0);

            Assert.AreEqual("Empire Burlesque", valueList[0].Value);
            Assert.AreEqual("Bob Dylan", valueList[1].Value);
            Assert.AreEqual("10.90", valueList[2].Value);
        }

        [TestCategory("unit"), TestMethod]
        //XPath ->  "/catalog/cd[last()]"
        public void SelectLastNode_SelectAllChildNodes_TestFile1_ValuesAreTheSame()
        {
            XPathComposer composer = new XPathComposer();
            IList<KeyValuePair<string, string>> valueList = composer.FileName(testFile1).
                                                                     RootNode("catalog").
                                                                     Select("cd").
                                                                     Last<string,string>();


            Assert.IsTrue(valueList.Count != 0);

            Assert.AreEqual("Greatest Hits", valueList[0].Value);
            Assert.AreEqual("Dolly Parton", valueList[1].Value);
            Assert.AreEqual("9.90", valueList[2].Value);
        }

        [TestCategory("unit"), TestMethod]
        //XPath ->  "/catalog/cd[last()]"
        //XPath ->  "/catalog/cd[@country = 'UK']/*"
        public void SelectMoreElementSpecifingFileNotInLine_TestFile1_ValuesAreTheSame()
        {
            //use the same file for more xml queries
            XPathComposer composer = new XPathComposer(testFile1);

            IList<KeyValuePair<string, string>> valueList = composer.RootNode("catalog").Select("cd").Last<string,string>();

            Assert.IsTrue(valueList.Count != 0);
            Assert.AreEqual("Greatest Hits", valueList[0].Value);
            Assert.AreEqual("Dolly Parton", valueList[1].Value);
            Assert.AreEqual("9.90", valueList[2].Value);

            valueList = composer.RootNode("catalog").
                                 Select("cd").
                                 Where("country").
                                 Equals("UK").
                                 SelectAllChildNodes().
                                 GetList<string, string>();

            Assert.IsTrue(valueList.Count != 0);

            Assert.AreEqual("Hide your heart", valueList[0].Value);
            Assert.AreEqual("Bonnie Tyler", valueList[1].Value);
            Assert.AreEqual("10.0", valueList[2].Value);
        }

        [TestCategory("unit"), TestMethod]
        public void CreateFile_FileHasBeenCreated()
        {
            XPathComposer composer = new XPathComposer();
            string xmlPath = TestContext.DeploymentDirectory + "\\temp\\" + "test.xml";

            composer.CreateXmlFile(xmlPath, "root");

            File.Exists(xmlPath);
        }

        [TestCategory("unit"), TestMethod]
        public void CreateFile_AddElementList_ElementsHaveBeenAdded()
        {
            XPathComposer composer = new XPathComposer();
            string xmlPath = TestContext.DeploymentDirectory + "\\temp\\" + "test.xml";

            IList<KeyValuePair<string, string>> nodeList = new List<KeyValuePair<string, string>>();

            KeyValuePair<string, string> node1 = new KeyValuePair<string, string>("title", "LifeForms");
            KeyValuePair<string, string> node2 = new KeyValuePair<string, string>("artist", "Future Sound Of London");
            KeyValuePair<string, string> node3 = new KeyValuePair<string, string>("tear", "1995");
            nodeList.Add(node1);
            nodeList.Add(node2);
            nodeList.Add(node3);

            composer.CreateXmlFile(xmlPath, "root").AddNodeList<string, string>(nodeList);

            File.Exists(xmlPath);

            nodeList = composer.RootNode("root").SelectAllChildNodes().GetList<string, string>();

            Assert.IsTrue(nodeList.Count != 0);

            Assert.AreEqual("LifeForms", nodeList[0].Value);
            Assert.AreEqual("Future Sound Of London", nodeList[1].Value);
            Assert.AreEqual("1995", nodeList[2].Value);
        }

        [TestCategory("unit"), TestMethod]
        public void CreateFile_AddAttributeToNode_NodeHasBeenFoundByAttribute()
        {
            XPathComposer composer = new XPathComposer();
            string xmlPath = TestContext.DeploymentDirectory + "\\temp\\" + "test.xml";

            IList<KeyValuePair<string, string>> nodeList = new List<KeyValuePair<string, string>>();

            KeyValuePair<string, string> node1 = new KeyValuePair<string, string>("title", "LifeForms");
            KeyValuePair<string, string> node2 = new KeyValuePair<string, string>("artist", "Future Sound Of London");
            KeyValuePair<string, string> node3 = new KeyValuePair<string, string>("year", "1995");
            nodeList.Add(node1);
            nodeList.Add(node2);
            nodeList.Add(node3);

            composer.CreateXmlFile(xmlPath, "root").AddNodeList<string, string>(nodeList);

            composer.FileName(xmlPath).RootNode("root").AddAttribute("id", "bestprice");

            File.Exists(xmlPath);

            string title = composer.RootNode("root").Where("id").Equals("bestprice").GetValueOf<string>("title");

            Assert.AreEqual("LifeForms", title);
        }

        [TestCategory("unit"), TestMethod]
        public void WriteValue_TestFile1_ValueIsChanged()
        {
            XPathComposer composer = new XPathComposer(testFile1);

            composer.RootNode("catalog").
                     Select("cd").
                     Where("country").
                     Equals("UK").
                     WriteValue("title", "newvalue");

            string newValue = composer.RootNode("catalog").
                                       Select("cd").
                                       Where("country").
                                       Equals("UK").
                                       GetValueOf<string>("title");

            Assert.AreEqual("newvalue", newValue);
        }

        [TestCategory("unit"), TestMethod]
        public void WriteValues_TestFile_ValuesAreChanged()
        {
            XPathComposer composer = new XPathComposer(testFile1);

            IList<KeyValuePair<string, string>> valueList = composer.RootNode("catalog").
                                                                     Select("cd").
                                                                     Where("country").
                                                                     Equals("UK").
                                                                     SelectAllChildNodes().
                                                                     GetList<string,string>();

            Assert.IsTrue(valueList.Count != 0);

            for (int i=0; i < valueList.Count; i++)
            {
                valueList[i] = new KeyValuePair<string, string>(valueList[i].Key, "newvalue");
            }

            composer.RootNode("catalog").
                     Select("cd").
                     Where("country").
                     Equals("UK").
                     WriteValues<string,string>(valueList);

            valueList = composer.RootNode("catalog").
                        Select("cd").
                        Where("country").
                        Equals("UK").
                        SelectAllChildNodes().
                        GetList<string, string>();

            Assert.IsTrue(valueList.Count != 0);

            foreach (KeyValuePair<string, string> node in valueList)
            {
                Assert.AreEqual("newvalue", node.Value);
                Assert.AreEqual("newvalue", node.Value);
                Assert.AreEqual("newvalue", node.Value);
            }
        }

        [TestCategory("unit"), TestMethod]
        public void InsertAfter_TestFile1_ValueHasBeenAdded()
        {
            XPathComposer composer = new XPathComposer(testFile1);
            KeyValuePair<string, string> node = new KeyValuePair<string, string>("label", "finalmuzik");

            composer.RootNode("catalog").
                     Select("cd").
                     Where("country").
                     Equals("UK").
                     InsertAfter<string,string>("price", node);

            IList<KeyValuePair<string, string>> valueList = composer.RootNode("catalog").
                                                              Select("cd").
                                                              Where("country").
                                                              Equals("UK").
                                                              SelectAllChildNodes().
                                                              GetList<string, string>();

            Assert.IsTrue(valueList.Count != 0);

            Assert.AreEqual("Hide your heart", valueList[0].Value);
            Assert.AreEqual("Bonnie Tyler", valueList[1].Value);
            Assert.AreEqual("10.0", valueList[2].Value);
            Assert.AreEqual("finalmuzik", valueList[3].Value);
        }

        [TestCategory("unit"), TestMethod]
        public void InsertBefore_TestFile1_ValueHasBeenAdded()
        {
            XPathComposer composer = new XPathComposer(testFile1);
            KeyValuePair<string, string> node = new KeyValuePair<string, string>("label", "finalmuzik");

            composer.RootNode("catalog").
                     Select("cd").
                     Where("country").
                     Equals("UK").
                     InsertBefore<string,string>("price", node);

            IList<KeyValuePair<string, string>> valueList = composer.RootNode("catalog").
                                                                     Select("cd").
                                                                     Where("country").
                                                                     Equals("UK").
                                                                     SelectAllChildNodes().
                                                                     GetList<string, string>();

            Assert.IsTrue(valueList.Count != 0);

            Assert.AreEqual("Hide your heart", valueList[0].Value);
            Assert.AreEqual("Bonnie Tyler", valueList[1].Value);
            Assert.AreEqual("finalmuzik", valueList[2].Value);
            Assert.AreEqual("10.0", valueList[3].Value);
        }

        [TestCategory("unit"), TestMethod]
        public void RemoveChildNode_TestFile1_ValueHasBeenRemoved()
        {
            XPathComposer composer = new XPathComposer(testFile1);
            composer.RootNode("catalog").
                     Select("cd").
                     Where("country").
                     Equals("UK").
                     RemoveNode("price");

            IList<KeyValuePair<string, string>> valueList = composer.RootNode("catalog").
                                                                     Select("cd").
                                                                     Where("country").
                                                                     Equals("UK").
                                                                     SelectAllChildNodes().
                                                                     GetList<string, string>();

            Assert.IsTrue(valueList.Count == 2);
        }

        [TestCategory("unit"), TestMethod]
        public void RemoveAllNodes_RemoveAllChildNodes_TestFile1_ValuesNotFound()
        {
            XPathComposer composer = new XPathComposer(testFile1);
            composer.RootNode("catalog").
                     Select("cd").
                     Where("country").
                     Equals("UK").
                     RemoveAllNodes();

            IList<KeyValuePair<string, string>> valueList = composer.RootNode("catalog").
                                                                     Select("cd").
                                                                     Where("country").
                                                                     Equals("UK").
                                                                     SelectAllChildNodes().
                                                                     GetList<string, string>();

            Assert.IsTrue(valueList.Count == 0);
        }

        [TestCategory("unit"), TestMethod]
        public void Remove_RemoveParentNodeWithAllChilds_TestFile1_NodeNotFound()
        {
            XPathComposer composer = new XPathComposer(testFile1);
            composer.RootNode("catalog").
                     Select("cd").
                     Where("country").
                     Equals("UK").
                     Remove();

            IList<KeyValuePair<string, string>> valueList = composer.RootNode("catalog").
                                                                     Select("cd").
                                                                     Where("country").
                                                                     Equals("UK").GetList<string, string>();
                                                                    

            Assert.IsTrue(valueList.Count == 0);
        }

        [TestCategory("unit"), TestMethod]
        //XPath ->  "/simulation/model/model[@id = 'Economies']/parameter[@id = 'StochasticRecalibrationMaximumMaturity']/value"
        public void SelectValue_TestFile2_StochasticRecalibrationMaximumMaturity_ValueIsTheSame()
        {
            XPathComposer composer = new XPathComposer();
            int value = composer.FileName(testfile2).
                                    RootNode("simulation").
                                    Select("model").
                                    Select("model").
                                    Where("id").
                                    Equals("Economies").
                                    Select("parameter").
                                    Where("id").
                                    Equals("StochasticRecalibrationMaximumMaturity").
                                    GetValueOf<int>("value");

            Assert.AreEqual(30, value);
        }

        [TestCategory("unit"), TestMethod]
        //XPath -> "/simulation/model/model[@id = 'Economies']/parameter[@id = 'ZCBP']/input[@id = 'Maturity']/*/control" itemList to return  -> "item" 
        public void SelectValuesList_TestFile2_ZCBP_Maturity_ValuesAreTheSame()
        {
            XPathComposer composer = new XPathComposer();
            IList<KeyValuePair<string, string>> valueList = composer.FileName(testfile2).
                                                                     RootNode("simulation").
                                                                     Select("model").
                                                                     Select("model").
                                                                     Where("id").
                                                                     Equals("Economies").
                                                                     Select("parameter").
                                                                     Where("id").
                                                                     Equals("ZCBP").
                                                                     Select("input").
                                                                     Where("id").
                                                                     Equals("CreditClass").
                                                                     SelectAllChildNodes("control").
                                                                     GetList<string, string>("item");

            Assert.IsTrue(valueList.Count != 0);

            for (int i = 0; i < valueList.Count; i++)
            {
                Assert.AreEqual(stubCreditClassList[i], valueList[i].Value);
            }
        }


        string stubXmlFile = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                 "<root>\r" +
                                  "<cd id=\"limited ed\">\r" +
                                       "<title>Insides - LP</title>\r" +
                                       "<artist country=\"uk\">Orbital</artist>\r" +
                                       "<year>1996</year>\r" +
                                    "</cd>\r" +
                                    "<cd id=\"bestprice\">\r" +
                                       "<title>LifeForms</title>\r" +
                                       "<artist country=\"uk\">Future Sound Of London</artist>\r" +
                                       "<year>1995</year>\r" +
                                    "</cd>\r\n" + 
                                 "</root>";

        List<string> stubCreditClassList = new List<string> {

                "Govt",
                "AAA",
                "AA",
                "A",
                "BBB",
                "BB",
                "B",
                "CCC"
        };

        List<string> stubMaturityList = new List<string> { 
            "0.08333",
            "0.16667",
            "0.25",
            "0.5",
            "0.75",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100",
            "101",
            "102",
            "103",
            "104",
            "105",
            "106",
            "107",
            "108",
            "109",
            "110",
            "111",
            "112",
            "113",
            "114",
            "115",
            "116",
            "117",
            "118",
            "119",
            "120"
        }; 
    }

    internal class CD
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Year { get; set; }

        public CD(string title, string artist, string year)
        {
            this.Title = title;
            this.Artist = artist;
            this.Year = year;
        }

        public IList<KeyValuePair<string, string>> GetList()
        {
            IList<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            KeyValuePair<string, string> title = new KeyValuePair<string, string>("title", this.Title);
            KeyValuePair<string, string> artist = new KeyValuePair<string, string>("artist", this.Artist);
            KeyValuePair<string, string> year = new KeyValuePair<string, string>("year", this.Year);

            list.Add(title);
            list.Add(artist);
            list.Add(year);

            return list;
        }
    }
}
