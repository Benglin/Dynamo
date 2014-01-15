using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Utilities;
using System.Collections.Generic;

namespace Dynamo.Models
{
    internal class Migration
    {
        /// <summary>
        /// A version after which this migration will be applied.
        /// </summary>
        public Version Version { get; set; }
        
        /// <summary>
        /// The action to perform during the upgrade.
        /// </summary>
        public Action Upgrade { get; set; }

        /// <summary>
        /// A migration which can be applied to a workspace to upgrade the workspace to the current version.
        /// </summary>
        /// <param name="v">A version number specified as x.x.x.x after which a workspace will be upgraded</param>
        /// <param name="upgrade">The action to perform during the upgrade.</param>
        public Migration(Version v, Action upgrade)
        {
            Version = v;
            Upgrade = upgrade;
        }
    }

    public class MigrationManager
    {
        private static MigrationManager _instance;

        /// <summary>
        /// The singleton instance property.
        /// </summary>
        public static MigrationManager Instance
        {
            get { return _instance ?? (_instance = new MigrationManager()); }
        }

        /// <summary>
        /// A collection of types which contain migration methods.
        /// </summary>
        public List<Type> MigrationTargets { get; set; }

        /// <summary>
        /// The private constructor.
        /// </summary>
        private MigrationManager()
        {
            MigrationTargets = new List<Type>();
        }

        /// <summary>
        /// Runs all migration methods found on the listed migration target types.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="version"></param>
        public void ProcessWorkspaceMigrations(XmlDocument xmlDoc, Version workspaceVersion)
        {
            var methods = MigrationTargets.SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static));

            var migrations =
                (from method in methods
                    let attribute =
                        method.GetCustomAttributes(false)
                            .OfType<WorkspaceMigrationAttribute>()
                            .FirstOrDefault()
                    where attribute != null
                    let result = new { method, attribute.From, attribute.To }
                    orderby result.From
                    select result).ToList();

            var currentVersion = dynSettings.Controller.DynamoModel.HomeSpace.WorkspaceVersion;

            while (workspaceVersion != null && workspaceVersion < currentVersion)
            {
                var nextMigration = migrations.FirstOrDefault(x => x.From >= workspaceVersion);

                if (nextMigration == null)
                    break;

                nextMigration.method.Invoke(null, new object[] { xmlDoc });
                workspaceVersion = nextMigration.To;
            }
        }

        public void ProcessNodesInWorkspace(XmlDocument xmlDoc, Version workspaceVersion)
        {
            XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");
            if (elNodes == null || (elNodes.Count == 0))
                elNodes = xmlDoc.GetElementsByTagName("dynElements");

            XmlNode elNodesList = elNodes[0];
            foreach (XmlNode elNode in elNodesList.ChildNodes)
            {
                string typeName = elNode.Attributes["type"].Value;
                typeName = Dynamo.Nodes.Utilities.PreprocessTypeName(typeName);
                System.Type type = Dynamo.Nodes.Utilities.ResolveType(typeName);

                // TODO(Ben): Implement this.
                // 
                // if (this.MigrateXmlNode(elNode, type, workspaceVersion))
                // {
                // }
            }

            // TODO(Ben): Replace the old child nodes with the new set.
        }

        // TODO(Ben): This method doesn't handle the case when a node gets turned into multiple ones.
        public bool MigrateXmlNode(XmlNode elNode, System.Type type, Version workspaceVersion)
        {
            var migrations = (from method in type.GetMethods()
                              let attribute =
                                  method.GetCustomAttributes(false).OfType<NodeMigrationAttribute>().FirstOrDefault()
                              where attribute != null
                              let result = new { method, attribute.From, attribute.To }
                              orderby result.From
                              select result).ToList();

            Version currentVersion = dynSettings.Controller.DynamoModel.HomeSpace.WorkspaceVersion;

            bool migrationAttempted = false;
            while (workspaceVersion != null && workspaceVersion < currentVersion)
            {
                var nextMigration = migrations.FirstOrDefault(x => x.From >= workspaceVersion);

                if (nextMigration == null)
                    break;

                migrationAttempted = true;
                nextMigration.method.Invoke(this, new object[] { elNode });
                workspaceVersion = nextMigration.To;
            }

            return migrationAttempted;
        }

        /// <summary>
        /// Remove revision number from 'fileVersion' (so we get '0.6.3.0' 
        /// instead of '0.6.3.20048'). This way all migration methods with 
        /// 'NodeMigration.from' attribute value '0.6.3.xyz' can be used to 
        /// migrate nodes in workspace version '0.6.3.ijk' (i.e. the revision 
        /// number does not have to be exact match for a migration method to 
        /// work).
        /// </summary>
        /// <param name="version">The version string to convert into Version 
        /// object. Valid examples include "0.6.3" and "0.6.3.20048".</param>
        /// <returns>Returns the Version object representation of 'version' 
        /// argument, except without the 'revision number'.</returns>
        /// 
        internal static Version VersionFromString(string version)
        {
            Version ver = string.IsNullOrEmpty(version) ?
                new Version(0, 0, 0, 0) : new Version(version);

            // Ignore revision number.
            return new Version(ver.Major, ver.Minor, ver.Build);
        }

        /// <summary>
        /// Call this method to create a XmlElement with a set of attributes 
        /// carried over from the source XmlElement. The new XmlElement will 
        /// have a name of "Dynamo.Nodes.DSFunction".
        /// </summary>
        /// <param name="srcElement">The source XmlElement object.</param>
        /// <param name="attribNames">The list of attribute names whose values 
        /// are to be carried over to the resulting XmlElement. This list is 
        /// mandatory and it cannot be empty. If a specified attribute cannot 
        /// be found in srcElement, an empty attribute with the same name will 
        /// be created in the resulting XmlElement.</param>
        /// <returns>Returns the resulting XmlElement with specified attributes
        /// duplicated from srcElement. The resulting XmlElement will also have
        /// a mandatory "type" attribute with value "Dynamo.Nodes.DSFunction".
        /// </returns>
        /// 
        internal static XmlElement CreateFunctionNodeFrom(
            XmlElement srcElement, string[] attribNames)
        {
            if (srcElement == null)
                throw new ArgumentNullException("srcElement");
            if (attribNames == null || (attribNames.Length <= 0))
                throw new ArgumentException("Argument cannot be empty", "attribNames");

            XmlDocument document = srcElement.OwnerDocument;
            XmlElement dstElement = document.CreateElement("Dynamo.Nodes.DSFunction");

            foreach (string attribName in attribNames)
            {
                var value = srcElement.GetAttribute(attribName);
                dstElement.SetAttribute(attribName, value);
            }

            dstElement.SetAttribute("type", "Dynamo.Nodes.DSFunction");
            return dstElement;
        }

        /// <summary>
        /// Call this method to create a duplicated XmlElement with 
        /// all the attributes found from the source XmlElement.
        /// </summary>
        /// <param name="srcElement">The source XmlElement to duplicate.</param>
        /// <returns>Returns the duplicated XmlElement with all attributes 
        /// found in the source XmlElement. The resulting XmlElement will also 
        /// have a mandatory "type" attribute with value "Dynamo.Nodes.DSFunction".
        /// </returns>
        /// 
        internal static XmlElement CreateFunctionNodeFrom(XmlElement srcElement)
        {
            if (srcElement == null)
                throw new ArgumentNullException("srcElement");

            XmlDocument document = srcElement.OwnerDocument;
            XmlElement dstElement = document.CreateElement("Dynamo.Nodes.DSFunction");

            foreach (XmlAttribute attribute in srcElement.Attributes)
                dstElement.SetAttribute(attribute.Name, attribute.Value);

            dstElement.SetAttribute("type", "Dynamo.Nodes.DSFunction");
            return dstElement;
        }
    }

    /// <summary>
    /// This class contains the resulting nodes as a result of node migration.
    /// Note that this class may contain other information (e.g. connectors) in
    /// the future in the event a migration process results in other elements.
    /// </summary>
    internal class NodeMigrationData
    {
        private List<XmlElement> migratedNodes = new List<XmlElement>();

        internal void AppendNode(XmlElement node)
        {
            migratedNodes.Add(node);
        }

        internal IEnumerable<XmlElement> MigratedNodes
        {
            get { return this.migratedNodes; }
        }
    }

    /// <summary>
    /// Marks methods on a NodeModel to be used for version migration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NodeMigrationAttribute : Attribute
    {
        /// <summary>
        /// Latest Version this migration applies to.
        /// </summary>
        public Version From { get; private set; }

        /// <summary>
        /// Version this migrates to.
        /// </summary>
        public Version To { get; private set; }

        public NodeMigrationAttribute(string from, string to="")
        {
            From = new Version(from);
            To = String.IsNullOrEmpty(to) ? null : new Version(to);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class WorkspaceMigrationAttribute : Attribute
    {
        public Version From { get; private set; }
        public Version To { get; private set; }

        public WorkspaceMigrationAttribute(string from, string to="")
        {
            From = new Version(from);
            To = String.IsNullOrEmpty(to) ? null : new Version(to);
        }
    }
}
