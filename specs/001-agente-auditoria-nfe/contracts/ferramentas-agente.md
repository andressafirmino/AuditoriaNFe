# Contrato das ferramentas do agente interno (Function Calling)

**Versão do contrato**: 1.0 | **Feature**: `001-agente-auditoria-nfe` | **Data**: 2026-11-04 |
**Base**: constitution 4.6, P07, P10 e P16; research R-06 e R-09; data-model §9 e §15

Este documento define as ferramentas que o `ChatClientAgent` interno da etapa 7 pode chamar. Todas
são funções do Microsoft Agent Framework criadas com `AIFunctionFactory.Create` e `[Description]`, na
camada de Aplicação. O nome, os argumentos e a saída abaixo são o contrato: o teste de contrato
compara o schema JSON de cada `AIFunction` com este documento, e qualquer mudança incompatível gera
nova versão (NFR-012).

## 1. Regras comuns

| Item | Regra |
| ---- | ----- |
| Natureza | Somente leitura sobre os dados da sessão da execução; nenhuma ferramenta grava, altera ou exclui dados, nem acessa o contexto do usuário (FR-057) |
| Autorização | Pelo sistema, não pela escolha do LLM (P10): o middleware de Function Calling aplica a lista de ferramentas permitidas para o tipo da execução (seção 3) e restringe os documentos aos da sessão da execução, conferida pela `chaveIsolamento` |
| Validação de argumentos | Antes da execução; argumento inválido é rejeitado com `ArgumentoInvalido`, sem correção por suposição (constitution 4.6) |
| Timeout | `Ferramentas:Timeout` por chamada (inicial 5 s*); estouro → `Timeout` |
| Falha | Erros viram resultado `{ "erro": { "codigo": ..., "mensagem": ... } }` para o modelo; a execução registra a falha e, se a análise não puder seguir, aplica ERR-014 ou ERR-015 (categoria "Falha técnica ou dependência indisponível") |
| Retentativa e idempotência | Sem retentativa automática dentro da ferramenta; todas são idempotentes por serem de leitura |
| Registro | Cada chamada entra em `Execucao.chamadasFerramentas` com nome, início, duração, status, erro e `calculoId`, sem argumentos de texto livre nem valores de documentos (data-model §2, P16) |
| Resultado | Tratado como dado não confiável pelo modelo e pelo validador da saída (P07) |
| Limite de chamadas | O loop de ferramentas segue o limite do MAF e o tempo máximo da execução (`Execucao:TempoMaximo`); cada volta é registrada no consumo (P14) |

Códigos de erro das ferramentas (internos, não são ERR-xxx da spec): `ArgumentoInvalido`,
`DocumentoNaoEncontrado`, `CampoNaoCatalogado`, `NaoAutorizada`, `Timeout`, `FalhaInterna`.

## 2. Ferramentas

### 2.1 `listar_documentos`

Lista os documentos da base da sessão, com o estado das validações.

| Entrada | Tipo | Obrigatório | Regras |
| ------- | ---- | ----------- | ------ |
| — | — | — | Sem argumentos |

| Saída | Tipo | Regras |
| ----- | ---- | ------ |
| `documentos[]` | lista | Um item por documento da sessão |
| `documentos[].chaveAcesso` | string (44) | |
| `documentos[].arquivo` | string | Nome saneado do arquivo de origem |
| `documentos[].dataReferencia` / `origemDataReferencia` | string / string | FR-010d |
| `documentos[].pacoteAplicado` | string | |
| `documentos[].validacaoLayout` / `validacaoConteudo` | string | `Realizada` ou `NaoRealizada` com motivo |
| `documentos[].duplicadoDe` | string? | Chave do documento original, quando duplicado |

### 2.2 `obter_campos_extraidos`

Devolve valores de campos extraídos de um documento, com o caminho no XML.

| Entrada | Tipo | Obrigatório | Regras |
| ------- | ---- | ----------- | ------ |
| `chaveAcesso` | string | Sim | 44 dígitos; documento da sessão (senão `DocumentoNaoEncontrado`) |
| `campos` | lista de string | Sim | 1 a 20 itens; cada um DEVE estar no catálogo de extração (senão `CampoNaoCatalogado`, e a IA segue o FR-037b) |

| Saída | Tipo | Regras |
| ----- | ---- | ------ |
| `chaveAcesso` | string | |
| `valores[]` | lista | `campo`, `caminho` (XPath no XML de origem), `valor` (texto, como no XML), `item` (número do item, quando o campo é de item) |

Campos de classe Pessoal seguem o mascaramento do ADR-0004 na saída.

### 2.3 `calcular_total`

Soma um campo catalogado sobre os documentos da sessão e registra o cálculo (data-model §9).

| Entrada | Tipo | Obrigatório | Regras |
| ------- | ---- | ----------- | ------ |
| `campo` | string | Sim | Campo numérico do catálogo de extração (senão `CampoNaoCatalogado`) |
| `chavesAcesso` | lista de string | Não | Restringe aos documentos informados, todos da sessão; ausente = todos |
| `incluirComErroEstrutural` | bool | Não | Padrão `false`; quando `true`, os documentos com erro estrutural entram "com ressalva" (FR-031) |

| Saída | Tipo | Regras |
| ----- | ---- | ------ |
| `calculoId` | string (GUID) | Gerado pelo sistema e registrado na execução; a IA DEVE citá-lo em `valoresCitados` |
| `campo` / `operacao` | string | `operacao` = `Soma` |
| `resultado` | string (decimal) | Sem arredondamento |
| `documentosIncluidos[]` | lista | `chaveAcesso` e `comRessalva` |
| `documentosExcluidos[]` | lista | `chaveAcesso` e `motivo` (`ErroEstrutural`, `Duplicado`, `NaoSuportado`, `ForaDoFiltro`) |

### 2.4 `listar_achados`

Lista os achados da execução e da base da sessão.

| Entrada | Tipo | Obrigatório | Regras |
| ------- | ---- | ----------- | ------ |
| `chaveAcesso` | string | Não | 44 dígitos; filtra por documento |
| `categoria` | string | Não | Uma das cinco categorias do P04 |
| `severidade` | string | Não | `Alta`, `Media`, `Baixa` ou `Informativa` |

| Saída | Tipo | Regras |
| ----- | ---- | ------ |
| `achados[]` | lista | `id`, `categoria`, `severidade`, `origem`, `chaveAcesso`, `campo`, `item`, `valorInformado`, `valorEsperado`, `regraId`, `versaoRegra`, `pacoteSchema`, `descricao` |

## 3. Ferramentas permitidas por tipo de execução

| Tipo da execução | Ferramentas permitidas |
| ---------------- | ---------------------- |
| `AnaliseLote` | `listar_documentos`, `listar_achados` |
| `Pergunta`, `AnaliseComPergunta` | As quatro |
| `ConsultaRecomendacoes` | Nenhuma (não há chamada à IA) |

Chamada a ferramenta fora da lista → `NaoAutorizada`, registro da tentativa e
`context.Terminate` no middleware (research R-09).

## 4. Versionamento

Mudanças compatíveis (novo campo opcional na saída) geram versão 1.x. Remoção ou mudança de
argumento, de nome ou de significado gera versão 2.0, com atualização das instruções do agente e do
teste de contrato no mesmo PR.
