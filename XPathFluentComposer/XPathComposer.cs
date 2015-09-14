using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XPathFluentComposer
{
    public class XPathComposer
    {
        private XmlDocument doc = new XmlDocument();
        private XPathQueryBuilder xPathBuilder;
        private string File { get; set; }

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="file">File name [optional].Use this to use same xml file more times.</param>
        public XPathComposer(string file = null)
        {
            xPathBuilder = new XPathQueryBuilder();
            this.File = file;

            if (file != null)
                doc.Load(file);
        }

        /// <summary>
        /// Set xml file name for in-line query
        /// </summary>
        /// <param name="file">file name</param>
        public XPathComposer FileName(string file)
        {
            xPathBuilder = new XPathQueryBuilder();
            this.File = file;
            doc.Load(file);
            return this;
        }

        /// <summary>
        /// Create xml file
        /// </summary>
        /// <param name="path">xml file path and name</param>
        /// <param name="root">root node name</param>
        public XPathComposer CreateXmlFile(string path, string root)
        {
            doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
            XmlElement startElement = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, startElement);

            XmlElement rootNode = doc.CreateElement(string.Empty, root, string.Empty);
            doc.AppendChild(rootNode);

            this.File = path;
            xPathBuilder.SelectElement(root);

            doc.Save(path);
            return this;
        }

        /// <summary>
        /// Set root node
        /// </summary>
        /// <param name="root">root node name</param>
        public XPathComposer RootNode(string root)
        {
            xPathBuilder.Clean();
            xPathBuilder.SelectElement(root);
            return this;
        }

        /// <summary>
        /// Select specific node
        /// </summary>
        /// <param name="node">node name</param>
        public XPathComposer Select(string node)
        {
            xPathBuilder.SelectElement(node);
            return this;
        }

        /// <summary>
        /// Select all child nodes
        /// </summary>
        /// <param name="node">parent node name [optional].</param>
        public XPathComposer SelectAllChildNodes(string node = null)
        {
            xPathBuilder.SelectElement("*");
            if (node != null)
                xPathBuilder.SelectElement(node);
            return this;
        }

        /// <summary>
        /// Set attribute name to filter
        /// </summary>
        /// <param name="attribute">attribute name</param>
        public XPathComposer Where(string attribute)
        {
            xPathBuilder.AddFilterName(attribute);
            return this;
        }

        /// <summary>
        /// Set attribute value to filter
        /// </summary>
        /// <param name="value">attribute value</param>
        public XPathComposer Equals(string value)
        {
            xPathBuilder.AddFilterValue(value);
            return this;
        }

        /// <summary>
        /// Add attribute to a node
        /// </summary>
        /// <param name="name">attribute name</param>
        /// <param name="value">attribute value</param>
        public XPathComposer AddAttribute(string name, string value)
        {
            XmlNode node = doc.SelectSingleNode(xPathBuilder.Query);
            XmlAttribute attribute = doc.CreateAttribute(name);
            attribute.Value = value;
            node.Attributes.Append(attribute);

            doc.Save(this.File);
            return this;
        }

        /// <summary>
        /// Add parent node
        /// </summary>
        /// <param name="node">node name</param>
        public XPathComposer AddParentNode(string node)
        {
            XmlNode xmlNode = doc.SelectSingleNode(xPathBuilder.Query);
            XmlElement xmlElement = doc.CreateElement(node);

            xPathBuilder.SelectElement(node);
            xmlNode.InsertAfter(xmlElement, xmlNode[xmlNode.Name]);

            doc.Save(this.File);
            return this;
        }

        /// <summary>
        /// Get the first node found with all childs
        /// </summary>
        /// <returns>return a name-value list</returns>
        public IList<KeyValuePair<string, T>> First<TKey, T>()
        {
            xPathBuilder.SelectFirst();
            xPathBuilder.SelectElement("*");
            XmlNodeList valueList = doc.SelectNodes(xPathBuilder.Query);

            xPathBuilder.Clean();

            return ConvertToList<string, T>(valueList);
        }

        /// <summary>
        /// Get the last node found with all childs
        /// </summary>
        /// <returns>return a name-value list</returns>
        public IList<KeyValuePair<string, T>> Last<TKey, T>()
        {
            xPathBuilder.SelectLast();
            xPathBuilder.SelectElement("*");
            XmlNodeList valueList = doc.SelectNodes(xPathBuilder.Query);

            xPathBuilder.Clean();

            return ConvertToList<string, T>(valueList);
        }

        /// <summary>
        /// Get list of nodes
        /// </summary>
        /// <param name="node">node name [optional].</param>
        /// <returns>return a list of nodes<KeyValuePair<string, string>></returns>
        public IList<KeyValuePair<string, T>> GetList<TKey,T>(string node = null)
        {
            XmlNodeList valueList = node != null ? 
                                    doc.SelectNodes(xPathBuilder.Query).Cast<XmlNode>().Select(elem => elem.SelectNodes(node)).First() : 
                                    doc.SelectNodes(xPathBuilder.Query);

            xPathBuilder.Clean();

            return ConvertToList<string,T>(valueList);
        }

        /// <summary>
        /// Get specified value
        /// </summary>
        /// <param name="node">node name</param>
        /// <returns>value with requested type</returns>
        public T GetValueOf<T>(string node)
        {
            xPathBuilder.SelectElement(node);
            string value = doc.SelectSingleNode(xPathBuilder.Query).InnerXml;

            xPathBuilder.Clean();
            return ConvertValue<T>(value);
        }

        /// <summary>
        /// Write value in a node
        /// </summary>
        /// <param name="node">node name</param>
        /// <param name="value">node value</param>
        public void WriteValue(string node, string value)
        {
            xPathBuilder.SelectElement(node);
            doc.SelectSingleNode(xPathBuilder.Query).InnerText = value;

            xPathBuilder.Clean();
            doc.Save(this.File);
        }

        /// <summary>
        /// Write values in a node list
        /// </summary>
        /// <param name="nodeList">node name and value list</param>
        public void WriteValues<Tkey,T>(IList<KeyValuePair<string, T>> nodeList)
        {
            foreach(KeyValuePair<string, T> node in nodeList)
            {
                doc.SelectSingleNode(xPathBuilder.Query + "/" + node.Key).InnerText = node.Value.ToString();
                doc.Save(this.File);
            }

            xPathBuilder.Clean();
        }

        /// <summary>
        /// Insert new node after a specified node
        /// </summary>
        /// <param name="node">node name and value</param>
        /// <param name="afterNodeName">existing node name</param>
        public void InsertAfter<TKey, T>(string afterNodeName, KeyValuePair<string, T> node)
        {
            XmlNode xmlNode = doc.SelectSingleNode(xPathBuilder.Query);
            XmlElement xmlElement = CreateNewNode<string, T>(node);

            xmlNode.InsertAfter(xmlElement, xmlNode[afterNodeName]);
            doc.Save(this.File);
        }

        /// <summary>
        /// Insert new node before a specified node
        /// </summary>
        /// <param name="node">node name and value</param>
        /// <param name="beforeNodeName">existing node name</param>
        public void InsertBefore<TKey, T>(string beforeNodeName, KeyValuePair<string, T> node)
        {
            XmlNode xmlNode = doc.SelectSingleNode(xPathBuilder.Query);
            XmlElement xmlElement = CreateNewNode<string, T>(node);

            xmlNode.InsertBefore(xmlElement, xmlNode[beforeNodeName]);
            doc.Save(this.File);
        }

        /// <summary>
        /// Remove parent node and all own childs
        /// </summary>
        public void Remove()
        {
            XmlNode node = doc.SelectSingleNode(xPathBuilder.Query);
            node.RemoveAll();

            xPathBuilder.Clean();
            doc.Save(this.File);
        }

        /// <summary>
        /// Remove all child nodes
        /// </summary>
        public void RemoveAllNodes()
        {
            this.SelectAllChildNodes();
            XmlNodeList nodeList = doc.SelectNodes(xPathBuilder.Query);
            xPathBuilder.RemoveLastEntry();

            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNode node = doc.SelectSingleNode(xPathBuilder.Query);
                node.RemoveChild(node[nodeList[i].Name]);
            }

            xPathBuilder.Clean();
        }

        /// <summary>
        /// Remove a specified child node
        /// </summary>
        /// <param name="name">node name to remove</param>
        public void RemoveNode(string name)
        {
            XmlNode node = doc.SelectSingleNode(xPathBuilder.Query);
           
            node.RemoveChild(node[name]);

            xPathBuilder.Clean();
            doc.Save(this.File);
        }

        /// <summary>
        /// Add nodes to xml
        /// </summary>
        /// <param name="nodeList">node name and value list</param>
        /// <param name="node">parent node name</param>
        public void AddNodeList<TKey,T>(IList<KeyValuePair<string, T>> nodeList)
        {
            XmlNode xmlNode = doc.SelectSingleNode(xPathBuilder.Query);

            foreach (KeyValuePair<string, T> element in nodeList.Reverse())
            {
                this.InsertAfter<string,T>(xmlNode.Name, element);
            }

            xPathBuilder.Clean();
        }

        internal IList<KeyValuePair<string, T>> ConvertToList<TKey,T>(XmlNodeList xmlNodeList)
        {
            IList<KeyValuePair<string, T>> list = new List<KeyValuePair<string, T>>();
            foreach (XmlNode node in xmlNodeList)
            {
                KeyValuePair<string, T> element = new KeyValuePair<string, T>(node.Name, ConvertValue<T>(node.InnerText));
                list.Add(element);
            }

            return list;
        }

        internal T ConvertValue<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        internal XmlElement CreateNewNode<TKey,T>(KeyValuePair<string, T> element)
        {
            XmlElement node = doc.CreateElement(element.Key);
            node.InnerText = element.Value.ToString();
            return node;
        }
    }
}
