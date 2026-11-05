# ADR-0008 — CI e proteção da `main`

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-26; desvio D-06; DP-20; constitution 3.5, 5.3 e Definition of Done

## Contexto

A constitution exige CI, PR e revisão por pessoa diferente do autor (5.3). O projeto tem uma única
mantenedora (DP-20), e o GitHub não permite que o autor aprove o próprio PR.

## Decisão

- **GitHub Actions**:
  - job `build-test`: restore, build com avisos como erro, testes (com Docker para Testcontainers),
    sem chave da OpenAI; a avaliação com OpenAI real (`Categoria=Avaliacao`) fica fora do CI;
  - job de build da imagem Docker.
- **Proteção da `main`**:
  - PR obrigatório;
  - check `build-test` obrigatório;
  - sem push direto e sem force push;
  - **zero aprovações obrigatórias** (desvio D-06).
- **Revisão pela mantenedora**: a descrição do PR traz um checklist de conformidade com a
  constitution (princípios, desvios, testes, segredos). A mantenedora preenche o checklist e comenta
  "Aprovado" antes do merge.
- **Segredos**: teste `SemSegredosTests` falha se encontrar padrões de chave de API no repositório.
- **Imagens**: publicação em registro (GHCR) só quando houver implantação (DP-25).

## Consequências

- Não há segundo revisor enquanto vigorar o D-06; o checklist e o CI são a única barreira.
- Quando outra pessoa entrar como mantenedora, a proteção passa a exigir uma aprovação, o D-06 é
  encerrado e a DP-20 é reaberta.

## Alternativas consideradas

- Exigir uma aprovação: impediria qualquer merge.
- Bypass de administrador: anularia a proteção para todos os PRs.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-25
- `specs/001-agente-auditoria-nfe/plan.md` — Complexity Tracking, D-06
