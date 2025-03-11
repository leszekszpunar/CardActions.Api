using System;

namespace CardActions.Application.Features.Version.Queries.GetVersionInfo;

/// <summary>
/// DTO zawierające informacje o wersji aplikacji
/// </summary>
public class VersionInfoDto
{
    /// <summary>
    /// Numer wersji semantycznej (np. 1.2.3)
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// Wersja pliku
    /// </summary>
    public string FileVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Wersja assembly
    /// </summary>
    public string AssemblyVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Pełna wersja z hashiem commita
    /// </summary>
    public string FullVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash commita
    /// </summary>
    public string CommitHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Data zbudowania aplikacji
    /// </summary>
    public string BuildDate { get; set; } = string.Empty;
    
    /// <summary>
    /// Kanał wydania (production/development)
    /// </summary>
    public string ReleaseChannel { get; set; } = string.Empty;
    
    /// <summary>
    /// Nazwa produktu
    /// </summary>
    public string Product { get; set; } = string.Empty;
    
    /// <summary>
    /// Opis produktu
    /// </summary>
    public string Description { get; set; } = string.Empty;
} 