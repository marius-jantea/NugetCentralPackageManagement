// See https://aka.ms/new-console-template for more information

using NugetCentralPackageManagement.ConsoleApp;
using System.Text.RegularExpressions;

var baseFolder = "{folder path}";

var allCsProjFilesFromFolder = GetAllFilesWithExtensionFromFolder("csproj", baseFolder);

var allProjectPackageInfo = new List<ProjectPackageInfo>();

foreach (var csProjFilePath in allCsProjFilesFromFolder)
{
    var allPackagesFromCsProjFile = GetPackagesFromCsproj(csProjFilePath);
    allProjectPackageInfo.Add(new ProjectPackageInfo(csProjFilePath, allPackagesFromCsProjFile));
}

Dictionary<string, Version> allPackagesForCentralPackageManagement = new Dictionary<string, Version>();

foreach (var projectPackageInfo in allProjectPackageInfo)
{
    foreach (var package in projectPackageInfo.Packages)
    {
        if (allPackagesForCentralPackageManagement.ContainsKey(package.Name))
        {
            if (allPackagesForCentralPackageManagement[package.Name] < package.Version)
            {
                allPackagesForCentralPackageManagement[package.Name] = package.Version;
            }
        }
        else
        {
            allPackagesForCentralPackageManagement.Add(package.Name, package.Version);
        }
    }
}

CreatePropsFile(allPackagesForCentralPackageManagement, $"{baseFolder}/Directory.Packages.props");

foreach(var csprojFile in allCsProjFilesFromFolder)
{
    RemovePackageVersions(csprojFile);
}

IEnumerable<string> GetAllFilesWithExtensionFromFolder(string extension, string folderPath)
{
    try
    {
        // Check if the directory exists
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"Directory not found: {folderPath}");
            return new List<string>();
        }

        // Get all .csproj files in the directory
        var csprojFiles = Directory.GetFiles(folderPath, $"*.{extension}", SearchOption.AllDirectories);

        // Convert array to List for easier manipulation
        return csprojFiles.ToList();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        return new List<string>();
    }
}

static List<NuGetPackage> GetPackagesFromCsproj(string csprojFilePath)
{
    List<NuGetPackage> packages = new List<NuGetPackage>();

    try
    {
        // Read the content of the .csproj file
        string csprojContent = File.ReadAllText(csprojFilePath);

        // Use regular expressions to find package references
        var packageMatches = Regex.Matches(csprojContent, @"<PackageReference Include=""(.*?)"" Version=""(.*?)""");

        foreach (Match match in packageMatches)
        {
            if (match.Success && match.Groups.Count > 2)
            {
                // Extract the package name and version from the match
                string packageName = match.Groups[1].Value;
                Version packageVersion = new Version(match.Groups[2].Value);

                packages.Add(new NuGetPackage { Name = packageName, Version = packageVersion });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while processing {csprojFilePath}: {ex.Message}");
    }

    return packages;
}

static void CreatePropsFile(Dictionary<string, Version> packageVersions, string filePath)
{
    try
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("<Project>");
            writer.WriteLine("  <PropertyGroup>");

            foreach (var package in packageVersions)
            {
                writer.WriteLine($"    <{package.Key}Version>{package.Value}</{package.Key}Version>");
            }

            writer.WriteLine("  </PropertyGroup>");
            writer.WriteLine("</Project>");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating the props file: {ex.Message}");
    }
}

static void RemovePackageVersions(string csprojFilePath)
{
    try
    {
        // Read the content of the .csproj file
        string csprojContent = File.ReadAllText(csprojFilePath);

        // Use regular expressions to find and remove package versions
        csprojContent = Regex.Replace(csprojContent, @"<PackageReference Include=""(.*?)"" Version=""(.*?)""", @"<PackageReference Include=""$1""");

        // Write the modified content back to the file
        File.WriteAllText(csprojFilePath, csprojContent);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while processing {csprojFilePath}: {ex.Message}");
    }
}