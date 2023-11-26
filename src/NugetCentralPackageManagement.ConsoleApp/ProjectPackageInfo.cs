namespace NugetCentralPackageManagement.ConsoleApp
{
    internal class ProjectPackageInfo
    {
        public ProjectPackageInfo(string projectFilePath, IEnumerable<NuGetPackage> listOfPackages)
        {
            ProjectFilePath = projectFilePath;
            Packages = listOfPackages;
        }

        public string ProjectFilePath { get; }
        public IEnumerable<NuGetPackage> Packages { get; }
    }
}
