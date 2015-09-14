using System.Linq;
using System.Text;

namespace XPathFluentComposer
{
    public class XPathQueryBuilder
    {
        private StringBuilder _query;

        public string Query 
        {
            get 
            {
                return _query.ToString();
            }
        }
   
        public XPathQueryBuilder()
        {
            _query = new StringBuilder();
        }

        public void Clean()
        {
            _query.Clear();
        }

        /* /element */
        public void SelectElement(string element)
        {
            AddSlashChar();
            _query.Append(element);
        }

        /* [1] */
        public void SelectFirst()
        {
            OpenSquareBracket();
            _query.Append("1");
            CloseSquareBracket();
        }

        /* [last()] */
        public void SelectLast()
        {
            OpenSquareBracket();
            _query.Append("last()");
            CloseSquareBracket();
        }

        /* [@ */
        public void AddFilterName(string element)
        {
            OpenSquareBracket();
            AddFilterChar();
            _query.Append(element);
        }

        /* : */
        public void AddAndOperator()
        {
            AddAndChar();
        }

        /* ='value'] */
        public void AddFilterValue(string value)
        {
            AddEqualChar();
            AddPeackChar();
            _query.Append(value);
            AddPeackChar();
            CloseSquareBracket();
        }

        public string GetLastEntryNode()
        {
            string tmp = _query.ToString();
            string[] values = tmp.Split('/');
            string lastEntry = values[values.Length - 1];

            return RemoveFilter(lastEntry);
        }

        public void RemoveLastEntry()
        {
            string tmp = _query.ToString();
            string[] values = tmp.Split('/');
            string lastEntry = values[values.Length-1];

            values = values.Where(val => val != lastEntry).ToArray();
             
            _query.Clear();
            _query.Append(string.Join("/", values));
        }

        internal void AddSlashChar()
        {
            _query.Append("/");
        }

        internal void OpenSquareBracket()
        {
            _query.Append("[");
        }

        internal void AddFilterChar()
        {
            _query.Append("@");
        }

        internal void CloseSquareBracket()
        {
            _query.Append("]");
        }

        internal void AddEqualChar()
        {
            _query.Append("=");
        }

        internal void AddPeackChar()
        {
            _query.Append("'");
        }

        internal void AddAndChar()
        {
            _query.Append(":");
        }

        internal string RemoveFilter(string value)
        {
            string[] split = value.Split('[');

            if (split.Length > 1)
            {
                return split[0];
            }
            else
            {
                return value;
            }
        }
    }
}
