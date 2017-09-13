namespace PackagesConfigRewriter
{
    internal abstract class ProjectFix
    {
        internal abstract void Fix(Project project);

        internal abstract bool AlreadyAppliedTo(Project project);
    }
}