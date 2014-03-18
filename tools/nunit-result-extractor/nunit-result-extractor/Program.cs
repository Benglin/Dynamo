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
                    var name = iterator.Current.GetAttribute("name", "");
                    var result = iterator.Current.GetAttribute("result", "");
                    builder.Append(string.Format("{0}\t{1}\t", name, result));

                    if (result.Equals("Failure"))
                    {
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
                    }
                    else if (result.Equals("Inconclusive"))
                    {
                        if (iterator.Current.MoveToChild("reason", ""))
                        {
                            if (iterator.Current.MoveToChild("message", ""))
                            {
                                var message = iterator.Current.Value;
                                builder.Append(message.Trim() + "\t");
                            }
                        }
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

            builder.AppendLine(string.Format("\nOnly in {0}\tCount: {1}",
                firstFile, onlyInFirst.Count()));

            foreach (var item in onlyInFirst)
                builder.AppendLine(string.Format("{0}\t{1}", item.Key, item.Value));

            builder.AppendLine(string.Format("\nOnly in {0}\tCount: {1}",
                secondFile, onlyInSecond.Count()));

            foreach (var item in onlyInSecond)
                builder.AppendLine(string.Format("{0}\t{1}", item.Key, item.Value));

            var commonQuery = from testName in firstList.Keys
                              where secondList.ContainsKey(testName)
                              let firstRes = firstList[testName]
                              let secondRes = secondList[testName]
                              select new { testName, firstRes, secondRes };

            var testsThatGotBetter = new List<Tuple<string, string, string>>();
            var testsThatGotWorse = new List<Tuple<string, string, string>>();
            var testsThatStaySame = new List<Tuple<string, string, string>>();

            foreach (var common in commonQuery)
            {
                bool firstSucceeded = IsTestSucceeded(common.firstRes);
                bool secondSucceeded = IsTestSucceeded(common.secondRes);

                if (firstSucceeded == secondSucceeded)
                {
                    testsThatStaySame.Add(new Tuple<string, string, string>(
                        common.testName, common.firstRes, common.secondRes));
                }
                else
                {
                    if (secondSucceeded)
                    {
                        testsThatGotBetter.Add(new Tuple<string, string, string>(
                            common.testName, common.firstRes, common.secondRes));
                    }
                    else
                    {
                        testsThatGotWorse.Add(new Tuple<string, string, string>(
                            common.testName, common.firstRes, common.secondRes));
                    }
                }
            }

            builder.AppendLine(string.Format(
                "\nTests with same results\t{0}\t{1}\tCount: {2}",
                firstFile, secondFile, testsThatStaySame.Count));

            foreach (var test in testsThatStaySame)
            {
                builder.AppendLine(string.Format("{0}\t{1}\t{2}",
                    test.Item1, test.Item2, test.Item3));
            }

            builder.AppendLine(string.Format(
                "\nTests that got worse\t{0}\t{1}\tCount: {2}",
                firstFile, secondFile, testsThatGotWorse.Count));

            foreach (var test in testsThatGotWorse)
            {
                builder.AppendLine(string.Format("{0}\t{1}\t{2}",
                    test.Item1, test.Item2, test.Item3));
            }

            builder.AppendLine(string.Format(
                "\nTests that got better\t{0}\t{1}\tCount: {2}",
                firstFile, secondFile, testsThatGotBetter.Count));

            foreach (var test in testsThatGotBetter)
            {
                builder.AppendLine(string.Format("{0}\t{1}\t{2}",
                    test.Item1, test.Item2, test.Item3));
            }

            streamWriter.Write(builder.ToString());
        }

        private string ExtractStackTrace(XPathNavigator xPathNavigator)
        {
            var lines = GetFirstFewLines(xPathNavigator.Value, 1);
            if (lines == null || (lines.Length <= 0))
                return "\t";

            var messages = lines[0];
            if (messages.StartsWith("at "))
                messages = messages.Substring(3);
            int openBracket = messages.IndexOf('(');
            if (openBracket != -1)
                messages = messages.Substring(0, openBracket);

            return messages + "\t";
        }

        private string ExtractMessage(XPathNavigator xPathNavigator)
        {
            int linesToTake = 1;
            var message = xPathNavigator.Value.Trim();
            if (message.StartsWith("Expected"))
                linesToTake = 2;

            var lines = GetFirstFewLines(message, linesToTake);
            if (lines == null || (lines.Length <= 0))
                return "\t";

            if (lines.Length == 1)
                return lines[0] + "\t";

            return string.Format("{0}, {1}\t", lines[0], lines[1]);

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

        private string[] GetFirstFewLines(string lines, int count)
        {
            string[] brokenLines = lines.Split(
                new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

            if (brokenLines == null || (brokenLines.Length < count))
                return new string[] { string.Empty };

            if (count == 1)
                return new string[] { brokenLines[0].Trim() };

            List<string> results = new List<string>();
            for (int index = 0; index < count; index++)
                results.Add(brokenLines[index].Trim());

            return results.ToArray();
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

        private bool IsTestSucceeded(string result)
        {
            switch (result)
            {
                case "Success":
                case "Inconclusive":
                case "Ignored":
                    return true;

                case "Failure":
                case "Error":
                    return false;
            }

            throw new ArgumentException(
                string.Format("Invalid result value: {0}", result));
        }
    }
}
