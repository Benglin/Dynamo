using Dynamo.Controls;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Windows.Controls;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class LibraryContainerViewModel : ViewModelBase
    {
        private DynamoViewModel dynamoViewModel;

        public LibraryContainerViewModel(DynamoViewModel dynamoViewModel, DynamoView dynamoView)
        {
            this.dynamoViewModel = dynamoViewModel;

            try
            {
                var geometryRoot = new ItemData();
                geometryRoot.text = "Geometry";
                geometryRoot.iconName = "Geometry";
                geometryRoot.itemType = "category";

                var rootNode = new ItemData();
                rootNode.childItems.Add(geometryRoot);

                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var type = Type.GetType("Dynamo.HostedContents.LibraryContainer, HostedContents");
                var result = Activator.CreateInstance(type, this);
                dynamoView.sidebarGrid.Children.Add(result as UserControl);

                ItemData classItemData = null;
                foreach (var entry in dynamoViewModel.Model.SearchModel.SearchEntries)
                {
                    if (!entry.IconName.StartsWith("Autodesk.DesignScript.Geometry.")) continue;

                    var parts = entry.IconName.Split('.');
                    if ((classItemData == null) || !classItemData.text.Equals(parts[3]))
                    {
                        classItemData = new ItemData();
                        classItemData.text = parts[3];
                        classItemData.iconName = parts[3];
                        classItemData.itemType = "none";
                        geometryRoot.childItems.Add(classItemData);
                    }

                    var itemType = "none";
                    var element = entry as NodeSearchElement;
                    if (element != null)
                    {
                        switch (element.Group)
                        {
                            case SearchElementGroup.Create: itemType = "creation"; break;
                            case SearchElementGroup.Action: itemType = "action"; break;
                            case SearchElementGroup.Query: itemType = "query"; break;
                        }
                    }

                    var methodItemData = new ItemData();
                    methodItemData.text = parts[4];
                    methodItemData.iconName = entry.IconName;
                    methodItemData.itemType = itemType;
                    classItemData.childItems.Add(methodItemData);
                }

                var outputStream = new MemoryStream();
                var serializer = new DataContractJsonSerializer(typeof(ItemData));
                serializer.WriteObject(outputStream, rootNode);
                outputStream.Position = 0;
                var inputStream = new StreamReader(outputStream);

                dynamoView.libraryContainer = result as ILibraryContainer;
                dynamoView.libraryContainer.SetLoadedTypesJson(inputStream.ReadToEnd());
                dynamoView.libraryContainer.WebBrowserLoaded += (object senderObject, EventArgs eventArgs) =>
                {
                    // Does nothing for now.
                };

                SetLoadedTypesRaw(dynamoView.libraryContainer, dynamoViewModel.Model.SearchModel.SearchEntries);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Called when the library container view receives a click event from the web browser.
        /// </summary>
        /// <param name="id">Id should contain the creation name of the clicked node.</param>
        public void OnLibraryContainerClicked(string id)
        {
            var nodeSearchElement = dynamoViewModel.SearchViewModel.Model.SearchEntries
                .Where(x => x.CreationName == id).FirstOrDefault();

            if (nodeSearchElement != null)
            {
                var nodeModel = nodeSearchElement.CreateNode();
                dynamoViewModel.SearchViewModel.OnSearchElementClicked(nodeModel, new System.Windows.Point(0, 0));
            }
        }

        private void SetLoadedTypesRaw(ILibraryContainer libraryContainer, IEnumerable<NodeSearchElement> searchEntries)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append(@"{ ""loadedTypes"": [");

            bool firstIteration = true;
            foreach (var entry in searchEntries)
            {
                if (firstIteration)
                    firstIteration = false;
                else
                    builder.Append(",");

                var itemType = "";
                switch (entry.Group)
                {
                    case SearchElementGroup.Create: itemType = "creation"; break;
                    case SearchElementGroup.Action: itemType = "action"; break;
                    case SearchElementGroup.Query: itemType = "query"; break;
                }

                builder.Append("{");
                builder.AppendFormat(" \"fullyQualifiedName\": \"{0}\", ", entry.FullyQualifiedName);
                builder.AppendFormat(" \"iconName\": \"{0}\", ", entry.IconName);
                builder.AppendFormat(" \"creationName\": \"{0}\", ", entry.CreationName);
                builder.AppendFormat(" \"itemType\": \"{0}\" ", itemType);
                builder.Append("}");
            }

            builder.Append(@"] }");
            WriteLibraryToFile(builder);
            libraryContainer.SetLoadedTypesRaw(builder.ToString());
        }

        private void WriteLibraryToFile(System.Text.StringBuilder builder)
        {
            // Write the unformatted JSON directly in the docs folder
            string path = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\..\\librarie.js\\docs\\";
            using (StreamWriter sw = new StreamWriter(path + "RawTypeData.json"))
            {
                sw.WriteLine(builder.ToString());
            }
        }
    }
}
