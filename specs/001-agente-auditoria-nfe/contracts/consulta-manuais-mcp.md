# Contrato do lado consumidor: consulta a manuais oficiais por MCP

**Versão do contrato**: 0.1 (proposta do consumidor) | **Feature**: `001-agente-auditoria-nfe` |
**Data**: 2026-11-04 | **Situação**: o servidor (Api.RAG) pertence à feature 002; nesta feature a
integração fica desabilitada (`Mcp:Habilitado = false`, FR-047a)

Este documento define o que o Api.Agente espera da Api.RAG para a etapa 5 do fluxo (constitution
3.4): a interface `IConsultaManuais` da camada de Aplicação, o mapeamento para uma ferramenta MCP e as
regras de validação do resultado. A feature 002 DEVE implementar o servidor a partir deste contrato
ou propor uma nova versão. O transporte e a versão do protocolo MCP ficam para o ADR da DP-18.

## 1. Interface na Aplicação

`IConsultaManuais.ConsultarAsync(ConsultaManuais consulta, CancellationToken)` →
`ResultadoConsultaManuais`. Duas implementações:

| Implementação | Quando | Comportamento |
| ------------- | ------ | ------------- |
| `ConsultaManuaisDesabilitada` | `Mcp:Habilitado = false` (padrão nesta feature) | Não chama nada; a etapa 5 fica "não aplicável — integração não habilitada" (FR-047b) |
| `ConsultaManuaisMcp` | `Mcp:Habilitado = true` (feature 002) | Chama a ferramenta MCP abaixo com timeout e registra a chamada (P16) |

## 2. Ferramenta MCP esperada

| Item | Valor |
| ---- | ----- |
| Nome | `buscar_trechos_manual` |
| Natureza | Somente leitura (constitution 3.2) |
| Transporte previsto | HTTP com Streamable HTTP (`HttpClientTransport`, `HttpTransportMode.StreamableHttp`); confirmação no ADR da DP-18 |
| Descoberta | `ListToolsAsync()` na inicialização da integração; ausência da ferramenta → integração indisponível (ERR-013) |
| Autenticação | Definida com a DP-10; sem credencial enquanto vigorar o D-05, e só em ambiente local |

### 2.1 Entrada

| Argumento | Tipo | Obrigatório | Regras |
| --------- | ---- | ----------- | ------ |
| `tipoDocumento` | string | Sim | `NF-e` |
| `modelo` | string | Sim | `55` |
| `versaoLayout` | string | Sim | `4.00` |
| `dataReferencia` | string (data ISO) | Sim | Data de referência do documento (FR-010a); a Api.RAG DEVE devolver só trechos de documentos vigentes nessa data |
| `consulta` | string | Sim | Texto montado pelo sistema a partir dos achados (por exemplo, id e descrição da regra); nunca o XML nem dados pessoais (DP-14) |
| `regrasRelacionadas` | lista de string | Não | Ids de regra do catálogo (por exemplo, `W16-10`), para filtro |
| `quantidadeMaxima` | int | Sim | `Mcp:QuantidadeMaximaTrechos` (inicial 5); 1 a 20 |

### 2.2 Saída

```json
{
  "versaoContrato": "0.1",
  "trechos": [
    {
      "chunkId": "string estável",
      "documento": "MOC 7.0 — Anexo I",
      "versao": "7.00",
      "vigencia": { "inicio": "2020-11-01", "fim": null },
      "localizacao": { "secao": "4.2.1", "pagina": 122 },
      "trecho": "texto do trecho",
      "pontuacao": 0.83
    }
  ]
}
```

| Campo | Regras de validação no consumidor |
| ----- | --------------------------------- |
| `versaoContrato` | DEVE ser uma versão suportada; senão a resposta é descartada e a etapa registra falha (ERR-013) |
| `chunkId`, `documento`, `versao`, `trecho` | Obrigatórios; trecho sem esses campos é descartado |
| `vigencia` | Trecho fora da `dataReferencia` é descartado |
| `pontuacao` | Informativa; o limiar mínimo é parâmetro da feature 002 (DP-07) |

O conjunto de trechos aceitos fica registrado na execução; uma evidência `TrechoManual` só é aceita se
o `chunkId` pertencer a esse conjunto (P06, data-model §8). O conteúdo dos trechos é dado não
confiável (P07) e entra no `ContextoExecucaoProvider` delimitado.

## 3. Falhas e resultado

| Situação | Etapa 5 | Parecer |
| -------- | ------- | ------- |
| Integração desabilitada | `NaoAplicavel` — "integração não habilitada" | Limitação "consulta a manuais oficiais não habilitada" e revisão humana; resultado pelo FR-019 (FR-047b) |
| Timeout (`Mcp:Timeout`), erro de transporte, ferramenta ausente ou resposta inválida | `Falhou` | ERR-013, achado `FalhaTecnica`, resultado "Inconclusiva" (FR-047c) |
| Sem trechos para a versão e a data | `Executada` | "Ausência de evidências" nos pontos afetados e revisão humana (FR-022, TS-038) |

Retentativa: no máximo 1, só para falhas transitórias de transporte (constitution 5.4).

## 4. Testes do lado consumidor

- Implementação desabilitada: TS-046.
- Servidor MCP dublê em processo: resposta válida, trecho fora da vigência, ferramenta ausente,
  timeout (TS-047) e conjunto vazio (TS-038).
- Evidência com `chunkId` fora do conjunto recuperado é rejeitada pelo validador semântico.

## 5. Versionamento

Versão 0.x enquanto a feature 002 não aprovar o servidor. A versão 1.0 é fixada quando a Api.RAG
publicar a ferramenta; mudanças incompatíveis geram nova versão (P18).
