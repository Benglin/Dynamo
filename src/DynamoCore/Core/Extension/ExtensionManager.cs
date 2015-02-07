using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Dynamo.Core.Extension
{
    class ExtensionInfo
    {
        private const string ExtensionModuleName = "ExtensionModule";
        private const string RemoteExecutableName = "RemoteExecutable";

        internal ExtensionInfo(string filePath)
        {
            var document = new XmlDocument();
            document.Load(filePath);

            var childNodes = document.DocumentElement.ChildNodes;
            foreach (XmlNode childNode in childNodes)
            {
                switch (childNode.Name)
                {
                    case ExtensionModuleName:
                        ExtensionModule = childNode.InnerText;
                        break;
                    case RemoteExecutableName:
                        RemoteExecutable = childNode.InnerText;
                        break;
                }
            }
        }

        internal string ExtensionModule { get; private set; }
        internal string RemoteExecutable { get; private set; }
    }

    class ExtensionManager
    {
        internal ExtensionManager()
        {
            var exePath = Assembly.GetCallingAssembly().Location;
            var directory = new DirectoryInfo(Path.GetDirectoryName(exePath));
            var files = directory.GetFiles("*.dynext");

            foreach (var fileInfo in files)
            {
                try
                {
                    var extensionInfo = new ExtensionInfo(fileInfo.FullName);
                    LoadExtension(extensionInfo);
                }
                catch (Exception)
                {
                }
            }
        }

        private void LoadExtension(ExtensionInfo extensionInfo)
        {
        }
    }
}
