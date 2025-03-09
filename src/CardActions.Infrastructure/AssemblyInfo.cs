using System.Runtime.CompilerServices;

// Udostępnienie wewnętrznych elementów dla projektów testowych
[assembly: InternalsVisibleTo("CardActions.Unit.Tests")]
[assembly: InternalsVisibleTo("CardActions.Integration.Tests")]
[assembly: InternalsVisibleTo("CardActions.Architecture.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]