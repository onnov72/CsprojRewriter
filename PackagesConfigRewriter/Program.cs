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
            DirectoryInfo solutionDirectory = new DirectoryInfo(args[0]);
            IEnumerable<Solution> solutions = CreateSolutionFiles(solutionDirectory.EnumerateFiles("*.sln"));
            foreach (Solution solution in solutions)
            {
                List<Project> msBuildProjects = new List<Project>();
                bool hasSdkProjects = false;
                bool hasWebApplicationProjects = false;
                foreach (Project project in solution.Projects)
                {
                    if (project.IsMsBuildProject)
                    {
                        msBuildProjects.Add(project);
                    }
                    else
                    {
                        hasSdkProjects = true;
                    }
                    hasWebApplicationProjects = hasWebApplicationProjects || project.IsWebApplication;
                }
                if (hasSdkProjects)
                {
                    List<ProjectFix> fixes = new List<ProjectFix>() { new UsePackageReferencesFix() };
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
                project.Save();
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
