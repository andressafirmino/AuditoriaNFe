---

description: "Lista de tarefas da feature 001-agente-auditoria-nfe"
---

# Tasks: Agente de Auditoria de NF-e (AuditoriaNFe)

**Input**: documentos de projeto em `specs/001-agente-auditoria-nfe/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md),
[data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md),
[vigencia-pacotes-schema.md](./vigencia-pacotes-schema.md),
[catalogo-regras-totalizadores.md](./catalogo-regras-totalizadores.md)

**Tests**: obrigatórios. A spec exige a automação dos cenários TS (seção Test Scenarios) e a
constitution exige testes e avaliação (P17). Em cada história, os testes são escritos antes e DEVEM
falhar antes da implementação. TS-031 e TS-042 ficam adiados pelo desvio D-05.

**Organization**: tarefas agrupadas por história de usuário. A ordem das fases segue a ordem de
implementação do plano: o canal A2A (US2) e o Sofia.Simulator (US6) ficam por último porque o
contrato com a SOFIA ainda é provisório (DP-27, research R-32), embora a US2 seja P1.

**Ajuste em relação à ordem do plano**: a análise por IA da etapa 7 (agente interno, saída
`ContratoIaV1`, validação e retentativas) entra na US1, com o `IChatClient` simulado nos testes. Sem
ela, a etapa 7 ficaria "não executada" e o resultado geral seria sempre "Inconclusiva"
(data-model §10, regra 1), o que impediria o cenário 1 da US1. Ferramentas, perguntas e cálculos
ficam na US3; recomendações, na US5.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: pode rodar em paralelo (arquivos diferentes, sem dependência de tarefa incompleta)
- **[Story]**: história a que a tarefa pertence (US1 a US7)
- Caminhos relativos à raiz do repositório

## Path Conventions

- Solução `AgenteAuditoriaNFe.slnx` na raiz; projetos em `src/` e testes em
  `tests/AuditoriaNFe.Tests/` (plan.md, Project Structure)
- Camadas: `src/Api.Agente` (Endpoints), `src/AuditoriaNFe.Application`,
  `src/AuditoriaNFe.Domain`, `src/AuditoriaNFe.Infrastructure`; cliente `src/Sofia.Simulator`
- Comando de teste: `dotnet test --project tests/AuditoriaNFe.Tests --filter-not-trait "Categoria=Avaliacao"`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: solução, padrões de build, ambiente Docker local, CI e proteção das branches.

- [X] T001 Criar `.gitignore` na raiz ignorando `bin/`, `obj/`, `.vs/`, `*.user`, `TestResults/`, `.env` e `data/`, mantendo `.env.example` versionado (constitution 5.1: sem segredos no repositório)
- [X] T002 Criar `global.json` na raiz fixando o SDK `10.0.403` com `"rollForward": "latestFeature"` (nunca o SDK 11 RC instalado na máquina) e `"test": { "runner": "Microsoft.Testing.Platform" }` (research §1, lição V-07)
- [X] T003 Criar `AgenteAuditoriaNFe.slnx` e os projetos `src/Api.Agente/Api.Agente.csproj` (web), `src/AuditoriaNFe.Application/AuditoriaNFe.Application.csproj`, `src/AuditoriaNFe.Domain/AuditoriaNFe.Domain.csproj`, `src/AuditoriaNFe.Infrastructure/AuditoriaNFe.Infrastructure.csproj` (bibliotecas), `src/Sofia.Simulator/Sofia.Simulator.csproj` (console) e `tests/AuditoriaNFe.Tests/AuditoriaNFe.Tests.csproj` (xUnit v3), com as referências de research R-28: Api.Agente → Application e Infrastructure; Infrastructure → Application e Domain; Application → Domain; Domain sem pacote externo; Sofia.Simulator sem referência a nenhum projeto do agente (FR-048). No projeto de Infraestrutura, incluir `Schemas/**` e `Catalogos/**` como conteúdo copiado para a saída (`CopyToOutputDirectory=PreserveNewest`)
- [X] T004 Criar `Directory.Build.props` na raiz com `net10.0`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true` e supressões pontuais documentadas: `MAAI001` em Application, Infrastructure e Tests; diagnósticos `[Experimental]` do hosting A2A só no Api.Agente (research §1)
- [X] T005 Criar `Directory.Packages.props` com Central Package Management e as versões exatas de research §1 (`Microsoft.Agents.AI*` 1.26.0, hosting e cliente A2A 1.26.0-preview.261023.1, `Microsoft.Extensions.AI.OpenAI` 10.11.0, `ModelContextProtocol.Core` 2.3.0, `MongoDB.Driver` 3.13.0, `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3, `Microsoft.AspNetCore.OpenApi` 10.0.14, `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1, `OpenTelemetry.Extensions.Hosting` 1.20.1, `xunit.v3` 4.0.2, `Microsoft.AspNetCore.Mvc.Testing` 10.0.14, `Testcontainers.MongoDb` e `Testcontainers.PostgreSql` 4.16.0)
- [X] T006 [P] Criar `.editorconfig` na raiz (C#: indentação de 4 espaços, `var` quando o tipo é evidente, namespaces com escopo de arquivo, UTF-8, fim de linha LF exceto `*.xsd`, que segue o `.gitattributes`)
- [X] T007 [P] Copiar o inventário das notas técnicas de `C:\Infis\fonte\Curso IA\docs\notas-tecnicas\inventario.csv` para `docs/notas-tecnicas/inventario.csv` (só o CSV, sem os PDFs) e acrescentar uma linha para o Anexo I do MOC 7.0, com URL do Portal Nacional da NF-e e SHA-256 calculado do PDF oficial em `C:\Infis\fonte\Curso IA\docs\`. O catálogo de regras referencia esse arquivo, e o cabeçalho `fontes[]` do `catalogo-regras.v1.json` usa os hashes dele (data-model §6)
- [X] T008 [P] Criar `.env.example` na raiz só com nomes de variáveis e valores não secretos (usuário e senha do MongoDB e do PostgreSQL vazios, `OPENAI_API_KEY` vazia, `Ia__Provedor=Simulado`, `OpenAI__Modelo` vazio), sem copiar chaves de outros projetos (research R-24)
- [X] T009 [P] Criar `infra/mongodb/init/01-colecoes.js`: usuário da aplicação com permissão só no banco `auditoria_agente`; coleções `sessoes`, `execucoes`, `documentos`, `sessoes_agente` e `contextos_usuario`; `$jsonSchema` de `contextos_usuario` com `additionalProperties: false` e só os campos de data-model §13; índice TTL em `expiraEm` com `expireAfterSeconds: 0`; índices por `chaveIsolamento` nas demais coleções
- [ ] T010 [P] Criar `infra/postgres/init/01-consumo.sql`: usuário exclusivo do Api.Agente, schema `consumo` e permissões só nesse schema (ADR-0006); a tabela vem da migração EF (T029)
- [ ] T011 Criar `src/Api.Agente/Dockerfile` multiestágio: SDK .NET 10 no build e `mcr.microsoft.com/dotnet/aspnet:10.0.14` no runtime, usuário não root (`$APP_UID`), `ASPNETCORE_HTTP_PORTS=8080`, sem segredos nem `appsettings` com credenciais, com schemas, manifesto e catálogos copiados como conteúdo (research R-24)
- [ ] T012 Criar `docker-compose.yml` na raiz (projeto `auditoria-nfe`): `api-agente` em `127.0.0.1:5310:8080` com `ASPNETCORE_ENVIRONMENT=Development`, `Seguranca__AutenticacaoHabilitada=false` e volume `./data/aprovacoes`; `mongodb` (`mongo:8.0.32`) em `127.0.0.1:27020:27017`; `postgres` (`pgvector/pgvector:0.8.7-pg17`) em `127.0.0.1:5436:5432`; segredos com `${VAR:?Defina VAR no arquivo .env}`, healthchecks, `depends_on` com `service_healthy`, scripts de `infra/` montados somente leitura, volumes e rede nomeados (plan.md, Docker)
- [ ] T013 [P] Criar `tests/AuditoriaNFe.Tests/Arquitetura/DependenciasCamadasTests.cs`: o Domínio não referencia pacote externo; a Aplicação não referencia OpenAI, MCP, A2A, MongoDB nem Npgsql; o Sofia.Simulator não referencia projetos do agente (constitution 3.3, R-28)
- [ ] T014 [P] Criar `tests/AuditoriaNFe.Tests/Arquitetura/SemSegredosTests.cs`, que falha ao encontrar padrões de chave de API ou senha no repositório (research R-25)
- [ ] T015 Criar `.github/workflows/ci.yml` com o job `build-test` (restore, build com avisos como erro, testes com Docker para Testcontainers, sem chave da OpenAI, excluindo `Categoria=Avaliacao`) e o job de build da imagem, disparados em PR para `dev` e `main` (ADR-0008)
- [ ] T016 [P] Criar `.github/pull_request_template.md` com o checklist de conformidade com a constitution (princípios afetados, desvios D-03 a D-06, testes, ausência de segredos) e a instrução de comentar "Aprovado" antes do merge (desvio D-06)
- [ ] T017 Depois da primeira execução verde do CI, exigir o check `build-test` na proteção das branches `dev` e `main` por `gh api repos/andressafirmino/AuditoriaNFe/branches/{dev,main}/protection`, mantendo zero aprovações, `enforce_admins` e o bloqueio de force push (ADR-0008)

**Checkpoint**: `dotnet build` e `dotnet test` passam com a solução vazia; `docker compose up` sobe os bancos.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: entidades centrais, contratos de resposta, persistência, registro de consumo, composição
do host, guarda do D-05 e infraestrutura de testes.

**⚠️ CRITICAL**: nenhuma história começa antes do fim desta fase.

### Domínio

- [ ] T018 [P] Criar os enums em `src/AuditoriaNFe.Domain/Validacao/`: `CategoriaAchado` (`ErroEstrutural`, `InconsistenciaNegocio`, `IndicioIa`, `AusenciaEvidencias`, `FalhaTecnica`), `Severidade` (`Alta`, `Media`, `Baixa`, `Informativa`), `OrigemAchado` (`Deterministica`, `IA`, `Sistema`), `EstadoEtapa` (`Executada`, `Falhou`, `NaoAplicavel`, `NaoExecutada`) e `EstadoValidacao` (`Realizada`, `NaoRealizada` com motivo) (data-model §2, §3, §7)
- [ ] T019 [P] Criar `Achado` e `Evidencia` em `src/AuditoriaNFe.Domain/Validacao/Achado.cs` e `Evidencia.cs`, com as invariantes de data-model §7 e §8: severidade "Erros estruturais sempre `Alta`"; `valorInformado` / `valorEsperado` "Obrigatórios em inconsistência de negócio (FR-014)"; `regraId` / `versaoRegra` "Obrigatórios em `InconsistenciaNegocio`"; `pacoteSchema` "Obrigatório em `ErroEstrutural` de schema"; `evidencias` "Obrigatória em `IndicioIa` (FR-021)"; tipos de evidência `CampoXml`, `RegraCatalogo`, `TrechoManual`, `Calculo`
- [ ] T020 [P] Criar `IdentificacaoSolicitante` em `src/AuditoriaNFe.Domain/Identificacao/IdentificacaoSolicitante.cs` (data-model §0): `usuarioId` "até 128 caracteres de `[A-Za-z0-9._:@-]`; ausente → `"anonimo"`"; `chamadorId` "mesmo formato; ausente → `"nao-informado"`"; `usuarioIdentificado` "`false` quando `usuarioId = "anonimo"`"; `chaveIsolamento` "Hash de `chamadorId` + separador + `usuarioId`" em SHA-256 hexadecimal
- [ ] T021 [P] Criar `Execucao` e `EtapaExecucao` em `src/AuditoriaNFe.Domain/Sessoes/Execucao.cs` (data-model §2): estados `Recebida`, `EmProcessamento`, `Concluida`, `Falhou`, `Rejeitada` e as transições do diagrama; `correlacaoId` "máx. 128 caracteres"; `canal` (`Direto`, `A2A`); `tipo` (`AnaliseLote`, `Pergunta`, `AnaliseComPergunta`, `ConsultaRecomendacoes`); nove etapas sempre presentes, com `justificativa` "obrigatória para `NaoAplicavel` e `NaoExecutada`"; `versoes`, `integracaoMcpHabilitada`, `consumoResumo`, `contextoUsuarioAtualizado`, `chamadasFerramentas` ("sem argumentos de texto livre nem valores de documentos") e `trechosConsultados` (NFR-008)
- [ ] T022 [P] Criar `Sessao` em `src/AuditoriaNFe.Domain/Sessoes/Sessao.cs` (data-model §1): estados `Ativa`, `Expirada`, `Encerrada`; `Expirada` "quando `agora - ultimaAtividadeEm > expiraApos`"; `hashesPorChave` (chave de acesso → SHA-256); `documentos`, `execucoes` e `historicoAgente`
- [ ] T023 [P] Criar os testes de unidade de T019 a T022 em `tests/AuditoriaNFe.Tests/Dominio/AchadoTests.cs`, `IdentificacaoSolicitanteTests.cs`, `ExecucaoTests.cs` e `SessaoTests.cs` (formatos inválidos de identificação, valores padrão, hash estável, transições proibidas, justificativa obrigatória)

### Aplicação

- [ ] T024 [P] Criar as abstrações em `src/AuditoriaNFe.Application/Abstracoes/`: `IRelogio`, `IGeradorIds` (GUID v7 com `Guid.CreateVersion7`, P09), `ISessaoRepositorio`, `IExecucaoRepositorio` e `IDocumentoRepositorio`, com toda leitura recebendo a `chaveIsolamento`
- [ ] T025 [P] Criar as classes de opções em `src/AuditoriaNFe.Application/Configuracao/` para todas as chaves da tabela de parâmetros do plano (`Limites`, `Sessao`, `ContextoUsuario`, `Execucao`, `Ia`, `Ferramentas`, `Concorrencia`, `LimiteTaxa`, `Xml`, `Schemas`, `Catalogos`, `Historico`, `Sensibilidade`, `Mcp`, `Aprovacao`, `Seguranca`, `Persistencia`, `OpenAI`), com validação na inicialização (`ValidateOnStart`), `OpenAI:Modelo` sem padrão (DP-05) e os timeouts `Persistencia:TimeoutMongo` e `Persistencia:TimeoutPostgres` obrigatórios (constitution 5.4)
- [ ] T026 [P] Criar `src/AuditoriaNFe.Application/Contratos/CodigoErro.cs` e `MensagensErro.cs` com ERR-001 a ERR-025 e mensagens em pt-BR (spec, Error Handling; NFR-010). ERR-004 e ERR-019 ficam reservados e sem uso enquanto vigorar o D-05
- [ ] T027 Criar `src/AuditoriaNFe.Application/Contratos/EnvelopeV1.cs` com os campos de data-model §16 e do schema `EnvelopeV1` de `contracts/api-agente.openapi.yaml`, preenchidos só pelo sistema (P09): `versaoEnvelope`, `versaoContratoIa`, `execucaoId`, `sessaoId`, `correlacaoId`, `estado`, `dataHora`, `canal`, `usuarioIdentificado`, `etapas`, `parecer`, `resposta`, `recomendacoes`, `erro` (`codigo`, `mensagem`, `arquivo`, `detalhes`) e `metadados` (`modelo`, `duracaoMs`, `consumo`, `versoes`) (depende de T026)

### Infraestrutura

- [ ] T028 Implementar o acesso ao MongoDB em `src/AuditoriaNFe.Infrastructure/Persistencia/Mongo/ContextoMongo.cs` (GUID em representação padrão, nomes em camelCase) e os repositórios `SessaoRepositorio.cs`, `ExecucaoRepositorio.cs` e `DocumentoRepositorio.cs`, que implementam T024, filtram sempre pela `chaveIsolamento` e nunca gravam o XML original (FR-037a); `Persistencia:TimeoutMongo` aplicado à seleção de servidor, à conexão e às operações do driver, e indisponibilidade ou timeout convertidos em ERR-025 (constitution 5.4)
- [ ] T029 [P] Criar `src/AuditoriaNFe.Infrastructure/Persistencia/Postgres/ConsumoDbContext.cs`, a entidade `ChamadaProvedor` e a migração inicial em `Persistencia/Postgres/Migrations/`, com a tabela `consumo.chamadas_provedor` de data-model §14: tokens "NULL quando o provedor não informar; nunca zero por suposição; CHECK ≥ 0"; custos `numeric(18,8)` "NULL sem preço na tabela ou sem tokens; CHECK ≥ 0"; `status` `Sucesso`, `Falha`, `Timeout` ou `RespostaInvalida`; `origem_valores` `InformadoProvedor` ou `EstimadoSistema`; `chave_isolamento` "nunca o `usuarioId` em claro"; índices em `execucao_id`, `iniciada_em` e (`chave_isolamento`, `iniciada_em`); visão `consumo.vw_consumo_diario` por dia, modelo e tipo de chamada; `Persistencia:TimeoutPostgres` aplicado ao comando e à conexão (constitution 5.4)
- [ ] T030 [P] Criar `src/AuditoriaNFe.Infrastructure/Catalogos/precos-openai.v1.json` com preços por milhão de tokens (entrada, entrada em cache e saída), em USD, do modelo candidato, a URL da página oficial de preços da OpenAI e a data da consulta. Os valores DEVEM vir da página oficial no momento da tarefa; nenhum preço é presumido (DP-22, P02)
- [ ] T031 Implementar `src/AuditoriaNFe.Infrastructure/Ia/CalculadoraCusto.cs` (tokens × preço ÷ 1.000.000, 8 casas, `MidpointRounding.AwayFromZero`; NULL sem preço ou sem tokens) e os testes em `tests/AuditoriaNFe.Tests/Aplicacao/Consumo/CalculadoraCustoTests.cs` (research R-16) (depende de T030)
- [ ] T032 Implementar `src/AuditoriaNFe.Infrastructure/Ia/RegistroConsumoChatClient.cs` (`DelegatingChatClient`): uma linha por chamada ao provedor, inclusive cada volta do loop de ferramentas e cada retentativa (`tentativa` ≥ 1, lida de um contexto da execução), tokens de `ChatResponse.Usage` com NULL quando ausentes, `status`, duração, versão da tabela de preços; falha ao gravar é registrada em log e métrica e marca `registroCompleto = false` no resumo da execução, sem alterar o resultado (research R-09, R-16; P14) (depende de T029, T031)
- [ ] T033 [P] Implementar `src/AuditoriaNFe.Infrastructure/Ia/Simulacao/ChatClientSimulado.cs`, provedor simulado para uso local com `Ia:Provedor=Simulado`, que nunca apresenta dado simulado como real e é recusado fora de `Development` (plan.md, Structure Decision)
- [ ] T034 Implementar `src/AuditoriaNFe.Infrastructure/Ia/FabricaChatClient.cs`: `IChatClient` OpenAI por Chat Completions, sem armazenamento no provedor (R-03), com `UseOpenTelemetry()` sem dados sensíveis e `RegistroConsumoChatClient`; escolhe o simulado quando configurado (depende de T032, T033)
- [ ] T035 [P] Criar os health checks em `src/AuditoriaNFe.Infrastructure/Saude/` para MongoDB, PostgreSQL e diretório `Aprovacao:Diretorio` gravável, sem expor dados nem detalhes internos (research R-21)

### Endpoints e composição

- [ ] T036 [P] Criar `src/Api.Agente/Identificacao/LeitorIdentificacaoHttp.cs`, que lê `X-Usuario-Id` e `X-Chamador-Id` e monta a `IdentificacaoSolicitante`; formato inválido → 400 com ERR-007, sem execução (FR-052a, FR-052c)
- [ ] T037 [P] Criar `src/Api.Agente/Erros/MapeamentoErrosHttp.cs` com o mapeamento do plano: ERR-001, ERR-003, ERR-007 e ERR-021 → 400; ERR-002 → 413; ERR-005 → 404; ERR-006 → 410; ERR-020 → 409; ERR-022 e ERR-025 → 503; ERR-024 → 429 com `Retry-After`; corpo com o envelope e o objeto `erro` (constitution 4.4)
- [ ] T038 Criar `src/Api.Agente/Program.cs` e `src/Api.Agente/appsettings.json` (sem segredos): opções, OpenTelemetry com `execucaoId` e `correlacaoId` em todos os logs e sem conteúdo de XML, pergunta, resposta, contexto do usuário ou `usuarioId` em claro (ADR-0009, P16); OpenAPI e Swagger UI só em `Development` (R-20); guarda do D-05 que recusa iniciar fora de `Development` enquanto `Seguranca:AutenticacaoHabilitada=false` (R-17); limite de corpo `Limites:CorpoHttpBytes`; limitação de taxa em janela fixa por `chaveIsolamento` (`LimiteTaxa:PermissoesPorJanela`, `LimiteTaxa:Janela`) nas rotas de análise e de decisão; limite de concorrência `Concorrencia:MaxExecucoes`; rejeição pelos dois limites com ERR-024 e `Retry-After`; migrações quando `Persistencia:AplicarMigracoes=true`; `/health/live` e `/health/ready` (depende de T025, T028, T029, T034 a T037)

### Infraestrutura de testes

- [ ] T039 [P] Criar os duplos em `tests/AuditoriaNFe.Tests/Duplos/`: `RelogioControlado.cs` e `ChatClientRoteirizado.cs`, que devolve respostas roteirizadas (válidas, inválidas, exceção, tempo esgotado, com e sem `Usage`) (research R-23)
- [ ] T040 [P] Criar `tests/AuditoriaNFe.Tests/Fixtures/Xml/GeradorNFeSintetica.cs`, que monta NF-e sintéticas (CNPJs fictícios, chave de acesso com DV calculado, modelo, `dhEmi` e `dhRecbto` com deslocamento, itens e totais coerentes, com ou sem `protNFe`, `ds:Signature` sintética apenas estrutural, porque a assinatura não é validada na v1) a partir de um modelo mínimo ajustável por pacote (P17, research R-23)
- [ ] T041 Criar `tests/AuditoriaNFe.Tests/Integracao/Infra/AmbienteIntegracao.cs`: Testcontainers com `mongo:8.0.32` e `pgvector/pgvector:0.8.7-pg17` executando os scripts de `infra/`, `WebApplicationFactory<Program>` em `Development` com `ChatClientRoteirizado`, `RelogioControlado` e `Aprovacao:Diretorio` temporário (depende de T038, T039)
- [ ] T042 Criar `tests/AuditoriaNFe.Tests/Integracao/HostTests.cs`: o host em `Production` com `Seguranca:AutenticacaoHabilitada=false` não inicia; `/swagger` e `/openapi/v1.json` não existem fora de `Development`; `/health/ready` responde sem dados (depende de T041)
- [ ] T043 Criar `tests/AuditoriaNFe.Tests/Integracao/ConsumoTests.cs`: uma linha por chamada e por retentativa; tokens ausentes gravados como NULL; custo NULL para modelo sem preço; PostgreSQL parado faz o registro falhar dentro de `Persistencia:TimeoutPostgres`, não altera o resultado e marca registro incompleto (NFR-009, P14, constitution 5.4) (depende de T041)

**Checkpoint**: host sobe em `Development`, recusa outros ambientes, grava consumo e responde aos
health checks; a infraestrutura de testes de integração está pronta.

---

## Phase 3: User Story 1 - Analisar um lote de XMLs e receber um parecer (Priority: P1) 🎯 MVP

**Goal**: pelo canal direto, receber um lote de XMLs, validar estrutura (pacote vigente na data de
referência) e totalizadores (catálogo aprovado), executar a análise por IA e devolver o parecer com
resultado geral calculado pelo sistema.

**Independent Test**: enviar por `POST /api/v1/execucoes` um lote com XMLs válidos, com erro de
layout e com divergência de somatório, e conferir que o parecer aponta exatamente os problemas
conhecidos de cada arquivo, com pacote e data de referência por documento.

### Tests for User Story 1 ⚠️

> Escrever primeiro e confirmar que falham.

- [ ] T044 [P] [US1] Gerar as fixtures com `GeradorNFeSintetica`: uma NF-e válida e uma `nfeProc` válida por pacote em `tests/AuditoriaNFe.Tests/Fixtures/Xml/<pacote>/` (19 pastas com os nomes de `manifesto.json`, data de referência dentro da vigência) e os casos em `tests/AuditoriaNFe.Tests/Fixtures/Xml/Casos/`: malformado, violação de schema, NFC-e modelo 65 válida no schema, modelo divergente entre `ide/mod` e chave, versão 3.10, evento de cancelamento, instruções ao agente em `infCpl`, totalizador divergente, erro de layout e de somatório juntos, duplicado e mesma chave com conteúdo divergente
- [ ] T045 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Dominio/IdentificacaoDocumentoTests.cs`: TS-052 (NFC-e → ERR-009 sem schema), TS-053 (modelo divergente → ERR-009), raiz ou namespace diferente → ERR-009, versão diferente de `4.00` → ERR-010, extração de `dhEmi` e `dhRecbto`
- [ ] T046 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Dominio/SelecaoPacoteTests.cs`: TS-048 (com protocolo, usa `dhRecbto` no pacote seguinte), TS-049 (sem protocolo, usa `dhEmi`), TS-051 (data anterior ao primeiro pacote → ERR-023), data posterior à data atual → ERR-023, limites de início e fim de cada vigência do manifesto, data de calendário no deslocamento do XML, e TS-060, TS-061 e TS-062 com uma tabela de vigência sintética que tem `fimConvivencia` (FR-010e, FR-010f)
- [ ] T047 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Dominio/Regras/RegrasICMSTotTests.cs` com um caso válido e um inválido por regra e por versão do grupo ICMSTot do catálogo, incluindo as quatro versões da W16-10 (tolerância de R$ 0,50 nos dois lados do limite, exceções de `tpOp = 2` e CFOP iniciado por 3), as duas versões da W07-10, as regras monofásicas com tolerância 0,01 nas quantidades e a não aplicação fora da vigência (FR-013a, research R-15)
- [ ] T048 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Dominio/Regras/RegrasISSQNtotTests.cs` com um caso válido e um inválido por regra do grupo ISSQNtot do catálogo
- [ ] T049 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Dominio/Regras/RegrasIbsCbsIsTests.cs` com um caso válido e um inválido por regra de IBS, CBS e IS do catálogo e a não aplicação quando os grupos não estão preenchidos
- [ ] T050 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Dominio/Regras/RegrasCobrancaPagamentoTests.cs` com um caso válido e um inválido por regra dos grupos Y e YA do catálogo (Y05-10, Y06-10, Y10-10, YA03-20, YA09-10 com a soma de `vPag`, pendência C5)
- [ ] T051 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Dominio/ResultadoGeralTests.cs`: as três regras de data-model §10 (TS-001, TS-009, TS-046), ERR-023 nunca resulta em "Sem inconsistências", etapa 5 `NaoAplicavel` não impede o resultado e gera limitação e revisão humana, e os critérios de revisão humana do FR-024
- [ ] T052 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Dominio/DuplicidadeTests.cs`: mesma chave e mesmo hash → ERR-011 Informativa e análise única; mesma chave e hash diferente → ERR-012 Alta, sem descartar arquivo (FR-009)
- [ ] T053 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Integracao/SchemasPorPeriodoTests.cs`: TS-050 e SC-011, cada fixture de T044 sem erro estrutural e validada contra o pacote da sua pasta
- [ ] T054 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Integracao/CargaSchemasCatalogosTests.cs`: a inicialização falha com SHA-256 divergente, XSD ausente, arquivo fora do manifesto, vigências sobrepostas ou com lacuna, regra sem fonte ou sem vigência, implementação inexistente, versões sobrepostas e regra com campo fora do catálogo de extração (data-model §5 e §6)
- [ ] T055 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Aplicacao/AnaliseIaTests.cs` com `ChatClientRoteirizado`: TS-018 (instrução em `infCpl` não altera comportamento nem contrato), TS-027 (provedor indisponível → ERR-014, determinísticos entregues, "Inconclusiva"), TS-028 (resposta inválida em todas as tentativas → ERR-015 após `Ia:MaxTentativas`, conteúdo descartado, cada tentativa no consumo), ERR-016 por `Execucao:TempoMaximo`, indício sem evidência verificável rejeitado, indício que tenta alterar achado determinístico ignorado (FR-021) e prompt sem CPF, nomes de pessoa física, endereço, telefone ou e-mail (ADR-0004)
- [ ] T056 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Aplicacao/ConsultaManuaisTests.cs`: TS-046 (MCP desabilitado → etapa "não aplicável — integração não habilitada", limitação e revisão humana), TS-026 e TS-047 (habilitado e indisponível → ERR-013, "Inconclusiva"), TS-038 (sem trechos → "Ausência de evidências" e revisão humana), FR-047d no parecer
- [ ] T057 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Integracao/ExecucoesLoteTests.cs` por `POST /api/v1/execucoes`: TS-001 a TS-013 (TS-006 exatamente no limite aceito; TS-007 um byte acima → ERR-002 em até 2 s, sem processamento parcial; TS-008 → ERR-003; ERR-001 sem pergunta e sem arquivos), TS-036 (XML original não persistido), `GET /api/v1/execucoes/{execucaoId}` com outra identificação → ERR-005, TS-063 (acima de `LimiteTaxa:PermissoesPorJanela` → 429 com ERR-024 e `Retry-After`, sem análise) e TS-064 (contêiner do MongoDB parado → ERR-025 dentro de `Persistencia:TimeoutMongo`, sem parecer parcial)
- [ ] T058 [P] [US1] Criar os testes de contrato em `tests/AuditoriaNFe.Tests/Contrato/`: `OpenApiContratoTests.cs` (documento gerado igual a `contracts/api-agente.openapi.yaml`), `EnvelopeV1ContratoTests.cs` (envelope conforme `EnvelopeV1`) e `ContratoIaV1SchemaTests.cs` (schema gerado pelo tipo igual a `contracts/contrato-ia-v1.schema.json`, R-05)
- [ ] T059 [P] [US1] Criar `tests/AuditoriaNFe.Tests/Integracao/RastreabilidadeExecucaoTests.cs` (NFR-008, P16): a partir só do `execucaoId`, reconstruir as nove etapas com início, fim e estado, o pacote aplicado e a data de referência de cada documento, as `regrasAplicadas` com versão e resultado, os `trechosConsultados` e as linhas de `consumo.chamadas_provedor` da execução
- [ ] T060 [P] [US1] Criar o conjunto de avaliação da análise por IA em `tests/AuditoriaNFe.Tests/Avaliacao/conjunto-avaliacao-analise.v1.json` (pelo menos 20 lotes sintéticos, com divergências conhecidas em cada grupo de regra e lotes sem divergência) e `tests/AuditoriaNFe.Tests/Avaliacao/AvaliacaoAnaliseTests.cs` com trait `Categoria=Avaliacao`, medindo os critérios do plano: 0 indícios sem evidência verificável, 0 indícios que contradizem ou repetem como novo um achado determinístico e 0 valores não rastreáveis no resumo (P17)

### Implementation for User Story 1

- [ ] T061 [P] [US1] Criar `ArquivoRecebido`, `IdentificacaoDocumento` e `DocumentoNFe` em `src/AuditoriaNFe.Domain/Documentos/` (data-model §3 e §4): `nome` "saneado, sem caminho"; `elementoRaiz` "`NFe` ou `nfeProc`; outro valor → não suportado"; `namespace` "DEVE ser `http://www.portalfiscal.inf.br/nfe`"; `versaoLayout` "só `4.00` é suportada (ERR-010)"; `modeloIde` / `modeloChave` "`ide/mod` e posições 21–22 da chave; os dois DEVEM ser `55` (FR-007a); NFC-e (`65`) → ERR-009"; `chaveAcesso` "44 caracteres, do atributo `Id` de `infNFe` sem o prefixo `NFe`"; `validacaoConteudo` "`NaoRealizada — layout inválido` quando o layout falhou (FR-015)"; `regrasAplicadas` com `regraId`, `versaoRegra` e `resultado` "`Conforme`, `Divergente`, `NaoAplicavel` com condição ou grupo ausente"
- [ ] T062 [P] [US1] Criar `PacoteSchema`, `VigenciaPacote` e `SeletorPacote` em `src/AuditoriaNFe.Domain/Schemas/` (data-model §5): data de referência "Data de calendário de `dhRecbto` (com protocolo) ou de `dhEmi`"; seleção "pacote com `vigenciaInicio ≤ dataReferencia ≤ vigenciaFim` (fim aberto = data atual do relógio do sistema). Nenhum pacote → ERR-023"; convivência genérica por `fimConvivencia`, com `pacoteVigente`, `pacoteAplicado`, `pacotesTentados` e `emConvivencia`
- [ ] T063 [P] [US1] Criar `RegraConteudo`, `IImplementacaoRegra` e `AplicadorRegras` em `src/AuditoriaNFe.Domain/Regras/` (data-model §6): versão aplicada "é a vigente na `dataReferencia`"; documento fora de toda vigência não gera achado; comparação em `decimal` exato salvo `tolerancia`; campo opcional ausente vale zero; achado `InconsistenciaNegocio` com arquivo, item, campos, valor informado, valor calculado, `regraId`, `versaoRegra` e severidade do catálogo (FR-014, FR-023); toda regra avaliada registrada em `regrasAplicadas` do arquivo (NFR-008)
- [ ] T064 [P] [US1] Implementar as regras do grupo ICMSTot em `src/AuditoriaNFe.Domain/Regras/ICMSTot/`, uma classe por regra e versão do catálogo aprovado, com as condições e exceções da tabela de versões da W16-10 (depende de T063)
- [ ] T065 [P] [US1] Implementar as regras do grupo ISSQNtot em `src/AuditoriaNFe.Domain/Regras/ISSQNtot/` (depende de T063)
- [ ] T066 [P] [US1] Implementar as regras de IBS, CBS e IS em `src/AuditoriaNFe.Domain/Regras/IbsCbsIs/`, aplicadas só com os grupos preenchidos (depende de T063)
- [ ] T067 [P] [US1] Implementar as regras de cobrança e pagamento em `src/AuditoriaNFe.Domain/Regras/CobrancaPagamento/` (depende de T063)
- [ ] T068 [P] [US1] Criar `Parecer`, `CalculadoraResultadoGeral` e `RevisaoHumana` em `src/AuditoriaNFe.Domain/Parecer/` (data-model §10): `resultadoGeral` `SemInconsistencias`, `ComInconsistencias` ou `Inconclusiva` calculado pelo sistema, nunca pela IA; limitações "consulta a manuais oficiais não habilitada" e "usuário não identificado"; o contexto do usuário não participa do cálculo (FR-058)
- [ ] T069 [P] [US1] Criar `src/AuditoriaNFe.Domain/Documentos/DetectorDuplicidade.cs` por chave de acesso e SHA-256 do conteúdo (FR-009, ERR-011, ERR-012)
- [ ] T070 [US1] Implementar `src/AuditoriaNFe.Infrastructure/Xml/LeitorXmlSeguro.cs`: `DtdProcessing.Prohibit`, `XmlResolver = null`, `MaxCharactersInDocument` e profundidade `Xml:ProfundidadeMaxima`; XML bem formado decidido pelo conteúdo, não pela extensão (FR-006, ERR-008)
- [ ] T071 [US1] Implementar `src/AuditoriaNFe.Infrastructure/Xml/IdentificadorNFe.cs` (FR-007, FR-007a, FR-008): raiz, namespace, `versao`, `ide/mod`, modelo na chave, protocolo, `dhEmi` e `dhRecbto` como no XML; não suportado não segue para o schema (depende de T061, T070)
- [ ] T072 [US1] Implementar `src/AuditoriaNFe.Infrastructure/Schemas/ProvedorPacotesSchema.cs` e `ResolvedorRestritoPacote.cs`: carga de `Schemas/NFe/manifesto.json` com as validações de data-model §5 (SHA-256 de cada XSD, nenhum arquivo fora do manifesto, vigências sem sobreposição nem lacuna, só o último com fim aberto, compilação com `TNFe` e `TNfeProc`), falha que impede a inicialização, resolvedor que só abre arquivos dentro da pasta do pacote e `XmlSchemaSet` compilado sob demanda e mantido em cache por pacote (research R-14) (depende de T062)
- [ ] T073 [US1] Implementar `src/AuditoriaNFe.Infrastructure/Xml/ValidadorXsd.cs`: `NFe` contra `nfe_v4.00.xsd` e `nfeProc` contra `procNFe_v4.00.xsd` do pacote selecionado, nova tentativa no pacote anterior quando a data estiver na convivência, cada violação como `ErroEstrutural` com linha, posição, caminho e pacote (FR-010, FR-010f, FR-011) (depende de T072)
- [ ] T074 [P] [US1] Criar `src/AuditoriaNFe.Infrastructure/Catalogos/catalogo-extracao.v1.json` (data-model §4): lista fechada com caminho no XML, tipo e classe de dado, cobrindo os totais de `ICMSTot`, `ISSQNtot`, `ISTot` e `IBSCBSTot`, os campos dos itens somados pelas regras, `cobr/fat`, `cobr/dup`, `pag`, `ide`, CNPJ do emitente e do destinatário e os campos das perguntas da avaliação
- [ ] T075 [US1] Criar `src/AuditoriaNFe.Infrastructure/Catalogos/catalogo-regras.v1.json` a partir de `catalogo-regras-totalizadores.md` (research R-15, data-model §6): uma entrada por versão de regra, com `id`, `versaoRegra`, `grupo`, `descricao` com a fórmula, `fonte` (`documento`, `versao`, `paginas`), `vigencia`, `severidade`, `tolerancia`, `aplicacaoSefaz`, `condicoes` e `implementacao`; cabeçalho com `versaoCatalogo`, `aprovacao` (`por`, `em`) e `fontes[]` com os SHA-256 de `docs/notas-tecnicas/inventario.csv`. Nenhuma regra fora do catálogo aprovado (P02) (depende de T007, T074)
- [ ] T076 [US1] Implementar `src/AuditoriaNFe.Infrastructure/Catalogos/CarregadorCatalogos.cs`, que valida os dois catálogos na inicialização (regra sem fonte, sem vigência, com implementação inexistente, com campo fora do catálogo de extração ou com versões sobrepostas impede a subida) e registra as versões para cada execução (depende de T063, T074, T075)
- [ ] T077 [US1] Implementar `src/AuditoriaNFe.Infrastructure/Xml/ExtratorDados.cs`, que extrai só os campos do catálogo de extração, cada valor com o caminho no XML, e descarta o conteúdo do arquivo ao fim da execução (FR-037a) (depende de T074)
- [ ] T078 [P] [US1] Criar `src/AuditoriaNFe.Infrastructure/Saude/SaudeSchemasCatalogos.cs` e registrá-lo no `/health/ready` (depende de T072, T076)
- [ ] T079 [P] [US1] Criar `src/AuditoriaNFe.Application/Abstracoes/IConsultaManuais.cs`, `src/AuditoriaNFe.Infrastructure/Mcp/ConsultaManuaisDesabilitada.cs` (etapa 5 "não aplicável — integração não habilitada") e `src/AuditoriaNFe.Infrastructure/Mcp/ConsultaManuaisMcp.cs` (`McpClient` com `HttpClientTransport` em `StreamableHttp`, `Mcp:Timeout`, `Mcp:QuantidadeMaximaTrechos`, falha → ERR-013) conforme `contracts/consulta-manuais-mcp.md` (FR-047a a FR-047d, R-13); trechos recuperados registrados em `Execucao.trechosConsultados` (NFR-008)
- [ ] T080 [P] [US1] Criar `src/AuditoriaNFe.Application/Contratos/ContratoIaV1.cs` (data-model §15): `versaoContrato` "Constante `"1.0"`; versão diferente é rejeitada como saída inválida"; `indicios[]` com `evidencias[]` "(≥ 1)"; `ausenciasEvidencia[]`; `resposta`; `foraDeEscopo`; `informacaoIndisponivel`; `recomendacoes[]`; `resumo`; sem ids, datas, consumo, resultado geral nem contexto do usuário (P09, FR-057)
- [ ] T081 [P] [US1] Criar as instruções versionadas em `src/AuditoriaNFe.Application/Agente/Instrucoes/instrucoes-agente.v1.md` (recurso embutido, pt-BR): dados do XML, da pergunta, do contexto do usuário e das ferramentas entram como conteúdo delimitado e não confiável (P07, FR-053); a IA não altera achados determinísticos nem calcula valores
- [ ] T082 [P] [US1] Implementar `src/AuditoriaNFe.Application/Servicos/MinimizadorDadosIa.cs` conforme ADR-0004: envia só dados extraídos necessários e resultados determinísticos, mascara CPF, nomes de pessoa física, endereços, telefones e e-mails e nunca envia o XML integral; lista de campos em configuração versionada
- [ ] T083 [US1] Implementar `src/AuditoriaNFe.Application/ContextProviders/ContextoExecucaoProvider.cs` com `ProviderSessionState<T>`: resultados determinísticos, estado das etapas, pacote e data de referência por documento e, quando houver, os trechos de manual recuperados (research R-07) (depende de T082)
- [ ] T084 [US1] Implementar `src/AuditoriaNFe.Application/Agente/FabricaAgenteInterno.cs`: `ChatClientAgent` com `Id` estável, instruções de T081, `ContextoExecucaoProvider` e execução por `RunAsync<ContratoIaV1>` (research R-05, §3.3) (depende de T080, T081, T083)
- [ ] T085 [US1] Implementar `src/AuditoriaNFe.Application/Middleware/MiddlewareExecucaoAgente.cs` (`AsBuilder().Use(runFunc:, runStreamingFunc:)`): correlação, `Ia:TimeoutChamada`, exceções do provedor → ERR-014, logs sem conteúdo (research R-09)
- [ ] T086 [US1] Implementar `src/AuditoriaNFe.Application/Servicos/ValidadorSaidaIa.cs` para indícios, ausências de evidência e resumo: validação estrutural e semântica (evidência `CampoXml` existe nos dados extraídos, `RegraCatalogo` existe no catálogo, `TrechoManual` pertence ao conjunto recuperado, achado citado existe), nova tentativa com mensagem de correção até `Ia:MaxTentativas` e ERR-015 ao esgotar (P08, data-model §15) (depende de T080)
- [ ] T087 [US1] Implementar `src/AuditoriaNFe.Application/Servicos/MontadorParecer.cs`: `resumoQuantitativo`, `documentos` com `dataReferencia`, `origemDataReferencia`, `pacoteAplicado`, `pacotesTentados` e `emConvivencia`, achados, limitações, validações não realizadas, revisão humana, `integracaoMcp`, versões e `resumoTextual` da IA validada ou de modelo de texto do sistema quando a IA estiver indisponível (FR-017, FR-010d, data-model §10) (depende de T068)
- [ ] T088 [US1] Implementar `src/AuditoriaNFe.Application/Fluxo/FluxoNoveEtapas.cs` e `src/AuditoriaNFe.Application/CasosDeUso/ExecutarSolicitacaoAuditoria.cs`: as nove etapas da constitution 3.4 na ordem do plano, com estado e justificativa por etapa; limites e conteúdo mínimo antes de ler os XMLs (ERR-001 a ERR-003); falha nas etapas 1 e 2 interrompe; erros por arquivo não impedem os demais; conteúdo só para layout aprovado (FR-015); etapa 6 "não aplicável" (sem ferramentas externas na v1); tempo máximo `Execucao:TempoMaximo` com ERR-016 e etapas concluídas e não executadas registradas; nova sessão quando não houver `sessaoId`; persistência da execução e dos documentos; XML descartado ao fim (depende de T069 a T073, T076, T077, T079, T084 a T087)
- [ ] T089 [US1] Implementar `src/Api.Agente/Endpoints/ExecucoesEndpoints.cs`: `POST /api/v1/execucoes` (multipart com `pergunta`, `arquivos[]`, `sessaoId`, `correlacaoId`) e `GET /api/v1/execucoes/{execucaoId}` conferindo a `chaveIsolamento` (ERR-005), respostas no envelope v1 e rejeição por limite em até 2 s (NFR-002) (depende de T088)
- [ ] T090 [US1] Criar `tests/AuditoriaNFe.Tests/Integracao/LogsSemConteudoTests.cs`, que captura os logs de uma execução completa e falha se encontrar trecho de XML, pergunta, resposta, CPF ou `usuarioId` em claro, ou linha sem `execucaoId` (NFR-007, P16) (depende de T089)

**Checkpoint**: US1 funciona sozinha pelo canal direto; TS-001 a TS-013, TS-018, TS-026 a TS-028,
TS-036, TS-038, TS-046 a TS-053, TS-063, TS-064 e o teste de rastreabilidade passam. É o MVP.

---

## Phase 4: User Story 4 - Continuar a conversa na mesma sessão (Priority: P2)

**Goal**: continuidade por `sessaoId` com isolamento por usuário e chamador, expiração,
encerramento, histórico reduzido sem perder resultados e o contexto do usuário entre sessões
(FR-036 a FR-042, FR-055 a FR-060).

**Independent Test**: analisar um lote, guardar o `sessaoId`, fazer uma pergunta de continuidade sem
reenviar arquivos e verificar que a resposta usa os resultados anteriores; repetir com outro
`X-Usuario-Id` e receber ERR-005.

### Tests for User Story 4 ⚠️

- [ ] T091 [P] [US4] Criar `tests/AuditoriaNFe.Tests/Integracao/SessoesTests.cs`: TS-021 (sessão criada e continuidade sem reenvio), TS-022 (novos arquivos incorporados com duplicidade detectada contra a sessão), TS-023 e SC-009 (outro usuário ou outro chamador → ERR-005 sem revelar existência), TS-024 (expirada → ERR-006 com `RelogioControlado`), TS-025 (encerrada → indisponível)
- [ ] T092 [P] [US4] Criar `tests/AuditoriaNFe.Tests/Aplicacao/HistoricoSessaoTests.cs`: histórico reduzido por `Historico:MaxMensagens` mantém os resultados das análises disponíveis (FR-042); pergunta em sessão sem análise informa que não há base analisada
- [ ] T093 [P] [US4] Criar `tests/AuditoriaNFe.Tests/Dominio/ContextoUsuarioTests.cs`: TS-057 (CNPJs com contagem, achados recorrentes e preferência, cada um com origem; sem conteúdo do XML, valores ou texto livre), só documentos aprovados no layout contam, só achados `Deterministica` contam, mesma execução não conta duas vezes, limites `ContextoUsuario:MaxCnpjs` e `ContextoUsuario:MaxAchados` mantêm os mais recentes, `expiraEm` renovado
- [ ] T094 [P] [US4] Criar `tests/AuditoriaNFe.Tests/Integracao/ContextoUsuarioTests.cs`: TS-054 e SC-012 (outro usuário ou chamador não vê o contexto), TS-056 (exclusão pelo canal direto), TS-058 (prazo reduzido: leitura ignora vencido e nova execução cria contexto vazio), TS-059 ("anonimo" e "nao-informado": nenhum contexto lido ou gravado, parecer indica usuário não identificado), `$jsonSchema` recusa campo fora de data-model §13
- [ ] T095 [P] [US4] Criar `tests/AuditoriaNFe.Tests/Aplicacao/ContextoUsuarioIaTests.cs`: TS-055, resposta da IA que tenta gravar ou alterar o contexto não muda nada (FR-057)

### Implementation for User Story 4

- [ ] T096 [US4] Implementar `src/AuditoriaNFe.Application/Sessoes/ServicoSessoes.cs`: criar ou continuar sessão, conferir `chaveIsolamento` (ERR-005), expiração por `Sessao:ExpiracaoInatividade` (ERR-006), sessão encerrada recusada com mensagem própria, `ultimaAtividadeEm` atualizada a cada solicitação aceita (FR-036, FR-039, FR-040)
- [ ] T097 [US4] Persistir a `AgentSession` do agente interno no campo `historicoAgente` da sessão por `SerializeSessionAsync` e `DeserializeSessionAsync` em `src/AuditoriaNFe.Infrastructure/Persistencia/Mongo/SessaoRepositorio.cs`, isolada pela chave (research R-08)
- [ ] T098 [US4] Implementar `src/AuditoriaNFe.Application/ContextProviders/ContextoSessaoProvider.cs` (resumo da base da sessão) e configurar a redução de histórico por `Historico:MaxMensagens`, confirmando na versão 1.26.0 o ponto de configuração do redutor (research R-08)
- [ ] T099 [US4] Integrar a continuidade em `src/AuditoriaNFe.Application/CasosDeUso/ExecutarSolicitacaoAuditoria.cs`: `sessaoId` informado, incorporação de novos documentos à base da sessão com duplicidade contra `hashesPorChave` (FR-038, FR-009) (depende de T096 a T098)
- [ ] T100 [P] [US4] Implementar `src/Api.Agente/Endpoints/SessoesEndpoints.cs` com `DELETE /api/v1/sessoes/{sessaoId}` (FR-041) (depende de T096)
- [ ] T101 [P] [US4] Criar `ContextoUsuario` em `src/AuditoriaNFe.Domain/ContextoUsuario/ContextoUsuario.cs` (data-model §13): `_id` = `chaveIsolamento`; `cnpjsAuditados[]` com `cnpj` "14 caracteres, numérico ou alfanumérico", `papel` "`Emitente` ou `Destinatario`", `quantidadeDocumentos`, `ultimaAuditoriaEm`, `origem`; `achadosRecorrentes[]` com `chave` "`regraId` ou tipo de erro estrutural", `categoria`, `frequencia`, `ultimaOcorrenciaEm`, `origem`; `preferencias` com `nivelDetalhe` "`Resumido` ou `Detalhado`" e `tributosInteresse[]` "(lista fechada)"; `origem` com `tipo` "`Execucao` ou `DeclaracaoUsuario`"; `execucoesContabilizadas` limitada; `expiraEm` "`ultimaAtividadeEm` + `ContextoUsuario:RetencaoInatividade` (90 dias)"
- [ ] T102 [US4] Implementar `src/AuditoriaNFe.Infrastructure/Persistencia/Mongo/ContextoUsuarioRepositorio.cs`: leitura e gravação pela `chaveIsolamento`, leitura que "ignora documento com `expiraEm` vencido, mesmo antes da remoção pelo TTL", exclusão total e de itens (depende de T101)
- [ ] T103 [US4] Implementar `src/AuditoriaNFe.Application/Servicos/AtualizadorContextoUsuario.cs` chamado na etapa 9: idempotente por execução, só para usuário identificado, a partir dos resultados determinísticos; marca `contextoUsuarioAtualizado` (FR-055a, FR-055b, FR-057) (depende de T102)
- [ ] T104 [US4] Implementar `src/AuditoriaNFe.Application/ContextProviders/ContextoUsuarioProvider.cs`, só leitura, dado delimitado e não confiável, limitado por `ContextoUsuario:MaxItensNoPrompt`, sem uso para "anonimo" e sem implementação de gravação em `StoreAIContextAsync` (research R-07, FR-058) (depende de T102)
- [ ] T105 [US4] Implementar `src/AuditoriaNFe.Application/CasosDeUso/GerirContextoUsuario.cs`: consultar, declarar ou corrigir preferências, remover CNPJ, remover achado recorrente e excluir o contexto; cada declaração renova `expiraEm` (FR-059, FR-060) (depende de T102)
- [ ] T106 [US4] Implementar `src/Api.Agente/Endpoints/ContextoUsuarioEndpoints.cs`: `GET /api/v1/contexto-usuario`, `PUT /api/v1/contexto-usuario/preferencias`, `DELETE /api/v1/contexto-usuario/cnpjs/{cnpj}`, `DELETE /api/v1/contexto-usuario/achados-recorrentes/{chave}` e `DELETE /api/v1/contexto-usuario` (depende de T105)

**Checkpoint**: US1 e US4 funcionam juntas; TS-021 a TS-025 e TS-054 a TS-059 passam.

---

## Phase 5: User Story 3 - Fazer perguntas sobre a base sem receber valores inventados (Priority: P2)

**Goal**: responder perguntas só com dados extraídos e cálculos determinísticos registrados, com
documentos incluídos e excluídos declarados, e recusar estimativas e perguntas fora do escopo
(FR-026 a FR-032).

**Independent Test**: com uma base conhecida, fazer perguntas de resposta conhecida, incluindo sobre
valores inexistentes, e verificar que todo valor monetário da resposta é rastreável a um campo de
XML ou a um `calculoId` registrado.

### Tests for User Story 3 ⚠️

- [ ] T107 [P] [US3] Criar `tests/AuditoriaNFe.Tests/Dominio/CalculadoraBaseTests.cs`: somas por campo catalogado com documentos incluídos e excluídos (erro estrutural, duplicidade) e motivo (FR-031)
- [ ] T108 [P] [US3] Criar `tests/AuditoriaNFe.Tests/Dominio/VerificadorValoresCitadosTests.cs`: valor com `calculoId` igual ao resultado, valor com {chave, caminho} igual ao extraído e número em formato monetário fora de `valoresCitados` rejeitado (FR-027, FR-028)
- [ ] T109 [P] [US3] Criar `tests/AuditoriaNFe.Tests/Ferramentas/FerramentasLeituraTests.cs`: para cada ferramenta, schema JSON da `AIFunction` igual ao de `contracts/ferramentas-agente.md` (nome, argumentos, limites e saída), argumentos inválidos com `ArgumentoInvalido` ou `CampoNaoCatalogado`, lista permitida por tipo de execução (contrato, seção 3), ferramenta fora da lista permitida encerrada com `context.Terminate`, `Ferramentas:Timeout` e ausência de acesso ao contexto do usuário (constitution 4.6, P10)
- [ ] T110 [P] [US3] Criar `tests/AuditoriaNFe.Tests/Aplicacao/PerguntasTests.cs` com `ChatClientRoteirizado`: TS-014 (valor total com arquivos, campos e cálculo), TS-015 (ERR-017 sem estimativa), TS-016 (conjunto adversarial: nenhum valor não rastreável chega ao envelope), TS-017 (ERR-018), TS-037 (campo não extraído: limitação e orientação de reenvio)
- [ ] T111 [P] [US3] Criar `tests/AuditoriaNFe.Tests/Integracao/PerguntasTests.cs`: TS-014 e TS-015 ponta a ponta pelo `POST /api/v1/execucoes` com `pergunta` e `sessaoId`; estender `tests/AuditoriaNFe.Tests/Integracao/RastreabilidadeExecucaoTests.cs` para reconstruir as `chamadasFerramentas` de uma pergunta com cálculo

### Implementation for User Story 3

- [ ] T112 [P] [US3] Criar `CalculadoraBase` e `RegistroCalculo` em `src/AuditoriaNFe.Domain/Calculos/` (data-model §9): `id` "Gerado pelo sistema e devolvido à IA na resposta da ferramenta", `operacao`, `campo` "Campo do catálogo de extração", `documentosIncluidos`, `documentosExcluidos` com motivo, `resultado` em `decimal`, `registradoEm`
- [ ] T113 [P] [US3] Criar `src/AuditoriaNFe.Domain/Rastreabilidade/VerificadorValoresCitados.cs` (data-model §15, validação semântica)
- [ ] T114 [US3] Implementar `src/AuditoriaNFe.Application/Ferramentas/FerramentasLeitura.cs` e `PoliticaFerramentas.cs` com `AIFunctionFactory.Create` e `[Description]`, seguindo `contracts/ferramentas-agente.md` v1: `listar_documentos`, `obter_campos_extraidos`, `calcular_total` (registra `calculoId` na execução) e `listar_achados`; todas só de leitura, com a lista permitida por tipo de execução da seção 3 do contrato (research R-06) (depende de T112)
- [ ] T115 [US3] Implementar `src/AuditoriaNFe.Application/Middleware/MiddlewareFunctionCalling.cs`: lista permitida por execução, validação de argumentos, timeout, registro de cada chamada em `Execucao.chamadasFerramentas` (nome, início, duração, status, erro, `calculoId`) e `context.Terminate` em violação (research R-09, P16) (depende de T114)
- [ ] T116 [US3] Estender `src/AuditoriaNFe.Application/Servicos/ValidadorSaidaIa.cs` para `resposta`: `valoresCitados` verificados por T113, `documentosExcluidos` quando houver cálculo, `foraDeEscopo` → ERR-018 e `informacaoIndisponivel` → ERR-017 como resposta e não como erro técnico, nova tentativa em falha (FR-028) (depende de T113)
- [ ] T117 [US3] Registrar as ferramentas e o middleware em `src/AuditoriaNFe.Application/Agente/FabricaAgenteInterno.cs` e acrescentar às instruções de `instrucoes-agente.v1.md` as regras de perguntas (só citar valores devolvidos por ferramentas; declarar indisponibilidade; escopo) (depende de T114, T115)
- [ ] T118 [US3] Tratar os tipos `Pergunta` e `AnaliseComPergunta` em `src/AuditoriaNFe.Application/CasosDeUso/ExecutarSolicitacaoAuditoria.cs`, com a resposta validada no envelope (FR-026, FR-032) (depende de T116, T117)
- [ ] T119 [P] [US3] Criar o conjunto de avaliação `tests/AuditoriaNFe.Tests/Avaliacao/conjunto-avaliacao.v1.json` (50+ perguntas, incluindo adversariais e de continuidade, sobre fixtures sintéticas) e `tests/AuditoriaNFe.Tests/Avaliacao/AvaliacaoTests.cs` com trait `Categoria=Avaliacao`, executado sob demanda com OpenAI real (SC-003, SC-008)

**Checkpoint**: TS-014 a TS-017 e TS-037 passam; nenhuma resposta do conjunto adversarial traz valor
não rastreável.

---

## Phase 6: User Story 5 - Receber recomendações demonstrativas identificadas (Priority: P3)

**Goal**: recomendações rotuladas, vinculadas a achados, sem valores não rastreáveis, classificadas
como sensíveis pelo sistema e retidas como "pendente de aprovação" sem conteúdo (FR-033 a FR-035c).

**Independent Test**: analisar um lote com inconsistências conhecidas e verificar que todas as
recomendações estão rotuladas, vinculadas a achados e não alteram o resultado, e que as sensíveis
aparecem sem conteúdo.

### Tests for User Story 5 ⚠️

- [ ] T120 [P] [US5] Criar `tests/AuditoriaNFe.Tests/Dominio/RecomendacaoTests.cs` e `ClassificadorSensibilidadeTests.cs`: rótulo e aviso constantes, `achadosVinculados` vazio rejeitado, estados e visibilidade, sensível por achado Alta, por `valoresCitados`, por valor monetário no texto e por termo de `Sensibilidade:TermosTributos`
- [ ] T121 [P] [US5] Criar `tests/AuditoriaNFe.Tests/Aplicacao/RecomendacoesTests.cs`: TS-019 (todas rotuladas, com aviso e vinculadas), TS-020 (parecer sem achados não recebe recomendação de problema inexistente), recomendação não altera resultado, achados nem revisão humana (FR-035), recomendação com achado inexistente descartada
- [ ] T122 [P] [US5] Criar `tests/AuditoriaNFe.Tests/Integracao/RecomendacoesParecerTests.cs`: TS-039, parecer entregue na hora com a recomendação sensível como "pendente de aprovação", só id e estado

### Implementation for User Story 5

- [ ] T123 [P] [US5] Criar `src/AuditoriaNFe.Domain/Recomendacoes/RecomendacaoDemonstrativa.cs` (data-model §11): `rotulo` constante "Recomendação demonstrativa"; `aviso` constante; `achadosVinculados` "≥ 1; DEVEM existir no parecer (FR-034)"; `valoresCitados` "Cada valor com `calculoId` ou {chave, caminho}"; `sensivel` "Calculado pelo sistema (FR-035a)"; `motivosSensibilidade` `AchadoSeveridadeAlta`, `MencionaValores`, `MencionaTributos`; `estado` `Liberada`, `PendenteAprovacao`, `Aprovada`, `Rejeitada`, `Indisponivel`; visibilidade "`Liberada` e `Aprovada` entregam o texto; `PendenteAprovacao` entrega só id e estado (FR-035c); `Rejeitada` e `Indisponivel` nunca entregam o texto (FR-035g)"
- [ ] T124 [US5] Implementar `src/AuditoriaNFe.Application/Servicos/ClassificadorSensibilidade.cs` com os critérios do FR-035a e a lista configurável `Sensibilidade:TermosTributos`; a IA não informa a classificação (depende de T123)
- [ ] T125 [US5] Estender `src/AuditoriaNFe.Application/Servicos/ValidadorSaidaIa.cs` para `recomendacoes[]`: achados vinculados existentes e valores rastreáveis (FR-034) (depende de T123)
- [ ] T126 [US5] Incluir as recomendações por estado em `src/AuditoriaNFe.Application/Servicos/MontadorParecer.cs` e persisti-las na execução, sem alterar resultado geral, achados nem revisão humana (depende de T124, T125)

**Checkpoint**: TS-019, TS-020 e TS-039 passam.

---

## Phase 7: User Story 7 - Aprovar a liberação de recomendações sensíveis (Priority: P3)

**Goal**: workflow de aprovação humana com `RequestPort` e checkpoints JSON locais, decisão única
pelo canal direto e registro de auditoria `CheckpointAprovacao` v1 (FR-035d a FR-035j, desvio D-03).

**Independent Test**: gerar um parecer com recomendação sensível, aprovar uma e rejeitar outra pelo
canal direto e verificar o que a consulta da execução devolve e o que fica no checkpoint.

### Tests for User Story 7 ⚠️

- [ ] T127 [P] [US7] Criar `tests/AuditoriaNFe.Tests/Aprovacao/FluxoAprovacaoTests.cs` com diretório temporário: TS-040 (aprovação grava checkpoint e libera), TS-041 (rejeição com justificativa nunca entrega conteúdo), TS-043 (segunda decisão → ERR-020), TS-044 (falha de gravação → ERR-022 e recomendação continua pendente), TS-045 (sessão expirada → `Indisponivel` registrado), retomada do workflow depois de reiniciar o processo
- [ ] T128 [P] [US7] Criar `tests/AuditoriaNFe.Tests/Aprovacao/RegistroAprovacaoJsonTests.cs`: escrita atômica, trava por recomendação, conflito de `versao` e ausência de XML, texto da recomendação e dados pessoais no arquivo
- [ ] T129 [P] [US7] Criar `tests/AuditoriaNFe.Tests/Integracao/AprovacaoEndpointsTests.cs`: listagem de pendências das sessões do mesmo chamador com conteúdo, achados, execução e sessão (FR-035d); rejeição sem justificativa → ERR-021; ERR-020 → 409; ERR-022 → 503; `GET /api/v1/execucoes/{execucaoId}` com recomendações por estado

### Implementation for User Story 7

- [ ] T130 [P] [US7] Criar `src/AuditoriaNFe.Domain/Recomendacoes/CheckpointAprovacao.cs` (data-model §12): `versaoSchema` `"1.0"`; `estado` `Pendente`, `Aprovada`, `Rejeitada`, `Indisponivel`; `aprovadorId` "obrigatório em `Aprovada` e `Rejeitada`"; `aprovadorChamadorId`; `aprovadorVerificado` "`false` enquanto vigorar o D-05"; `justificativa` "Obrigatória em `Rejeitada` (ERR-021); máx. 2.000 caracteres"; `motivoIndisponibilidade` "`SessaoExpirada` ou `SessaoEncerrada`"; `referenciaWorkflow`; `versao` "Controle de concorrência otimista"
- [ ] T131 [US7] Implementar `src/AuditoriaNFe.Infrastructure/Aprovacao/RegistroAprovacaoJson.cs` em `{Aprovacao:Diretorio}/registros/{recomendacaoId}.json`: arquivo temporário seguido de substituição atômica, trava por recomendação no processo, falha → ERR-022 mantendo `Pendente` (depende de T130)
- [ ] T132 [P] [US7] Implementar `src/AuditoriaNFe.Infrastructure/Aprovacao/ArmazenamentoCheckpointsWorkflow.cs` com `FileSystemJsonCheckpointStore` em `{Aprovacao:Diretorio}/workflow/`, com as mesmas restrições de conteúdo
- [ ] T133 [US7] Implementar `src/AuditoriaNFe.Application/Workflows/Aprovacao/FluxoAprovacaoRecomendacao.cs`: executores registrar pendência, `RequestPort<SolicitacaoAprovacao, DecisaoAprovacao>` e aplicar decisão; `[YieldsOutput(typeof(T))]` nos executores que chamam `YieldOutputAsync`; estado por `QueueStateUpdateAsync`/`ReadStateAsync` com escopo nomeado; `CheckpointManager`; retomada por `InProcessExecution.ResumeStreamingAsync`; mensagens só com identificadores e dados da decisão (research R-10, §3.8) (depende de T131, T132)
- [ ] T134 [US7] Implementar `src/AuditoriaNFe.Application/CasosDeUso/DecidirRecomendacao.cs` e `ConsultarRecomendacoes.cs`: aprovador = usuário informado, sem verificação (D-05); decisão única (ERR-020); justificativa obrigatória na rejeição (ERR-021); consulta por execução ou sessão com a visibilidade por estado (FR-035e, FR-035g) (depende de T133)
- [ ] T135 [US7] Iniciar um workflow por recomendação sensível na etapa 9 de `src/AuditoriaNFe.Application/CasosDeUso/ExecutarSolicitacaoAuditoria.cs`, sem esperar a decisão (FR-035c) (depende de T133)
- [ ] T136 [US7] Em `src/AuditoriaNFe.Application/Sessoes/ServicoSessoes.cs`, ao expirar ou encerrar a sessão, passar as recomendações `PendenteAprovacao` para `Indisponivel` e registrar o motivo no checkpoint (FR-035i) (depende de T131)
- [ ] T137 [US7] Implementar `src/Api.Agente/Endpoints/RecomendacoesEndpoints.cs`: `GET /api/v1/recomendacoes/pendentes`, `POST /api/v1/recomendacoes/{id}/aprovacao` e `POST /api/v1/recomendacoes/{id}/rejeicao` (`justificativa`) (depende de T134)

**Checkpoint**: TS-040, TS-041 e TS-043 a TS-045 passam pelo canal direto; SC-004a vale no REST.

---

## Phase 8: User Story 2 - SOFIA descobre o agente e o invoca por A2A (Priority: P1)

**Goal**: servidor A2A real com o SDK fixado, Agent Card em `/.well-known/agent-card.json` e o mesmo
caso de uso do canal direto (FR-043 a FR-047, proposta v1 provisória do contrato, DP-27, D-04).

**Independent Test**: obter o Agent Card, enviar o mesmo lote por A2A e pelo canal direto e comparar
os achados determinísticos.

**Dependência de contrato**: o adaptador desta fase pode mudar com o contrato definitivo da SOFIA
(DP-27); mudanças acordadas entram como nova versão de `contracts/sofia-a2a.md`.

### Tests for User Story 2 ⚠️

- [ ] T138 [P] [US2] Criar `tests/AuditoriaNFe.Tests/Contrato/AgentCardTests.cs`: TS-029, card válido no SDK fixado com todos os campos do FR-044, limites da configuração, extensão `urn:agente-auditoria-nfe:contrato` com `situacaoContrato: "provisorio"` e `autenticacao: "nenhuma — somente ambiente local de testes"`, sem `securitySchemes`, segredos nem dados de usuário (FR-045, FR-052b, NFR-013)
- [ ] T139 [P] [US2] Criar `tests/AuditoriaNFe.Tests/Contrato/MensagensA2ATests.cs`: TS-032 (mensagem malformada, parte `url` ou `data` → ERR-007), metadados inválidos (`usuarioId`, `chamadorId`, `correlacaoId` fora de `[A-Za-z0-9._:-]` ou acima de 128 caracteres, `preferencias` fora da lista) → ERR-007, nível dos metadados (solicitação ou mensagem) no SDK fixado e as lições V-06, V-09 e V-10 do AgenteFinanceiro (research R-11, R-33)
- [ ] T140 [P] [US2] Criar `tests/AuditoriaNFe.Tests/Integracao/A2AFimAFimTests.cs` com o cliente `Microsoft.Agents.AI.A2A`: TS-030 (achados determinísticos idênticos aos do REST), TS-034 (correlação devolvida com `execucaoId` e `sessaoId`), TS-033 limitado pelo D-04 (só o estado final), continuidade por `contextId` e `sessaoId`, `contextId` reutilizado por outro usuário sem acesso ao histórico, operação `consultar-recomendacoes` devolvendo aprovadas e nunca o conteúdo das rejeitadas (FR-035g, SC-004a), `preferencias` gravadas no contexto do usuário

### Implementation for User Story 2

- [ ] T141 [US2] Implementar `src/AuditoriaNFe.Application/Agente/AgenteAuditoriaNFe.cs` (`AIAgent` personalizado): converte texto e `DataContent` XML em solicitação, lê metadados de `AgentRunOptions.AdditionalProperties`, decide a operação (`analisar`, `perguntar`, `consultar-recomendacoes`) e delega a `ExecutarSolicitacaoAuditoria` ou `ConsultarRecomendacoes`; devolve resumo em texto, envelope v1 como `DataContent` JSON e identificadores em `AgentResponse.AdditionalProperties` (research R-04, R-11)
- [ ] T142 [US2] Implementar `src/AuditoriaNFe.Infrastructure/Persistencia/Mongo/AgentSessionStoreMongo.cs` (`AgentSessionStore`, coleção `sessoes_agente`), que guarda só o mapa `chaveIsolamento → sessaoId` por `contextId`, sem histórico nem conteúdo (data-model §1, research R-08)
- [ ] T143 [P] [US2] Implementar `src/Api.Agente/A2A/LeitorMetadadosA2A.cs` com as regras de `contracts/sofia-a2a.md` §4.3 (prioridade para os metadados da solicitação) e a montagem da `IdentificacaoSolicitante`
- [ ] T144 [P] [US2] Implementar `src/Api.Agente/A2A/FabricaAgentCard.cs`, que gera o card de `contracts/sofia-a2a.md` §2 a partir da configuração (limites, versão), com as três habilidades e sem `securitySchemes`
- [ ] T145 [US2] Implementar `src/Api.Agente/A2A/RegistroA2A.cs` e ligá-lo em `src/Api.Agente/Program.cs`: agente com chave `"auditoria-nfe"`, `AgentSessionStore` com a mesma chave, `AddA2AServer("auditoria-nfe", o => o.AgentRunMode = AgentRunMode.DisallowBackground)`, `MapA2AHttpJson` e `MapA2AJsonRpc` em `/a2a/auditoria-nfe`, `MapWellKnownAgentCard` em `/.well-known/agent-card.json`, sem `AgentIsolationKeyProvider` na v1 (depende de T141 a T144)
- [ ] T146 [US2] Atualizar `specs/001-agente-auditoria-nfe/contracts/sofia-a2a.md` no mesmo PR se o SDK fixado usar nomes ou níveis de metadados diferentes dos documentados (contrato, §2 Regras)

**Checkpoint**: TS-029, TS-030, TS-032 a TS-034 passam; o mesmo caso de uso atende os dois canais.

---

## Phase 9: User Story 6 - Validar a integração A2A com o Sofia.Simulator (Priority: P2)

**Goal**: cliente de console que descobre o agente pelo Agent Card e envia uma solicitação A2A real
com XMLs sintéticos e uma pergunta, exibindo o parecer (FR-048 a FR-051a).

**Independent Test**: executar o simulador contra uma instância do agente e verificar que ele exibe
o card, o estado da tarefa, o parecer, o resumo e os identificadores; com o agente desligado, verificar
a falha de descoberta sem envio.

### Tests for User Story 6 ⚠️

- [ ] T147 [P] [US6] Criar `tests/AuditoriaNFe.Tests/Integracao/SimuladorTests.cs`: TS-035 (fluxo básico contra o host de teste: card exibido, solicitação enviada, parecer, estado e identificadores exibidos) e TS-035a (card inacessível: falha de descoberta informada, código de saída diferente de zero, nenhuma solicitação enviada)

### Implementation for User Story 6

- [ ] T148 [P] [US6] Criar os XMLs sintéticos do simulador em `src/Sofia.Simulator/Exemplos/` (um válido, um com erro de layout e um com totalizador divergente, gerados com dados fictícios) (FR-050)
- [ ] T149 [US6] Implementar `src/Sofia.Simulator/Descoberta/DescobertaAgente.cs` com `A2ACardResolver`; falha → mensagem clara e término sem envio (FR-051a, research R-12)
- [ ] T150 [US6] Implementar `src/Sofia.Simulator/Cenarios/CenarioBasico.cs` e `src/Sofia.Simulator/Exibicao/ExibicaoConsole.cs`: `GetAIAgentAsync` com `PreferredBindings = [ProtocolBindingNames.HttpJson]`, mensagem com `TextContent` e `DataContent` (`application/xml`), metadados `usuarioId`, `chamadorId` e `correlacaoId`, exibição das habilidades, do estado, do envelope, do resumo e dos identificadores (FR-049) (depende de T149)
- [ ] T151 [P] [US6] Implementar os cenários opcionais `src/Sofia.Simulator/Cenarios/CenarioContinuidade.cs`, `CenarioRecomendacoes.cs` e `CenarioSemIdentificacao.cs` (FR-051) (depende de T150)
- [ ] T152 [US6] Implementar `src/Sofia.Simulator/Program.cs` com os argumentos `--endereco` (padrão `http://localhost:5310`) e `--cenario` (`basico`, `continuidade`, `recomendacoes`, `sem-identificacao`) (depende de T150, T151)

**Checkpoint**: TS-035 e TS-035a passam; SC-005 atendido com o endereço como única configuração.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: metas não funcionais, confirmação dos parâmetros marcados com *, avaliação real e
validação do ambiente.

- [ ] T153 Criar `tests/AuditoriaNFe.Tests/Integracao/CargaTests.cs` (trait `Categoria=Carga`, fora do CI padrão): lote de 5 MB e 20 arquivos em até 2 min p95 (NFR-001, SC-006), continuidade em até 30 s p95 (NFR-003), 10 análises simultâneas (NFR-004) e validação concorrente sobre o mesmo `XmlSchemaSet` em cache (research R-14); registrar os valores confirmados de `Ia:TimeoutChamada`, `Ferramentas:Timeout`, `Xml:ProfundidadeMaxima`, `Historico:MaxMensagens`, `ContextoUsuario:MaxCnpjs`, `ContextoUsuario:MaxAchados`, `ContextoUsuario:MaxItensNoPrompt`, `Mcp:Timeout`, `LimiteTaxa:PermissoesPorJanela`, `LimiteTaxa:Janela`, `Persistencia:TimeoutMongo` e `Persistencia:TimeoutPostgres` em `src/Api.Agente/appsettings.json`
- [ ] T154 Executar os dois conjuntos de avaliação (análise e perguntas) com a OpenAI real (`--filter-trait "Categoria=Avaliacao"`), escolher `OpenAI:Modelo` pelo resultado (DP-05), conferir o preço do modelo em `precos-openai.v1.json` e registrar o resultado em `docs/avaliacao/avaliacao-v1.md` (SC-003, SC-004, SC-008)
- [ ] T155 [P] Revisar com o responsável fiscal a lista `Sensibilidade:TermosTributos` em `src/Api.Agente/appsettings.json` (FR-035a)
- [ ] T156 [P] Rodar `dotnet list package --vulnerable --include-transitive`, corrigir as versões em `Directory.Packages.props` ou registrar a justificativa em `docs/seguranca/dependencias-v1.md`; confirmar que `SemSegredosTests` e `LogsSemConteudoTests` passam (constitution 5.1, 5.2)
- [ ] T157 Validar `specs/001-agente-auditoria-nfe/quickstart.md` do zero (`docker compose up`, Swagger, execução, simulador, consulta de consumo em `consumo.vw_consumo_diario`) e corrigir o guia no mesmo PR
- [ ] T158 [P] Conferir que `contracts/api-agente.openapi.yaml`, `contracts/contrato-ia-v1.schema.json`, `contracts/ferramentas-agente.md` e `contracts/sofia-a2a.md` refletem a implementação (testes de contrato verdes) e passar o Redocly lint no OpenAPI
- [ ] T159 Preencher na descrição do PR para a `dev` (modelo `.github/pull_request_template.md`) o checklist de conformidade com a constitution e a Definition of Done, citando as limitações do D-05 (execução só local), as pendências do ADR-0005 (prazos de remoção física) e da DP-27, e comentar "Aprovado" antes do merge (D-06)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependências. T017 espera a primeira execução verde de T015.
- **Foundational (Phase 2)**: depende do Setup; bloqueia todas as histórias.
- **US1 (Phase 3)**: depende da Foundational. É o MVP.
- **US4 (Phase 4)**: depende da US1 (fluxo e persistência da execução).
- **US3 (Phase 5)**: depende da US1 (agente interno e validador); usa a continuidade da US4 em
  TS-014 e TS-037, mas os testes de aplicação rodam só com a US1.
- **US5 (Phase 6)**: depende da US1 (achados e validador da saída da IA).
- **US7 (Phase 7)**: depende da US5 (recomendações sensíveis) e da US4 (expiração e encerramento
  para o FR-035i).
- **US2 (Phase 8)**: depende da US1; a operação `perguntar` usa a US3, `consultar-recomendacoes`
  depende da US7, e o metadado `preferencias` depende da US4.
- **US6 (Phase 9)**: depende da US2.
- **Polish (Phase 10)**: depende das histórias desejadas.

### User Story Dependencies

```text
Setup → Foundational → US1
US1 → US4, US3, US5           (podem andar em paralelo)
US4 + US5 → US7
US3 + US4 + US7 → US2 → US6
histórias concluídas → Polish
```

### Within Each User Story

- Testes primeiro, falhando antes da implementação.
- Domínio antes de Infraestrutura e Aplicação; casos de uso antes dos endpoints.
- `ExecutarSolicitacaoAuditoria.cs` e `ValidadorSaidaIa.cs` são alterados em várias histórias:
  essas tarefas não rodam em paralelo entre si.
- Commit por tarefa ou grupo lógico, com mensagem semântica e sem o trailer `Co-authored-by`.

### Parallel Opportunities

- Setup: T006 a T010, T013, T014 e T016.
- Foundational: T018 a T026 (domínio, abstrações, opções e erros), T029, T030, T033, T035 a T037,
  T039 e T040.
- US1: todos os testes T044 a T060; domínio T061 a T069; T074, T078 a T082.
- US4, US3 e US5 podem andar em paralelo depois da US1, com cuidado nos arquivos compartilhados.

---

## Parallel Example: User Story 1

```bash
# Testes de domínio da US1 juntos:
Task: "IdentificacaoDocumentoTests em tests/AuditoriaNFe.Tests/Dominio/IdentificacaoDocumentoTests.cs"
Task: "SelecaoPacoteTests em tests/AuditoriaNFe.Tests/Dominio/SelecaoPacoteTests.cs"
Task: "RegrasICMSTotTests em tests/AuditoriaNFe.Tests/Dominio/Regras/RegrasICMSTotTests.cs"
Task: "ResultadoGeralTests em tests/AuditoriaNFe.Tests/Dominio/ResultadoGeralTests.cs"

# Regras do catálogo por grupo, depois de T063:
Task: "Regras ICMSTot em src/AuditoriaNFe.Domain/Regras/ICMSTot/"
Task: "Regras ISSQNtot em src/AuditoriaNFe.Domain/Regras/ISSQNtot/"
Task: "Regras IBS, CBS e IS em src/AuditoriaNFe.Domain/Regras/IbsCbsIs/"
Task: "Regras de cobrança e pagamento em src/AuditoriaNFe.Domain/Regras/CobrancaPagamento/"
```

## Parallel Example: User Story 4

```bash
Task: "SessoesTests em tests/AuditoriaNFe.Tests/Integracao/SessoesTests.cs"
Task: "ContextoUsuarioTests em tests/AuditoriaNFe.Tests/Dominio/ContextoUsuarioTests.cs"
Task: "ContextoUsuario em src/AuditoriaNFe.Domain/ContextoUsuario/ContextoUsuario.cs"
Task: "SessoesEndpoints em src/Api.Agente/Endpoints/SessoesEndpoints.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir a Phase 1 (Setup), com CI verde e T017 aplicado.
2. Concluir a Phase 2 (Foundational).
3. Concluir a Phase 3 (US1).
4. **Parar e validar**: lote misto pelo `POST /api/v1/execucoes` no ambiente do compose, com o
   provedor simulado e depois com a OpenAI real.
5. Abrir PR para a `dev`.

### Incremental Delivery

1. Setup + Foundational → base pronta.
2. US1 → parecer pelo canal direto (MVP).
3. US4 → continuidade e contexto do usuário.
4. US3 → perguntas sem valores inventados.
5. US5 + US7 → recomendações e aprovação humana.
6. US2 + US6 → A2A e simulador, contra o contrato v1 provisório (DP-27).
7. Polish → metas não funcionais, avaliação e quickstart.

Cada incremento passa no CI e vai para a `dev` por PR; a `main` recebe a `dev` quando o conjunto
for publicado (ADR-0008).

---

## Notes

- [P] = arquivos diferentes, sem dependência de tarefa incompleta.
- [Story] liga a tarefa à história para rastreabilidade.
- **Adiados pelo D-05**: TS-031 (FR-052, ERR-004) e TS-042 (FR-035h, ERR-019). Voltam com o ADR da
  DP-10 e o fim do desvio.
- **Dependências marcadas pelo Constitution Check**: identificação, aprovação e exposição dependem
  do D-05 (T020, T036, T038, T042, T130 a T137, T143 a T145); a primeira chamada ao LLM com dados de
  documento segue o ADR-0004 (T082 a T084); o adaptador A2A segue a proposta provisória da DP-27
  (Phase 8 e Phase 9).
- **Fontes oficiais**: nenhuma regra, layout, schema ou preço é presumido (P02); NFePHP e ACBr não
  são fontes. Os XMLs de teste e de exemplo são sintéticos.
- Commits sem segredos, sem `.env` e sem o trailer `Co-authored-by: Cursor`; nada de push direto na
  `dev` ou na `main`.
