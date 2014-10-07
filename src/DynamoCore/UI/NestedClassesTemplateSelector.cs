using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Nodes.Search;

namespace Dynamo.Controls
{
    public class NestedClassesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SubclassesTemplate { get; set; }
        public DataTemplate ClassDetailsTemplate { get; set; }
        public DataTemplate NestedClassesTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // TODO: That won't work for categories like "Operators" or 'BuiltIn Functions". Need another logic.
            if (item is ClassInformation)
                return ClassDetailsTemplate;

            if (item is BrowserItem)
            {
                if (Dynamo.Nodes.Utilities.ConsistOfNestedClasses(item as BrowserItem)) 
                    return NestedClassesTemplate;

                return SubclassesTemplate;
            }

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
