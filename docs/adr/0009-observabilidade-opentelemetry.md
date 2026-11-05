# ADR-0009 — Observabilidade com OpenTelemetry

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-23; P16; constitution 5.1

## Contexto

O P16 exige rastreabilidade de cada execução sem expor conteúdo sensível. O MAF e o
Microsoft.Extensions.AI já emitem telemetria OpenTelemetry.

## Decisão

- OpenTelemetry para traces, métricas e logs; exportação OTLP configurável e console em
  desenvolvimento.
- `UseOpenTelemetry()` no agente e no `IChatClient`, sem registrar conteúdo das mensagens.
- Toda linha de log carrega `execucaoId` e `correlacaoId`.
- Não são registrados: XML, perguntas, respostas, contexto do usuário, `usuarioId` em claro ou dados
  pessoais. O usuário aparece só pela chave de isolamento em hash.
- Health checks `/health/live` e `/health/ready` sem dados nem detalhes internos.

## Consequências

- Diagnóstico depende de correlacionar ids, não de ler conteúdo.
- O destino da exportação (coletor, APM) fica para a decisão de implantação (DP-25).

## Alternativas consideradas

- Serilog com sinks próprios: duplicaria a telemetria que o MAF já emite em OpenTelemetry.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-21, R-22
