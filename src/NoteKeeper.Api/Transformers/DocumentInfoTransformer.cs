using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace NoteKeeper.Api.Transformers;

public class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    private readonly AppOptions _appOptions;

    public DocumentInfoTransformer(IOptions<AppOptions> appOptions)
    {
        _appOptions = appOptions.Value;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Servers.Add(new OpenApiServer
        {
            Url = _appOptions.BaseApiUrl
        });

        return Task.CompletedTask;
    }
}