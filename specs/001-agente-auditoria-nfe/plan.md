# Implementation Plan: Agente de Auditoria de NF-e (AuditoriaNFe)

**Branch**: `001-agente-auditoria-nfe` (PR para a `dev`; fluxo `feature → dev → main`, ADR-0008) | **Date**: 2026-10-29, revisado em 2026-11-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-agente-auditoria-nfe/spec.md` (esclarecida em
2026-10-29 e 2026-11-04) e constitution 2.1.0

**Artefatos desta etapa**: [research.md](./research.md), [data-model.md](./data-model.md),
[contracts/api-agente.openapi.yaml](./contracts/api-agente.openapi.yaml),
[contracts/sofia-a2a.md](./contracts/sofia-a2a.md),
[contracts/contrato-ia-v1.schema.json](./contracts/contrato-ia-v1.schema.json),
[contracts/consulta-manuais-mcp.md](./contracts/consulta-manuais-mcp.md),
[contracts/ferramentas-agente.md](./contracts/ferramentas-agente.md),
[quickstart.md](./quickstart.md). Anexos da spec usados pelo plano:
[vigencia-pacotes-schema.md](./vigencia-pacotes-schema.md) e
[catalogo-regras-totalizadores.md](./catalogo-regras-totalizadores.md).

## Summary

O AgenteAuditoriaNFe recebe um ou mais XMLs de NF-e (modelo 55, layout 4.00) e perguntas em
linguagem natural, pela API REST (canal direto) ou por A2A, e devolve o que pode estar incorreto.
A versão 1 valida, de forma determinística:

1. a **estrutura**, contra o pacote de schema oficial vigente na data de referência de cada
  documento (`dhRecbto` quando há protocolo, senão `dhEmi`), escolhido entre 19 pacotes oficiais
   versionados no repositório com manifesto e SHA-256;
2. os **totalizadores**, pelo catálogo de regras aprovado em 2026-11-04 (totais iguais à soma dos
  itens de ICMS, ISSQN, IBS, CBS e IS, e fechamento de cobrança e pagamento), com as versões de
   regra vigentes na mesma data.

A IA (OpenAI) entra só depois, para indícios, respostas e recomendações demonstrativas sob contrato
estruturado e validado. O agente mantém memória conversacional por sessão e um contexto próprio de
cada usuário (CNPJs auditados, achados recorrentes e preferências declaradas) no MongoDB, registra
tokens e custos de cada chamada no PostgreSQL e roda em Docker.

A versão 1 não tem autenticação (desvio temporário D-05): o chamador informa o usuário e a própria
identificação, sem verificação, e o agente só roda em ambiente local de testes. O contrato com a
SOFIA é uma proposta v1 provisória (DP-27); por isso o núcleo é implementado e testado primeiro pelo
canal direto, e o canal A2A e o Sofia.Simulator vêm por último.

Abordagem técnica: .NET 10 e ASP.NET Core 10; **Microsoft Agent Framework 1.26.0** (MAF, exigido
pela constitution 2.0.0) para o agente (`ChatClientAgent` sobre `IChatClient` OpenAI), ferramentas
por Function Calling, context providers, middleware, sessões e workflow de aprovação humana com
checkpoints JSON locais; hosting A2A do MAF com o SDK oficial A2A 1.0; MongoDB para sessões,
resultados e contexto do usuário; PostgreSQL para consumo e custos; ponto de integração MCP
preparado e desabilitado (a Api.RAG chega na feature 002). O fluxo das nove etapas da constitution é
orquestrado por código determinístico; o LLM nunca decide a ordem das etapas nem o resultado geral.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (LTS); ASP.NET Core 10 (Minimal APIs). O `global.json` fixa o
SDK 10 e o runner `Microsoft.Testing.Platform`. A máquina de desenvolvimento tem o SDK 10.0.403
desde 2026-11-04, ao lado dos SDKs 9.0 e 11.0 RC; por isso o `global.json` é obrigatório.

**Primary Dependencies** (versões fixadas em `Directory.Packages.props`; detalhes em
[research.md §1](./research.md#1-pacotes-oficiais-e-versões-fixadas)):

- `Microsoft.Agents.AI` 1.26.0, `Microsoft.Agents.AI.OpenAI` 1.26.0, `Microsoft.Agents.AI.Workflows`
1.26.0
- `Microsoft.Agents.AI.Hosting.A2A.AspNetCore` 1.26.0-preview.261023.1 (inclui `A2A` e
`A2A.AspNetCore` 1.0.0-preview3), `Microsoft.Agents.AI.Hosting.AspNetCore`
1.26.0-preview.261023.1, `Microsoft.Agents.AI.A2A` 1.26.0-preview.261023.1 (cliente)
- `Microsoft.Extensions.AI.OpenAI` 10.11.0 (SDK `OpenAI` ≥ 2.14.0)
- `ModelContextProtocol.Core` 2.3.0
- `MongoDB.Driver` 3.13.0; `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3
- `Microsoft.AspNetCore.OpenApi` 10.0.14; `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1;
`OpenTelemetry.Extensions.Hosting` 1.20.1
- Sem pacote de autenticação na v1 (D-05)

**Storage**: MongoDB (sessões, sessões serializadas do MAF, execuções, pareceres, recomendações,
dados extraídos e contexto do usuário — sem XML original); PostgreSQL (schema `consumo`); arquivos
JSON locais (checkpoints de aprovação, apenas demonstração); arquivos versionados no repositório
(19 pacotes de schemas XSD oficiais com `manifesto.json`, catálogo de regras, catálogo de extração,
tabela de preços, instruções do agente).

**Testing**: xUnit v3 4.0.2 (Microsoft.Testing.Platform); `Microsoft.AspNetCore.Mvc.Testing` 10.0.14
(`WebApplicationFactory`); Testcontainers 4.16.0 (MongoDB e PostgreSQL reais e descartáveis);
`IChatClient` simulado; XMLs sintéticos, um por período de vigência; conjunto de avaliação com OpenAI
real sob demanda.

**Target Platform**: contêiner Linux (`mcr.microsoft.com/dotnet/aspnet:10.0.14`, usuário não root);
ambiente local por Docker Compose, com portas só em `127.0.0.1` (D-05). Implantação fora desta
feature (DP-25).

**Project Type**: serviço web (API REST + servidor A2A) com cliente de console (Sofia.Simulator).

**Performance Goals**: parecer de lote no limite (5 MB, 20 arquivos) em até 2 min p95 (NFR-001);
rejeição por limite em até 2 s (NFR-002); continuidade em até 30 s p95 (NFR-003); 10 análises
simultâneas (NFR-004). Orçamento por execução: etapas 1–4 até 10 s (inclui seleção de pacote e XSD
com conjuntos compilados em cache); etapas 7–8 (IA, incluindo retentativas) até o restante do tempo
máximo de 120 s.

**Constraints**: validações 100% determinísticas (NFR-005); XML sem DTD nem entidades externas;
schemas resolvidos só dentro da pasta do pacote; nenhum valor financeiro não rastreável
(FR-027/FR-028); metadados pelo sistema (P09); contexto do usuário gravado só pelo sistema (FR-057);
sem segredos no repositório, nas imagens ou nos logs; respostas e mensagens em pt-BR; uma instância
do Api.Agente enquanto os checkpoints forem arquivos locais; execução só local enquanto vigorar o
D-05.

**Scale/Scope**: uma API (Api.Agente) com 3 projetos de camada, 1 cliente de console e 1 projeto de
testes; 3 habilidades A2A; 11 rotas REST de negócio; 19 pacotes de schema; cerca de 80 regras de
totalizador (com versões); mais de 70 requisitos funcionais e 64 cenários de teste na spec.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Avaliação contra a constitution **2.1.0** (2026-11-04). Legenda: **Conforme**; **Desvio D-0x**
(registrado e aprovado em Complexity Tracking); **Bloqueio** (impede as tarefas indicadas até a
resolução); **N/A** (fora desta feature).

### Princípios


| Princípio                       | Avaliação                                                                                                                                                                                                                                                                                                                                     | Situação                           |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------- |
| P01 Spec antes do código        | Spec esclarecida em três rodadas; plano sem código de aplicação. Os XSDs oficiais e o manifesto já estão no repositório como dados, não como código                                                                                                                                                                                           | Conforme                           |
| P02 Fontes oficiais             | 19 pacotes do Portal Nacional da NF-e com URL e SHA-256 no manifesto (DP-03); catálogo de regras com documento, versão e página do Anexo I e das NTs, aprovado em 2026-11-04 (DP-04); a carga rejeita regra sem fonte; fontes de terceiros (NFePHP, ACBr) não são usadas                                                                      | Conforme                           |
| P03 Determinístico prevalece    | Etapas 1–4 (inclui seleção de pacote e totalizadores) e resultado geral em código; IA recebe resultados como dados e não os altera                                                                                                                                                                                                            | Conforme                           |
| P04 Cinco categorias            | Enum fechado no Domínio; IA só produz `IndicioIa`                                                                                                                                                                                                                                                                                             | Conforme                           |
| P05 Sem conformidade incompleta | Nove etapas com estado; regra do resultado geral em [data-model §10](./data-model.md#10-parecer-m-dentro-da-execução-contrato-v1); ERR-023 (sem pacote para a data) impede "Sem inconsistências". Etapa 5 com MCP desabilitado = "não aplicável (com justificativa)", estado previsto no P05: entendimento da spec **confirmado**, sem desvio | Conforme                           |
| P06 Fundamentação rastreável    | Evidências verificadas contra dados extraídos, regras do catálogo com versão, registro de cálculos e conjunto recuperado; parecer informa pacote e data de referência                                                                                                                                                                         | Conforme (RAG real na feature 002) |
| P07 Conteúdo não confiável      | XML, pergunta, contexto do usuário e resultados de ferramentas entram como dados delimitados; testes TS-018 e TS-055                                                                                                                                                                                                                          | Conforme                           |
| P08 Saída estruturada           | `RunAsync<ContratoIaV1>`; schema publicado em `contracts/contrato-ia-v1.schema.json`; validação estrutural e semântica; até 3 tentativas                                                                                                                                                                                                      | Conforme                           |
| P09 Metadados do sistema        | Ids GUID v7, datas, consumo, estado e identificação preenchidos pelo sistema                                                                                                                                                                                                                                                                  | Conforme                           |
| P10 Ferramentas governadas      | Lista permitida e validação no middleware de Function Calling; ferramentas só de leitura; nenhuma ferramenta acessa o contexto do usuário                                                                                                                                                                                                     | Conforme                           |
| P11 Dados por serviço           | Api.Agente só acessa seus bancos; manuais apenas por MCP (feature 002)                                                                                                                                                                                                                                                                        | Conforme                           |
| P12 Camadas e simplicidade      | Projetos por camada; abstrações do MAF na Aplicação, permitidas pela 3.3; Workflow só na aprovação (D-03)                                                                                                                                                                                                                                     | Conforme (D-03 registrado)         |
| P13 Configuração externa        | Parâmetros em configuração (tabela em Design); modelo escolhido por avaliação                                                                                                                                                                                                                                                                 | Conforme                           |
| P14 Consumo fiel                | Middleware de `IChatClient` registra cada chamada e tentativa; NULL quando desconhecido                                                                                                                                                                                                                                                       | Conforme                           |
| P15 Memória e contexto isolados | Sessão e contexto do usuário isolados pela chave de `chamadorId` + `usuarioId`; contexto separado do histórico; retenção de 90 dias por TTL; limite de itens enviados ao modelo; MongoDB registrado no ADR-0002. O isolamento usa identificação **informada e não verificada**                                                                    | Conforme, com o risco do D-05      |
| P16 Observabilidade             | OpenTelemetry; `execucaoId` e `correlacaoId` em todos os logs; sem conteúdo sensível nem `usuarioId` em claro                                                                                                                                                                                                                                 | Conforme                           |
| P17 Testes e avaliação          | Cenários TS automatizados; Testcontainers; um XML sintético por período de vigência; conjunto de avaliação                                                                                                                                                                                                                                    | Conforme                           |
| P18 Contratos A2A/MCP           | Contrato A2A versionado (proposta provisória, DP-27); contrato MCP do lado consumidor; mesmo caso de uso nos dois canais. A regra "mesma autenticação nos dois canais" é cumprida com ausência de autenticação em ambos (D-05)                                                                                                                | Conforme (ver D-04 e D-05)         |




### Seções 3 a 5


| Seção                             | Avaliação                                                                                                                                                                                                               | Situação                                                                        |
| --------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| 3.1 Plataforma                    | .NET 10, ASP.NET Core, OpenAI, MAF, PostgreSQL para custos, MongoDB para memória e contexto do usuário                                                                                                                  | Conforme                                                                        |
| 3.1 Nome da solução               | `AgenteAuditoriaNFe` (research R-27)                                                                                                                                                                                    | Conforme                                                                        |
| 3.2 Serviços                      | Api.Agente com todas as responsabilidades exclusivas, inclusive o contexto do usuário; Api.RAG na feature 002                                                                                                           | Conforme (Api.RAG N/A)                                                          |
| 3.3 Camadas                       | Endpoints → Aplicação → Domínio; Infraestrutura implementa abstrações. Abstrações do MAF e do Microsoft.Extensions.AI na Aplicação; OpenAI, MCP, A2A, MongoDB e Npgsql fora dela                                        | Conforme                                                                        |
| 3.4 Fluxo de validação            | Nove etapas em ordem, com estado; etapa 3 inclui identificação (FR-007a); etapa 4 inclui seleção do pacote e totalizadores; falha em 1–2 interrompe; 3–6 seguem DP-24 da spec                                           | Conforme                                                                        |
| 3.5 Repositório                   | GitHub; fluxo `feature → dev → main`; `main` e `dev` protegidas (PR obrigatório, sem push direto nem force push); check `build-test` exigido quando o CI existir (research R-25, ADR-0008)                             | Conforme (check de CI pendente até a primeira tarefa)                           |
| 3.5 Contêineres                   | Dockerfile multiestágio, não root, sem segredos; compose de um comando com portas próprias                                                                                                                              | Conforme                                                                        |
| 3.5 Implantação                   | Sem implantação nesta feature; DP-25 pendente; proibida enquanto vigorar o D-05                                                                                                                                         | N/A                                                                             |
| 4.1–4.3 Base de conhecimento      | Feature 002; contrato do consumidor preparado                                                                                                                                                                           | N/A                                                                             |
| 4.4 Contratos do LLM e envelope   | `ContratoIaV1` e envelope v1 versionados; OpenAPI de referência; erros padronizados                                                                                                                                     | Conforme                                                                        |
| 4.5 Consumo                       | Uma linha por chamada e tentativa; embeddings e cache previstos; NULL quando desconhecido                                                                                                                               | Conforme                                                                        |
| 4.6 Contrato de ferramentas       | Nome, contrato, validação, autorização pelo sistema, timeout, registro e testes por ferramenta, documentados e versionados em `contracts/ferramentas-agente.md`                                                         | Conforme                                                                        |
| 4.7 Memória e contexto do usuário | Histórico, contexto e conhecimento separados; contexto só com os itens do FR-055a e origem; gravado pelo sistema; enviado como dado não confiável; consulta, correção e exclusão pelo canal direto; retenção de 90 dias | Conforme                                                                        |
| 5.1 Proteção de dados             | Classificação de campos no data-model; XML descartado; CPF nunca no contexto; minimização e mascaramento ao LLM pelo ADR-0004 (DP-14)                                                                                   | Conforme                                                                        |
| 5.2 Segurança de entrada          | XML seguro; limites antes do processamento; limitação de taxa e concorrência; Swagger só em `Development`. **Sem autenticação do chamador**                                                                             | **Desvio D-05**                                                                 |
| 5.3 Qualidade                     | Nullable, avisos como erro, CI. **PR aprovado pela própria autora**, única mantenedora (DP-20)                                                                                                                         | **Desvio D-06**                                                                 |
| 5.4 Resiliência                   | Timeouts explícitos; retentativas limitadas; falha explícita                                                                                                                                                            | Conforme                                                                        |




### Definition of Done (itens afetados)


| Item da seção 7                                                              | Situação                                                  |
| ---------------------------------------------------------------------------- | --------------------------------------------------------- |
| "Nenhum endpoint não público aceita solicitações sem credencial do chamador" | Não atendido na v1 — coberto pelo D-05                    |
| "O CI está verde, e a revisão foi aprovada por pessoa diferente do autor"    | CI exigido; revisão pela própria autora — coberto pelo D-06 |
| "Os desvios foram registrados conforme a seção 8.3"                          | D-03 a D-06 registrados e aprovados por Andressa Firmino em 2026-11-04 |
| Demais itens                                                                 | Atendidos pelo desenho; verificação na implementação      |




### Decisões pendentes da constitution que afetam a feature


| DP                                 | Situação após o plano                                                                                                                                                          |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| DP-01 Memória                      | Resolvida na constitution 2.0.0 (MongoDB); registrada no ADR-0002 (exigido pelo P15)                                                                                           |
| DP-02 Escopo documental            | Resolvida pela spec: NF-e modelo 55, layout 4.00                                                                                                                               |
| DP-03 Schemas oficiais             | **Resolvida nesta feature**: 19 pacotes versionados com manifesto, seleção pela data de referência (research R-14). Manuais e NTs a ingerir ficam para a Api.RAG (feature 002) |
| DP-04 Regras de conteúdo           | **Resolvida nesta feature**: catálogo de totalizadores aprovado em 2026-11-04 (research R-15)                                                                                  |
| DP-05 Modelo OpenAI                | Resolvida como parâmetro obrigatório escolhido por avaliação                                                                                                                   |
| DP-06, DP-07, DP-15                | N/A (feature 002)                                                                                                                                                              |
| DP-08, DP-09, DP-12, DP-21, DP-24  | Resolvidas pela spec e detalhadas no data-model e nos contratos                                                                                                                |
| DP-10 Autenticação                 | **Adiada pelo desvio D-05**; ADR-0003 registra a ausência temporária e o modelo futuro. Bloqueia qualquer execução fora da máquina local                                       |
| DP-11 Ferramentas externas         | Resolvida: nenhuma na v1                                                                                                                                                       |
| DP-13 Retenção                     | Parcial: sessão (24 h) e contexto do usuário (90 dias) definidos pela spec; demais prazos no ADR-0005                                                                          |
| DP-14 Minimização                  | Resolvida: ADR-0004 (a primeira chamada ao LLM com dados de documento DEVE seguir a política do ADR)                                                                           |
| DP-16 Topologia PostgreSQL         | Resolvida (ADR-0006)                                                                                                                                                           |
| DP-17 A2A                          | Resolvida para esta feature: A2A 1.0 pelo SDK fixado (ADR-0007)                                                                                                                |
| DP-18 MCP                          | Adiada para a feature 002; contrato do consumidor em `contracts/consulta-manuais-mcp.md`                                                                                       |
| DP-19 Estrutura                    | Resolvida: projetos separados (ADR-0001)                                                                                                                                       |
| DP-20 Aprovadores e local dos ADRs | Resolvida na constitution 2.1.0: Andressa Firmino aprova constitution, desvios, ADRs e PRs enquanto for a única mantenedora; ADRs em `docs/adr/`                               |
| DP-22 Tabela de preços             | Resolvida: arquivo versionado da fonte oficial                                                                                                                                 |
| DP-23 Observabilidade              | Resolvida: OpenTelemetry (ADR-0009)                                                                                                                                            |
| DP-25 Implantação                  | Pendente, fora desta feature (Vercel avaliada — research R-26)                                                                                                                 |
| DP-26 CI e proteção                | Resolvida: GitHub Actions, branches `dev` e `main` protegidas (ADR-0008)                                                                                                       |
| DP-27 Contrato com a SOFIA         | **Pendente** — contrato A2A é proposta v1 provisória; o canal A2A fica no fim das tarefas (research R-32)                                                                      |


**Resultado do gate (pré-design)**: **aprovado**. Não há violação sem justificativa: os antigos
desvios D-01 (Semantic Kernel) e D-02 (MAF na Aplicação) deixaram de existir com a emenda 2.0.0; os
desvios D-03, D-04, D-05 e D-06 e os ADR-0001 a ADR-0010 foram aprovados por Andressa Firmino em
2026-11-04 (DP-20). As tarefas DEVEM marcar as dependências de D-05 (todas as que tratam de
identificação, aprovação e exposição), do ADR-0004 (primeira chamada ao LLM com dados de documento)
e da DP-27 (adaptador A2A, que pode mudar com o contrato definitivo).

**Reavaliação pós-design**: o desenho da Fase 1 (data-model, quatro contratos e quickstart) não
introduziu novas violações. Confirmações:

- a etapa 5 desabilitada não impede "Sem inconsistências" e gera limitação e revisão humana
(FR-047b);
- a seleção do pacote e as regras de conteúdo usam a mesma data de referência (FR-013a), e a falta de
pacote para a data nunca resulta em "Sem inconsistências";
- o contexto do usuário não entra no cálculo do resultado geral nem como evidência, e nenhuma
ferramenta da IA o acessa;
- os checkpoints JSON não guardam XML nem texto de recomendação; o aprovador fica marcado como não
verificado;
- o Agent Card não expõe segredos e declara a ausência de autenticação;
- o canal A2A e o REST usam o mesmo caso de uso.

O gate continua **aprovado**. A única decisão pendente que afeta a feature é a DP-27 (contrato
definitivo com a SOFIA), que não bloqueia o núcleo.

## Project Structure



### Documentation (this feature)

```text
specs/001-agente-auditoria-nfe/
├── spec.md                               # Especificação (esclarecida em 2026-10-29 e 2026-11-04)
├── vigencia-pacotes-schema.md            # Anexo: vigência dos pacotes de schema (aprovado)
├── catalogo-regras-totalizadores.md      # Anexo: catálogo de regras v1 (aprovado)
├── plan.md                               # Este arquivo
├── research.md                           # Fase 0: decisões e APIs do MAF, A2A e MCP
├── data-model.md                         # Fase 1: entidades, validações e estados
├── quickstart.md                         # Fase 1: guia de execução e validação
├── contracts/
│   ├── api-agente.openapi.yaml           # Canal direto (REST v1) e envelope v1
│   ├── sofia-a2a.md                      # Contrato A2A — proposta v1 provisória (DP-27)
│   ├── contrato-ia-v1.schema.json        # JSON Schema da saída estruturada do LLM
│   ├── ferramentas-agente.md             # Contrato v1 das ferramentas do agente interno (constitution 4.6)
│   └── consulta-manuais-mcp.md           # Contrato do lado consumidor para a Api.RAG (feature 002)
├── checklists/
│   └── requirements.md                   # Checklist de qualidade da spec
└── tasks.md                              # Fase 2 (/speckit-tasks — não criado por este comando)
```



### Source Code (repository root)

```text
AgenteAuditoriaNFe.slnx
global.json                      # SDK .NET 10 fixado; runner Microsoft.Testing.Platform (V-07)
Directory.Build.props            # Nullable, avisos como erro, supressões documentadas (MAAI001, A2A)
Directory.Packages.props         # Versões centrais (research §1)
.editorconfig
.gitattributes                   # XSDs oficiais como -text (já criado)
.env.example                     # Variáveis sem valores secretos
docker-compose.yml               # api-agente, mongodb, postgres; portas em 127.0.0.1
infra/
├── mongodb/init/01-colecoes.js  # Usuário da aplicação, coleções com $jsonSchema, índices, TTL
└── postgres/init/01-consumo.sql # Usuário da aplicação e schema consumo (tabela por migração EF)
.github/workflows/ci.yml         # build-test e build da imagem
docs/adr/                        # ADR-0001 a ADR-0010 (DP-20)

src/
├── Api.Agente/                          # Camada de Endpoints (host ASP.NET Core)
│   ├── Program.cs                       # Composição: OpenAPI, health, A2A, limites, guarda do D-05
│   ├── Endpoints/                       # Execuções, sessões, recomendações, contexto do usuário (REST v1)
│   ├── A2A/                             # Registro do servidor A2A, Agent Card, leitura de metadados
│   ├── Identificacao/                   # X-Usuario-Id / X-Chamador-Id e metadados A2A → IdentificacaoSolicitante
│   ├── Erros/                           # Mapeamento ERR-xxx → HTTP e envelope
│   ├── appsettings.json                 # Sem segredos
│   └── Dockerfile
├── AuditoriaNFe.Application/            # Camada de Aplicação
│   ├── Agente/                          # AgenteAuditoriaNFe (AIAgent), fábrica do ChatClientAgent
│   │   └── Instrucoes/                  # Instruções versionadas (recurso embutido)
│   ├── Ferramentas/                     # Funções de leitura e cálculo (AIFunctionFactory) e política
│   ├── ContextProviders/                # ContextoExecucao, ContextoSessao, ContextoUsuario
│   ├── Middleware/                      # Execução do agente e Function Calling
│   ├── Fluxo/                           # Nove etapas da constitution 3.4
│   ├── CasosDeUso/                      # ExecutarSolicitacaoAuditoria, ConsultarRecomendacoes,
│   │                                    # DecidirRecomendacao, EncerrarSessao, GerirContextoUsuario
│   ├── Servicos/                        # Montagem do parecer, validador da saída da IA,
│   │                                    # classificador de sensibilidade, atualização do contexto
│   ├── Workflows/Aprovacao/             # FluxoAprovacaoRecomendacao (RequestPort, executores)
│   ├── Sessoes/                         # Expiração, isolamento, continuidade
│   ├── Contratos/                       # ContratoIaV1, EnvelopeV1
│   └── Abstracoes/                      # Repositórios, validador de schema, provedor de pacotes,
│                                        # catálogos, consulta a manuais, consumo, relógio
├── AuditoriaNFe.Domain/                 # Camada de Domínio (sem pacotes externos)
│   ├── Identificacao/                   # IdentificacaoSolicitante, chave de isolamento
│   ├── Documentos/                      # DocumentoNFe, ArquivoRecebido, identificação (FR-007a)
│   ├── Schemas/                         # PacoteSchema, VigenciaPacote, seleção por data e convivência
│   ├── Validacao/                       # Achado, Evidencia, categorias, severidades, estados de etapa
│   ├── Regras/                          # RegraConteudo (com versão e vigência), implementações por id
│   ├── Calculos/                        # Calculadora da base, RegistroCalculo
│   ├── Parecer/                         # Parecer, cálculo do resultado geral, revisão humana
│   ├── Recomendacoes/                   # Recomendação, estados, CheckpointAprovacao
│   ├── Sessoes/                         # Sessao, Execucao e seus estados
│   ├── ContextoUsuario/                 # ContextoUsuario, regras de atualização e retenção
│   └── Rastreabilidade/                 # Verificação de valores citados
├── AuditoriaNFe.Infrastructure/         # Camada de Infraestrutura
│   ├── Ia/                              # IChatClient OpenAI, RegistroConsumoChatClient
│   │   └── Simulacao/                   # IChatClient simulado (só Development e testes)
│   ├── Persistencia/Mongo/              # Repositórios (sessões, execuções, contexto), AgentSessionStore
│   ├── Persistencia/Postgres/           # DbContext de consumo, migrações, visão diária
│   ├── Aprovacao/                       # Registro JSON do FR-035f, store de checkpoints do workflow
│   ├── Xml/                             # Leitor seguro, resolvedor restrito, validador XSD, extrator
│   ├── Schemas/NFe/                     # 19 pacotes de XSDs oficiais + manifesto.json (já copiados)
│   ├── Catalogos/                       # catalogo-regras.v1, catalogo-extracao.v1, precos-openai.v1
│   ├── Mcp/                             # Consulta a manuais por MCP e implementação desabilitada
│   └── Saude/                           # Health checks MongoDB, PostgreSQL, diretório, schemas
└── Sofia.Simulator/                     # Console: descoberta do card, cliente A2A real, exibição
    ├── Cenarios/                        # basico, continuidade, recomendacoes, sem-identificacao
    └── Exemplos/                        # XMLs sintéticos ou anonimizados (FR-050)

tests/
└── AuditoriaNFe.Tests/
    ├── Dominio/                         # Seleção de pacote, regras, cálculos, resultado geral, contexto
    ├── Aplicacao/                       # Fluxo com IChatClient simulado, retentativas, ERR-013 a 016
    ├── Ferramentas/                     # Contrato, validação de argumentos, autorização
    ├── Aprovacao/                       # Workflow, checkpoints JSON, ERR-020 a ERR-022
    ├── Contrato/                        # Agent Card, mensagens A2A, envelope, OpenAPI, contrato do LLM
    ├── Integracao/                      # WebApplicationFactory + Testcontainers; A2A ponta a ponta
    ├── Arquitetura/                     # Dependências entre camadas; SemSegredos
    ├── Avaliacao/                       # Conjunto de 50+ perguntas (Categoria=Avaliacao)
    ├── Fixtures/Xml/                    # XMLs sintéticos, uma pasta por pacote de schema
    └── Duplos/                          # IChatClient roteirizado, relógio, MCP indisponível
```

**Structure Decision**: solução `AgenteAuditoriaNFe` com os projetos pedidos pelo solicitante
(`Api.Agente`, `AuditoriaNFe.Application`, `AuditoriaNFe.Domain`, `AuditoriaNFe.Infrastructure`,
`Sofia.Simulator`, `AuditoriaNFe.Tests`), mapeados às camadas da constitution 3.3 (DP-19). A Api.RAG
(REST, MCP Server, PgVector, ingestão) é da feature 002 e não tem projetos nesta feature. A SOFIA não
tem projeto nesta solução: o `Sofia.Simulator` apenas valida o contrato A2A e não contém lógica de
orquestração. Os "dados financeiros simulados" pedidos para a Infraestrutura correspondem ao
`IChatClient` simulado e aos XMLs sintéticos; nenhum dado simulado é apresentado como real, e o
provedor simulado é recusado fora de `Development`.

## Design (Fase 1)



### Componentes do Microsoft Agent Framework


| Conceito            | Uso nesta feature                                                                                                                                                | Projeto                         |
| ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------- |
| `IChatClient`       | Cliente OpenAI (Chat Completions) com `RegistroConsumoChatClient` e OpenTelemetry                                                                                | Infrastructure                  |
| `ChatClientAgent`   | Agente interno da etapa 7, com instruções versionadas, ferramentas e saída `ContratoIaV1`; `Id` estável                                                          | Application                     |
| `AIAgent`           | `AgenteAuditoriaNFe`, agente personalizado que recebe mensagens (A2A ou REST), executa o caso de uso e devolve texto + envelope (`DataContent` JSON) e metadados | Application                     |
| `AgentSession`      | Histórico e estado dos providers do agente interno, na sessão de domínio; a sessão do hosting A2A guarda só o mapa chave → `sessaoId`                            | Application / Infrastructure    |
| `AIContextProvider` | `ContextoExecucaoProvider`, `ContextoSessaoProvider` e `ContextoUsuarioProvider` (só leitura), com `ProviderSessionState<T>`                                     | Application                     |
| Middleware          | Execução (correlação, timeout, erros), Function Calling (autorização, validação, registro), `IChatClient` (consumo)                                              | Application / Infrastructure    |
| Ferramentas         | `AIFunctionFactory.Create` com `[Description]`: listar documentos, obter campos, calcular total, listar achados                                                  | Application (cálculo no Domain) |
| Workflow            | `FluxoAprovacaoRecomendacao` com `RequestPort` e `FileSystemJsonCheckpointStore`                                                                                 | Application / Infrastructure    |
| A2A                 | `AddA2AServer` com `DisallowBackground`, `MapA2AHttpJson`, `MapA2AJsonRpc`, `AgentSessionStore` MongoDB; sem `AgentIsolationKeyProvider` na v1                   | Api.Agente                      |
| Agent Card          | `MapWellKnownAgentCard` com habilidades, limites, versão e situação do contrato, ausência de autenticação                                                        | Api.Agente                      |
| Cliente A2A         | `A2ACardResolver`, `GetAIAgentAsync` com HTTP+JSON                                                                                                               | Sofia.Simulator                 |


Uso detalhado das APIs: [research.md §3](./research.md#3-uso-documentado-das-apis-do-microsoft-agent-framework).

### Fluxo de uma solicitação (constitution 3.4)


| Etapa                          | Componente                                                                                                                                                                                                                                               | Falha                                                                       |
| ------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| 1. Receber e identificar       | Endpoint REST ou `AgenteAuditoriaNFe` (A2A) → `ExecutarSolicitacaoAuditoria`; ids pelo sistema; `IdentificacaoSolicitante` dos cabeçalhos ou metadados, com valores padrão (FR-052a, FR-052c); sessão conferida pela chave                               | ERR-005, ERR-006, ERR-007 interrompem                                       |
| 2. Tamanho, formato, condições | Validação de limites e de conteúdo mínimo antes de ler os XMLs                                                                                                                                                                                           | ERR-001 a ERR-003, ERR-007 interrompem                                      |
| 3. Tipo e versão               | Leitor XML seguro; identificação por raiz, namespace, versão e modelo em `ide/mod` e na chave (FR-007a); extração de `dhEmi` e `dhRecbto`; duplicidade (FR-009)                                                                                          | Por arquivo (ERR-008 a ERR-012)                                             |
| 4. Layout (XSD)                | Data de referência → pacote vigente pelo manifesto → validação com o schema de `NFe` ou `nfeProc` do pacote; convivência quando prevista (FR-010 a FR-010f)                                                                                              | Por arquivo; ERR-023 sem pacote; conteúdo "não realizada — layout inválido" |
| 4b. Conteúdo                   | Regras do catálogo vigentes na mesma data, no Domínio, sobre os dados extraídos (FR-012 a FR-014)                                                                                                                                                        | Por arquivo                                                                 |
| 5. Manuais por MCP             | `IConsultaManuais`; desabilitado → "não aplicável"                                                                                                                                                                                                       | ERR-013 quando habilitado e indisponível                                    |
| 6. Ferramentas externas        | Nenhuma na v1 → "não aplicável"                                                                                                                                                                                                                          | —                                                                           |
| 7. Análise por IA              | `ChatClientAgent.RunAsync<ContratoIaV1>` com context providers (inclui o contexto do usuário, salvo "anonimo") e ferramentas de leitura                                                                                                                  | ERR-014                                                                     |
| 8. Validação da saída          | Validador estrutural e semântico; retentativas                                                                                                                                                                                                           | ERR-015                                                                     |
| 9. Retorno                     | Montagem do parecer (com pacote e data de referência por documento), cálculo do resultado geral, classificação de sensibilidade, início dos workflows de aprovação, atualização do contexto do usuário a partir dos resultados determinísticos, envelope | ERR-016 (tempo máximo)                                                      |




### Seleção do pacote de schema (FR-010 a FR-010f)

- **Fonte**: `src/AuditoriaNFe.Infrastructure/Schemas/NFe/manifesto.json`, com os 19 pacotes da
tabela aprovada. O pacote vigente hoje (2026-11-04) é o PL_010e_v1.02; o PL_010g_v1.03 vale a partir
de 2026-11-16.
- **Inicialização**: conferência de SHA-256 de todos os XSDs e da consistência das vigências; falha
impede a subida (health `ready` também verifica).
- **Por documento**: data de referência (data de calendário de `dhRecbto` ou `dhEmi`, no fuso do
XML) → pacote com início ≤ data ≤ fim (o último até a data atual) → `XmlSchemaSet` compilado do
pacote, em cache → erros como `ErroEstrutural` com linha, posição e pacote.
- **Convivência**: regra genérica pelo campo `fimConvivencia` (vazio em todos os pacotes atuais);
testada com tabela sintética.
- **Atualização da tabela**: ajuste aprovado na tabela de vigência gera nova versão do manifesto no
mesmo PR, com os testes de período (TS-050, SC-011).



### Canal direto (REST v1)

Contrato completo em [contracts/api-agente.openapi.yaml](./contracts/api-agente.openapi.yaml). Sem
autenticação na v1 (D-05): todas as rotas aceitam os cabeçalhos opcionais `X-Usuario-Id` e
`X-Chamador-Id`. Erros usam o envelope ou o objeto `erro` (constitution 4.4); o mapeamento HTTP é:
ERR-001/003/007/021 → 400, ERR-002 → 413, ERR-005 → 404, ERR-006 → 410, ERR-020 → 409,
ERR-022/025 → 503, ERR-024 → 429 com `Retry-After`.


| Método e rota                                                                              | Finalidade                                                      | Requisitos       |
| ------------------------------------------------------------------------------------------ | --------------------------------------------------------------- | ---------------- |
| `POST /api/v1/execucoes` (multipart: `pergunta`, `arquivos[]`, `sessaoId`, `correlacaoId`) | Análise, pergunta ou ambos                                      | FR-001 a FR-042  |
| `GET /api/v1/execucoes/{execucaoId}`                                                       | Envelope da execução, com recomendações por estado              | FR-035g, NFR-008 |
| `DELETE /api/v1/sessoes/{sessaoId}`                                                        | Encerrar sessão                                                 | FR-041           |
| `GET /api/v1/recomendacoes/pendentes`                                                      | Pendências das sessões do mesmo chamador, com conteúdo e origem | FR-035d          |
| `POST /api/v1/recomendacoes/{id}/aprovacao`                                                | Aprovar (aprovador = usuário informado, não verificado)         | FR-035e, FR-035f |
| `POST /api/v1/recomendacoes/{id}/rejeicao` (`justificativa`)                               | Rejeitar                                                        | FR-035e, ERR-021 |
| `GET /api/v1/contexto-usuario`                                                             | Consultar o contexto do usuário                                 | FR-059           |
| `PUT /api/v1/contexto-usuario/preferencias`                                                | Declarar ou corrigir preferências                               | FR-055a, FR-059  |
| `DELETE /api/v1/contexto-usuario/cnpjs/{cnpj}` e `.../achados-recorrentes/{chave}`         | Corrigir itens do contexto                                      | FR-059           |
| `DELETE /api/v1/contexto-usuario`                                                          | Excluir o contexto                                              | FR-059           |
| `GET /health/live`, `GET /health/ready`                                                    | Saúde, sem dados                                                | constitution 5.2 |
| `GET /openapi/v1.json`, `/swagger`                                                         | Documentação (só `Development`)                                 | constitution 5.2 |


O canal A2A está em [contracts/sofia-a2a.md](./contracts/sofia-a2a.md) (proposta v1 provisória).

### Contexto do usuário (FR-055 a FR-060)

- **Coleção** `contextos_usuario`, um documento por chave de isolamento, com `$jsonSchema` que só
aceita os itens do FR-055a e índice TTL em `expiraEm` (90 dias sem atividade, configurável).
- **Gravação** pelo caso de uso na etapa 9 (CNPJs dos documentos aprovados no layout, achados
determinísticos recorrentes) e por declaração explícita (endpoint de preferências ou metadado
`preferencias` do A2A). Idempotente por execução. A IA não grava nada (FR-057).
- **Leitura** pelo `ContextoUsuarioProvider`, como dado delimitado e não confiável, limitado por
`ContextoUsuario:MaxItensNoPrompt`; nunca como evidência (FR-058).
- **Usuário "anonimo"**: sem leitura nem gravação; o parecer informa "usuário não identificado".
- Detalhes: [research.md R-31](./research.md#r-31--contexto-do-usuário-fr-055-a-fr-060-p15-constitution-47)
e [data-model.md §13](./data-model.md#13-contextousuario-m-coleção-contextos_usuario).



### Docker e ambiente local (constitution 3.5)

- **Dockerfile** em `src/Api.Agente/Dockerfile`: multiestágio (SDK .NET 10 no build;
`mcr.microsoft.com/dotnet/aspnet:10.0.14` no runtime), usuário não root (`$APP_UID`), porta interna
8080, sem segredos. Schemas, manifesto e catálogos entram como conteúdo do projeto de
Infraestrutura.
- **Compose** na raiz, projeto `auditoria-nfe`, no modelo do projeto-ia-infis (versões fixas,
`${VAR:?}`, healthchecks, scripts de inicialização somente leitura, volumes e rede nomeados):

  | Serviço      | Imagem                               | Host                      | Dependência                                                  |
  | ------------ | ------------------------------------ | ------------------------- | ------------------------------------------------------------ |
  | `api-agente` | build de `src/Api.Agente/Dockerfile` | `127.0.0.1:5310` → 8080   | `mongodb` e `postgres` saudáveis; volume `./data/aprovacoes` |
  | `mongodb`    | `mongo:8.0.32`                       | `127.0.0.1:27020` → 27017 | `infra/mongodb/init`                                         |
  | `postgres`   | `pgvector/pgvector:0.8.7-pg17`       | `127.0.0.1:5436` → 5432   | `infra/postgres/init`                                        |

- As portas não conflitam com os outros projetos da máquina (research R-24). O `api-agente` sobe com
`ASPNETCORE_ENVIRONMENT=Development` e `Seguranca__AutenticacaoHabilitada=false`.
- **Documentação da API**: Swagger UI sobre o OpenAPI gerado, só em `Development` (research R-20).



### Parâmetros configuráveis (P13)

Valores iniciais; os marcados com * DEVEM ser confirmados pela avaliação ou pelos testes de carga
antes do merge.


| Chave                                                                             | Inicial                                                 | Origem                         |
| --------------------------------------------------------------------------------- | ------------------------------------------------------- | ------------------------------ |
| `Limites:TamanhoTotalBytes` / `Limites:QuantidadeArquivos`                        | 5 MB / 20                                               | FR-004                         |
| `Limites:CorpoHttpBytes`                                                          | 2 × tamanho total                                       | research R-29                  |
| `Sessao:ExpiracaoInatividade`                                                     | 24 h                                                    | spec, Assumptions              |
| `ContextoUsuario:RetencaoInatividade`                                             | 90 dias                                                 | FR-060                         |
| `ContextoUsuario:MaxCnpjs` / `ContextoUsuario:MaxAchados`                         | 200* / 100*                                             | P15 (limite de armazenamento)  |
| `ContextoUsuario:MaxItensNoPrompt`                                                | 20*                                                     | P15 (limite enviado ao modelo) |
| `Execucao:TempoMaximo`                                                            | 120 s                                                   | NFR-001, ERR-016               |
| `Ia:MaxTentativas`                                                                | 3                                                       | P08, ERR-015                   |
| `Ia:TimeoutChamada`                                                               | 30 s*                                                   | constitution 5.4               |
| `Ferramentas:Timeout`                                                             | 5 s*                                                    | seção 4.6                      |
| `Concorrencia:MaxExecucoes`                                                       | 10                                                      | NFR-004                        |
| `LimiteTaxa:PermissoesPorJanela` / `LimiteTaxa:Janela`                            | 30* / 1 min* (janela fixa por chave de isolamento)      | constitution 5.2, ERR-024      |
| `Persistencia:TimeoutMongo` / `Persistencia:TimeoutPostgres`                      | 5 s* / 5 s*                                             | constitution 5.4, ERR-025      |
| `Xml:ProfundidadeMaxima` / `Xml:MaxCaracteres`                                    | 64* / derivado do limite                                | constitution 5.2               |
| `Schemas:Diretorio`                                                               | `Schemas/NFe`                                           | research R-14                  |
| `Catalogos:Regras` / `Catalogos:Extracao` / `Catalogos:Precos`                    | arquivos v1                                             | research R-15, R-16            |
| `Historico:MaxMensagens`                                                          | 40*                                                     | FR-042                         |
| `Sensibilidade:TermosTributos`                                                    | lista revisada pelo responsável fiscal                  | FR-035a                        |
| `Mcp:Habilitado` / `Mcp:Endpoint` / `Mcp:Timeout` / `Mcp:QuantidadeMaximaTrechos` | `false` / vazio / 10 s* / 5                             | FR-047a, contrato MCP          |
| `Aprovacao:Diretorio`                                                             | `./data/aprovacoes`                                     | FR-035j                        |
| `Seguranca:AutenticacaoHabilitada`                                                | `false` (único valor aceito na v1, só em `Development`) | D-05                           |
| `Persistencia:AplicarMigracoes`                                                   | `true` só no ambiente local                             | research R-16                  |
| `OpenAI:Modelo`                                                                   | sem padrão                                              | DP-05                          |
| `Retencao:*` (demais)                                                             | definidos no ADR-0005                                   | DP-13                          |


Os valores padrão "anonimo" e "nao-informado" são constantes da spec (FR-052c), e não configuração.

### Segurança e privacidade

- **Sem autenticação (D-05)**: identificação informada e não verificada; guarda de inicialização que
recusa subir fora de `Development`; portas só em `127.0.0.1`; Agent Card declarando a ausência de
autenticação; dados sintéticos. Limitação de taxa por chave de isolamento.
- XML: `DtdProcessing.Prohibit`, `XmlResolver = null`, limites de caracteres e profundidade; tipo
pelo conteúdo; schemas carregados só da pasta do pacote, com SHA-256 conferido.
- Limites e rate limit antes de processamento custoso; corpo HTTP limitado.
- Logs sem XML, pergunta, resposta, contexto do usuário, `usuarioId` em claro ou dados pessoais;
telemetria do MAF sem dados sensíveis.
- Envio ao LLM conforme ADR-0004 (minimização e mascaramento).
- Contexto do usuário sem CPF, nomes, endereços, valores ou texto livre, garantido pelo
`$jsonSchema` da coleção e pelo caso de uso.
- Checkpoints JSON apenas com identificadores e dados da decisão.



### Testes por categoria (P17)


| Categoria                        | Cenários da spec                                                                                                                                                                                                                                                                                                       |
| -------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Domínio                          | TS-004, TS-005, TS-013, TS-014 (cálculo), resultado geral (TS-001, TS-009, TS-046), sensibilidade (TS-039), seleção de pacote e convivência (TS-048 a TS-051, TS-060 a TS-062), identificação (TS-052, TS-053), regras do catálogo (um caso válido e um inválido por regra e versão), atualização do contexto (TS-057) |
| Aplicação (IChatClient simulado) | TS-015 a TS-020, TS-026 a TS-028, TS-036 a TS-038, TS-047, TS-055, TS-059                                                                                                                                                                                                                                              |
| Ferramentas                      | Contrato, argumentos inválidos, autorização e timeout de cada ferramenta                                                                                                                                                                                                                                               |
| Aprovação                        | TS-040, TS-041, TS-043 a TS-045                                                                                                                                                                                                                                                                                        |
| Contrato                         | TS-029, TS-032, TS-034, envelope v1, OpenAPI de referência, schema do contrato do LLM                                                                                                                                                                                                                                  |
| Integração (Testcontainers)      | TS-002, TS-003, TS-006 a TS-012, TS-021 a TS-025, TS-030, TS-033 (limitado por D-04), TS-035, TS-035a, TS-054, TS-056, TS-058, TS-063, TS-064; rastreabilidade a partir do `execucaoId` (NFR-008, P16)                                                                                                                  |
| Arquitetura                      | Domínio sem pacotes externos; Aplicação sem OpenAI, MCP, A2A, MongoDB ou Npgsql; `SemSegredos`                                                                                                                                                                                                                         |
| Avaliação                        | Com OpenAI real, sob demanda (P17). Perguntas e recomendações: SC-003, SC-004, SC-008. Análise por IA (lotes sintéticos com divergências conhecidas por grupo de regra): 0 indícios sem evidência verificável, 0 indícios que contradizem ou repetem como novo um achado determinístico e 0 valores não rastreáveis no resumo |
| Adiados pelo D-05                | TS-031, TS-042                                                                                                                                                                                                                                                                                                         |




### Ordem de implementação (insumo para `/speckit-tasks`)

1. Solução, `global.json`, Docker (Dockerfile, compose, scripts de inicialização), CI e testes de base.
2. Núcleo pelo canal direto:
  1. leitura segura do XML e identificação da NF-e;
  2. seleção do pacote pela data de referência e validação XSD;
  3. totalizadores do catálogo;
  4. parecer e envelope.
3. Memória conversacional, contexto do usuário e controle de tokens e custos.
4. IA (etapas 7 e 8), ferramentas, recomendações e aprovação humana.
5. Canal A2A e `Sofia.Simulator`, contra a proposta v1 do contrato (DP-27).



### ADRs

Registrados em `docs/adr/` e aprovados por Andressa Firmino em 2026-11-04 (DP-20).


| ADR      | Tema                                                                                           | Situação |
| -------- | ---------------------------------------------------------------------------------------------- | -------- |
| ADR-0001 | Estrutura da solução e camadas, com abstrações do MAF na Aplicação (DP-19, constitution 3.3)   | Aprovado |
| ADR-0002 | MongoDB para memória, sessões e contexto do usuário (DP-01, P15)                               | Aprovado |
| ADR-0003 | Versão 1 sem autenticação (D-05) e modelo futuro de credencial de serviço (DP-10)              | Aprovado |
| ADR-0004 | Minimização de dados ao LLM (DP-14)                                                            | Aprovado |
| ADR-0005 | Retenção de sessões, contexto do usuário, consumo e checkpoints (DP-13)                        | Aprovado |
| ADR-0006 | Topologia PostgreSQL (DP-16)                                                                   | Aprovado |
| ADR-0007 | Protocolo A2A 1.0, Agent Card, contrato provisório e limitação de tarefas (DP-17, DP-27, D-04) | Aprovado |
| ADR-0008 | CI, branches `dev` e `main` protegidas, com autoaprovação de PR (DP-26, D-06)                  | Aprovado |
| ADR-0009 | Observabilidade com OpenTelemetry (DP-23)                                                      | Aprovado |
| ADR-0010 | Schemas XSD oficiais versionados no repositório e seleção pela data de referência (DP-03)      | Aprovado |


Os antigos ADR-0001 (MAF no lugar do Semantic Kernel) e ADR-0002 (MAF na Aplicação) deixaram de ser
necessários com a constitution 2.0.0; a numeração acima substitui a da versão anterior do plano.
Pela aprovação do ADR-0004, a primeira chamada ao LLM com dados de documento está liberada, desde
que siga a política de minimização e mascaramento do ADR.

### Limitações técnicas

- **Sem autenticação (D-05)**: qualquer processo com acesso à porta pode enviar solicitações, se
passar por outro usuário e aprovar recomendações sensíveis. Execução só local até o fim do desvio.
- **Checkpoints JSON locais**: apenas demonstração; uma instância; sem transação nem criptografia;
exigem volume persistente no contêiner (FR-035j).
- **A2A em pré-lançamento**: pacotes A2A e de hosting em preview; versões fixadas e testes de
contrato obrigatórios a cada atualização.
- **Contrato com a SOFIA provisório (DP-27)**: mudanças acordadas exigem nova versão do contrato e do
adaptador A2A.
- **Sem tarefas A2A em segundo plano**: o hosting do MAF na versão fixada só devolve mensagens
imediatas; estados intermediários não são observáveis por A2A (D-04).
- **Tabela de vigência sujeita a ajustes**: aprovada com a ressalva de ajustes futuros; datas
inferidas e pendências P1 a P5 registradas na tabela.
- **Vercel**: o runtime de contêiner da Vercel (beta) executa Functions sem disco durável; não é
usado nesta feature (research R-26).
- **MCP desabilitado**: sem consulta a manuais; pareceres com limitação declarada e revisão humana
até a feature 002.



## Complexity Tracking

> Desvios da constitution e da spec, registrados conforme a seção 8.3. Responsável pela proposta:
> arquiteto do projeto. **Aprovado por Andressa Firmino, 2026-11-04** (D-03, D-04, D-05 e D-06,
> conforme DP-20). Os desvios D-01 e D-02 da versão anterior deste plano foram encerrados pela
> constitution 2.0.0.


| Violation                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Why Needed                                                                                                                                     | Simpler Alternative Rejected Because                                                                                                                                                                                                              |
| ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **D-03** — Workflow do MAF e registro JSON próprio para a aprovação (P12, complexidade). Impacto: dois arquivos por recomendação sensível. **Aprovado por Andressa Firmino, 2026-11-04**                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Pedido de Workflow com aprovação humana e checkpoints; o formato interno de checkpoint do MAF não é contrato estável para os campos do FR-035f | Só uma máquina de estados com arquivo JSON não atende ao pedido de Workflow; só o checkpoint do MAF não garante schema estável de auditoria                                                                                                       |
| **D-04** — Spec FR-046 e TS-033: estados de tarefa intermediários não disponíveis por A2A na v1. Impacto: a SOFIA só vê o estado final. Prazo: reavaliar a cada versão do hosting A2A. **Aprovado por Andressa Firmino, 2026-11-04**                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | O hosting A2A do MAF ainda não suporta respostas em segundo plano (`DisallowBackground`); o FR-043 exige o SDK oficial                         | Implementar tarefas A2A manualmente contraria o FR-043; o lote máximo cabe no tempo síncrono (NFR-001). A spec DEVE ser ajustada por `/speckit-clarify` ou o desvio aprovado                                                                      |
| **D-05** — Constitution 5.2 (autenticação do chamador), P18 (autenticação igual nos canais) e Definition of Done ("nenhum endpoint não público sem credencial"); spec FR-052, FR-035h, ERR-004, ERR-019, TS-031 e TS-042 adiados. **Impacto**: identificação do usuário e do chamador informada sem verificação; isolamento de sessões e do contexto do usuário depende da boa-fé do chamador; aprovador não verificado. **Riscos**: personificação de usuário, leitura do contexto de outro usuário por quem souber o identificador, aprovação indevida de recomendações sensíveis. **Mitigação**: execução só em `Development` (guarda de inicialização), portas em `127.0.0.1`, dados sintéticos ou anonimizados, card declarando ausência de autenticação, aprovador marcado como não verificado. **Responsável**: solicitante da feature. **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04. **Prazo de revisão**: encerrar antes de qualquer implantação fora da máquina local ou da integração com a SOFIA real; reavaliar no planejamento da feature 002 | A versão 1 serve para testar a validação dos XMLs; o solicitante decidiu adiar a autenticação para implementação futura                        | Implementar a credencial de serviço já na v1 exigiria decidir a DP-10 (mecanismo, rotação, multi-tenant) antes de validar o núcleo; JWT de desenvolvimento sem ADR seria uma autenticação provisória com a mesma ausência de garantia em produção |
| **D-06** — Constitution 5.3 ("todo PR DEVE passar por revisão de ao menos uma pessoa diferente do autor") e Definition of Done ("revisão aprovada por pessoa diferente do autor"). **Impacto**: PRs revisados e aprovados pela própria autora; proteção da `main` e da `dev` com zero aprovações obrigatórias, porque o GitHub não permite que o autor aprove o próprio PR. **Riscos**: erros e desvios da constitution sem segundo olhar. **Mitigação**: PR obrigatório, check `build-test` obrigatório, sem push direto nem force push, checklist de conformidade com a constitution na descrição do PR, preenchido e comentado como "Aprovado" antes do merge. **Responsável**: Andressa Firmino. **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04. **Prazo de revisão**: encerrar quando outra pessoa entrar como mantenedora (junto com a reabertura da DP-20) | O projeto tem uma única mantenedora (DP-20) | Exigir uma aprovação na proteção da `main` e da `dev` impediria qualquer merge, pois o GitHub bloqueia a autoaprovação; contornar com bypass de administrador anularia a proteção para todos os PRs |


