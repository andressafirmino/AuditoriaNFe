# ADR-0004 — Minimização de dados enviados ao LLM

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-14; constitution 5.1; P07, P15; FR-055a

## Contexto

A DP-14 exige ADR antes da primeira chamada ao LLM com dados de documento. NF-e contém dados
pessoais (CPF, nomes, endereços, telefones, e-mails) que não são necessários para a análise dos
totalizadores e da estrutura.

## Decisão

- Enviar ao provedor apenas os dados extraídos necessários à análise (catálogo de extração) e os
  resultados determinísticos (achados, cálculos, etapas).
- Nunca enviar o XML integral.
- Mascarar CPF, nomes de pessoas físicas, endereços, telefones e e-mails antes do envio.
- Do contexto do usuário, enviar só CNPJs, contagens, achados recorrentes e preferências (FR-055a),
  dentro do limite `ContextoUsuario:MaxItensNoPrompt`.
- Usar Chat Completions sem armazenamento de conversa no provedor; o histórico fica no MongoDB.
- A lista de campos enviados é configuração versionada e revisada em PR.

## Consequências

- A primeira chamada ao LLM com dados de documento está liberada, desde que siga esta política.
- Testes DEVEM verificar que os campos mascarados não aparecem no conteúdo enviado ao `IChatClient`.
- Perguntas que dependam de dado mascarado recebem resposta de informação indisponível.

## Alternativas consideradas

- Enviar o XML integral: expõe dados pessoais sem necessidade.
- API Responses com conversa armazenada no provedor: estado fora do sistema, conflito com DP-13 e
  DP-14.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-03, R-18
- `specs/001-agente-auditoria-nfe/data-model.md` — classificação de campos
