# TÍTULO

PIX-MANUAL-001 — Páginas reais do pagamento Pix manual

# OBJETIVO

Implementar em paralelo a experiência completa nas páginas existentes, consumindo `IPagamentoPixService` e o fake de Development já fornecido pela base.

# CONTEXTO

A implementação Backend não é necessária para começar, compilar ou testar a interface. O fake implementa o mesmo contrato e é identificado por `Simulado`.

# ESCOPO PERMITIDO

- Configuração Pix em `/configuracoes`.
- Pagamento em `/loja/{slug}/meus-pedidos`.
- Confirmação administrativa em `/pedidos/{id}`.
- Componentes novos e CSS isolado.

# FORA DO ESCOPO

- Services, contratos, fake, `Program.cs`, Domain, Infrastructure, banco ou migrations.
- Regras de negócio no Razor.
- `app.css` global.
- Gateway, cartão, webhook, comprovante ou confirmação automática.

# ARQUIVOS PROVÁVEIS

- `Components/Pages/Configuracoes/Index.razor` e CSS isolado
- `Components/Pages/Loja/MeusPedidos.razor` e CSS isolado
- `Components/Pages/Pedidos/Detalhe.razor` e CSS isolado
- novos componentes em `Components/Shared/PagamentoPix/`

# CONTRATO

Consumir os tipos e métodos congelados em `PagamentoPixContracts.cs`. Não duplicar DTO, interface, estado ou mensagem.

# REGRAS DE NEGÓCIO

- Cliente vê valor, beneficiário e chave e pode copiá-la.
- Cliente nunca confirma pagamento.
- Comerciante confirma na tela administrativa.
- Exibir claramente quando os dados forem simulados em Development.
- Status de pagamento é visualmente separado do status operacional.

# CRITÉRIOS DE ACEITE

- [ ] Três páginas integradas ao contrato
- [ ] Estados carregando, indisponível, aguardando, copiado, pago e erro
- [ ] Feedback acessível para cópia e confirmação
- [ ] Responsivo em 390, 768 e 1366 px
- [ ] Build sem erros
- [ ] Nenhum arquivo Backend ou contrato alterado

# TESTES OBRIGATÓRIOS

Executar com o fake em Development e validar todos os estados, teclado, cópia, confirmação repetida e breakpoints. Executar `dotnet build SalgaFacil.slnx --no-restore`.

# RESTRIÇÕES

Não esperar o Backend. Não alterar DI ou criar outro fake. Não fazer merge ou push. Criar commit pequeno com o relatório.

# RESULTADO ESPERADO

Frontend real pronto para receber a implementação Backend sem retrabalho de contrato.

# FORMATO DO RELATÓRIO FINAL

Preencher `docs/RELATORIOS/PIX-MANUAL-001-FRONTEND.md`.