# ADR-0002 — MongoDB para memória, sessões e contexto do usuário

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-01 (resolvida na constitution 2.0.0); P15; constitution 3.1 e 4.7; FR-037 a
  FR-042, FR-055 a FR-060; D-05

## Contexto

A constitution 2.0.0 definiu o MongoDB para a memória conversacional e o contexto do usuário e
exige que a decisão seja registrada em ADR antes da implementação (P15). Os stores em memória do
hosting do MAF servem apenas para desenvolvimento.

## Decisão

- **Coleções**: `sessoes`, `execucoes` (com parecer e recomendações), dados extraídos por documento,
  sessões serializadas do MAF (por uma implementação de `AgentSessionStore`) e `contextos_usuario`.
- **O XML original nunca é persistido** (FR-037a).
- **Isolamento**: chave = SHA-256 de `chamadorId` e `usuarioId`. Na v1 os identificadores são
  informados e não verificados (D-05); sem eles, valem "nao-informado" e "anonimo". Todo acesso a
  sessão confere a chave; sessão de outra chave responde ERR-005, sem revelar sua existência.
- **Sessão A2A**: a sessão do hosting guarda só o mapa chave → `sessaoId`. O histórico do agente
  interno fica na sessão de domínio. `AgentIsolationKeyProvider` não é usado na v1.
- **Contexto do usuário**: um documento por chave, com `$jsonSchema` que aceita só os itens do
  FR-055a (CNPJs auditados, achados determinísticos recorrentes, preferências declaradas), cada um
  com origem. Índice TTL em `expiraEm`. Gravado só pelo sistema; lido como dado não confiável e com
  limite de itens; nunca para o usuário "anonimo".
- **Expiração da sessão**: 24 h de inatividade, configurável.

## Consequências

- Histórico, contexto do usuário e conhecimento ficam separados (constitution 4.7).
- Um `contextId` A2A reutilizado por outro usuário não dá acesso ao histórico do primeiro.
- Enquanto vigorar o D-05, o isolamento depende da boa-fé de quem informa os identificadores.

## Alternativas consideradas

- Redis: não permitido pela constitution para memória.
- Contexto do usuário dentro da sessão do MAF: misturaria histórico e contexto (P15).
- `AgentIsolationKeyProvider` lendo a identidade do corpo: acoplaria a borda ao formato A2A.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-08, R-31
- `specs/001-agente-auditoria-nfe/data-model.md` — §1, §13
