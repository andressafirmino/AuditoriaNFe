using AuditoriaNFe.Application.Abstracoes;
using AuditoriaNFe.Application.Contratos;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Agente.Erros;

/// <summary>
/// Armazenamento de sessões indisponível vira ERR-025 (HTTP 503); as demais exceções seguem para a
/// resposta padrão, sem detalhes internos fora de <c>Development</c>.
/// </summary>
public sealed class TratadorExcecoes : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ArmazenamentoIndisponivelException)
        {
            return false;
        }

        await MapeamentoErrosHttp.Erro(CodigoErro.ArmazenamentoIndisponivel).ExecuteAsync(httpContext);
        return true;
    }
}
