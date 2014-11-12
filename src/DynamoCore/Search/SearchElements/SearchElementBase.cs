﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Nodes.Search;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// A base class for elements found in search </summary>
    public abstract class SearchElementBase : BrowserInternalElement
    {
        /// <summary>
        /// The name that is used during node creation
        /// </summary>
        public virtual string CreationName { get { return this.Name; } }

        /// <summary>
        /// Searchable property </summary>
        /// <value>
        /// A bool indicating if the object will appear in searches </value>
        public abstract bool Searchable { get; }

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        public abstract string Type { get; }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        public abstract string Description { get; }

        /// <summary>
        /// Keywords property</summary>
        /// <value>
        /// A set of keywords for the object, joined by spaces</value>
        public abstract string Keywords { get; set; }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search. 
        /// Higher = closer to the top of search results </value>
        public abstract double Weight { get; set; }

        public virtual void Execute()
        {
            this.OnExecuted();
        }

        public delegate void SearchElementHandler(SearchElementBase ele);
        internal event SearchElementHandler Executed;
        protected void OnExecuted()
        {
            if (Executed != null)
            {
                Executed(this);
            }
        }
    }

    /// <summary>
    /// A simple version of the SearchElementBase class needed for sending data to a web client
    /// </summary>
    public class LibraryItem
    {
        /// <summary>
        /// Full category name
        /// </summary>
        [DataMember]
        public string Category { get; private set; }

        /// <summary>
        /// A string describing the type of object
        /// </summary>
        [DataMember]
        public string Type { get; private set; }

        /// <summary>
        /// Model name in the list of all node models
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Unique name that is used during node creation
        /// </summary>
        [DataMember]
        public string CreationName { get; private set; }

        /// <summary>
        /// The name that will be displayed on node itself 
        /// </summary>
        [DataMember]
        public string DisplayName { get; private set; }

        /// <summary>
        /// A string describing what the node does
        /// </summary>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// A bool indicating if the object will appear in searches
        /// </summary>
        [DataMember]
        public bool Searchable { get; private set; }

        /// <summary>
        /// Number defining the relative importance of the element in search. 
        /// Higher = closer to the top of search results
        /// </summary>
        [DataMember]
        public double Weight { get; private set; }

        [DataMember]
        public IEnumerable<string> Keywords { get; private set; }

        [DataMember]
        public IEnumerable<string> Parameters { get; private set; }

        [DataMember]
        public IEnumerable<string> ReturnKeys { get; private set; }

        [DataMember]
        public IEnumerable<object> DefaultValues { get; private set; }

        public LibraryItem(SearchElementBase node, DynamoModel dynamoModel)
        {
            Category = node.FullCategoryName;
            Type = node.Type;
            DisplayName = Name = node.Name;
            CreationName = node.CreationName;
            Description = node.Description;
            Searchable = node.Searchable;
            Weight = node.Weight;
            Keywords = dynamoModel.SearchModel.SearchDictionary.GetTags(node);
            
            PopulateKeysAndParameters(dynamoModel);
        }

        private void PopulateKeysAndParameters(DynamoModel dynamoModel)
        {
            var controller = dynamoModel.EngineController;
            var functionItem = (controller.GetFunctionDescriptor(CreationName));
            NodeModel newElement = null;
            if (functionItem != null)
            {
                DisplayName = functionItem.DisplayName;
                if (functionItem.IsVarArg)
                    newElement = new DSVarArgFunction(dynamoModel.CurrentWorkspace, functionItem);
                else
                    newElement = new DSFunction(dynamoModel.CurrentWorkspace, functionItem);
            }
            else
            {
                TypeLoadData tld = null;

                if (dynamoModel.BuiltInTypesByName.ContainsKey(CreationName))
                {
                    tld = dynamoModel.BuiltInTypesByName[CreationName];
                }
                else if (dynamoModel.BuiltInTypesByNickname.ContainsKey(CreationName))
                {
                    tld = dynamoModel.BuiltInTypesByNickname[CreationName];
                }

                if (tld != null)
                {
                    newElement = (NodeModel)Activator.CreateInstance(tld.Type, dynamoModel.CurrentWorkspace);
                }
            }

            if (newElement != null)
            {
                Parameters = newElement.InPorts.Select(elem => elem.PortName);
                ReturnKeys = newElement.OutPorts.Select(elem => elem.PortName);
                DefaultValues = newElement.InPortData.Select(elem => elem.DefaultValue);
            }
            else
            {
                Parameters = new[] { "Input" };
                ReturnKeys = new[] { "Output" };
                DefaultValues = new object[0];
            }
        }
    }
}
