using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PackagesConfigRewriter
{
    internal class AddAutoGenerateBindingRedirectsFix : ProjectFix
    {
        private static readonly XName AutoGenerateBindingRedirectsElement = Project.MsBuildNamespace + "AutoGenerateBindingRedirects";
        private static readonly XName GenerateBindingRedirectsOutputTypeElement = Project.MsBuildNamespace + "GenerateBindingRedirectsOutput";
        private static readonly XName OutputTypeElement = Project.MsBuildNamespace + "OutputType";

        internal override bool AlreadyAppliedTo(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }
            if (!project.IsMsBuildProject)
            {
                return true;
            }
            if (!IsTestProject(project) && !IsExecutable(project))
            {
                return true;
            }

            return  AppliedAutoGenerateBindingRedirects(project)
                && AppliedGenerateBindingRedirectsOutputType(project);
        }

        private bool IsExecutable(Project project)
        {
            return GetElement(project, OutputTypeElement)?.Value?.Equals("Exe", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        private bool AppliedGenerateBindingRedirectsOutputType(Project project)
        {
            return IsPropertySetToTrue(project, GenerateBindingRedirectsOutputTypeElement);
        }

        private bool IsPropertySetToTrue(Project project, XName elementName)
        {
            return (bool?)(GetElement(project, elementName)) ?? false;
        }

        private static XElement GetElement(Project project, XName elementName)
        {
            return project.Root.Elements(Project.PropertyGroupElement)
                            .Elements(elementName)
                            .FirstOrDefault();
        }

        private bool AppliedAutoGenerateBindingRedirects(Project project)
        {
            return IsPropertySetToTrue(project, AutoGenerateBindingRedirectsElement);
        }

        private bool IsTestProject(Project project)
        {
            return project.File.Name.ToLowerInvariant().Contains("tests");
        }

        internal override void Fix(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (!AlreadyAppliedTo(project))
            {
                if (!AppliedAutoGenerateBindingRedirects(project))
                {
                    ApplyAutoGenerateBindingRedirects(project);
                }
                if (!AppliedGenerateBindingRedirectsOutputType(project))
                {
                    ApplyGenerateBindingRedirectsOutputType(project);
                }
            }
        }

        private void ApplyGenerateBindingRedirectsOutputType(Project project)
        {
            SetElementValue(project, GenerateBindingRedirectsOutputTypeElement, AutoGenerateBindingRedirectsElement);
        }

        private void ApplyAutoGenerateBindingRedirects(Project project)
        {
            SetElementValue(project, AutoGenerateBindingRedirectsElement, GenerateBindingRedirectsOutputTypeElement);
        }

        private void SetElementValue(Project project, XName elementName, XName siblingsElementName)
        {
            XElement propertyGroupElement =
                GetElement(project, elementName)?.Parent ??
                GetElement(project, siblingsElementName)?.Parent ??
                CreateProperyGroupElement(project);
            propertyGroupElement.SetElementValue(elementName, true);
        }

        private XElement CreateProperyGroupElement(Project project)
        {
            XElement element = new XElement(Project.PropertyGroupElement);
            project.Root.Add(element);
            return element;
        }
    }
}
