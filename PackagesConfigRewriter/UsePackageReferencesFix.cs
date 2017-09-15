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
        private static readonly XName RestoreProjectStyleElement = Project.MsBuildNamespace + "RestoreProjectStyle";
        private static readonly XName NoneElement = Project.MsBuildNamespace + "None";
        private static readonly XName IncludeAttribute = "Include";
        private static readonly XName PackageElementName = "package";
        private static readonly XName PackageReferenceElement = Project.MsBuildNamespace + ProjectRestoreStyleValue;
        private static readonly XName PackageIdAttribute ="id";
        private static readonly XName MsBuildVersionAttribute = "Version";
        private static readonly XName PackageVersionAttribute = "version";
        private static readonly XName ReferenceElement = Project.MsBuildNamespace + "Reference";
        private static readonly XName HintPathElement = Project.MsBuildNamespace + "HintPath";

        // <RestoreProjectStyle>PackageReference</RestoreProjectStyle>

        internal override bool AlreadyAppliedTo(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            return UsesPackageReference(project);
        }

        internal static bool UsesPackageReference(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            return project.IsMsBuildProject && GetRestoreProjectStyleElement(project)?.Value == ProjectRestoreStyleValue;
        }

        private static XElement GetRestoreProjectStyleElement(Project project)
        {
            return project.Root.Elements(Project.PropertyGroupElement).Elements(RestoreProjectStyleElement).FirstOrDefault();
        }

        internal override void Fix(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

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
            project.Root.Elements(Project.ItemGroupElement).Elements(ReferenceElement)
                .Where(x => x.Element(HintPathElement)?.Value?.StartsWith(@"..\packages\", StringComparison.OrdinalIgnoreCase) ?? false)
                .Remove();
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

        public override string ToString()
        {
            return "use PackageReference instead of packages.config";
        }
    }
}