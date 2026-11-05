namespace Api.Agente.Documentacao;

/// <summary>OpenAPI e Swagger UI só em <c>Development</c> (research R-20).</summary>
public static class DocumentacaoApiExtensions
{
    public const string RotaOpenApi = "/openapi/v1.json";

    public static IServiceCollection AdicionarDocumentacaoApi(this IServiceCollection services, IHostEnvironment ambiente)
    {
        if (ambiente.IsDevelopment())
        {
            services.AddOpenApi();
        }

        return services;
    }

    public static WebApplication UsarDocumentacaoApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint(RotaOpenApi, "Api.Agente v1"));
        }

        return app;
    }
}
