# Research: Agente de Auditoria de NF-e (AuditoriaNFe)

**Feature**: `001-agente-auditoria-nfe` | **Data**: 2026-10-29, revisado em 2026-11-04 |
**Plano**: [plan.md](./plan.md)

Este documento consolida a Fase 0 do plano técnico: decisões, justificativas, alternativas avaliadas e
o uso documentado das APIs públicas atuais do Microsoft Agent Framework (MAF), do SDK A2A e do SDK
MCP. As versões foram consultadas no NuGet em 2026-10-29. Nenhum código de aplicação é definido aqui;
os trechos de código ilustram apenas a forma das APIs oficiais.

**Revisão de 2026-11-04**: alinhamento à constitution 2.0.0 (MAF obrigatório, contexto do usuário,
DP-27) e às esclarecimentos da spec da mesma data: escopo NF-e modelo 55, validação de estrutura e
de totalizadores, seleção do pacote de schema pela data de referência, contexto do usuário, versão 1
sem autenticação (desvio D-05), contrato com a SOFIA provisório, Docker com portas próprias, lições do
AgenteFinanceiro e padrão de custos do projeto-ia-infis.

## 1. Pacotes oficiais e versões fixadas

As versões abaixo DEVEM ser fixadas por Central Package Management (`Directory.Packages.props`).
Pacotes em pré-lançamento podem mudar de forma incompatível entre versões; a atualização exige PR
próprio com execução completa dos testes de contrato A2A.

| Pacote | Versão | Situação | Uso | Projeto |
| ------ | ------ | -------- | --- | ------- |
| `Microsoft.Agents.AI` | 1.26.0 | Estável | `AIAgent`, `ChatClientAgent`, `AgentSession`, `AIContextProvider`, middleware | Application |
| `Microsoft.Agents.AI.OpenAI` | 1.26.0 | Estável | Criação de agentes sobre o cliente OpenAI (`AsAIAgent`) e `IChatClient` OpenAI | Infrastructure |
| `Microsoft.Agents.AI.Workflows` | 1.26.0 | Estável | Workflow de aprovação humana, `RequestPort`, checkpoints | Application, Infrastructure |
| `Microsoft.Agents.AI.Hosting.A2A.AspNetCore` | 1.26.0-preview.261023.1 | Pré-lançamento | Servidor A2A (inclui `Microsoft.Agents.AI.Hosting.A2A`, `A2A` e `A2A.AspNetCore` 1.0.0-preview3) | Api.Agente |
| `Microsoft.Agents.AI.Hosting.AspNetCore` | 1.26.0-preview.261023.1 | Pré-lançamento | Hospedagem do agente; o isolamento por claims fica para o fim do desvio D-05 | Api.Agente |
| `Microsoft.Agents.AI.A2A` | 1.26.0-preview.261023.1 | Pré-lançamento | Cliente A2A (`A2ACardResolver`, `A2AAgent`) | Sofia.Simulator, Tests |
| `A2A` / `A2A.AspNetCore` | 1.0.0-preview3 | Pré-lançamento | SDK oficial A2A (transitivo; `AgentCard`, `MapWellKnownAgentCard`) | Api.Agente, Sofia.Simulator |
| `Microsoft.Extensions.AI.OpenAI` | 10.11.0 | Estável | Adaptador `IChatClient` para OpenAI (transitivo do MAF OpenAI) | Infrastructure |
| `OpenAI` | ≥ 2.14.0 | Estável | SDK oficial OpenAI (transitivo) | Infrastructure |
| `ModelContextProtocol.Core` | 2.3.0 | Estável | Cliente MCP (`McpClient`, `HttpClientTransport`) | Infrastructure |
| `MongoDB.Driver` | 3.13.0 | Estável | Sessões, execuções, resultados e contexto do usuário | Infrastructure |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.3 | Estável | Registro de consumo e custos | Infrastructure |
| `Microsoft.AspNetCore.OpenApi` | 10.0.14 | Estável | Documento OpenAPI do canal direto | Api.Agente |
| `Swashbuckle.AspNetCore.SwaggerUI` | 10.3.1 | Estável | Interface Swagger UI (somente a interface) | Api.Agente |
| `OpenTelemetry.Extensions.Hosting` | 1.20.1 | Estável | Traces, métricas e logs | Api.Agente |
| `xunit.v3` | 4.0.2 | Estável | Testes | Tests |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.14 | Estável | `WebApplicationFactory` | Tests |
| `Testcontainers.MongoDb` / `Testcontainers.PostgreSql` | 4.16.0 | Estável | Bancos reais e descartáveis (P17) | Tests |

`Microsoft.AspNetCore.Authentication.JwtBearer` saiu da lista: a versão 1 não tem autenticação
(desvio D-05, R-17). O pacote do mecanismo de credencial será escolhido no ADR da DP-10.

Diagnósticos experimentais com supressão pontual e documentada no `Directory.Build.props`:

- APIs do hosting A2A marcadas com `[Experimental]` (continuações de resposta), no `Api.Agente`.
- `MAAI001` (`AgentSessionStore` experimental no MAF 1.26), no `AuditoriaNFe.Application`, no
  `AuditoriaNFe.Infrastructure` e nos testes (lição V-08 do AgenteFinanceiro, R-33).

**SDK do .NET**: o `global.json` fixa o SDK 10 (LTS) e o runner de testes
`Microsoft.Testing.Platform` (lição V-07). Em 2026-11-04 a máquina de desenvolvimento tinha apenas
os SDKs 9.0.302 e 11.0 RC e o runtime ASP.NET Core 10.0.14; o SDK 10 DEVE ser instalado antes da
implementação (ver quickstart).

## 2. Decisões

### R-01 — Plataforma: .NET 10 e ASP.NET Core 10 (Minimal APIs)

- **Decisão**: .NET 10 (LTS) e ASP.NET Core 10 com Minimal APIs no `Api.Agente`; aplicativo de console
  .NET 10 no `Sofia.Simulator`. Nullable habilitado e avisos tratados como erro (seção 5.3).
- **Justificativa**: exigido pela constitution (3.1) e pela spec; Minimal APIs reduzem cerimônia sem
  misturar regra de negócio nos endpoints (3.3).
- **Alternativas**: Controllers MVC (mais código sem ganho para o número de endpoints da feature).

### R-02 — Framework de agentes: Microsoft Agent Framework

- **Decisão**: usar o Microsoft Agent Framework 1.26.0 para o agente, as ferramentas por Function
  Calling (`AIFunctionFactory` do Microsoft.Extensions.AI), os context providers, o middleware, as
  sessões, o workflow de aprovação e a hospedagem A2A.
- **Justificativa**: exigido pela constitution 2.0.0 (seção 3.1). O MAF cobre, em um único modelo,
  Function Calling, sessões serializáveis, context providers (usados no contexto do usuário),
  middleware, workflows com checkpoint e hospedagem A2A oficial. O AgenteFinanceiro usa a mesma
  versão e serve de referência (R-33).
- **Situação**: o antigo desvio D-01 (Semantic Kernel) deixou de existir com a emenda 2.0.0.
- **Alternativas**: Semantic Kernel (exigiria ADR pela seção 3.1 e não oferece hospedagem A2A nem
  workflows com checkpoint integrados).

### R-03 — Provedor de IA: OpenAI por Chat Completions, sem armazenamento no provedor

- **Decisão**: `Microsoft.Agents.AI.OpenAI` sobre o SDK oficial OpenAI, usando o cliente de Chat
  Completions (`OpenAIClient.GetChatClient(modelo)`), convertido em `IChatClient` com middleware de
  consumo. O histórico fica no MongoDB, e não no provedor.
- **Justificativa**: o histórico sob controle do sistema atende ao P15, à retenção (DP-13) e à
  minimização (DP-14); a API de Chat Completions suporta Structured Outputs com JSON Schema e Function
  Calling, exigidos pelo P08 e pelas ferramentas.
- **Modelo (DP-05)**: parâmetro obrigatório `OpenAI:Modelo`, sem valor padrão no código. O modelo
  DEVE suportar Structured Outputs com JSON Schema estrito e Function Calling. A escolha final DEVE
  ser registrada com o resultado do conjunto de avaliação (SC-003, P13) antes do merge da
  implementação de IA.
- **Alternativas**: API Responses com conversas armazenadas no provedor (rejeitada: estado fora do
  sistema, conflito com DP-13/DP-14); Azure OpenAI (fora da decisão "OpenAI" da spec).

### R-04 — Orquestração: fluxo determinístico em código e IA somente na etapa 7

- **Decisão**: o fluxo das nove etapas (constitution 3.4) é executado por um caso de uso da camada de
  Aplicação (`ExecutarSolicitacaoAuditoria`), em código determinístico. O LLM participa apenas da
  etapa 7, por um `ChatClientAgent` interno que recebe os resultados das etapas 3 e 4 como dados. Um
  agente personalizado derivado de `AIAgent` (`AgenteAuditoriaNFe`) é a porta de entrada do canal A2A
  e delega ao mesmo caso de uso usado pelo canal REST (P18: sem desvio de canal).
- **Justificativa**: P03 (determinístico precede e prevalece), FR-016, NFR-005 e ERR-013 a ERR-016
  exigem que validações e resultados determinísticos existam mesmo sem IA. Deixar o LLM decidir a
  ordem das etapas violaria o P10.
- **Alternativas**: (a) agente único que chama ferramentas de validação por decisão do LLM —
  rejeitado (P03, P10); (b) fluxo inteiro como Workflow do MAF — rejeitado nesta versão: as etapas
  são sequenciais, e os checkpoints do workflow conteriam dados extraídos do XML, contrariando a
  restrição dos checkpoints.

### R-05 — Saída estruturada versionada e validada (P08)

- **Decisão**: o agente interno é executado com `RunAsync<ContratoIaV1>(...)`, que gera o JSON Schema a
  partir do tipo e solicita Structured Outputs ao provedor. O schema publicado em
  [contracts/contrato-ia-v1.schema.json](./contracts/contrato-ia-v1.schema.json) é a referência
  normativa; um teste de contrato compara o schema gerado pelo tipo com o arquivo publicado. O
  sistema valida o resultado em duas etapas: estrutural (desserialização e campos obrigatórios) e
  semântica (achados citados existem, evidências apontam para campos extraídos, todo valor
  financeiro é rastreável a um campo ou a um cálculo registrado — FR-027/FR-028). Falha gera nova
  tentativa com mensagem de correção, até `Ia:MaxTentativas` (inicial: 3 tentativas no total). Cada
  tentativa é registrada no consumo (P14).
- **Justificativa**: `RunAsync<T>` é o caminho oficial do MAF para saída estruturada; a validação
  semântica não é coberta pelo schema. O projeto-ia-infis já usa JSON Schema em modo `strict`
  (`PromptAtendimento.cs`), o que confirma a viabilidade com a OpenAI.
- **Alternativas**: `AgentRunOptions.ResponseFormat` com `ChatResponseFormat.ForJsonSchema(...)`
  carregando o arquivo publicado (mantido como alternativa caso o schema gerado pelo tipo divirja do
  publicado).

### R-06 — Ferramentas por Function Calling e autorização pelo sistema (P10)

- **Decisão**: ferramentas criadas com `AIFunctionFactory.Create(...)` e `[Description]`, todas de
  leitura sobre os dados da sessão: listar documentos, obter campos extraídos de um documento, calcular
  totais determinísticos (com documentos incluídos e excluídos — FR-031) e listar achados. O cálculo
  é feito no Domínio, e cada resultado recebe um `calculoId` registrado na execução. Um middleware de
  Function Calling aplica a lista de ferramentas permitidas por execução, valida argumentos, impõe
  timeout e registra a chamada (seção 4.6). A consulta a manuais (etapa 5) é executada pelo sistema, e
  não oferecida ao LLM como ferramenta nesta versão. Nenhuma ferramenta lê ou grava o contexto do
  usuário (FR-057).
- **Justificativa**: o LLM não calcula valores; ele só cita valores devolvidos por ferramentas
  determinísticas, o que torna a verificação de FR-027/FR-028 objetiva.
- **Aprovação de ferramentas**: `ApprovalRequiredAIFunction` não é usado na v1, porque não há
  ferramenta com efeito colateral. A aprovação humana da spec é sobre recomendações (R-10).
- **Alternativas**: deixar o LLM somar valores e verificar depois (rejeitado: mais regenerações e
  risco de FR-030).

### R-07 — Context providers

- **Decisão**: três `AIContextProvider` no agente interno:
  1. `ContextoExecucaoProvider`: injeta, como dados delimitados, os resultados determinísticos da
     execução, o estado das etapas, o pacote de schema e a data de referência de cada documento e,
     quando habilitados, os trechos de manual recuperados.
  2. `ContextoSessaoProvider`: injeta o resumo da base analisada da sessão para perguntas de
     continuidade (FR-037, FR-042).
  3. `ContextoUsuarioProvider`: injeta o contexto do usuário (R-31) como dado não confiável e
     delimitado, limitado por `ContextoUsuario:MaxItensNoPrompt` (P15, FR-058). Não é usado para
     o usuário "anonimo" (FR-052c). Não implementa gravação em `StoreAIContextAsync`: a gravação é
     feita pelo caso de uso, a partir dos resultados determinísticos (FR-057).
  O estado por sessão usa `ProviderSessionState<T>`, nunca campos da instância do provider.
- **Justificativa**: separa a montagem do contexto do caso de uso e mantém os resultados da sessão
  disponíveis mesmo quando o histórico textual é reduzido. O AgenteFinanceiro usa o mesmo mecanismo
  (`ContextoFinanceiroProvider.cs`).
- **Alternativas**: concatenar tudo na mensagem do usuário (mistura instrução e dado, contra o P07);
  gravar o contexto do usuário no `StoreAIContextAsync` a partir da resposta do LLM (rejeitado:
  FR-057 proíbe que a IA grave o contexto).

### R-08 — Sessões e memória conversacional: MongoDB (DP-01, resolvida na constitution 2.0.0)

- **Decisão**: MongoDB para (a) sessões do MAF serializadas (`SerializeSessionAsync` e
  `DeserializeSessionAsync`) por meio de uma implementação de `AgentSessionStore`; (b) documentos de
  domínio: `sessoes`, `execucoes` (com parecer e recomendações), dados extraídos por documento e
  `contextos_usuario` (R-31). O XML original não é persistido (FR-037a). A expiração por
  inatividade (24 h, configurável) é verificada a cada solicitação; a remoção física segue o ADR de
  retenção (DP-13).
- **Isolamento sem autenticação (D-05)**: a chave de isolamento é o SHA-256 de `chamadorId` e
  `usuarioId`, informados nos metadados (A2A) ou nos cabeçalhos (REST), ou dos valores padrão
  "nao-informado" e "anonimo" (FR-052a, FR-052c). O caso de uso confere a chave em todo acesso a
  sessão e responde ERR-005 sem revelar a existência da sessão (FR-039).
- **Sessão A2A**: a sessão do hosting A2A segue o `contextId`. Como não há claims, o
  `AgentIsolationKeyProvider` do hosting não é usado na v1. A sessão do agente externo
  (`AgenteAuditoriaNFe`) guarda apenas um mapa `chaveIsolamento → sessaoId`, sem histórico nem
  conteúdo; o histórico do agente interno fica na sessão de domínio, isolada pela chave. Assim, um
  `contextId` reutilizado por outro usuário não dá acesso ao histórico nem altera a continuidade do
  primeiro usuário.
- **Redução de histórico (FR-042)**: o histórico textual é reduzido por um redutor do
  Microsoft.Extensions.AI configurado no provedor de histórico da sessão; o ponto exato de
  configuração DEVE ser confirmado na versão 1.26.0 durante as tarefas. Os resultados de análise ficam
  fora do histórico e não são afetados.
- **Justificativa**: a constitution 2.0.0 (3.1) define MongoDB para memória e contexto do usuário; a
  documentação do hosting indica que o `InMemoryAgentSessionStore` e o `InMemoryTaskStore` servem
  apenas para desenvolvimento. O identificador `contextId` do A2A não é token de autorização.
- **Alternativas**: Redis (não permitido pela constitution 2.0.0 para memória); usar o
  `AgentIsolationKeyProvider` com identidade lida do corpo (rejeitado: o provedor só recebe o
  `HttpContext`, e ler o corpo antes do hosting acoplaria a borda ao formato A2A).

### R-09 — Middleware

- **Decisão**: três níveis de middleware:
  1. **Execução do agente** (`AsBuilder().Use(runFunc:, runStreamingFunc:)`): correlação, timeout da
     execução (ERR-016), mapeamento de exceções do provedor para ERR-014 e logs sem conteúdo.
  2. **Function Calling** (`Use(Func<AIAgent, FunctionInvocationContext, ...>)`): lista de
     ferramentas permitidas, validação de argumentos, timeout, registro de execução e
     `context.Terminate` em violação.
  3. **`IChatClient`** (um `DelegatingChatClient` próprio mais `UseOpenTelemetry()`): registra cada
     chamada ao provedor, inclusive cada volta do loop de ferramentas e cada retentativa, com
     `ChatResponse.Usage` (`InputTokenCount`, `OutputTokenCount` e tokens de entrada em cache,
     quando informados). Valor ausente é gravado como NULL, nunca como zero (P14, R-16).
- **Justificativa**: cada responsabilidade transversal fica em um ponto testável, fora dos casos de uso.
- **Alternativas**: registrar consumo no caso de uso (perderia as chamadas internas do loop de
  ferramentas).

### R-10 — Aprovação humana: Workflow do MAF com `RequestPort` e checkpoints JSON locais

- **Decisão**: cada recomendação sensível inicia um workflow `FluxoAprovacaoRecomendacao` com três
  executores: registrar pendência, `RequestPort<SolicitacaoAprovacao, DecisaoAprovacao>` e aplicar a
  decisão. O workflow roda com `CheckpointManager` sobre `FileSystemJsonCheckpointStore`, em um
  diretório configurável. A execução termina com o pedido pendente salvo em checkpoint; a decisão
  do aprovador (REST) retoma o workflow com `InProcessExecution.ResumeStreamingAsync(...)`, recebe
  de novo o `RequestInfoEvent` pendente e envia a resposta. Além do checkpoint do workflow, o sistema
  grava o registro de auditoria `CheckpointAprovacao` (schema v1 próprio, campos do FR-035f) em JSON,
  com escrita atômica e controle de versão.
- **Aprovador na v1 (D-05)**: o aprovador é o usuário informado na solicitação de decisão,
  registrado com o chamador e sem verificação (FR-035d). O controle de papel (FR-035h, ERR-019) volta
  com a autenticação.
- **Lições do AgenteFinanceiro**: executores que chamam `YieldOutputAsync` DEVEM declarar
  `[YieldsOutput(typeof(T))]`; o estado compartilhado entre executores usa
  `QueueStateUpdateAsync`/`ReadStateAsync` com escopo nomeado, que sobrevive ao checkpoint.
- **Justificativa**: atende ao pedido de Workflow com aprovação humana e ao FR-035j. O formato
  interno de checkpoint do MAF não é contrato estável; por isso, os campos de auditoria exigidos pelo
  FR-035f ficam no registro próprio. As mensagens do workflow carregam apenas identificadores e
  dados da decisão, nunca conteúdo de XML ou dados pessoais.
- **Limitações**: válido para uma única instância e para demonstração (ver plano, Limitações). Não
  funciona em ambiente sem disco durável (por exemplo, Vercel Functions).
- **Alternativas**: máquina de estados simples gravando apenas o registro JSON (mais simples, mas não
  atende ao pedido de Workflow); `ApprovalRequiredAIFunction` (aprova execução de ferramenta, não
  liberação de conteúdo).

### R-11 — Servidor A2A e Agent Card (proposta v1 provisória, DP-27)

- **Decisão**: registrar o agente `AgenteAuditoriaNFe` como `AIAgent` com chave
  `"auditoria-nfe"`, registrar o `AgentSessionStore` MongoDB com a mesma chave, chamar
  `builder.AddA2AServer("auditoria-nfe", o => o.AgentRunMode = AgentRunMode.DisallowBackground)` e
  mapear `MapA2AHttpJson` (preferencial) e `MapA2AJsonRpc` em `/a2a/auditoria-nfe`. Publicar o card
  com `MapWellKnownAgentCard` em `/.well-known/agent-card.json`. Protocolo A2A 1.0, conforme o SDK
  1.0.0-preview3. O contrato [contracts/sofia-a2a.md](./contracts/sofia-a2a.md) é uma proposta v1
  provisória até a DP-27; mudanças acordadas com a SOFIA afetam só o adaptador A2A e o card.
- **Autenticação no card (D-05)**: o card não declara `securitySchemes` e informa, na extensão do
  contrato, `autenticacao: "nenhuma — somente ambiente local de testes"` (FR-052b).
- **Limitação relevante**: respostas em segundo plano (tarefas A2A de longa duração) ainda não são
  suportadas pelo hosting A2A do MAF; todas as respostas são mensagens imediatas. O FR-046 e o TS-033
  ficam parcialmente atendidos (ver plano, desvio D-04).
- **Conversão de conteúdo**: partes de arquivo e de dados do A2A chegam ao agente como `DataContent`
  (ou `UriContent`); `DataContent` de resposta é enviado como parte de arquivo; metadados da mensagem
  A2A chegam em `AgentRunOptions.AdditionalProperties`, e `AgentResponse.AdditionalProperties` volta
  como `Message.Metadata`. O contrato usa esse mapeamento para correlação e identificadores.
- **Lições do AgenteFinanceiro (R-33)**: V-06 (metadados de status não chegam à tarefa), V-09
  (conflito de nome `AgentSkill`) e V-10 (mensagens para tarefas encerradas barradas antes do
  handler) DEVEM ser cobertas por testes de contrato antes de qualquer uso de tarefas A2A.
- **Alternativas**: API obsoleta `MapA2A(agent, path, agentCard)` (rejeitada); implementar o protocolo
  manualmente (rejeitado pelo FR-043).

### R-12 — Cliente A2A do Sofia.Simulator

- **Decisão**: `A2ACardResolver` para obter o card e `GetAIAgentAsync(...)` com
  `A2AClientOptions { PreferredBindings = [ProtocolBindingNames.HttpJson] }` para criar o `AIAgent`
  remoto; envio de `ChatMessage` com `TextContent` (pergunta) e `DataContent` (XML,
  `application/xml`), com `usuarioId`, `chamadorId` e `correlacaoId` nos metadados; leitura da
  resposta em texto e da parte de arquivo JSON com o envelope. Sem token na v1 (D-05).
- **Justificativa**: usa somente o contrato público (FR-048) e o SDK oficial.
- **Alternativas**: `HttpClient` cru contra o endpoint (rejeitado pelo FR-043/FR-048).

### R-13 — Ponto de integração MCP (preparado e desabilitado)

- **Decisão**: interface `IConsultaManuais` na Aplicação; implementação MCP na Infraestrutura com
  `McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions { Endpoint, Name,
  TransportMode = HttpTransportMode.StreamableHttp, AdditionalHeaders }))` e `ListToolsAsync()`;
  implementação nula quando `Mcp:Habilitado = false`, que registra a etapa 5 como "não aplicável —
  integração não habilitada" (FR-047b). O contrato do lado consumidor está em
  [contracts/consulta-manuais-mcp.md](./contracts/consulta-manuais-mcp.md); o transporte definitivo
  e a versão do protocolo ficam para o ADR da DP-18 na feature 002.
- **Alternativas**: chamar a Api.RAG por REST (viola a seção 3.2 e o P11).

### R-14 — Validação de XML e XSD com seleção do pacote pela data (resolve DP-03 nesta feature)

- **Leitura segura**: `XmlReader` com `DtdProcessing.Prohibit`, `XmlResolver = null`,
  `MaxCharactersInDocument` e limite de profundidade; detecção de XML bem formado pelo conteúdo.
- **Identificação (FR-007, FR-007a, FR-008)**: elemento raiz `NFe` ou `nfeProc` no namespace
  `http://www.portalfiscal.inf.br/nfe`, atributo `versao="4.00"`, `ide/mod = 55` e modelo 55 na chave
  de acesso (posições 21–22 da chave de 44 caracteres). Qualquer divergência → não suportado
  (ERR-009 ou ERR-010), antes do schema. A NFC-e (modelo 65) é recusada aqui, embora o schema seja o
  mesmo. São extraídos `dhEmi` e, com protocolo, `protNFe/infProt/dhRecbto`.
- **Schemas versionados no repositório**: 19 pacotes oficiais em
  `src/AuditoriaNFe.Infrastructure/Schemas/NFe/<pacote>/`, com o fechamento de includes de
  `nfe_v4.00.xsd` e o `procNFe_v4.00.xsd`. Os pacotes da série PL_010 não trazem `procNFe`; é usado o
  do PL_009q, idêntico ao de todos os pacotes PL_009 exceto pelo fim de linha. O `manifesto.json` da
  pasta registra, por pacote: ordem, notas técnicas, vigência (início, fim, início inferido), zip
  oficial de origem (título, URL do Portal, data de obtenção, SHA-256) e SHA-256 de cada XSD. O
  `.gitattributes` marca os XSDs como `-text`, para que o Git não altere os bytes conferidos. Em
  2026-11-04, os 19 pacotes compilaram sem erros no `XmlSchemaSet`, com os tipos `TNFe` e
  `TNfeProc`.
- **Integridade na inicialização**: o sistema confere o SHA-256 de cada XSD contra o manifesto e não
  inicia se houver divergência, arquivo ausente ou arquivo fora do manifesto.
- **Resolução de includes**: `XmlSchemaSet` com um `XmlResolver` próprio que só resolve caminhos
  dentro da pasta do pacote (sem URL remota, sem `..` para fora da pasta); qualquer outro pedido falha.
  O documento recebido nunca resolve recursos externos.
- **Seleção do pacote (FR-010 a FR-010d)**: a data de referência é a data de calendário de
  `dhRecbto`, quando houver protocolo, ou de `dhEmi`, no deslocamento de fuso informado no próprio
  XML (as datas de vigência das NTs são datas de Brasília, e o XML traz hora local com deslocamento).
  O pacote é o que satisfaz `vigenciaInicio ≤ data ≤ vigenciaFim`; o último pacote tem fim aberto e
  vale até a data atual do relógio do sistema. Data anterior ao primeiro pacote ou posterior à data
  atual → ERR-023. A fonte da tabela é o próprio `manifesto.json`, derivado de
  [vigencia-pacotes-schema.md](./vigencia-pacotes-schema.md), aprovada em 2026-11-04 e sujeita a
  ajustes, que entram como nova versão do manifesto.
- **Convivência (FR-010e, FR-010f)**: o manifesto tem o campo `fimConvivencia`, vazio em todos os
  pacotes atuais, porque nenhuma NT do leiaute 4.00 prevê convivência. A regra de nova tentativa
  com o pacote anterior é implementada de forma genérica e testada com uma tabela de vigência
  sintética de teste (TS-060 a TS-062).
- **Desempenho e concorrência**: o `XmlSchemaSet` de cada pacote é compilado uma única vez (sob
  demanda) e mantido em cache somente leitura. A validação concorrente sobre o mesmo conjunto
  compilado DEVE ser confirmada por teste de carga (NFR-004); se falhar, usa-se um conjunto por
  validação a partir de um pool por pacote.
- **Validação**: `NFe` isolada pelo `nfe_v4.00.xsd` e `nfeProc` pelo `procNFe_v4.00.xsd` do pacote
  selecionado. Cada violação vira achado `ErroEstrutural` com linha, posição e caminho (FR-011).
- **Alternativas**: baixar os schemas em tempo de execução (rejeitado: dependência externa e sem
  reprodutibilidade, NFR-005); bibliotecas de terceiros, como NFePHP ou ACBr, como fonte de schemas
  (rejeitado: não são fonte oficial, P02); um único pacote para todas as datas (rejeitado: documentos
  antigos seriam reprovados por campos criados depois).

### R-15 — Catálogo de regras de conteúdo (resolve DP-04 nesta feature)

- **Decisão**: o catálogo aprovado em
  [catalogo-regras-totalizadores.md](./catalogo-regras-totalizadores.md) é convertido em
  `src/AuditoriaNFe.Infrastructure/Catalogos/catalogo-regras.v1.json`. Cada entrada tem: `id` (por
  exemplo, `W04-10`), `versaoRegra`, descrição, fonte (`documento`, `versao`, `paginas`), vigência
  (`inicio`, `fim`), severidade, tolerância (decimal, opcional), aplicação na SEFAZ (`Obrig.` ou
  `Facult.`, com código de rejeição) e `implementacao` (id da implementação determinística no
  Domínio). A W16-10 tem quatro entradas, uma por versão. A W07-10 tem duas.
- **Carga**: o sistema não inicia se houver regra sem fonte, sem vigência, com implementação
  inexistente ou com versões de uma mesma regra com vigências sobrepostas.
- **Aplicação (FR-012 a FR-014, FR-013a)**: para cada documento aprovado no layout, aplica as
  versões vigentes na mesma data de referência usada na seleção do schema. Comparação em `decimal`
  exato, exceto as tolerâncias do catálogo: R$ 0,50 na W16-10 (Anexo I, p. 140) e 0,01 nas
  quantidades monofásicas. Campo opcional ausente vale zero. As regras de IBS, CBS e IS só se aplicam
  com os grupos preenchidos.
- **Catálogo de extração**: lista fechada de campos necessários às regras e às perguntas, mantida em
  `Catalogos/catalogo-extracao.v1.json`; todo campo usado por uma regra DEVE estar no catálogo de
  extração (verificado na carga).
- **Testes (P17)**: cada regra tem ao menos um XML sintético válido e um inválido, com a data de
  referência dentro da vigência; regras com versões têm um caso por versão.
- **Alternativas**: regras codificadas sem arquivo de catálogo (rejeitado: FR-013 exige fonte e
  vigência auditáveis por execução).

### R-16 — Consumo e custos: PostgreSQL com EF Core (padrão do projeto-ia-infis ampliado)

- **Base**: o projeto-ia-infis registra uma linha por interação em `consumo_tokens`, com custos em
  `decimal(18,8)`, preços por milhão de tokens na configuração e cálculo
  `tokens × preço ÷ 1.000.000` arredondado em 8 casas (`MidpointRounding.AwayFromZero`)
  (`Api.Atendimento/Llm/CustoDeTokens.cs`). Este plano reaproveita o cálculo e a escala.
- **Ampliações exigidas pela constitution 4.5 e pela skill `controle-custos-llm`**:
  1. uma linha por chamada ao provedor, inclusive cada retentativa (`tentativa`) e cada volta do loop
     de ferramentas, e não uma por interação;
  2. `tipo_chamada` (`Chat` ou `Embedding`) e `tokens_embedding`; nesta feature não há chamadas de
     embeddings (a Api.RAG chega na feature 002 e a DP-15 define como o consumo dela é correlacionado);
  3. `tokens_entrada_cache`, quando o provedor informar;
  4. status, duração, provedor, serviço e operação;
  5. tokens e custos desconhecidos como NULL, nunca zero; custo NULL quando não houver preço do modelo
     na tabela;
  6. `origem_valores` (`InformadoProvedor` ou `EstimadoSistema`), moeda e versão da tabela de preços;
  7. distinção entre custo calculado pelo sistema e faturamento efetivo: só o custo calculado é
     registrado; o faturamento da OpenAI não é importado.
- **Tabela de preços (DP-22)**: arquivo versionado `Catalogos/precos-openai.v{n}.json`, com preço por
  milhão de tokens de entrada, de entrada em cache e de saída por modelo, moeda USD, URL da página
  oficial de preços e data de consulta. Os valores DEVEM ser preenchidos a partir da página oficial no
  momento da tarefa; este plano não presume preços.
- **Falha de gravação**: estratégia "melhor esforço documentado": a falha ao gravar o consumo é
  registrada em log e em métrica e não altera o resultado da execução; o resumo de consumo da
  execução indica "registro incompleto".
- **Relatório**: visão `consumo.vw_consumo_diario` (por dia, modelo e tipo de chamada) e resumo por
  execução no envelope (NFR-009).
- **Topologia (DP-16)**: instância PostgreSQL local do compose, banco `auditoria_agente`, schema
  `consumo` e usuário exclusivo do Api.Agente, com permissão só nesse schema; a Api.RAG terá banco
  próprio na feature 002. Migrações EF Core aplicadas na inicialização somente quando
  `Persistencia:AplicarMigracoes = true` (ambiente local).
- **Alternativas**: Dapper com scripts SQL (menos verboso, mas sem migrações versionadas); SQL Server
  como no projeto-ia-infis (rejeitado: a constitution 3.1 exige PostgreSQL).

### R-17 — Autenticação: fora da versão 1 (desvio D-05)

- **Decisão**: a versão 1 não autentica o chamador nem o usuário. O chamador informa `chamadorId` e
  `usuarioId` nos metadados A2A ou nos cabeçalhos `X-Chamador-Id` e `X-Usuario-Id` do canal direto;
  sem eles, usam-se "nao-informado" e "anonimo" (FR-052a, FR-052c). Os valores são validados quanto
  ao formato (até 128 caracteres de `[A-Za-z0-9._:@-]`), registrados e usados no isolamento, sem
  verificação de identidade.
- **Mitigação técnica do D-05 (FR-052b)**:
  1. o `Api.Agente` recusa iniciar fora do ambiente `Development` enquanto
     `Seguranca:AutenticacaoHabilitada = false`, que é o único valor aceito na v1;
  2. o compose publica todas as portas só em `127.0.0.1`;
  3. o Agent Card declara que não há autenticação;
  4. os dados de teste e os exemplos do simulador são sintéticos ou anonimizados.
- **Modelo futuro (DP-10, ADR)**: o da constitution 5.2 — credencial de serviço do chamador,
  rotacionável e fornecida por configuração segura, com o usuário informado pelo chamador
  autenticado; controle do papel de aprovador (FR-035h, ERR-019). O fim do D-05 exige ADR aprovado,
  testes TS-031 e TS-042 ativos e o isolamento combinando o usuário com o chamador autenticado.
- **Alternativas**: JWT com `dotnet user-jwts` já na v1 (rejeitado pelo solicitante em 2026-11-04: a
  v1 serve para testar a validação, e a autenticação entra depois).

### R-18 — Minimização de dados enviados ao LLM (DP-14, ADR-0004 aprovado)

- **Decisão** (ADR-0004): enviar ao provedor apenas os dados extraídos necessários à análise e os
  resultados determinísticos; mascarar CPF, nomes de pessoas físicas, endereços, telefones e e-mails;
  nunca enviar o XML integral; sem armazenamento no provedor (R-03). O contexto do usuário enviado
  contém só CNPJs, contagens, achados recorrentes e preferências (FR-055a). A lista de campos enviados
  é configuração versionada e revisada.
- **Situação**: ADR-0004 aprovado por Andressa Firmino em 2026-11-04; a primeira chamada ao LLM com
  dados de documento DEVE seguir esta política.

### R-19 — Retenção (DP-13, ADR-0005 aprovado)

- **Definido pela spec**: XML descartado ao fim da execução (FR-037a); sessão expirada após 24 h de
  inatividade (configurável); contexto do usuário excluído após 90 dias sem atividade do usuário
  (FR-060, configurável), com prazo renovado a cada execução ou declaração.
- **Implementação**: o contexto do usuário tem o campo `expiraEm` (última atividade + prazo) com
  índice TTL do MongoDB (`expireAfterSeconds: 0`); a renovação atualiza `expiraEm`. A remoção pelo
  TTL ocorre em até cerca de um minuto após o vencimento (ciclo do monitor de TTL); a leitura também
  ignora contextos vencidos, para que o prazo seja exato do ponto de vista do usuário.
- **Decisão** (ADR-0005): sessões expiradas ou encerradas ficam indisponíveis na hora e são
  removidas fisicamente após prazo configurável; registros de consumo mantidos sem conteúdo;
  checkpoints JSON de demonstração removidos junto com a sessão. Os prazos de remoção física de
  sessões e de retenção do consumo não foram definidos: enquanto não houver valor aprovado, não há
  remoção física automática além do TTL do contexto do usuário. Isso não bloqueia a v1 local (D-05),
  mas DEVE ser resolvido antes de qualquer uso com dados reais.

### R-20 — Documentação da API: OpenAPI e Swagger UI

- **Decisão**: `Microsoft.AspNetCore.OpenApi` gera o documento; `Swashbuckle.AspNetCore.SwaggerUI`
  exibe a interface. O contrato de referência está em
  [contracts/api-agente.openapi.yaml](./contracts/api-agente.openapi.yaml); um teste de contrato
  compara o documento gerado com ele. A interface só é mapeada em `Development` (constitution 5.2),
  que é o único ambiente permitido enquanto vigorar o D-05.
- **Alternativas**: Scalar (interface equivalente; rejeitada porque a spec pede "interface Swagger").

### R-21 — Health checks

- **Decisão**: `/health/live` (processo) e `/health/ready` (MongoDB, PostgreSQL, diretório de
  checkpoints gravável e integridade dos schemas e catálogos carregados), com verificações próprias na
  Infraestrutura. A OpenAI não é verificada ativamente para não gerar custo; a indisponibilidade
  aparece como ERR-014. As respostas não expõem dados nem detalhes internos.
- **Alternativas**: pacotes da comunidade `AspNetCore.HealthChecks.*` (dependência extra para
  verificações simples).

### R-22 — Observabilidade (DP-23)

- **Decisão**: OpenTelemetry para traces, métricas e logs, exportação OTLP configurável e console em
  desenvolvimento; `UseOpenTelemetry()` no agente e no `IChatClient`, sem dados sensíveis. Toda linha
  de log carrega `execucaoId` e `correlacaoId`; conteúdo de XML, perguntas, respostas, contexto do
  usuário e identificadores de usuário não são logados (o log usa a chave de isolamento em hash).

### R-23 — Testes

- **Decisão**: um projeto `AuditoriaNFe.Tests` (xUnit v3, Microsoft.Testing.Platform) organizado por
  pastas: unidade (Domínio, seleção de pacote, regras do catálogo e cálculos), aplicação (fluxo com
  `IChatClient` simulado), ferramentas, aprovação (workflow e checkpoints em diretório temporário),
  contrato (A2A, OpenAPI, schema do contrato do LLM), integração (`WebApplicationFactory` com MongoDB
  e PostgreSQL reais via Testcontainers, P17), arquitetura (dependências entre camadas) e avaliação
  (conjunto de 50+ perguntas do SC-003, executado sob demanda com OpenAI real).
- **XMLs de teste**: sintéticos, um por período de vigência (TS-050, SC-011), gerados a partir de um
  modelo mínimo válido e ajustados ao pacote; nenhum dado real de contribuinte (P17).
- **Duplos de teste**: `IChatClient` determinístico que devolve respostas roteirizadas, inclusive
  inválidas, para TS-016, TS-027, TS-028 e TS-055; relógio controlável para expiração e vigência.
- **Comandos (lição V-07)**: `dotnet test --project tests/AuditoriaNFe.Tests`; a avaliação é marcada
  com o trait `Categoria=Avaliacao` e excluída por padrão com `--filter-not-trait`.

### R-24 — Contêineres e ambiente local

- **Dockerfile** (`src/Api.Agente/Dockerfile`): multiestágio, com SDK .NET 10 no build e
  `mcr.microsoft.com/dotnet/aspnet:10.0.14` no runtime (mesma versão das bibliotecas ASP.NET Core
  fixadas), tag de patch fixa e nunca flutuante. Executa como usuário não root (`$APP_UID`), porta
  interna 8080 por `ASPNETCORE_HTTP_PORTS`, sem segredos e sem `appsettings` com credenciais. Os
  schemas, o manifesto e os catálogos são copiados para a imagem como conteúdo do projeto de
  Infraestrutura.
- **Compose** (`docker-compose.yml` na raiz, projeto `auditoria-nfe`), no modelo do projeto-ia-infis:
  imagens com versão fixa, segredos obrigatórios com `${VAR:?Defina VAR no arquivo .env}`,
  healthchecks, scripts de inicialização montados somente leitura, volumes e rede com nome próprio.

  | Serviço | Imagem | Porta no host | Observação |
  | ------- | ------ | ------------- | ---------- |
  | `api-agente` | build local | `127.0.0.1:5310` → 8080 | `depends_on` com `service_healthy` dos bancos; volume de checkpoints |
  | `mongodb` | `mongo:8.0.32` | `127.0.0.1:27020` → 27017 | `infra/mongodb/init`: coleções com `$jsonSchema`, índices e TTL do contexto do usuário |
  | `postgres` | `pgvector/pgvector:0.8.7-pg17` | `127.0.0.1:5436` → 5432 | `infra/postgres/init`: usuário e schema `consumo`; a extensão `vector` fica disponível para a feature 002 |

- **Portas**: escolhidas para não conflitar com os outros projetos da máquina (SofiaInfis e
  projeto-ia-infis usam 27018, 5052, 5053, 5054, 5055, 5056, 1434, 5434, 5435, 5673 e 1026; o
  AgenteFinanceiro usa 5100; outras APIs usam 5001, 5147, 5253, 5285 e 8080). Todas publicadas só em
  `127.0.0.1` (FR-052b).
- **Segredos**: `.env` não versionado; `.env.example` versionado sem valores. A chave da OpenAI é
  opcional com `Ia__Provedor=Simulado`. Nenhuma chave é copiada de outros projetos.
- **Alternativas**: imagem `postgres` sem `vector` (a extensão não é usada nesta feature; mantida pela
  feature 002 e por ser a imagem já disponível localmente); executar só os bancos em contêiner (fica
  como alternativa de desenvolvimento no quickstart).

### R-25 — CI e proteção de branch (DP-26)

- **Decisão**: GitHub Actions com job `build-test` (restore, build com avisos como erro, testes com
  Docker para Testcontainers, sem chave da OpenAI) e job de build da imagem. Proteção da `main`: PR
  obrigatório, check `build-test` obrigatório, sem push direto nem force push e **zero aprovações
  obrigatórias** (desvio D-06: o GitHub não permite que o autor aprove o próprio PR, e o projeto tem
  uma única mantenedora). Em troca, a descrição do PR traz um checklist de conformidade com a
  constitution, que a mantenedora preenche e confirma com o comentário "Aprovado" antes do merge.
  Registro de imagens (GHCR) somente quando houver implantação (DP-25). Um teste de varredura
  (`SemSegredosTests`, como no AgenteFinanceiro) falha se encontrar padrões de chave de API no
  repositório.
- **Registro**: ADR-0008, aprovado por Andressa Firmino em 2026-11-04.

### R-26 — Implantação (DP-25): Vercel avaliada e não adotada nesta feature

- **Fato verificado**: desde 2026-10-27 a Vercel oferece, em beta, um runtime de contêiner que
  implanta um `Dockerfile` (ou `Dockerfile.vercel`) como Vercel Functions, sem estado local durável,
  com escala sob demanda e cobrança por CPU ativa; a porta padrão é 80, configurável por `PORT`.
- **Decisão**: esta feature roda localmente com Docker, também por causa do D-05 (FR-052b). A Vercel
  só poderá hospedar o Api.Agente após o fim do D-05, após ADR da DP-25 e após trocar os checkpoints
  JSON locais por armazenamento durável, com MongoDB Atlas e PostgreSQL externos. O texto da DP-25 na
  constitution ("a Vercel não oferece runtime oficial para .NET") está desatualizado e deve ser
  corrigido por emenda PATCH.

### R-27 — Nome da solução

- **Decisão**: `AgenteAuditoriaNFe`, como exige a constitution (3.1). O nome "AgenteAnaliseDIMP"
  citado na descrição de arquitetura do solicitante não é adotado (DIMP é outra obrigação acessória);
  se for intencional, exige emenda da constitution.

### R-28 — Estrutura física das camadas (DP-19)

- **Decisão**: projetos separados: `Api.Agente` (Endpoints), `AuditoriaNFe.Application`,
  `AuditoriaNFe.Domain`, `AuditoriaNFe.Infrastructure`, mais `Sofia.Simulator` e
  `AuditoriaNFe.Tests`. A Api.RAG terá seus projetos na feature 002.
- **Dependências**: a Aplicação referencia as abstrações do MAF (`Microsoft.Agents.AI`,
  `Microsoft.Agents.AI.Workflows`, `Microsoft.Extensions.AI.Abstractions`), permitido pela
  constitution 2.0.0 (3.3). Clientes concretos (OpenAI, MCP, MongoDB, Npgsql) ficam na
  Infraestrutura; o servidor A2A fica no `Api.Agente`. O Domínio não referencia nenhum pacote
  externo. O antigo desvio D-02 deixou de existir.

### R-29 — Limites de requisição e concorrência (constitution 5.2)

- **Decisão**: limite do corpo HTTP configurado acima do limite lógico (para que o sistema consiga
  responder ERR-002 com o total recebido; o padrão é o dobro do limite total de arquivos, por causa da
  codificação base64 no A2A); validação de tamanho total e quantidade antes de qualquer
  processamento; `ConcurrencyLimiter` do ASP.NET Core para as rotas de análise (inicial: 10
  execuções simultâneas, NFR-004) e limitação de taxa por chave de isolamento com janela fixa.

### R-30 — Identificadores

- **Decisão**: identificadores de execução, sessão, recomendação e cálculo gerados pelo sistema com
  `Guid.CreateVersion7()` (ordenáveis por tempo), nunca pelo LLM (P09).

### R-31 — Contexto do usuário (FR-055 a FR-060, P15, constitution 4.7)

- **Armazenamento**: coleção MongoDB `contextos_usuario`, um documento por chave de isolamento
  (SHA-256 de `chamadorId` e `usuarioId`), com `usuarioId` e `chamadorId` em claro para consulta e
  exclusão pelo canal direto. Validação de schema (`$jsonSchema`) na criação da coleção, no modelo do
  `01-historico.js` do projeto-ia-infis, para impedir campos fora do FR-055a.
- **Conteúdo (FR-055a)**:
  1. `cnpjsAuditados`: CNPJ (14 caracteres, inclusive o formato alfanumérico do `TCnpj` do PL_010e),
     papel (emitente ou destinatário), quantidade de documentos e data da última auditoria. CPF
     nunca é gravado;
  2. `achadosRecorrentes`: chave (`regraId` ou tipo de erro estrutural), frequência e data da última
     ocorrência, só de achados determinísticos;
  3. `preferencias`: `nivelDetalhe` (`Resumido` ou `Detalhado`) e `tributosInteresse` (lista fechada
     de códigos, como ICMS, ICMS-ST, IPI, PIS, COFINS, ISSQN, IBS, CBS e IS).
  Cada item guarda a origem (`execucaoId` ou declaração, com `sessaoId` e data — FR-055b).
- **Gravação (FR-057)**: só pelo caso de uso, na etapa 9, a partir dos documentos e achados
  determinísticos da execução, e pelo endpoint de preferências ou pelos metadados `preferencias` do
  A2A (declaração explícita e estruturada; nunca extraída do texto livre pela IA). Operações
  idempotentes por execução (a mesma execução não conta duas vezes).
- **Leitura (FR-058)**: pelo `ContextoUsuarioProvider` (R-07), como dado não confiável e delimitado,
  com limite de itens; não entra como evidência nem altera achados.
- **Usuário "anonimo" (FR-052c)**: nenhuma leitura nem gravação; o parecer indica usuário não
  identificado.
- **Retenção (FR-060)**: TTL de 90 dias sem atividade, configurável (R-19). **Exclusão e correção
  (FR-059)**: endpoints do canal direto (plano, Canal direto).
- **Alternativas**: guardar o contexto dentro da sessão do MAF (rejeitado: mistura histórico e
  contexto, contra o P15); deixar o LLM resumir o usuário ao fim de cada conversa (rejeitado: FR-057).

### R-32 — Contrato com a SOFIA provisório (DP-27)

- **Decisão**: o contrato A2A é uma proposta v1 provisória, versionada e própria deste projeto,
  validada pelo Sofia.Simulator. O núcleo do agente é testado pelo canal direto, sem depender da
  SOFIA. Nas tarefas, o canal A2A e o simulador vêm depois do núcleo REST (leitura, identificação,
  seleção de pacote, XSD, totalizadores, parecer, memória, contexto e custos).
- **Justificativa**: a SOFIA ainda não chama agentes externos; o adaptador A2A isola o caso de uso de
  mudanças no contrato.

### R-33 — Lições do AgenteFinanceiro (mesma versão do MAF e do SDK A2A)

Achados registrados em `AgenteFinanceiro/specs/001-agente-fluxo-caixa/research.md` durante a
implementação, aplicáveis aqui:

| ID | Achado | Aplicação neste projeto |
| -- | ------ | ----------------------- |
| V-06 | No A2A 1.0.0-preview3, `TaskProjection.Apply` não copia os metadados de um evento de status para `AgentTask.Metadata`, e um evento `Task` substitui o estado inteiro da tarefa | A v1 não usa tarefas A2A (D-04); os metadados vão na mensagem de resposta. Se tarefas forem ativadas, publicar um snapshot da tarefa com metadados antes do status final |
| V-07 | `dotnet test` em modo VSTest falha com xUnit v3 no SDK .NET 10 | `global.json` com `"test": { "runner": "Microsoft.Testing.Platform" }`; comandos com `--project` e filtros `--filter-trait`/`--filter-not-trait` |
| V-08 | `AgentSessionStore` é experimental no MAF 1.26 (`MAAI001`) | Supressão pontual de `MAAI001` nos projetos que usam o store (seção 1) |
| V-09 | `A2A.AgentSkill` tem o mesmo nome de `Microsoft.Agents.AI.AgentSkill` | No `Api.Agente`, referenciar os tipos do SDK A2A com `global::A2A.` |
| V-10 | O `A2AServer` recusa mensagens para tarefas encerradas com `UnsupportedOperation` antes de chamar o handler | Sem efeito na v1 (sem tarefas e sem decisão de aprovação por A2A); registrado para quando tarefas forem ativadas |

Outros padrões reaproveitados: `IChatClient` dublê com roteiro fixo, `WebApplicationFactory`,
testes de arquitetura, health checks, `SemSegredosTests` e a ordem de tarefas do `tasks.md`.

## 3. Uso documentado das APIs do Microsoft Agent Framework

Fonte principal: documentação oficial do Microsoft Agent Framework em Microsoft Learn (páginas
"Context Providers", "Agent Middleware", "Using function tools", "Using function tools with human in
the loop approvals", "Workflows - Checkpoints", "Host agents with A2A", "A2A Hosting (.NET)",
"Self-host Agent Framework applications", "A2A agent service", "Custom Agents", "Using MCP Tools") e
o repositório `microsoft/agent-framework`. Os trechos abaixo mostram a forma pública das APIs; não são
código da aplicação.

### 3.1 `IChatClient`

Abstração do Microsoft.Extensions.AI para modelos de chat. É a base do `ChatClientAgent` e o ponto de
middleware de baixo nível (consumo, telemetria).

```csharp
IChatClient chatClient = new OpenAIClient(apiKey)
    .GetChatClient(modelo)
    .AsIChatClient()
    .AsBuilder()
    .Use(getResponseFunc: RegistrarConsumo, getStreamingResponseFunc: null)
    .UseOpenTelemetry()
    .Build();
// ChatResponse.Usage: UsageDetails { InputTokenCount, OutputTokenCount, TotalTokenCount, ... }
```

### 3.2 `AIAgent`

Classe base abstrata de todos os agentes. Métodos públicos: `RunAsync(...)`, `RunStreamingAsync(...)`,
`RunAsync<T>(...)` (saída estruturada), `CreateSessionAsync()`, `SerializeSessionAsync(...)`,
`DeserializeSessionAsync(...)`. Um agente personalizado sobrescreve os métodos protegidos
`RunCoreAsync(IEnumerable<ChatMessage>, AgentSession?, AgentRunOptions?, CancellationToken)`,
`RunCoreStreamingAsync(...)`, `CreateSessionCoreAsync(CancellationToken)` e os de serialização de
sessão. `DelegatingAIAgent` implementa o padrão decorador sobre um agente interno.

### 3.3 `ChatClientAgent`

Implementação de `AIAgent` sobre um `IChatClient`, com loop de Function Calling
(`FunctionInvokingChatClient`) embutido.

```csharp
AIAgent agente = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    Id = "auditoria-nfe-analise",          // estável: exigido para workflows com checkpoint
    Name = "auditoria-nfe-analise",
    ChatOptions = new() { Instructions = instrucoes, Tools = ferramentas },
    AIContextProviders = [contextoExecucao, contextoSessao, contextoUsuario],
});
// Atalho com o SDK OpenAI: openAiClient.GetChatClient(modelo).AsAIAgent(options)
AgentResponse<ContratoIaV1> r = await agente.RunAsync<ContratoIaV1>(mensagens, sessao);
ContratoIaV1 resultado = r.Result;
```

### 3.4 `AgentSession`

Estado de conversa de um agente (histórico, estado dos providers em `AgentSessionStateBag`).

```csharp
AgentSession sessao = await agente.CreateSessionAsync();
await agente.RunAsync(mensagem, sessao);
JsonElement serializada = await agente.SerializeSessionAsync(sessao);
AgentSession restaurada = await agente.DeserializeSessionAsync(serializada);
```

Persistência hospedada: `AgentSessionStore` (abstrata, experimental — `MAAI001`) com
`SaveSessionAsync(AIAgent agent, string sessionStoreId, AgentSession session, CancellationToken)` e
operações assíncronas de leitura e exclusão. O `sessionStoreId` é opaco.

### 3.5 `AIContextProvider`

Executa antes e depois de cada invocação do agente para adicionar contexto (instruções, mensagens,
ferramentas) e extrair informação da resposta.

```csharp
sealed class ContextoExemplo : AIContextProvider
{
    private readonly ProviderSessionState<Estado> _estado;
    public ContextoExemplo() : base(null, null)
        => _estado = new ProviderSessionState<Estado>(_ => new Estado(), nameof(ContextoExemplo));
    public override string StateKey => _estado.StateKey;

    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context, CancellationToken cancellationToken = default)
    {
        Estado e = _estado.GetOrInitializeState(context.Session);
        return new(new AIContext { Instructions = "...", Messages = [...], Tools = [...] });
    }

    protected override ValueTask StoreAIContextAsync(
        InvokedContext context, CancellationToken cancellationToken = default)
    {
        Estado e = _estado.GetOrInitializeState(context.Session);
        _estado.SaveState(context.Session, e);
        return ValueTask.CompletedTask;
    }
}
```

Variante avançada: sobrescrever `InvokingCoreAsync` e `InvokedCoreAsync`. Regra: estado por sessão
somente via `ProviderSessionState<T>`, nunca em campos do provider (o provider é compartilhado entre
sessões).

### 3.6 Middleware

```csharp
AIAgent agenteComMiddleware = agente
    .AsBuilder()
    .Use(runFunc: ExecucaoMiddleware, runStreamingFunc: ExecucaoStreamingMiddleware)
    .Use(FunctionCallingMiddleware)
    .Build();

// Execução: Task<AgentResponse> (IEnumerable<ChatMessage> messages, AgentSession? session,
//           AgentRunOptions? options, AIAgent innerAgent, CancellationToken ct)
// Function Calling: ValueTask<object?> (AIAgent agent, FunctionInvocationContext context,
//           Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
//           CancellationToken ct) — context.Terminate = true encerra o loop.
// Use(sharedFunc: ...) só inspeciona a entrada, sem interceptar a saída.
```

O middleware de Function Calling só funciona com agentes que usam `FunctionInvokingChatClient`, como o
`ChatClientAgent`.

### 3.7 Ferramentas e aprovação de ferramentas

```csharp
AIFunction f = AIFunctionFactory.Create(MetodoComDescription);   // [Description] nos parâmetros
AIFunction comAprovacao = new ApprovalRequiredAIFunction(f);       // não usado na v1
// A resposta traz ToolApprovalRequestContent (.ToolCall: FunctionCallContent);
// responde-se com requestContent.CreateResponse(true|false) numa ChatMessage do usuário,
// na mesma sessão.
```

### 3.8 Workflow com aprovação humana e checkpoints

```csharp
var porta = RequestPort.Create<SolicitacaoAprovacao, DecisaoAprovacao>("aprovacao");
Workflow wf = new WorkflowBuilder(registrar)
    .AddEdge(registrar, porta)
    .AddEdge(porta, aplicarDecisao)
    .WithOutputFrom(aplicarDecisao)
    .Build();

var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(diretorio));
CheckpointManager cm = CheckpointManager.CreateJson(store);
StreamingRun run = await InProcessExecution.RunStreamingAsync(wf, entrada, cm);
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    // RequestInfoEvent: pedido pendente (req.Request.TryGetDataAs<T>(out ...))
    // SuperStepCompletedEvent: evt.CompletionInfo?.Checkpoint (CheckpointInfo)
    // WorkflowOutputEvent / WorkflowErrorEvent
}
// Retomada: InProcessExecution.ResumeStreamingAsync(wf, checkpointInfo, cm);
// o RequestInfoEvent pendente é reemitido; responde-se com
// run.SendResponseAsync(req.Request.CreateResponse(decisao)).
```

Executores derivam de `Executor<TEntrada>("id")` e implementam `HandleAsync(mensagem,
IWorkflowContext contexto, CancellationToken)`, usando `contexto.SendMessageAsync(...)` e
`contexto.YieldOutputAsync(...)` (com `[YieldsOutput(typeof(T))]`). O workflow usado na retomada DEVE
ter a mesma estrutura e os mesmos ids de executores; agentes dentro de workflows precisam de
`ChatClientAgentOptions.Id` estável.

### 3.9 A2A — servidor

```csharp
builder.Services.AddKeyedSingleton<AIAgent>("auditoria-nfe", (sp, _) => /* AgenteAuditoriaNFe */);
builder.Services.AddKeyedSingleton<AgentSessionStore>("auditoria-nfe", (sp, _) => /* store MongoDB */);
builder.AddA2AServer("auditoria-nfe", o => o.AgentRunMode = AgentRunMode.DisallowBackground);

app.MapA2AHttpJson("auditoria-nfe", "/a2a/auditoria-nfe");   // POST .../message:send e :stream
app.MapA2AJsonRpc("auditoria-nfe", "/a2a/auditoria-nfe");
```

Componentes substituíveis por serviço com chave (nome do agente): `IAgentHandler` (padrão
`A2AAgentHandler`), `AgentSessionStore` (padrão `InMemoryAgentSessionStore`, só para
desenvolvimento), `ITaskStore` (padrão `InMemoryTaskStore`). Com um `AgentIsolationKeyProvider`
registrado, o session store e o task store são envolvidos por decoradores de isolamento em modo
estrito; na v1 ele não é registrado (R-08). As sessões seguem o `contextId` do A2A. `AgentRunMode`:
`DisallowBackground` (padrão), `AllowBackgroundIfSupported`, `AllowBackgroundWhen(...)`; respostas em
segundo plano ainda não são suportadas no hosting A2A.

### 3.10 Agent Card

```csharp
app.MapWellKnownAgentCard(new AgentCard
{
    Name = "...", Description = "...", Version = "...",
    DefaultInputModes = [...], DefaultOutputModes = [...],
    Capabilities = new() { /* streaming, extensões */ },
    Skills = [ new global::A2A.AgentSkill { /* id, nome, descrição, tags, exemplos */ } ],
    SupportedInterfaces =
    [
        new AgentInterface { Url = "...", ProtocolBinding = ProtocolBindingNames.HttpJson, ProtocolVersion = "1.0" },
    ],
});
// Publicado em GET /.well-known/agent-card.json; um card por host.
```

`MapWellKnownAgentCard` vem do pacote `A2A.AspNetCore` (SDK A2A), e não do MAF. Os nomes exatos das
propriedades de extensões DEVEM seguir o SDK 1.0.0-preview3 fixado; o teste de contrato TS-029 valida
o card publicado.

### 3.11 A2A — cliente

```csharp
A2ACardResolver resolver = new(new Uri(urlAgente));
AgentCard card = await resolver.GetAgentCardAsync();
AIAgent remoto = await resolver.GetAIAgentAsync(options: new A2AClientOptions
{
    PreferredBindings = [ProtocolBindingNames.HttpJson],
});
AgentSession sessao = await remoto.CreateSessionAsync();
AgentResponse resposta = await remoto.RunAsync(mensagemComTextoEArquivos, sessao);
// Alternativas: card.AsAIAgent(options: ...); new A2AClient(uri).AsAIAgent(name:, description:).
// Binding padrão: HTTP+JSON, com JSON-RPC como alternativa.
```

### 3.12 MCP — cliente

```csharp
await using McpClient cliente = await McpClient.CreateAsync(new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Endpoint = new Uri(urlApiRag), Name = "api-rag",
        TransportMode = HttpTransportMode.StreamableHttp, AdditionalHeaders = cabecalhos,
    }));
IList<McpClientTool> ferramentas = await cliente.ListToolsAsync();
// McpClientTool deriva de AIFunction; pode ser passado como AITool ou chamado diretamente.
```

## 4. Riscos e pontos de atenção

| Risco | Impacto | Mitigação |
| ----- | ------- | --------- |
| Sem autenticação na v1 (D-05) | Qualquer processo com acesso à porta pode enviar solicitações, se passar por outro usuário e aprovar recomendações | Só `Development`, portas em `127.0.0.1`, dados sintéticos, card declarando ausência de autenticação; D-05 encerrado antes de qualquer implantação ou da SOFIA real |
| Pacotes A2A e hosting em pré-lançamento | Mudanças incompatíveis entre versões | Versões fixadas; testes de contrato A2A; atualização em PR próprio |
| Respostas em segundo plano não suportadas no hosting A2A | FR-046 e TS-033 parcialmente atendidos | Desvio D-04; análises dentro de 2 min (NFR-001); reavaliar a cada versão do MAF |
| `contextId` reutilizado por outro usuário | Acesso a sessão de outro usuário | Sessão A2A só com mapa de chave para `sessaoId`; conferência da chave no caso de uso; TS-023, TS-054 e SC-009 |
| Contrato com a SOFIA ainda não acordado (DP-27) | Retrabalho do adaptador A2A | Núcleo testado pelo canal direto; A2A no fim das tarefas; contrato versionado |
| `DataContent` de resposta vira parte de arquivo | Cliente precisa decodificar o envelope | Contrato define `mediaType` e nome do arquivo; o simulador valida |
| Checkpoints JSON locais | Perda em contêiner sem volume; sem suporte a várias instâncias | Volume obrigatório no compose; limitação registrada; uma instância |
| Tabela de vigência sujeita a ajustes | Pacote errado para alguma data | Manifesto versionado; TS-050 e SC-011 com um XML por período; pendências P1 a P5 da tabela |
| Concorrência sobre o `XmlSchemaSet` compilado | Erros intermitentes de validação sob carga | Teste de carga (NFR-004); pool por pacote se necessário |
| Ambiguidades do Anexo I (C2 a C5 do catálogo) | Achado divergente da SEFAZ em casos de borda | Decisões registradas no catálogo; testes com os casos de borda |
| SDK .NET 10 ausente na máquina | Build local falha | Instalar o SDK 10 antes das tarefas; build também pelo Dockerfile |

## 5. Itens "NEEDS CLARIFICATION" resolvidos

| Item | Resolução |
| ---- | --------- |
| Framework de agentes | MAF 1.26.0, exigido pela constitution 2.0.0 (R-02) |
| Modelo OpenAI (DP-05) | Parâmetro obrigatório sem padrão, escolhido por avaliação (R-03) |
| Memória e contexto do usuário (DP-01) | MongoDB, resolvida na constitution 2.0.0; ADR registra a decisão (R-08, R-31) |
| Schemas oficiais e seleção (DP-03) | 19 pacotes versionados com manifesto; seleção pela data de referência (R-14) |
| Catálogo de regras (DP-04) | Catálogo de totalizadores aprovado em 2026-11-04; arquivo JSON versionado (R-15) |
| Protocolo A2A (DP-17) | A2A 1.0 pelo SDK 1.0.0-preview3; HTTP+JSON e JSON-RPC (R-11) |
| Contrato com a SOFIA (DP-27) | Proposta v1 provisória; continua pendente de acordo (R-32) |
| Estrutura física (DP-19) | Projetos separados (R-28) |
| Topologia PostgreSQL (DP-16) | Banco e usuário exclusivos do Api.Agente (R-16) |
| Tabela de preços (DP-22) | Arquivo versionado preenchido a partir da fonte oficial (R-16) |
| Observabilidade (DP-23) | OpenTelemetry (R-22) |
| CI e proteção de branch (DP-26) | GitHub Actions e regras da `main` (R-25) |
| Implantação (DP-25) | Não implantar nesta feature; Vercel só após ADR e fim do D-05 (R-26) |
| Documentação da API | Swagger UI, só em `Development` (R-20) |
| Docker e portas | Compose com portas 5310, 27020 e 5436 em `127.0.0.1` (R-24) |
| Autenticação (DP-10) | Fora da v1 (desvio D-05); modelo futuro no ADR (R-17) |
| Minimização (DP-14) e retenção (DP-13) | ADR-0004 e ADR-0005 aprovados (R-18, R-19); prazos de sessão e de contexto do usuário definidos pela spec; prazos de remoção física e de retenção do consumo ainda abertos |
