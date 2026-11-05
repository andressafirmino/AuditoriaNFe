# ADR-0008 — CI, branches `dev` e `main` e proteção

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
- **Branches**:
  - `main` é a branch principal (constitution 3.5) e só recebe publicações, por PR vindo da `dev`;
  - `dev` é a branch de integração e recebe as features por PR;
  - cada fase do `tasks.md` de uma feature tem branch própria, `feat/NNN-fase-N-nome` (por exemplo,
    `feat/001-fase-2-fundacao`), criada a partir da `dev` atualizada depois do merge da fase
    anterior, com um PR por fase para a `dev` (decisão da mantenedora em 2026-11-04);
  - dentro da branch da fase, um commit por tarefa, com o ID da tarefa na mensagem.

  ```text
  feat/NNN-fase-N-nome  ──PR──▶  dev  ──PR (publicação)──▶  main
  ```

- **Proteção da `main` e da `dev`** (mesmas regras nas duas):
  - PR obrigatório;
  - check `build-test` obrigatório, assim que o workflow de CI existir;
  - sem push direto, sem force push e sem exclusão da branch;
  - **zero aprovações obrigatórias** (desvio D-06).
- **Revisão pela mantenedora**: a descrição do PR traz um checklist de conformidade com a
  constitution (princípios, desvios, testes, segredos). A mantenedora preenche o checklist e comenta
  "Aprovado" antes do merge.
- **Segredos**: teste `SemSegredosTests` falha se encontrar padrões de chave de API no repositório.
- **Imagens**: publicação em registro (GHCR) só quando houver implantação (DP-25).

## Consequências

- Não há segundo revisor enquanto vigorar o D-06; o checklist e o CI são a única barreira.
- A `main` só muda em publicações; a `dev` acumula as features já revisadas.
- Branches de feature podem receber force push enquanto o PR estiver aberto (por exemplo, para
  corrigir mensagens de commit), nunca a `dev` nem a `main`.
- Quando outra pessoa entrar como mantenedora, a proteção passa a exigir uma aprovação, o D-06 é
  encerrado e a DP-20 é reaberta.

## Alternativas consideradas

- Exigir uma aprovação: impediria qualquer merge.
- Bypass de administrador: anularia a proteção para todos os PRs.
- Só a `main`, sem `dev`: rejeitado pela mantenedora em 2026-11-04; ela quer separar integração e
  publicação.
- Uma branch por feature (`NNN-nome-da-feature`), com um único PR: substituída em 2026-11-04 por
  uma branch por fase, para PRs menores e revisáveis.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-25
- `specs/001-agente-auditoria-nfe/plan.md` — Complexity Tracking, D-06
