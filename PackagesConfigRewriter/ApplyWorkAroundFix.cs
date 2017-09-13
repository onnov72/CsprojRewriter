using System.Linq;
using System.Xml.Linq;

namespace PackagesConfigRewriter
{
    internal class ApplyWorkAroundFix : ProjectFix
    {
        private const string WorkAroundName = "RemoveNetFxForceConflicts";
        private static readonly XName TargetElement = Project.MsBuildNamespace + "Target";
        private readonly XName NameAttribute = "Name";
        private readonly XName BeforeTargetsAttribute = "BeforeTargets";
        private readonly XName ReferencePathElement = Project.MsBuildNamespace + "ReferencePath";
        private readonly XName RemoveAttribute = "Remove";
        private readonly XName ConditionAttribute;

        internal override bool AlreadyAppliedTo(Project project)
        {
            return project.IsMsBuildProject && project.Root.Elements(TargetElement).Attributes(NameAttribute).Any(x => x.Value == WorkAroundName);
        }

        internal override void Fix(Project project)
        {
            /*
              <Target Name="RemoveNetFxForceConflicts" BeforeTargets="BuiltProjectOutputGroupDependencies">
                <ItemGroup>
                  <ReferencePath Remove="@(ReferencePath)" Condition="'%(FileName)' == 'netfx.force.conflicts'" />
                </ItemGroup>
              </Target>
             */
            if (!AlreadyAppliedTo(project))
            {
                project.Add(new XElement(TargetElement,
                    new XAttribute(NameAttribute, WorkAroundName),
                    new XAttribute(BeforeTargetsAttribute, "BuiltProjectOutputGroupDependencies"),
                    new XElement(Project.ItemGroupElement,
                        new XElement(ReferencePathElement,
                            new XAttribute(RemoveAttribute, "@(ReferencePath)"),
                            new XAttribute(ConditionAttribute, "'%(FileName)' == 'netfx.force.conflicts'")))));
            }
        }
    }
}