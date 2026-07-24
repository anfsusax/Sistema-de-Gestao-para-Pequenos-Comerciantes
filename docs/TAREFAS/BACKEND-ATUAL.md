# TÍTULO

Nenhuma tarefa Backend liberada.

# OBJETIVO

Modelo para a próxima tarefa fechada do Cloud Code.

# CONTEXTO

A primeira ação recomendada é o baseline runtime no ambiente Principal. Uma tarefa Backend só deve ser liberada após existir um defeito reproduzido ou uma entrega priorizada.

# ESCOPO PERMITIDO

- Trabalhar somente na worktree Backend.
- Ler `WORKTREE.md`, `ESTADO.md`, `CONTRATOS.md` e este arquivo.
- Alterar apenas os arquivos listados na tarefa aprovada.

# FORA DO ESCOPO

- Frontend Blazor, CSS e layouts.
- Mudança de arquitetura.
- Repository, Unit of Work, Controllers, API REST ou camada Application.
- Migration sem autorização.
- Autenticação, refatoração geral ou atualização de pacotes fora da tarefa.

# ARQUIVOS PROVÁVEIS

A definir conforme o defeito ou entrega priorizada.

# CONTRATO

Nenhum contrato ativo. Não alterar assinaturas consumidas pelo Frontend.

# REGRAS DE NEGÓCIO

Preservar as regras existentes até que a tarefa registre explicitamente a mudança autorizada.

# CRITÉRIOS DE ACEITE

- [ ] Escopo fechado e verificável
- [ ] Contrato definido quando houver consumo pelo Frontend
- [ ] Build sem erros
- [ ] Testes aplicáveis executados
- [ ] Nenhum arquivo fora do escopo alterado

# TESTES OBRIGATÓRIOS

Definir cenários antes da execução. Sempre compilar a solução e executar os testes aplicáveis.

# RESTRIÇÕES

- Não gerar migration.
- Não alterar frontend.
- Não mudar arquitetura.
- Não executar refatoração ampla.
- Não tocar em arquivo compartilhado sem coordenação.

# RESULTADO ESPERADO

Uma entrega pequena, reproduzível, compilada e documentada.

# FORMATO DO RELATÓRIO FINAL

Preencher `docs/RELATORIOS/BACKEND.md` com resumo, arquivos alterados, testes, resultado do build, riscos, pendências e hash do commit pequeno e descritivo.
