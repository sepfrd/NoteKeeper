using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Presentation.Transformers;

public class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    private readonly IConfiguration _configuration;

    public DocumentInfoTransformer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var baseUrl = _configuration.GetValue<string>(ConfigurationConstants.BaseUrlKey);

        document.Servers.Add(new OpenApiServer
        {
            Url = baseUrl
        });

        return Task.CompletedTask;
    }
}