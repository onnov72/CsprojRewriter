using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PackagesConfigRewriter
{
    internal class Solution
    {
        private FileInfo _solutionFile;
        private List<Project> _projects = null;

        public Solution(FileInfo solutionFile)
        {
            _solutionFile = solutionFile ?? throw new ArgumentNullException(nameof(solutionFile));
        }

        public IEnumerable<Project> Projects => GetProjects();

        private IEnumerable<Project> GetProjects()
        {
            if (_projects != null)
            {
                return _projects;
            }
            else
            {
                return GenerateProjects();
            }
        }

        private IEnumerable<Project> GenerateProjects()
        {
            _projects = new List<Project>();
            foreach (Project project in ParsedProjects())
            {
                _projects.Add(project);
                yield return project;
            }
        }

        private IEnumerable<Project> ParsedProjects()
        {
            foreach (string line in ReadLines().Where(x => x.StartsWith("Project(\"{")))
            {
                if (TryParseProjectLine(line, out Project project))
                {
                    yield return project;
                }
            }
        }

        private bool TryParseProjectLine(string line, out Project project)
        {
            project = default;
            var parts = line.Split('=');
            if (parts.Length != 2)
            {
                return false;
            }
            string rightSide = parts[1];
            parts = rightSide.Split(',');
            if (parts.Length != 3)
            {
                return false;
            }
            string middle = parts[1].Trim().Trim('"');
            FileInfo projectFile = new FileInfo(Path.Combine(_solutionFile.Directory.FullName, middle));
            if (!projectFile.Exists)
            {
                return false;
            }
            project = new Project(projectFile);
            return true;
        }

        private IEnumerable<string> ReadLines()
        {
            using (TextReader reader = _solutionFile.OpenText())
            {
                string line;
                do
                {
                    line = reader.ReadLine();
                    yield return line;
                } while (line != null);
            }
        }
    }
}