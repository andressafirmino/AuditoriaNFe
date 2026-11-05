# ADR-0006 — Topologia do PostgreSQL

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-16; P11, P14; constitution 4.5; DP-22

## Contexto

A constitution exige PostgreSQL para consumo e custos e que cada serviço acesse só os próprios
dados (P11). A Api.RAG, na feature 002, também usará PostgreSQL (PgVector).

## Decisão

- Ambiente local: uma instância PostgreSQL do compose (`pgvector/pgvector:0.8.7-pg17`, porta
  `127.0.0.1:5436`).
- Api.Agente: banco `auditoria_agente`, schema `consumo`, usuário exclusivo com permissão só nesse
  schema. Tabela `consumo.chamadas_provedor` (uma linha por chamada e tentativa) e visão
  `consumo.vw_consumo_diario`.
- Api.RAG: banco e usuário próprios na feature 002.
- Migrações EF Core aplicadas na inicialização só quando `Persistencia:AplicarMigracoes = true`
  (ambiente local). O script de inicialização cria apenas usuário e schema.

## Consequências

- Os serviços compartilham a instância local, mas não credenciais nem bancos.
- A extensão `vector` fica disponível para a feature 002 sem trocar de imagem.

## Alternativas consideradas

- Uma instância por serviço já no ambiente local: mais recursos sem ganho de isolamento lógico.
- Dapper com scripts SQL: sem migrações versionadas.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-16, R-24
- `specs/001-agente-auditoria-nfe/data-model.md` — ChamadaProvedor
