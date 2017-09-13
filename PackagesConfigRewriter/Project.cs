using System.IO;
using System.Xml.Linq;

namespace PackagesConfigRewriter
{
    internal class Project : XDocument
    {
        public static readonly XNamespace MsBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        public static readonly XName PropertyGroupElement = MsBuildNamespace + "PropertyGroup";
        public readonly static XName ItemGroupElement = MsBuildNamespace + "ItemGroup";
        private readonly FileInfo _projectFile;
        private bool _dirty = false;
        private readonly XName ProjectTypeGuidsElement = MsBuildNamespace + "ProjectTypeGuids";

        private Project(FileInfo projectFile, XDocument document) : base(document)
        {
            _projectFile = projectFile;
            Changed += ProjectChanged;
        }

        private void ProjectChanged(object sender, XObjectChangeEventArgs e)
        {
            _dirty = true;
        }

        public static Project Create(FileInfo projectFile)
        {
            if (projectFile == null)
            {
                throw new System.ArgumentNullException(nameof(projectFile));
            }

            return new Project(projectFile, Load(projectFile.FullName));
        }

        public bool Save()
        {
            if (_dirty)
            {
                Save(_projectFile.FullName);
                _dirty = false;
                return true;
            }
            return false;
        }

        public bool IsMsBuildProject => Root.Name.Namespace == MsBuildNamespace;

        public bool IsWebApplication => IsMsBuildProject &&
            (Root.Element(PropertyGroupElement)?.
                Element(ProjectTypeGuidsElement)?.Value?.Contains("{349c5851-65df-11da-9384-00065b846f21}") ?? false);

        public FileInfo File => _projectFile;
    }
}