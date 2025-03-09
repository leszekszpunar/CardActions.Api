using System.Text.RegularExpressions;

namespace CardActions.Api.Extensions;

/// <summary>
///     Transformer parametrów routingu na format kebab-case
/// </summary>
public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    /// <summary>
    ///     Transformuje wartość parametru na format kebab-case
    /// </summary>
    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;

        // Konwertuj PascalCase/camelCase na kebab-case
        return Regex.Replace(
                value.ToString()!,
                "([a-z])([A-Z])",
                "$1-$2",
                RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(100))
            .ToLowerInvariant();
    }
}