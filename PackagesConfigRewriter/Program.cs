using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PackagesConfigRewriter
{
    public static class Program
    {
        static void Main(string[] args)
        {
            bool forceFix = true;
            DirectoryInfo solutionDirectory = new DirectoryInfo(args[0]);
            IEnumerable<Solution> solutions = CreateSolutionFiles(solutionDirectory.EnumerateFiles("*.sln"));
            foreach (Solution solution in solutions)
            {
                Console.WriteLine($"Inspecting solution {solution.Name}");
                List<Project> msBuildProjects = new List<Project>();
                bool usesPackageReferences = false;
                bool hasWebApplicationProjects = false;
                foreach (Project project in solution.Projects)
                {
                    if (project.IsMsBuildProject)
                    {
                        msBuildProjects.Add(project);
                        usesPackageReferences = usesPackageReferences || UsePackageReferencesFix.UsesPackageReference(project);
                    }
                    else
                    {
                        usesPackageReferences = true;
                    }
                    hasWebApplicationProjects = hasWebApplicationProjects || project.IsWebApplication;
                }
                if (forceFix || usesPackageReferences)
                {
                    List<ProjectFix> fixes = new List<ProjectFix>()
                    {
                        new UsePackageReferencesFix(),
                        new AddAutoGenerateBindingRedirectsFix()
                    };
                    if (hasWebApplicationProjects)
                    {
                        fixes.Add(new ApplyWorkAroundFix());
                    }
                    foreach (Project msBuildProject in msBuildProjects)
                    {
                        foreach (ProjectFix fix in fixes)
                        {
                            msBuildProject.Apply(fix);
                        }
                    }
                }
            }
        }

        internal static void Apply(this Project project, ProjectFix fix)
        {
            if (!fix.AlreadyAppliedTo(project))
            {
                fix.Fix(project);
                if(project.Save())
                {
                    Console.WriteLine($"Applied {fix} to project {project.File.Name}");
                }
            }
        }

        private static IEnumerable<Solution> CreateSolutionFiles(IEnumerable<FileInfo> solutionFiles)
        {
            foreach (FileInfo solutionFile in solutionFiles)
            {
                yield return new Solution(solutionFile);
            }
        }
    }
}
