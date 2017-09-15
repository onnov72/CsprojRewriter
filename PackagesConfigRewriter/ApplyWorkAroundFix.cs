using System.Linq;
using System.Xml.Linq;

namespace PackagesConfigRewriter
{
    internal class ApplyWorkAroundFix : ProjectFix
    {
        private const string WorkAroundName = "RemoveNetFxForceConflicts";
        private static readonly XName TargetElement = Project.MsBuildNamespace + "Target";
        private static readonly XName NameAttribute = "Name";
        private static readonly XName BeforeTargetsAttribute = "BeforeTargets";
        private static readonly XName ReferencePathElement = Project.MsBuildNamespace + "ReferencePath";
        private static readonly XName RemoveAttribute = "Remove";
        private static readonly XName ConditionAttribute = "Condition";

        internal override bool AlreadyAppliedTo(Project project)
        {
            if (project == null)
            {
                throw new System.ArgumentNullException(nameof(project));
            }

            return project.IsMsBuildProject && project.Root.Elements(TargetElement).Attributes(NameAttribute).Any(x => x.Value == WorkAroundName);
        }

        /*
        <Target Name="RemoveNetFxForceConflicts" BeforeTargets="BuiltProjectOutputGroupDependencies">
            <ItemGroup>
                 <ReferencePath Remove="@(ReferencePath)" Condition="'%(FileName)' == 'netfx.force.conflicts'" />
            </ItemGroup>
        </Target>
        */
        internal override void Fix(Project project)
        {
            if (project == null)
            {
                throw new System.ArgumentNullException(nameof(project));
            }

            if (!AlreadyAppliedTo(project))
            {
                project.Root.Add(new XElement(TargetElement,
                    new XAttribute(NameAttribute, WorkAroundName),
                    new XAttribute(BeforeTargetsAttribute, "BuiltProjectOutputGroupDependencies"),
                    new XElement(Project.ItemGroupElement,
                        new XElement(ReferencePathElement,
                            new XAttribute(RemoveAttribute, "@(ReferencePath)"),
                            new XAttribute(ConditionAttribute, "'%(FileName)' == 'netfx.force.conflicts'")))));
            }
        }

        public override string ToString()
        {
            return "fix for \"Could not load file or assembly 'netfx.force.conflicts'\"";
        }
    }
}