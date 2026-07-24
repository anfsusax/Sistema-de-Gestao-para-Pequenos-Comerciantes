# TÍTULO

PIX-MANUAL-001 — Regras e persistência do pagamento Pix manual

# OBJETIVO

Implementar os modelos, validações e serviços internos definidos no contrato `PIX-MANUAL-001`, sem alterar frontend e sem gerar migration.

# CONTEXTO

O sistema atualmente registra apenas a forma de pagamento. O primeiro corte aprovado permitirá exibir a chave Pix da empresa ao cliente e permitirá que o comerciante confirme manualmente o recebimento.

# ESCOPO PERMITIDO

- Trabalhar somente na worktree Backend.
- Ler `WORKTREE.md`, `ESTADO.md`, `CONTRATOS.md` e este arquivo.
- Alterar somente Domain, mapeamento EF Core e Services listados nesta tarefa.

# FORA DO ESCOPO

- Qualquer `.razor`, CSS, JavaScript ou layout.
- Mudança de arquitetura.
- Repository, Unit of Work, Controllers, API REST ou camada Application.
- Qualquer arquivo em `Data/Migrations`.
- Gateway, webhook, cartão, credenciais bancárias, comprovante e confirmação automática.
- Autenticação, refatoração geral ou atualização de pacotes.

# ARQUIVOS PROVÁVEIS

- `src/SalgaFacil.Domain/Entities/Empresa.cs`
- `src/SalgaFacil.Domain/Entities/Pedido.cs`
- novo `src/SalgaFacil.Domain/Enums/StatusPagamento.cs`
- `src/SalgaFacil.Infrastructure/Data/SalgaFacilDbContext.cs`
- `src/SalgaFacil.Web/Services/EmpresaService.cs`
- `src/SalgaFacil.Web/Services/LojaPublicaService.cs`
- `src/SalgaFacil.Web/Services/PedidoService.cs`

# CONTRATO

Implementar exatamente `PIX-MANUAL-001` em `docs/CONTRATOS.md`.

# REGRAS DE NEGÓCIO

- Pix manual é configurado por empresa.
- Não armazenar credenciais ou segredos bancários.
- Cliente apenas consulta e copia a chave.
- Somente comerciante autenticado confirma recebimento.
- Confirmação não muda `StatusPedido`, estoque, total ou forma de pagamento.
- Toda consulta pública deve validar empresa, pedido e telefone normalizado.
- Pedido Pix antigo com status nulo é tratado como aguardando.

# CRITÉRIOS DE ACEITE

- [ ] Campos e enum do contrato implementados
- [ ] Mapeamentos EF configurados sem migration
- [ ] Configuração Pix validada no `EmpresaService`
- [ ] Consulta pública não permite acesso cruzado
- [ ] Confirmação Pix é autenticada e idempotente
- [ ] Build sem erros
- [ ] Nenhum arquivo fora do escopo alterado

# TESTES OBRIGATÓRIOS

- Empresa com Pix inativo.
- Pix ativo sem chave.
- Pix ativo sem beneficiário.
- Pedido Pix do telefone correto.
- Pedido de outro telefone e de outra empresa.
- Pedido não Pix.
- Confirmação inicial e confirmação repetida.
- Confirmar que `StatusPedido` e estoque não mudam.
- Executar `dotnet build SalgaFacil.slnx --no-restore`.
- Se não houver infraestrutura de testes automatizados aplicável, registrar explicitamente a limitação no relatório; não criar novo projeto de testes nem adicionar pacotes nesta tarefa.

# RESTRIÇÕES

- Não gerar nem editar migration ou snapshot.
- Não alterar frontend.
- Não mudar arquitetura.
- Não executar refatoração ampla.
- Não tocar em arquivo compartilhado além do DbContext autorizado.
- Não executar `git add` nem criar commit; o gestor fará isso após a revisão.

# RESULTADO ESPERADO

Backend compilando, pronto para revisão e posterior migration no Principal.

# FORMATO DO RELATÓRIO FINAL

Preencher `docs/RELATORIOS/BACKEND.md` com resumo, arquivos alterados, testes, resultado do build, riscos, pendências, confirmação de ausência de migration e hash do commit.
