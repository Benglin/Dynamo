using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace nunit_result_extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var xmlResultPath = @"C:\Results\nunit-result.xml";
            Program program = new Program(xmlResultPath);
        }

        internal Program(string xmlResultPath)
        {
            ProcessXmlResultFile(xmlResultPath);
        }

        private void ProcessXmlResultFile(string xmlResultPath)
        {
            XPathDocument document = new XPathDocument(xmlResultPath);
            XPathNavigator navigator = document.CreateNavigator();

            XPathExpression expression = navigator.Compile("//test-case");
            XPathNodeIterator iterator = navigator.Select(expression);

            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Test Name\tResult\tMessage\tException Method\n");

                while (iterator.MoveNext())
                {
                    builder.Append(iterator.Current.GetAttribute("name", "") + "\t");
                    builder.Append(iterator.Current.GetAttribute("result", "") + "\t");

                    if (iterator.Current.MoveToChild("failure", ""))
                    {
                        if (iterator.Current.MoveToChild("message", ""))
                        {
                            builder.Append(ExtractMessage(iterator.Current));
                            iterator.Current.MoveToParent(); // Back to "failure"
                        }

                        if (iterator.Current.MoveToChild("stack-trace", ""))
                        {
                            builder.Append(ExtractStackTrace(iterator.Current));
                            iterator.Current.MoveToParent(); // Back to "failure"
                        }

                        iterator.Current.MoveToParent(); // Back to "test-case".
                    }

                    builder.Append("\n");
                }

                var outputPath = Path.Combine(Path.GetDirectoryName(xmlResultPath), "output.txt");
                using (StreamWriter writer = new StreamWriter(outputPath, false))
                {
                    writer.Write(builder.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string ExtractStackTrace(XPathNavigator xPathNavigator)
        {
            var messages = GetFirstLineOf(xPathNavigator.Value);

            if (messages.StartsWith("at "))
                messages = messages.Substring(3);
            int openBracket = messages.IndexOf('(');
            if (openBracket != -1)
                messages = messages.Substring(0, openBracket);

            return messages + "\t";
        }

        private string ExtractMessage(XPathNavigator xPathNavigator)
        {
            var message = xPathNavigator.Value;
            return GetFirstLineOf(message.Trim()) + "\t";
/*
            var segments = message.Split(new char[] { ':' });

            string exception = message;
            if (segments != null && (segments.Length > 1))
            {
                if (segments.Length == 2)
                    exception = segments[0].Trim();
                else if (segments.Length == 3)
                    exception = segments[1].Trim();
            }

            return GetFirstLineOf(exception.Trim()) + "\t";
*/
        }

        private string GetFirstLineOf(string lines)
        {
            string[] brokenLines = lines.Split(
                new string[] { "\r\n" },
                StringSplitOptions.RemoveEmptyEntries);

            if (brokenLines == null || (brokenLines.Length < 1))
                return string.Empty;

            return brokenLines[0];
        }
    }
}
