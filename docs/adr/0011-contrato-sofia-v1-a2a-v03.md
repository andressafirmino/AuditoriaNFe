# ADR-0011 — Contrato SOFIA v1.0 sobre A2A v0.3

- **Situação**: Proposto (substitui o [ADR-0007](./0007-protocolo-a2a.md) quando aprovado)
- **Data**: 2026-11-05
- **Aprovação**: pendente (Andressa Firmino)
- **Relacionado a**: DP-27 (resolvida na constitution 2.2.0), DP-17, DP-10; desvios D-04, D-05 e
  D-07 (novo); constitution 3.1; P07, P18; FR-043 a FR-052d; ADR-0003, ADR-0004, ADR-0005

## Contexto

O ADR-0007 definiu um contrato A2A provisório (A2A 1.0, hosting A2A do MAF, respostas só por
mensagem) enquanto a DP-27 esperava o acordo com a SOFIA. A INFIS publicou o contrato oficial da
turma para agentes especializados, **contrato SOFIA v1.0**:

- protocolo A2A v0.3.x, JSON-RPC 2.0, método `message/send`;
- a SOFIA envia uma `SolicitacaoAnaliseSofia` (IDs da orquestração, nome do usuário, pergunta e
  documentos já convertidos em texto) em uma parte de texto da Message;
- o agente devolve uma Task concluída com um Artifact que contém um `ResultadoAnaliseSofia`
  (status `concluido`, `parcial` ou `erro`, análise, achados com evidências e observações);
- o registry da SOFIA identifica cada agente por um `idAgente` único.

Verificado no SDK na versão fixada (`A2A` 1.0.0-preview3):

- o pacote oficial `A2A.V0_3Compat` 1.0.0-preview3 serve o perfil v0.3 sobre o SDK 1.0
  (`MapA2AWithV03Compat` e `MapAgentCardGetWithV03Compat`, com o formato do card escolhido pelo
  cabeçalho `A2A-Version`);
- o hosting A2A do MAF (`AddA2AServer` com `AgentRunMode.DisallowBackground`) responde com Message,
  não com Task, e por isso não atende o contrato;
- o servidor do SDK (`A2AServer` com um `IAgentHandler` próprio, `TaskUpdater` e
  `InMemoryTaskStore`) produz Tasks com Artifacts sem implementar o protocolo à mão.

## Decisão

- **Contrato de negócio**: contrato SOFIA v1.0, documentado em `contracts/sofia-a2a.md`, com os
  JSON Schemas da solicitação e do resultado. O canal A2A fala só esse contrato e cobre só a
  análise de documentos com pergunta. Perguntas sem documentos, continuidade de sessão e consulta
  posterior de recomendações ficam no canal direto (REST).
- **Perfil de protocolo**: A2A v0.3.x pelo SDK oficial `A2A` 1.0.0-preview3 com
  `A2A.V0_3Compat` 1.0.0-preview3. O endpoint `POST /a2a/auditoria-nfe` aceita v0.3 e v1.0
  (`MapA2AWithV03Compat`); só `message/send` v0.3 é contratual.
- **Servidor**: `A2AServer` do SDK com o manipulador `ManipuladorSofiaA2A : IAgentHandler`, que
  valida a solicitação, chama o mesmo caso de uso do canal direto e publica a Task pelo
  `TaskUpdater`. Tasks em `InMemoryTaskStore` (uma instância). Os pacotes de hosting e cliente A2A
  do MAF deixam de ser usados.
- **Desvio D-07 (constitution 3.1)**: a constitution exige o MAF na hospedagem A2A. O servidor do
  SDK A2A substitui só essa camada; o agente interno, as ferramentas, os context providers, o
  middleware e o workflow continuam no MAF. Um spike no início da implementação verifica se o
  `AddA2AServer` do MAF aceita o `IAgentHandler` próprio e se o seu `IA2ARequestHandler` pode ser
  mapeado com `MapA2AWithV03Compat`; se sim, esse caminho é adotado e o D-07 é encerrado. O D-07
  fica registrado no "Complexity Tracking" do plano e precisa de aprovação antes da implementação.
- **Agent Card**: `GET /.well-known/agent-card.json` pelo `MapAgentCardGetWithV03Compat`, formato
  v0.3 estrito por padrão (`blendedCard = false`); habilidade `analisar_documentos`; extensão
  `urn:infis:sofia:contrato` (não obrigatória) com versão do contrato, `idAgente`, nome do Artifact,
  limites, documentos suportados e ausência de autenticação; sem `securitySchemes` na v1 (D-05).
- **`idAgente`**: `agente-auditoria-nfe`, configurável em `A2A:IdAgente`. O exemplo `agente-fiscal`
  da referência não é usado porque colidiria com outros agentes da turma no registry.
- **Identificação (enquanto durar o D-05)**: `chamadorId` = `A2A:ChamadorPadrao` (`sofia`);
  `usuarioNome` = `usuario.nome` sem espaços nas pontas, persistido na sessão e na execução, fora de
  logs, do envio ao LLM (ADR-0004) e do contexto do usuário; `usuarioId` = `nome-sha256:` + SHA-256
  do nome normalizado. Com o ADR da DP-10, a identificação passa a vir da credencial de serviço.
- **Execução**: uma sessão nova por solicitação, tipo `AnaliseComPergunta`, canal `A2A`;
  `idExecucao` e `idSolicitacao` da SOFIA gravados na execução, `correlacaoId` = `idSolicitacao`;
  retentativa gera nova execução.
- **Estados (D-04 revisado)**: a Task é devolvida já no estado final (`completed` com Artifact ou
  `failed` quando não há IDs válidos para montar um resultado). Os estados intermediários não são
  observáveis na v1; a SOFIA usa tempo limite de pelo menos 150 segundos.

## Consequências

- O adaptador A2A traduz o contrato SOFIA para o caso de uso e de volta; o domínio ganha só o
  `idDocumento` do arquivo recebido e os IDs da SOFIA na execução (data-model §2, §3 e §18).
- O `Sofia.Simulator` passa a usar o cliente oficial v0.3 (`A2A.V0_3`) e a montar
  `SolicitacaoAnaliseSofia` com dados sintéticos.
- Homônimos compartilham o contexto do usuário até a autenticação; o nome é dado pessoal e segue a
  retenção do ADR-0005.
- `A2A.V0_3Compat` está em preview: versão fixada, spike de validação no início da implementação e
  testes de contrato do card, da Message e da Task a cada atualização.
- A lição V-06 do AgenteFinanceiro (resultado em mensagem de estado) deixa de se aplicar, porque o
  resultado vai no Artifact; V-09 e V-10 continuam cobertas por testes.
- Mudanças compatíveis no contrato seguem `1.x`; incompatíveis exigem nova versão maior, testes de
  compatibilidade e convivência (NFR-012).

## Alternativas consideradas

- **Manter o hosting A2A do MAF**: responde com Message e não atende o contrato SOFIA.
- **Implementar o JSON-RPC v0.3 à mão**: contraria o FR-043 (SDK oficial).
- **Fixar o SDK A2A 0.3.x**: abandona a linha 1.0 já fixada e a convivência com A2A 1.0 que o
  `A2A.V0_3Compat` oferece.
- **`idAgente` = `agente-fiscal`**: colide no registry da SOFIA.
- **Manter o contrato provisório do ADR-0007 ao lado do SOFIA v1.0**: dois contratos no mesmo
  canal, sem cliente para o provisório.

## Referências

- `specs/001-agente-auditoria-nfe/contracts/sofia-a2a.md`
- `specs/001-agente-auditoria-nfe/contracts/sofia-solicitacao-analise-v1.schema.json`
- `specs/001-agente-auditoria-nfe/contracts/sofia-resultado-analise-v1.schema.json`
- `specs/001-agente-auditoria-nfe/research.md` — R-11, R-12, R-32, R-33
- SDK A2A .NET, tag `v1.0.0-preview3`: `src/A2A.V0_3Compat`
