using System.Reflection;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CardActions.Architecture.Tests;

public class ProjectFileTests
{
    private readonly XDocument _directoryBuildProps;
    private readonly ITestOutputHelper _output;
    private readonly string _directoryBuildPropsPath;
    private readonly List<string> _projectFiles;

    public ProjectFileTests(ITestOutputHelper output)
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
        
        _directoryBuildPropsPath = Path.Combine(solutionDir, "Directory.Build.props");
        
        _output.WriteLine($"Assembly location: {assemblyLocation}");
        _output.WriteLine($"Solution directory: {solutionDir}");
        _output.WriteLine($"Directory.Build.props path: {_directoryBuildPropsPath}");
        _output.WriteLine($"File exists: {File.Exists(_directoryBuildPropsPath)}");
        
        if (File.Exists(_directoryBuildPropsPath))
        {
            _directoryBuildProps = XDocument.Load(_directoryBuildPropsPath);
        }
        else
        {
            throw new FileNotFoundException($"Nie znaleziono pliku Directory.Build.props pod ścieżką: {_directoryBuildPropsPath}");
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
    public void ProjectFiles_ShouldNotContainDuplicateProperties()
    {
        // Arrange
        var errors = new List<string>();
        
        // Pobierz właściwości zdefiniowane w Directory.Build.props
        var directoryBuildPropsProperties = _directoryBuildProps.Descendants()
            .Where(e => e.Parent?.Name.LocalName == "PropertyGroup")
            .Select(e => e.Name.LocalName)
            .ToList();
        
        // Act
        foreach (var projectFile in _projectFiles)
        {
            var projectDoc = XDocument.Load(projectFile);
            var projectProperties = projectDoc.Descendants()
                .Where(e => e.Parent?.Name.LocalName == "PropertyGroup")
                .Select(e => e.Name.LocalName)
                .ToList();
            
            var duplicateProperties = projectProperties.Intersect(directoryBuildPropsProperties).ToList();
            
            if (duplicateProperties.Any())
            {
                errors.Add($"Plik {Path.GetFileName(projectFile)} zawiera zduplikowane właściwości: {string.Join(", ", duplicateProperties)}");
            }
        }
        
        // Assert
        Assert.Empty(errors);
    }
    
    [Fact]
    public void ProjectFiles_ShouldNotSpecifyVersionsForPackages()
    {
        // Arrange
        var errors = new List<string>();
        
        // Act
        foreach (var projectFile in _projectFiles)
        {
            var projectDoc = XDocument.Load(projectFile);
            var packageReferences = projectDoc.Descendants()
                .Where(e => e.Name.LocalName == "PackageReference");
            
            foreach (var packageReference in packageReferences)
            {
                var versionAttribute = packageReference.Attribute("Version");
                if (versionAttribute != null)
                {
                    errors.Add($"Plik {Path.GetFileName(projectFile)} zawiera atrybut Version dla pakietu {packageReference.Attribute("Include")?.Value}");
                }
            }
        }
        
        // Assert
        Assert.Empty(errors);
    }
} 