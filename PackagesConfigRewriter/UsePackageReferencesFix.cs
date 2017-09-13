using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PackagesConfigRewriter
{
    internal class UsePackageReferencesFix : ProjectFix
    {
        private const string ProjectRestoreStyleValue = "PackageReference";
        private readonly XName RestoreProjectStyleElement = Project.MsBuildNamespace + "RestoreProjectStyle";
        private readonly XName NoneElement = Project.MsBuildNamespace + "None";
        private readonly XName IncludeAttribute = "Include";
        private readonly XName PackageElementName = "package";
        private readonly XName PackageReferenceElement = Project.MsBuildNamespace + ProjectRestoreStyleValue;
        private readonly XName PackageIdAttribute ="id";
        private readonly XName MsBuildVersionAttribute = Project.MsBuildNamespace + "Version";
        private readonly XName PackageVersionAttribute = "version";

        // <RestoreProjectStyle>PackageReference</RestoreProjectStyle>

        internal override bool AlreadyAppliedTo(Project project)
        {
            return project.IsMsBuildProject && GetRestoreProjectStyleElement(project)?.Value == ProjectRestoreStyleValue;
        }

        private XElement GetRestoreProjectStyleElement(Project project)
        {
            return project.Root.Elements(Project.PropertyGroupElement).Elements(RestoreProjectStyleElement).FirstOrDefault();
        }

        internal override void Fix(Project project)
        {
            if (!AlreadyAppliedTo(project))
            {
                project.Root.Element(Project.PropertyGroupElement).SetElementValue(RestoreProjectStyleElement, ProjectRestoreStyleValue);
                FileInfo packagesConfigFile = GetProjectPackagesConfigFile(project);
                if (packagesConfigFile.Exists)
                {
                    project.Root.Add(
                        new XElement(Project.ItemGroupElement,
                            PackagesConfigAsPackageReferences(packagesConfigFile)));
                    RemovePackagesConfigFrom(project);
                }
            }
        }

        private void RemovePackagesConfigFrom(Project project)
        {
            project.Root.
                Elements(Project.ItemGroupElement).
                Elements(NoneElement).Where(x => string.Equals(
                    x.Attribute(IncludeAttribute)?.Value, 
                    "packages.config", StringComparison.OrdinalIgnoreCase)).Remove();
        }

        private IEnumerable<XElement> PackagesConfigAsPackageReferences(FileInfo packagesConfigFile)
        {
            return XDocument.Load(packagesConfigFile.FullName).Root.Elements().Select(x => AsPackageReference(x));
        }

        private XElement AsPackageReference(XElement packageElement)
        {
            if (packageElement.Name == PackageElementName)
            {
                return new XElement(PackageReferenceElement,
                    new XAttribute(IncludeAttribute, (string)packageElement.Attribute(PackageIdAttribute)),
                    new XAttribute(MsBuildVersionAttribute, (string)packageElement.Attribute(PackageVersionAttribute)));
            }
            return null;
        }

        private FileInfo GetProjectPackagesConfigFile(Project project)
        {
            return new FileInfo(Path.Combine(project.File.Directory.FullName, "packages.config"));
        }
    }
}