# Contrato A2A: SOFIA ↔ AgenteAuditoriaNFe

**Situação**: **proposta v1 provisória** — o contrato definitivo com a SOFIA depende da DP-27
(constitution 2.1.0) | **Versão do contrato**: 1.0 | **Protocolo**: A2A 1.0 (SDK oficial `A2A`
1.0.0-preview3, hosting MAF 1.26.0-preview.261023.1) | **Feature**: `001-agente-auditoria-nfe` |
**Data**: 2026-10-29, revisado em 2026-11-04

A SOFIA é um orquestrador externo, não implementado neste projeto, e ainda não chama agentes
externos. Este documento é a proposta do agente para a interface entre os dois; até o acordo com o
responsável pela SOFIA (DP-27), ele é validado apenas pelo Sofia.Simulator. Mudanças acordadas
afetam o adaptador A2A e o Agent Card, e não o caso de uso de análise, que é o mesmo do canal direto
([api-agente.openapi.yaml](./api-agente.openapi.yaml)).

Este contrato complementa a [spec](../spec.md) (FR-043 a FR-047, FR-035g, FR-048 a FR-052c) e o
[modelo de dados](../data-model.md). Os exemplos JSON seguem a representação HTTP+JSON do A2A 1.0;
os nomes exatos das propriedades são os do SDK fixado e são verificados pelos testes de contrato
(TS-029, TS-032, TS-034, TS-035).

> **Sem autenticação na v1 (desvio D-05).** O agente só pode ser executado em ambiente local de
> testes, com as portas publicadas em `127.0.0.1` e dados sintéticos ou anonimizados (FR-052b). A
> integração com a SOFIA real exige o fim do D-05.

## 1. Endereços

| Recurso | Método e caminho | Autenticação |
| ------- | ---------------- | ------------ |
| Agent Card | `GET /.well-known/agent-card.json` | Pública (sem segredos nem dados de usuário — FR-045) |
| Envio de mensagem (HTTP+JSON, preferencial) | `POST /a2a/auditoria-nfe/message:send` | Nenhuma na v1 (D-05) |
| Envio com streaming (HTTP+JSON) | `POST /a2a/auditoria-nfe/message:stream` | Nenhuma na v1 (não contratual na v1, ver 7) |
| JSON-RPC 2.0 (alternativo) | `POST /a2a/auditoria-nfe` com método `SendMessage` | Nenhuma na v1 (D-05) |

A URL base é configurável (`A2A:UrlPublica`; no ambiente local, `http://localhost:5310`) e é a que
aparece em `supportedInterfaces` do card.

## 2. Agent Card

Gerado a partir da configuração na inicialização. Exemplo (valores de limites são os iniciais da
spec):

```json
{
  "name": "AgenteAuditoriaNFe",
  "description": "Analisa lotes de XML de NF-e (modelo 55, layout 4.00): valida a estrutura contra o schema oficial vigente na data de cada documento e confere os totalizadores do catálogo aprovado, emite parecer estruturado e responde perguntas sobre a base analisada sem inventar valores. Versão sem autenticação, somente para ambiente local de testes.",
  "version": "1.0.0",
  "supportedInterfaces": [
    { "url": "http://localhost:5310/a2a/auditoria-nfe", "protocolBinding": "HTTP+JSON", "protocolVersion": "1.0" },
    { "url": "http://localhost:5310/a2a/auditoria-nfe", "protocolBinding": "JSONRPC", "protocolVersion": "1.0" }
  ],
  "capabilities": {
    "streaming": false,
    "pushNotifications": false,
    "extensions": [
      {
        "uri": "urn:agente-auditoria-nfe:contrato",
        "description": "Contrato de solicitação e envelope de resposta do AgenteAuditoriaNFe (proposta v1 provisória).",
        "required": true,
        "params": {
          "versaoContrato": "1.0",
          "situacaoContrato": "provisorio",
          "versaoEnvelope": "1.0",
          "autenticacao": "nenhuma — somente ambiente local de testes",
          "limites": { "tamanhoTotalBytes": 5242880, "quantidadeArquivos": 20 },
          "formatosArquivo": ["application/xml", "text/xml"],
          "documentosSuportados": [{ "tipo": "NF-e", "modelo": "55", "versoesLayout": ["4.00"] }],
          "validacoes": ["schema oficial vigente na data de referência", "totalizadores do catálogo de regras v1"],
          "idioma": "pt-BR"
        }
      }
    ]
  },
  "defaultInputModes": ["text/plain", "application/xml"],
  "defaultOutputModes": ["text/plain", "application/json"],
  "skills": [
    {
      "id": "analisar-lote-nfe",
      "name": "Analisar lote de NF-e",
      "description": "Recebe um ou mais XMLs de NF-e modelo 55, com ou sem pergunta, valida estrutura e totalizadores e devolve o parecer.",
      "tags": ["nfe", "auditoria", "xml", "parecer"],
      "examples": ["Analise estes XMLs e informe as inconsistências."],
      "inputModes": ["text/plain", "application/xml"],
      "outputModes": ["text/plain", "application/json"]
    },
    {
      "id": "perguntar-base-analisada",
      "name": "Perguntar sobre a base analisada",
      "description": "Responde perguntas sobre os documentos já analisados na sessão, somente com valores rastreáveis.",
      "tags": ["nfe", "perguntas", "sessao"],
      "examples": ["Qual o valor total das notas analisadas?"],
      "inputModes": ["text/plain"],
      "outputModes": ["text/plain", "application/json"]
    },
    {
      "id": "consultar-recomendacoes",
      "name": "Consultar recomendações demonstrativas",
      "description": "Devolve o estado das recomendações de uma execução ou sessão e o conteúdo das liberadas ou aprovadas.",
      "tags": ["recomendacoes", "aprovacao"],
      "examples": ["Consultar recomendações da execução informada."],
      "inputModes": ["text/plain"],
      "outputModes": ["application/json"]
    }
  ]
}
```

Regras:

- O card publica nome, descrição, versão do agente (`version`), versão e situação do contrato
  (extensão), habilidades, formatos, limites e a ausência de autenticação (FR-044, FR-052b).
- O card não declara `securitySchemes` nem `securityRequirements` na v1. Com o fim do D-05, o
  esquema da credencial de serviço (ADR da DP-10) entra no card em nova versão menor do contrato.
- Mudança de limite por configuração altera o card sem nova versão do contrato (FR-005).
- O card não contém segredos, chaves, endereços internos nem dados de usuários (FR-045).
- A estrutura de `extensions` segue o SDK A2A fixado; se o SDK usar nomes diferentes, prevalece o
  SDK, e este documento é atualizado no mesmo PR.

## 3. Identificação do usuário e do chamador (D-05)

- O chamador informa, nos metadados da solicitação, o identificador do usuário final (`usuarioId`)
  e a sua própria identificação (`chamadorId`). Os dois são registrados e usados no isolamento de
  sessões e do contexto do usuário, **sem verificação** (FR-052a).
- Sem `usuarioId`, vale `"anonimo"`; sem `chamadorId`, vale `"nao-informado"`. O usuário
  `"anonimo"` não tem contexto do usuário lido nem gravado, e o parecer indica que o usuário não foi
  identificado (FR-052c).
- Uma sessão só é acessível pelo mesmo `usuarioId` informado pelo mesmo `chamadorId` (FR-039).
- Não há papel de aprovador na v1; aprovar ou rejeitar só pelo canal direto (FR-035d).
- Obrigação da SOFIA (quando a integração for acordada): informar `usuarioId` e `chamadorId` em toda
  solicitação. Com o fim do D-05, `chamadorId` passa a ser o da credencial de serviço autenticada.

## 4. Solicitação

### 4.1 Estrutura

```json
{
  "message": {
    "messageId": "0193f0c2-5b1e-7c3a-9d2f-1a2b3c4d5e6f",
    "contextId": "opcional — omitido na primeira solicitação",
    "role": "ROLE_USER",
    "parts": [
      { "text": "Quais notas têm totalizador divergente?" },
      { "raw": "<conteúdo do XML em base64>", "mediaType": "application/xml", "filename": "nfe-0001.xml" },
      { "raw": "<conteúdo do XML em base64>", "mediaType": "application/xml", "filename": "nfe-0002.xml" }
    ]
  },
  "metadata": {
    "usuarioId": "usuario-demo-01",
    "chamadorId": "sofia-simulador",
    "correlacaoId": "sofia-2026-11-04-000123",
    "operacao": "analisar",
    "sessaoId": "opcional — continuidade explícita"
  }
}
```

### 4.2 Partes da mensagem

| Parte | Cardinalidade | Regras |
| ----- | ------------- | ------ |
| Texto (`text`) | 0..1 | Pergunta em linguagem natural; tratada como dado, nunca como instrução ao sistema (FR-053) |
| Arquivo (`raw` + `mediaType` + `filename`) | 0..N | `mediaType` `application/xml` ou `text/xml`; o tipo real é verificado pelo conteúdo (FR-006); `filename` obrigatório |
| Arquivo por URL (`url`) | 0 | Não aceito (o agente não busca recursos remotos); gera ERR-007 |
| Dados (`data`) | 0 | Não aceito na v1; gera ERR-007 |

Ao menos uma parte de texto ou de arquivo é obrigatória (senão ERR-001). Os limites de tamanho total
(soma dos bytes decodificados) e de quantidade são validados antes de qualquer análise (ERR-002,
ERR-003, NFR-002).

### 4.3 Metadados

Os metadados da solicitação são entregues pelo hosting do MAF ao agente em
`AgentRunOptions.AdditionalProperties`. O teste de contrato confirma o nível (solicitação ou
mensagem) no SDK fixado; o agente lê os dois, com prioridade para o da solicitação.

| Chave | Obrigatória | Regras |
| ----- | ----------- | ------ |
| `usuarioId` | Não | Até 128 caracteres `[A-Za-z0-9._:@-]`; ausente → `"anonimo"`; inválido → ERR-007 |
| `chamadorId` | Não | Mesmo formato; ausente → `"nao-informado"`; inválido → ERR-007 |
| `correlacaoId` | Não | Até 128 caracteres `[A-Za-z0-9._:-]`; devolvido sem alteração (FR-047); inválido gera ERR-007 |
| `operacao` | Não | `analisar`, `perguntar` ou `consultar-recomendacoes`. Se omitida: com arquivos → `analisar`; só texto → `perguntar` |
| `sessaoId` | Não | Continuidade explícita; se não pertencer ao mesmo usuário e chamador → ERR-005; expirada → ERR-006 |
| `execucaoId` | Em `consultar-recomendacoes` | Execução cujas recomendações serão consultadas (ou use `sessaoId`) |
| `preferencias` | Não | `{ "nivelDetalhe": "Resumido" \| "Detalhado", "tributosInteresse": [...] }`; declaração explícita gravada no contexto do usuário (FR-055a, FR-055b); ignorada para `"anonimo"`; valor fora da lista → ERR-007 |

### 4.4 Sessão e `contextId`

- A sessão A2A segue o `contextId`. Na primeira solicitação, a SOFIA omite o `contextId`; a resposta
  traz o `contextId` e o `sessaoId` (FR-036).
- Para continuar, a SOFIA reenvia o mesmo `contextId` e, recomendado, o `sessaoId` em metadados.
- O `contextId` não é credencial: a sessão de domínio é conferida pelo `usuarioId` e pelo
  `chamadorId`. Um `contextId` desconhecido para essa identificação, sem `sessaoId`, inicia nova
  sessão, sem revelar se o identificador existe para outro usuário (FR-039).

## 5. Resposta

### 5.1 Estrutura

Na v1, toda resposta é uma mensagem imediata do agente (sem tarefa A2A — ver 7).

```json
{
  "message": {
    "messageId": "0193f0c3-0000-7000-8000-000000000001",
    "contextId": "ctx-gerado-ou-recebido",
    "role": "ROLE_AGENT",
    "parts": [
      { "text": "Foram recebidos 2 arquivos e analisados 2. Resultado geral: Com inconsistências. ..." },
      { "raw": "<envelope JSON em base64>", "mediaType": "application/json", "filename": "envelope-auditoria-nfe.v1.json" }
    ],
    "metadata": {
      "execucaoId": "0193f0c2-6a00-7b00-8c00-0000000000aa",
      "sessaoId": "0193f0c2-6a00-7b00-8c00-0000000000bb",
      "correlacaoId": "sofia-2026-11-04-000123",
      "estado": "Concluida",
      "versaoContrato": "1.0",
      "versaoEnvelope": "1.0",
      "dataHora": "2026-11-04T14:03:12Z"
    }
  }
}
```

- **Parte de texto**: resumo textual em pt-BR (FR-025, NFR-010).
- **Parte de arquivo JSON**: envelope v1 completo, produzido a partir de um `DataContent`
  `application/json` (o SDK converte `DataContent` em parte de arquivo). O cliente DEVE decodificar
  `raw` e validar `versaoEnvelope`.
- **Metadados**: identificadores e estado preenchidos pelo sistema (FR-054, P09), copiados do
  envelope para leitura rápida. Em caso de divergência, prevalece o envelope.

### 5.2 Envelope v1 (resumo)

Definição completa em [data-model.md](../data-model.md) §16 e no schema `EnvelopeV1` de
[api-agente.openapi.yaml](./api-agente.openapi.yaml). Exemplo reduzido de análise:

```json
{
  "versaoEnvelope": "1.0",
  "versaoContratoIa": "1.0",
  "execucaoId": "0193f0c2-6a00-7b00-8c00-0000000000aa",
  "sessaoId": "0193f0c2-6a00-7b00-8c00-0000000000bb",
  "correlacaoId": "sofia-2026-11-04-000123",
  "canal": "A2A",
  "estado": "Concluida",
  "dataHora": "2026-11-04T14:03:12Z",
  "usuarioIdentificado": true,
  "etapas": [
    { "numero": 4, "nome": "Validação de estrutura e schema", "estado": "Executada" },
    { "numero": 5, "nome": "Consulta aos manuais por MCP", "estado": "NaoAplicavel", "justificativa": "integração não habilitada" }
  ],
  "parecer": {
    "resumoQuantitativo": { "recebidos": 2, "analisados": 2, "comAchados": 1, "naoAnalisados": 0 },
    "resultadoGeral": "ComInconsistencias",
    "documentos": [
      {
        "arquivo": "nfe-0002.xml",
        "chaveAcesso": "<44 caracteres>",
        "dataReferencia": "2026-09-15",
        "origemDataReferencia": "dhRecbto",
        "pacoteAplicado": "PL_010e_v1.02",
        "pacotesTentados": ["PL_010e_v1.02"],
        "emConvivencia": false,
        "validacaoLayout": "Realizada",
        "validacaoConteudo": "Realizada"
      }
    ],
    "achados": [
      {
        "id": "0193f0c2-7000-7000-8000-000000000101",
        "categoria": "InconsistenciaNegocio",
        "severidade": "Alta",
        "origem": "Deterministica",
        "arquivo": "nfe-0002.xml",
        "campo": "NFe/infNFe/total/ICMSTot/vICMS",
        "valorInformado": "<valor do XML>",
        "valorEsperado": "<soma dos itens>",
        "regraId": "W04-10",
        "versaoRegra": 1,
        "descricao": "Total do ICMS diferente da soma do ICMS dos itens.",
        "evidencias": [{ "tipo": "RegraCatalogo", "regraId": "W04-10", "versaoCatalogo": "1" }]
      }
    ],
    "limitacoes": ["consulta a manuais oficiais não habilitada"],
    "validacoesNaoRealizadas": [],
    "revisaoHumana": { "necessaria": true, "motivos": ["consulta a manuais oficiais não habilitada"] },
    "recomendacoes": [
      { "id": "0193f0c2-7000-7000-8000-000000000201", "estado": "PendenteAprovacao", "rotulo": "Recomendação demonstrativa" }
    ],
    "integracaoMcp": { "habilitada": false },
    "resumoTextual": "..."
  },
  "metadados": {
    "modelo": "<modelo configurado>",
    "duracaoMs": 18432,
    "consumo": { "tokensEntrada": 5120, "tokensSaida": 860, "custoEstimado": null, "moeda": null, "origem": "InformadoProvedor", "registroCompleto": true }
  }
}
```

Em perguntas, `parecer` é omitido e `resposta` traz `texto`, `valoresCitados` (com `calculoId` ou campo
de origem) e `documentosExcluidos` (FR-026 a FR-031).

### 5.3 Consulta de recomendações (FR-035g)

Solicitação com `metadata.operacao = "consultar-recomendacoes"` e `execucaoId` ou `sessaoId`. O
envelope traz `recomendacoes`:

```json
{
  "recomendacoes": [
    { "id": "…201", "estado": "Aprovada", "rotulo": "Recomendação demonstrativa", "aviso": "…", "texto": "…", "achadosVinculados": ["…101"] },
    { "id": "…202", "estado": "PendenteAprovacao", "rotulo": "Recomendação demonstrativa" },
    { "id": "…203", "estado": "Rejeitada", "rotulo": "Recomendação demonstrativa" },
    { "id": "…204", "estado": "Indisponivel", "rotulo": "Recomendação demonstrativa" }
  ]
}
```

`texto` só aparece em `Liberada` e `Aprovada`; nunca em `PendenteAprovacao`, `Rejeitada` ou
`Indisponivel` (FR-035c, FR-035g, SC-004a).

## 6. Erros

| Código | Como chega à SOFIA | Estado |
| ------ | ------------------ | ------ |
| ERR-007 (estrutura A2A inválida) | Erro de protocolo do SDK A2A (HTTP 400 no HTTP+JSON; erro JSON-RPC no JSON-RPC) | — |
| ERR-007 (violação deste contrato: parte `url` ou `data`, metadado inválido, mais de uma parte de texto) | Mensagem do agente com envelope `estado = Rejeitada` e `erro` | Rejeitada |
| ERR-001, ERR-002, ERR-003, ERR-005, ERR-006 | Mensagem do agente com envelope `estado = Rejeitada` e `erro` | Rejeitada |
| ERR-024 (limite de taxa ou de análises simultâneas) | Mensagem do agente com envelope `estado = Rejeitada` e `erro.detalhes.tentarNovamenteEmSegundos` | Rejeitada |
| ERR-025 (armazenamento de sessões indisponível) | Mensagem do agente com envelope `estado = Rejeitada` e `erro`; nenhum parecer parcial | Rejeitada |
| ERR-008 a ERR-012, ERR-023 | No parecer, por arquivo; os demais arquivos seguem | Concluida |
| ERR-013, ERR-014, ERR-015 | Parecer com resultados determinísticos, resultado "Inconclusiva" e achado de falha técnica | Concluida |
| ERR-016 | Envelope com etapas concluídas e não executadas | Falhou |
| ERR-017, ERR-018 | Resposta normal declarando indisponibilidade ou fora de escopo (não são erros técnicos) | Concluida |
| ERR-004, ERR-019 | Adiados com o desvio D-05; não ocorrem na v1 | — |

Formato de `erro` no envelope: `{ "codigo": "ERR-002", "mensagem": "<pt-BR>", "arquivo": null,
"detalhes": { "limiteBytes": 5242880, "totalRecebidoBytes": 6291456 } }`.

## 7. Estados da tarefa e limitações (FR-046)

- O hosting A2A do MAF na versão fixada não suporta respostas em segundo plano
  (`AgentRunMode.DisallowBackground`). A v1 devolve sempre uma mensagem final, com `estado`
  `Concluida`, `Falhou` ou `Rejeitada` nos metadados e no envelope. Os estados intermediários
  "recebida" e "em processamento" não são observáveis por A2A na v1 (desvio D-04 do plano).
- A SOFIA DEVE usar tempo limite de requisição de pelo menos 150 segundos (NFR-001 mais margem). O
  agente encerra a execução no tempo máximo configurado (`Execucao:TempoMaximo`, inicial 120 s) com
  ERR-016.
- Mapeamento reservado para quando o hosting suportar tarefas: `submitted` → Recebida, `working` →
  EmProcessamento, `completed` → Concluida, `failed` → Falhou, `rejected` → Rejeitada. A ativação
  exige nova versão menor do contrato, `AllowBackgroundIfSupported` e os cuidados V-06 e V-10
  (research R-33).
- `message:stream` não é contratual na v1 (`capabilities.streaming = false`).

## 8. Versionamento

- Versão do contrato `1.0`, situação `provisorio`, publicadas no card (extensão) e em cada resposta.
- O acordo com a SOFIA (DP-27) fixa a versão definitiva; se exigir mudanças incompatíveis, ela sai
  como `2.0`.
- Alteração compatível (campo opcional novo, nova habilidade): versão menor (`1.1`).
- Alteração incompatível: nova versão maior, nova URL de interface no card e período de convivência
  com a versão anterior (NFR-012, P18).

## 9. Uso pelo Sofia.Simulator

| Cenário | Comportamento esperado | Referência |
| ------- | ---------------------- | ---------- |
| Descoberta | Obtém o card, valida campos do FR-044 e exibe habilidades, limites e a ausência de autenticação | TS-029 |
| Fluxo básico | Envia pergunta e XMLs de exemplo sintéticos, com `usuarioId` e `chamadorId`; exibe estado, resumo, parecer (inclusive pacote aplicado por documento), `execucaoId` e `sessaoId` | TS-035 |
| Card inacessível | Informa falha de descoberta e não envia solicitação | TS-035a |
| Continuidade (opcional) | Reenvia `contextId` e `sessaoId` com nova pergunta | TS-021 |
| Recomendações (opcional) | Usa `consultar-recomendacoes` com o `execucaoId` | TS-039 |
| Sem identificação (opcional) | Envia sem `usuarioId` e `chamadorId`; o parecer indica usuário não identificado | TS-059 |
