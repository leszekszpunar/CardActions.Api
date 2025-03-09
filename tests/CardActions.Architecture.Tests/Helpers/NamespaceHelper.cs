using System.Reflection;

namespace CardActions.Architecture.Tests.Helpers;

/// <summary>
///     Klasa pomocnicza do dynamicznego pobierania przestrzeni nazw z assembly
/// </summary>
public static class NamespaceHelper
{
    private const string DomainNamespace = "CardActions.Domain";
    private const string ApplicationNamespace = "CardActions.Application";
    private const string InfrastructureNamespace = "CardActions.Infrastructure";
    private const string InfrastructureDataNamespace = "CardActions.Infrastructure.Data";
    private const string ApiNamespace = "CardActions.Api";

    /// <summary>
    ///     Pobiera przestrzeń nazw dla podanego typu
    /// </summary>
    public static string GetNamespace<T>()
    {
        return typeof(T).Namespace ?? string.Empty;
    }

    /// <summary>
    ///     Pobiera przestrzeń nazw warstwy Domain
    /// </summary>
    public static string GetDomainNamespace()
    {
        return DomainNamespace;
    }

    /// <summary>
    ///     Pobiera przestrzeń nazw warstwy Application
    /// </summary>
    public static string GetApplicationNamespace()
    {
        return ApplicationNamespace;
    }

    /// <summary>
    ///     Pobiera przestrzeń nazw warstwy Infrastructure
    /// </summary>
    public static string GetInfrastructureNamespace()
    {
        return InfrastructureNamespace;
    }

    /// <summary>
    ///     Pobiera przestrzeń nazw warstwy Infrastructure.Data
    /// </summary>
    public static string GetInfrastructureDataNamespace()
    {
        return InfrastructureDataNamespace;
    }

    /// <summary>
    ///     Pobiera przestrzeń nazw warstwy Api
    /// </summary>
    public static string GetApiNamespace()
    {
        return ApiNamespace;
    }

    /// <summary>
    ///     Pobiera nazwę assembly dla podanego typu
    /// </summary>
    public static string GetAssemblyName<T>()
    {
        return typeof(T).Assembly.GetName().Name ?? string.Empty;
    }

    /// <summary>
    ///     Pobiera assembly dla warstwy Domain
    /// </summary>
    public static Assembly GetDomainAssembly()
    {
        return Assembly.Load(DomainNamespace);
    }

    /// <summary>
    ///     Pobiera assembly dla warstwy Application
    /// </summary>
    public static Assembly GetApplicationAssembly()
    {
        return Assembly.Load(ApplicationNamespace);
    }

    /// <summary>
    ///     Pobiera assembly dla warstwy Infrastructure
    /// </summary>
    public static Assembly GetInfrastructureAssembly()
    {
        return Assembly.Load(InfrastructureNamespace);
    }

    /// <summary>
    ///     Pobiera assembly dla warstwy Infrastructure.Data
    /// </summary>
    public static Assembly GetInfrastructureDataAssembly()
    {
        return Assembly.Load(InfrastructureDataNamespace);
    }

    /// <summary>
    ///     Pobiera assembly dla warstwy Api
    /// </summary>
    public static Assembly GetApiAssembly()
    {
        return Assembly.Load(ApiNamespace);
    }
}