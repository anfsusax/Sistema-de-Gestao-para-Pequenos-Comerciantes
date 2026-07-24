# TÍTULO

PIX-MANUAL-001 — Componentes visuais do pagamento Pix manual

# OBJETIVO

Construir em paralelo os componentes visuais reutilizáveis do Pix manual, compiláveis sem depender da implementação Backend.

# CONTEXTO

**LIBERADA PARA EXECUÇÃO PARALELA.** O contrato funcional está congelado. O Backend implementará regras e persistência enquanto o Frontend implementará componentes puramente visuais. A ligação com páginas e Services será feita depois no ambiente Principal.

# ESCOPO PERMITIDO

- Trabalhar somente na worktree Frontend.
- Criar componentes novos e CSS isolado.
- Receber dados por parâmetros primitivos e comunicar ações por `EventCallback`.
- Usar apenas tipos já existentes no branch ou tipos privados de apresentação.

# FORA DO ESCOPO

- Alterar páginas existentes nesta etapa.
- Alterar Services, entidades, enums, DbContext, migrations ou `Program.cs`.
- Criar serviço falso, duplicado ou regra de negócio no Frontend.
- Gateway, webhook, cartão, comprovante ou confirmação automática.

# ARQUIVOS PROVÁVEIS

- novo `src/SalgaFacil.Web/Components/Shared/PagamentoPix/PagamentoPixClienteCard.razor`
- novo `src/SalgaFacil.Web/Components/Shared/PagamentoPix/PagamentoPixClienteCard.razor.css`
- novo `src/SalgaFacil.Web/Components/Shared/PagamentoPix/ConfiguracaoPixSection.razor`
- novo `src/SalgaFacil.Web/Components/Shared/PagamentoPix/ConfiguracaoPixSection.razor.css`
- novo `src/SalgaFacil.Web/Components/Shared/PagamentoPix/ConfirmacaoPagamentoPixCard.razor`
- novo `src/SalgaFacil.Web/Components/Shared/PagamentoPix/ConfirmacaoPagamentoPixCard.razor.css`

# CONTRATO

Seguir os estados e textos de `PIX-MANUAL-001` em `docs/CONTRATOS.md`. Os componentes devem expor parâmetros simples para valor, chave, beneficiário, disponibilidade, pagamento confirmado, data, carregamento e erro.

# REGRAS DE NEGÓCIO

- Não implementar regra de negócio.
- Cliente vê dados e copia a chave, mas nunca confirma pagamento.
- Componente administrativo apenas dispara `EventCallback` de confirmação.
- Não confundir status de pagamento com status operacional do pedido.

# CRITÉRIOS DE ACEITE

- [ ] Três componentes novos compilam isoladamente
- [ ] Estados carregando, indisponível, aguardando, copiado, pago e erro estão representados
- [ ] Ações são expostas por `EventCallback`
- [ ] Nenhuma página ou arquivo Backend foi alterado
- [ ] Layout responsivo e acessível
- [ ] Build sem erros

# TESTES OBRIGATÓRIOS

- Renderizar visualmente todos os estados dos componentes.
- Validar breakpoints 390, 768 e 1366 px.
- Validar navegação por teclado e feedback de cópia.
- Executar `dotnet build SalgaFacil.slnx --no-restore`.

# RESTRIÇÕES

- Não alterar `app.css`; usar CSS isolado.
- Não adicionar pacote ou JavaScript global.
- Não editar arquivo compartilhado.
- Criar um commit pequeno e descritivo.

# RESULTADO ESPERADO

Componentes visuais prontos para serem conectados às páginas no Principal após a entrega Backend, sem impedir trabalho paralelo.

# FORMATO DO RELATÓRIO FINAL

Preencher `docs/RELATORIOS/FRONTEND.md` com componentes criados, parâmetros, eventos, estados validados, breakpoints, build e hash do commit.
