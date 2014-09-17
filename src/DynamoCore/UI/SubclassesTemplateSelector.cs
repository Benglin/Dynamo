using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Nodes.Search;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    public class SubclassesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SubclassesTemplate { get; set; }
        public DataTemplate ClassDetailsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is StandardPanelViewModel)
                return ClassDetailsTemplate;

            if (item is BrowserRootElement)
                return SubclassesTemplate;

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
