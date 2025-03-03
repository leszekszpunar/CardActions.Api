using System.Reflection;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CardActions.Architecture.Tests;

public class PackageVersionTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _directoryPackagesPropsPath;
    private readonly XDocument _directoryPackagesProps;
    private readonly List<string> _projectFiles;

    public PackageVersionTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Find solution directory by looking for CardActions.sln
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var currentDir = Path.GetDirectoryName(assemblyLocation)!;
        var solutionDir = FindSolutionDirectory(currentDir);
        
        if (solutionDir == null)
        {
            throw new DirectoryNotFoundException("Could not find solution directory containing CardActions.sln");
        }
        
        _directoryPackagesPropsPath = Path.Combine(solutionDir, "Directory.Packages.props");
        
        _output.WriteLine($"Assembly location: {assemblyLocation}");
        _output.WriteLine($"Solution directory: {solutionDir}");
        _output.WriteLine($"Directory.Packages.props path: {_directoryPackagesPropsPath}");
        _output.WriteLine($"File exists: {File.Exists(_directoryPackagesPropsPath)}");
        
        if (File.Exists(_directoryPackagesPropsPath))
        {
            _directoryPackagesProps = XDocument.Load(_directoryPackagesPropsPath);
        }
        else
        {
            throw new FileNotFoundException($"Nie znaleziono pliku Directory.Packages.props pod ścieżką: {_directoryPackagesPropsPath}");
        }
        
        // Find all project files
        _projectFiles = Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories)
            .Where(file => !file.Contains("tests"))
            .ToList();
    }

    private string? FindSolutionDirectory(string startPath)
    {
        var currentDir = startPath;
        while (currentDir != null)
        {
            if (File.Exists(Path.Combine(currentDir, "CardActions.sln")))
            {
                return currentDir;
            }
            currentDir = Path.GetDirectoryName(currentDir);
        }
        return null;
    }

    [Fact]
    public void CentralPackageManagement_ShouldBeEnabled()
    {
        // Act
        var managePackageVersionsCentrally = _directoryPackagesProps.Root
            ?.Element("PropertyGroup")
            ?.Element("ManagePackageVersionsCentrally");

        // Assert
        Assert.NotNull(managePackageVersionsCentrally);
        Assert.Equal("true", managePackageVersionsCentrally.Value);
    }

    [Fact]
    public void ProjectFiles_ShouldNotSpecifyPackageVersions()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        foreach (var projectFile in _projectFiles)
        {
            var projectDoc = XDocument.Load(projectFile);
            var packageReferences = projectDoc.Root!.Descendants()
                .Where(e => e.Name.LocalName == "PackageReference");

            foreach (var packageRef in packageReferences)
            {
                var versionAttribute = packageRef.Attribute("Version");
                if (versionAttribute != null && !string.IsNullOrEmpty(versionAttribute.Value) && 
                    !versionAttribute.Value.StartsWith("$(") && !versionAttribute.Value.EndsWith(")"))
                {
                    errors.Add($"Project {Path.GetFileName(projectFile)} specifies version {versionAttribute.Value} for package {packageRef.Attribute("Include")?.Value}");
                }
            }
        }

        // Wypisz wersje pakietów w konsoli
        if (errors.Count > 0)
        {
            _output.WriteLine("Znaleziono pakiety z określonymi wersjami:");
            foreach (var error in errors)
            {
                _output.WriteLine(error);
            }
        }

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void AllPackages_ShouldHaveVersionsSpecified()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var packageVersions = _directoryPackagesProps.Root
            ?.Element("ItemGroup")
            ?.Elements("PackageVersion")
            .ToList();

        if (packageVersions == null || !packageVersions.Any())
        {
            errors.Add("Nie znaleziono żadnych pakietów w Directory.Packages.props");
        }
        else
        {
            foreach (var package in packageVersions)
            {
                var packageName = package.Attribute("Include")?.Value;
                var packageVersion = package.Attribute("Version")?.Value;

                if (string.IsNullOrEmpty(packageVersion))
                {
                    errors.Add($"Pakiet {packageName} nie ma określonej wersji");
                }
            }
        }

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void NetArchTest_ShouldBeSpecifiedInCentralPackageManagement()
    {
        // Act
        var netArchTestPackage = _directoryPackagesProps.Descendants()
            .Where(e => e.Name.LocalName == "PackageVersion")
            .FirstOrDefault(e => e.Attribute("Include")?.Value == "NetArchTest.Rules");

        // Assert
        Assert.NotNull(netArchTestPackage);
        Assert.NotNull(netArchTestPackage.Attribute("Version"));
    }
} 