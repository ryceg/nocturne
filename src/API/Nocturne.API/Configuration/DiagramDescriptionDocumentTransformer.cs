using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nocturne.API.Configuration;

/// <summary>
/// Builds the OpenAPI info.description from the diagram manifest, embedding
/// SVG image references so Scalar displays architectural diagrams at the top.
/// </summary>
public sealed class DiagramDescriptionDocumentTransformer : IOpenApiDocumentTransformer
{
    private readonly string _description;

    public DiagramDescriptionDocumentTransformer(IWebHostEnvironment env)
    {
        _description = BuildDescription(env);
    }

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info.Description = _description;
        return Task.CompletedTask;
    }

    private static string BuildDescription(IWebHostEnvironment env)
    {
        // Prefer the copy under wwwroot (shipped in the container alongside the SVGs).
        // Fall back to the repo source path for local runs where wwwroot isn't populated yet.
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var manifestPath = Path.Combine(webRoot, "diagrams", "diagrams.yaml");
        if (!File.Exists(manifestPath))
        {
            manifestPath = Path.Combine(env.ContentRootPath, "..", "..", "..", "docs", "diagrams", "diagrams.yaml");
        }

        if (!File.Exists(manifestPath))
        {
            return "Modern diabetes management API. For support, join our Discord.";
        }

        var yaml = File.ReadAllText(manifestPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var manifest = deserializer.Deserialize<DiagramManifest>(yaml);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Modern diabetes management API. For support, join our Discord.");
        sb.AppendLine();
        sb.AppendLine("## Architecture");
        sb.AppendLine();

        foreach (var diagram in manifest.Diagrams)
        {
            sb.AppendLine($"### {diagram.Title}");
            if (!string.IsNullOrWhiteSpace(diagram.Description))
            {
                sb.AppendLine(diagram.Description);
            }
            sb.AppendLine();

            var svgName = Path.GetFileNameWithoutExtension(diagram.Source) + ".svg";
            var svgPath = $"/diagrams/{svgName}";
            sb.AppendLine($"[![{diagram.Title}]({svgPath})]({svgPath})");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private sealed class DiagramManifest
    {
        public List<DiagramEntry> Diagrams { get; set; } = [];
    }

    private sealed class DiagramEntry
    {
        public string Source { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Auto { get; set; }
        public string? Module { get; set; }
        public List<string>? Tags { get; set; }
    }
}
