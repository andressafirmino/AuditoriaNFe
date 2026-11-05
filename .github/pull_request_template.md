## Resumo

<!-- O que muda e por quê. Cite a feature, as tarefas (Txxx) e os requisitos (FR, NFR, TS) atendidos. -->

## Princípios da constitution afetados

<!-- Marque os princípios tocados por este PR e explique como cada um foi atendido. -->

- [ ] P01 — Especificação antes da implementação
- [ ] P02 — Fontes oficiais como única base normativa
- [ ] P03 — Validação determinística precede e prevalece sobre o LLM
- [ ] P04 — Classificação explícita dos achados
- [ ] P05 — Nenhuma conformidade sem validações obrigatórias concluídas
- [ ] P06 — Fundamentação rastreável
- [ ] P07 — Conteúdo externo é dado não confiável
- [ ] P08 — Saída estruturada contratada e validada
- [ ] P09 — Metadados operacionais pertencem ao sistema
- [ ] P10 — Ferramentas governadas pelo sistema
- [ ] P11 — Serviços proprietários dos seus dados
- [ ] P12 — Camadas separadas e simplicidade justificada
- [ ] P13 — Configuração externa e parâmetros avaliados
- [ ] P14 — Contabilização fiel de consumo e custo
- [ ] P15 — Memória e contexto do usuário isolados e governados
- [ ] P16 — Observabilidade e rastreabilidade de execução
- [ ] P17 — Testabilidade e avaliação contínua
- [ ] P18 — Interoperabilidade por contrato (A2A e MCP)

## Desvios aprovados (plan.md, Complexity Tracking)

- [ ] D-03 — Workflow do MAF e registro JSON próprio para a aprovação: o PR mantém os dois formatos coerentes
- [ ] D-04 — Sem estados intermediários de tarefa por A2A: nenhuma resposta em segundo plano foi introduzida
- [ ] D-05 — Versão 1 sem autenticação: execução só em `Development`, portas em `127.0.0.1`, identificação tratada como não verificada
- [ ] D-06 — Revisão pela própria mantenedora: este checklist foi preenchido e o PR recebeu o comentário "Aprovado"
- [ ] Nenhum desvio novo; ou o desvio novo está registrado e aprovado no `plan.md`

## Testes

- [ ] Build sem avisos (`dotnet build AgenteAuditoriaNFe.slnx`)
- [ ] `dotnet test --project tests/AuditoriaNFe.Tests --filter-not-trait "Categoria=Avaliacao"` aprovado localmente e no check `build-test`
- [ ] Testes novos escritos antes da implementação, cobrindo os cenários TS citados no resumo
- [ ] Dados de teste sintéticos ou anonimizados (sem XML real de contribuinte)
- [ ] Avaliação com a OpenAI real executada, quando o PR muda instruções, contrato do LLM ou modelo (`Categoria=Avaliacao`)

## Segurança e dados

- [ ] Nenhum segredo, `.env`, chave ou credencial versionado (`SemSegredosTests` aprovado)
- [ ] Nenhum conteúdo de XML, pergunta, resposta, contexto do usuário ou `usuarioId` em claro em logs
- [ ] Regras fiscais, leiautes e schemas só de fontes oficiais, com documento, versão e hash

## Aprovação (desvio D-06)

Antes do merge, a mantenedora revisa este checklist e comenta **"Aprovado"** no PR.
