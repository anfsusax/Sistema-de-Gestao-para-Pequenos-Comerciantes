# TÍTULO

PIX-MANUAL-001 — Interface de pagamento Pix manual

# OBJETIVO

Adicionar configuração Pix, pagamento em “Meus pedidos” e confirmação administrativa, consumindo exatamente o contrato aprovado.

# CONTEXTO

**BLOQUEADA:** iniciar somente depois que o Backend estiver revisado, integrado à `main`, a worktree Frontend estiver sincronizada e a migration autorizada tiver sido aplicada no ambiente de teste.

# ESCOPO PERMITIDO

- Trabalhar somente na worktree Frontend.
- Ler `WORKTREE.md`, `ESTADO.md`, `CONTRATOS.md` e este arquivo.
- Alterar somente as páginas e estilos isolados listados nesta tarefa.

# FORA DO ESCOPO

- Banco, DbContext, migrations, entidades, enums, Services e persistência.
- Regras de negócio e novos serviços duplicados.
- Autenticação sem autorização.
- Mudança de arquitetura ou refatoração geral.
- Gateway, webhook, cartão, upload de comprovante ou confirmação automática.

# ARQUIVOS PROVÁVEIS

- `src/SalgaFacil.Web/Components/Pages/Configuracoes/Index.razor`
- novo `src/SalgaFacil.Web/Components/Pages/Configuracoes/Index.razor.css`
- `src/SalgaFacil.Web/Components/Pages/Loja/MeusPedidos.razor`
- novo `src/SalgaFacil.Web/Components/Pages/Loja/MeusPedidos.razor.css`
- `src/SalgaFacil.Web/Components/Pages/Pedidos/Detalhe.razor`
- novo `src/SalgaFacil.Web/Components/Pages/Pedidos/Detalhe.razor.css`

# CONTRATO

Consumir exatamente `PIX-MANUAL-001` em `docs/CONTRATOS.md`. Não alterar Services, entidades, enums ou assinaturas.

# REGRAS DE NEGÓCIO

- Configurações: permitir ativar Pix manual, informar chave e beneficiário e explicar que a confirmação é manual.
- Meus pedidos: oferecer `Pagar com Pix` somente para pedido Pix não pago.
- Exibir total, beneficiário, chave e ação de copiar.
- Nunca oferecer ao cliente botão para confirmar pagamento.
- Pedido pago exibe badge e data de confirmação.
- Detalhe administrativo permite confirmar o recebimento apenas para Pix aguardando.
- Não confundir status do pagamento com status operacional do pedido.

# CRITÉRIOS DE ACEITE

- [ ] Configuração Pix clara e validada
- [ ] Pagamento aparece apenas nos pedidos elegíveis
- [ ] Chave é copiada com feedback acessível
- [ ] Cliente não confirma pagamento
- [ ] Comerciante consegue confirmar sem alterar o pedido operacional
- [ ] Mobile e responsividade contemplados
- [ ] Estados de carregamento, vazio, sucesso e erro definidos
- [ ] Build sem erros
- [ ] Nenhum arquivo fora do escopo alterado

# TESTES OBRIGATÓRIOS

- Validar Pix inativo, indisponível, aguardando e pago.
- Validar pedido não Pix.
- Validar falha ao copiar e falha ao carregar.
- Validar confirmação administrativa repetida.
- Validar breakpoints 390, 768 e 1366 px.
- Executar `dotnet build SalgaFacil.slnx --no-restore`.

# RESTRIÇÕES

- Não alterar banco, entidades, enums, Services ou migrations.
- Não criar serviço duplicado.
- JavaScript somente para copiar a chave, se necessário.
- Não alterar `app.css`; usar CSS isolado das páginas.
- Respeitar o padrão visual existente e priorizar mobile.
- Criar um commit pequeno e descritivo.

# RESULTADO ESPERADO

Fluxo visual Pix manual responsivo, acessível e fiel ao contrato.

# FORMATO DO RELATÓRIO FINAL

Preencher `docs/RELATORIOS/FRONTEND.md` com resumo, arquivos alterados, breakpoints e cenários validados, resultado do build, riscos, pendências e hash do commit.
