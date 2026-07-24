# TÍTULO

Nenhuma tarefa Frontend liberada.

# OBJETIVO

Modelo para a próxima tarefa fechada do Cursor.

# CONTEXTO

A primeira ação recomendada é o baseline runtime no ambiente Principal. Uma tarefa Frontend só deve ser liberada após existir um defeito reproduzido ou uma entrega priorizada.

# ESCOPO PERMITIDO

- Trabalhar somente na worktree Frontend.
- Ler `WORKTREE.md`, `ESTADO.md`, `CONTRATOS.md` e este arquivo.
- Alterar apenas páginas, componentes e estilos listados na tarefa aprovada.

# FORA DO ESCOPO

- Banco, DbContext, migrations, entidades e persistência.
- Regras de negócio e novos serviços duplicados.
- Autenticação sem autorização.
- Mudança de arquitetura ou refatoração geral.

# ARQUIVOS PROVÁVEIS

A definir conforme o defeito ou entrega priorizada.

# CONTRATO

Nenhum contrato ativo. Consumir somente os Services e modelos aprovados.

# REGRAS DE NEGÓCIO

Não inventar validações, descontos, preços, estados ou comportamentos.

# CRITÉRIOS DE ACEITE

- [ ] Escopo fechado e verificável
- [ ] Mobile e responsividade contemplados
- [ ] Estados de carregamento, vazio, sucesso e erro definidos
- [ ] Build sem erros
- [ ] Nenhum arquivo fora do escopo alterado

# TESTES OBRIGATÓRIOS

Definir páginas, cenários e breakpoints antes da execução. Sempre compilar o projeto e validar visualmente os estados aplicáveis.

# RESTRIÇÕES

- Não alterar banco, entidades ou migrations.
- Não criar serviço duplicado.
- JavaScript somente quando estritamente necessário.
- Não tocar em CSS global ou layout compartilhado sem coordenação.
- Respeitar o padrão visual existente e priorizar mobile.

# RESULTADO ESPERADO

Uma entrega visual pequena, responsiva, integrada e documentada.

# FORMATO DO RELATÓRIO FINAL

Preencher `docs/RELATORIOS/FRONTEND.md` com resumo, arquivos alterados, breakpoints e cenários validados, resultado do build, riscos, pendências e hash do commit pequeno e descritivo.
