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
            if (args.Length < 2) // Insufficient arguments
                return;

            var cmdSwitch = args[0].ToLower();
            switch (cmdSwitch)
            {
                case "raw":
                    if (!Directory.Exists(args[1]))
                        return;

                    Program program = new Program(args[1]);
                    break;

                case "compare":
                    if (args.Length < 3) // Insufficient arguments
                        return;
                    if (!File.Exists(args[1]) || (!File.Exists(args[2])))
                        return; // Source files do not exist.

                    Program program2 = new Program(args[1], args[2]);
                    break;
            }
        }

        internal Program(string xmlResultFolder)
        {
            if (Directory.Exists(xmlResultFolder) == false)
                return;

            string[] xmlFilePaths = Directory.GetFiles(xmlResultFolder, "*.xml");
            if (xmlFilePaths == null || (xmlFilePaths.Length <= 0))
                return;

            var outputPath = GetOutputFilePath(xmlResultFolder);
            using (StreamWriter streamWriter = new StreamWriter(outputPath, false))
            {
                foreach (string xmlFilePath in xmlFilePaths)
                    ProcessXmlResultFile(xmlFilePath, streamWriter);
            }
        }

        internal Program(string firstFile, string secondFile)
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(firstFile));
            var outputPath = GetOutputFilePath(folder);
            using (StreamWriter streamWriter = new StreamWriter(outputPath, false))
            {
                CompareResultFiles(firstFile, secondFile, streamWriter);
            }
        }

        private string GetOutputFilePath(string folder)
        {
            var timePrefix = DateTime.Now.ToString("yyyyMMddHHmm");
            var outputFile = string.Format("results-{0}.txt", timePrefix);
            return Path.Combine(folder, outputFile);
        }

        private void ProcessXmlResultFile(string xmlResultPath, StreamWriter streamWriter)
        {
            XPathDocument document = new XPathDocument(xmlResultPath);
            XPathNavigator navigator = document.CreateNavigator();

            XPathExpression expression = navigator.Compile("//test-case");
            XPathNodeIterator iterator = navigator.Select(expression);

            try
            {
                StringBuilder builder = new StringBuilder();

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

                streamWriter.Write(builder.ToString()); // Write to output file.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CompareResultFiles(string firstFile,
            string secondFile, StreamWriter streamWriter)
        {
            var firstList = ExtractResultFromFile(firstFile);
            var secondList = ExtractResultFromFile(secondFile);

            var onlyInFirst = firstList.Where((x) => !secondList.ContainsKey(x.Key));
            var onlyInSecond = secondList.Where((y) => !firstList.ContainsKey(y.Key));

            StringBuilder builder = new StringBuilder();

            builder.AppendLine(string.Format("\nOnly in {0}", firstFile));
            foreach (var item in onlyInFirst)
                builder.AppendLine(string.Format("{0}\t{1}", item.Key, item.Value));

            builder.AppendLine(string.Format("\nOnly in {0}", secondFile));
            foreach (var item in onlyInSecond)
                builder.AppendLine(string.Format("{0}\t{1}", item.Key, item.Value));

            var commonQuery = from testName in firstList.Keys
                              where secondList.ContainsKey(testName)
                              let firstRes = firstList[testName]
                              let secondRes = secondList[testName]
                              select new { testName, firstRes, secondRes };

            builder.AppendLine(string.Format("\nCommon test cases\t{0}\t{1}",
                firstFile, secondFile));

            foreach (var common in commonQuery)
            {
                builder.AppendLine(string.Format("{0}\t{1}\t{2}",
                    common.testName, common.firstRes, common.secondRes));
            }

            streamWriter.Write(builder.ToString());
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
                new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

            if (brokenLines == null || (brokenLines.Length < 1))
                return string.Empty;

            return brokenLines[0];
        }

        private Dictionary<string, string> ExtractResultFromFile(string filePath)
        {
            var results = new Dictionary<string, string>();

            StreamReader streamReader = new StreamReader(filePath);
            while (streamReader.EndOfStream == false)
            {
                string line = streamReader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(new char[] { '\t' });
                if (parts == null || (parts.Length < 2))
                    continue;

                results.Add(parts[0], parts[1]);
            }

            return results;
        }
    }
}
