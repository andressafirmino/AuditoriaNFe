# ADR-0001 — Estrutura da solução e camadas

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-19; constitution 3.1 e 3.3; P12; feature `001-agente-auditoria-nfe`

## Contexto

A constitution exige camadas Endpoints → Aplicação → Domínio, com a Infraestrutura implementando
abstrações (3.3), e deixa a forma física das camadas para o primeiro plano técnico (DP-19). A
versão 2.0.0 passou a permitir as abstrações do Microsoft Agent Framework (MAF) na Aplicação.

## Decisão

- Solução `AgenteAuditoriaNFe` (nome exigido pela constitution 3.1).
- Um projeto por camada:
  - `Api.Agente` — Endpoints: REST, servidor A2A, identificação, mapeamento de erros;
  - `AuditoriaNFe.Application` — agente, ferramentas, context providers, middleware, fluxo das nove
    etapas, casos de uso e workflow de aprovação;
  - `AuditoriaNFe.Domain` — documentos, seleção de pacote, regras, cálculos, parecer, recomendações,
    sessões e contexto do usuário, sem pacotes externos;
  - `AuditoriaNFe.Infrastructure` — OpenAI, MongoDB, PostgreSQL, XML/XSD, catálogos, MCP e checkpoints.
- Projetos auxiliares: `Sofia.Simulator` (cliente A2A de console) e `AuditoriaNFe.Tests`.
- A Aplicação referencia apenas abstrações (`Microsoft.Agents.AI`, `Microsoft.Agents.AI.Workflows`,
  `Microsoft.Extensions.AI.Abstractions`). Clientes concretos (OpenAI, MCP, MongoDB, Npgsql) ficam
  na Infraestrutura; o servidor A2A fica no `Api.Agente`.
- A Api.RAG terá projetos próprios na feature 002.

## Consequências

- As dependências entre camadas são verificadas por testes de arquitetura.
- O fluxo das nove etapas é orquestrado por código determinístico na Aplicação; o LLM não decide a
  ordem das etapas.
- Trocar de provedor de IA ou de banco afeta só a Infraestrutura.

## Alternativas consideradas

- Projeto único com pastas por camada: mais simples, mas sem garantia de dependências pelo
  compilador.
- Semantic Kernel no lugar do MAF: substituído pela constitution 2.0.0.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-02, R-27, R-28
- `specs/001-agente-auditoria-nfe/plan.md` — Project Structure
