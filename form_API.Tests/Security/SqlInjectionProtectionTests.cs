namespace form_API.Tests.Security
{
    public class SqlInjectionProtectionTests
    {
        [Fact]
        public void ApplicationCode_ShouldNotUseRawSqlExecutionApis()
        {
            var backendPath = FindBackendPath();
            var forbiddenTokens = new[]
            {
                "FromSqlRaw",
                "ExecuteSqlRaw",
                "SqlQueryRaw",
                "FromSqlInterpolated",
                "ExecuteSqlInterpolated"
            };

            var offenders = Directory
                .GetFiles(backendPath, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"))
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}form_API.Tests{Path.DirectorySeparatorChar}"))
                .SelectMany(path =>
                {
                    var text = File.ReadAllText(path);
                    return forbiddenTokens
                        .Where(token => text.Contains(token, StringComparison.Ordinal))
                        .Select(token => $"{Path.GetRelativePath(backendPath, path)} usa {token}");
                })
                .ToArray();

            Assert.True(
                offenders.Length == 0,
                "Use LINQ/EF Core parametrizado ou APIs parametrizadas. Ocorrencias: " + string.Join("; ", offenders));
        }

        private static string FindBackendPath()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current != null)
            {
                var backend = Path.Combine(current.FullName, "Backend_API");
                if (File.Exists(Path.Combine(backend, "form_API.csproj")))
                {
                    return backend;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Nao foi possivel localizar Backend_API.");
        }
    }
}
