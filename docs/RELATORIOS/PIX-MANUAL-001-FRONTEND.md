# Relatório PIX-MANUAL-001 — Frontend

- Situação: ENTREGUE
- Branch: `feature/pix-manual-frontend`
- Base: `00f3e49` (chore: prepara execucao paralela por contrato)
- Data: 2026-07-24

## Resumo

Experiência Pix manual integrada nas três páginas previstas, consumindo `IPagamentoPixService` e o fake de Development já registrado. Componentes reutilizáveis em `Components/Shared/PagamentoPix/` com CSS isolado. Cliente consulta valor/beneficiário/chave e copia; nunca confirma. Comerciante confirma recebimento na tela administrativa, com suporte a confirmação idempotente. Banner explícito quando `Simulado` é verdadeiro.

## Arquivos Alterados

- `src/SalgaFacil.Web/Components/Shared/PagamentoPix/StatusPagamentoPixBadge.razor` (+ `.razor.css`)
- `src/SalgaFacil.Web/Components/Shared/PagamentoPix/PagamentoPixClienteCard.razor` (+ `.razor.css`)
- `src/SalgaFacil.Web/Components/Shared/PagamentoPix/ConfirmacaoPagamentoPixCard.razor` (+ `.razor.css`)
- `src/SalgaFacil.Web/Components/Shared/PagamentoPix/ConfiguracaoPixSection.razor` (+ `.razor.css`)
- `src/SalgaFacil.Web/Components/Pages/Configuracoes/Index.razor`
- `src/SalgaFacil.Web/Components/Pages/Loja/MeusPedidos.razor`
- `src/SalgaFacil.Web/Components/Pages/Pedidos/Detalhe.razor` (+ `Detalhe.razor.css`)
- `docs/RELATORIOS/PIX-MANUAL-001-FRONTEND.md` (este relatório)

## Estados E Breakpoints Validados

Estados cobertos na UI:

- carregando
- indisponível (mensagem do contrato)
- aguardando pagamento
- chave copiada (`aria-live`)
- pago com data de confirmação
- erro (carga, save, cópia, confirmação)
- confirmação repetida (idempotente) no admin

Breakpoints tratados via CSS isolado: 390 px, 768 px e 1366 px (grid do detalhe, ações do cliente e tipografia da configuração).

Validação runtime completa no navegador com fake não foi executada nesta sessão (sem smoke E2E automatizado no repositório); build e revisão estática dos estados/bindings foram feitos.

## Contrato

- Consumidos apenas tipos/métodos de `PagamentoPixContracts.cs`.
- Nenhuma duplicação de DTO, interface, enum ou mensagem de indisponibilidade inventada.
- Status de pagamento visualmente separado do status operacional (`StatusBadge` vs `StatusPagamentoPixBadge`).
- `Program.cs`, fake, Domain, Infrastructure e contrato não alterados.

## Build E Testes

- `dotnet restore SalgaFacil.slnx` (necessário: assets ausentes na worktree)
- `dotnet build SalgaFacil.slnx --no-restore` → sucesso, **0 erros**
- Avisos pré-existentes: CS8602 em `Pdv/Index.razor`; NU1903 de dependências

## Limitações E Riscos

- Cards Pix no cliente/admin só aparecem quando `FormaPagamento == Pix`; validação visual depende de pedido Pix real (ou criado no checkout).
- Cópia usa `navigator.clipboard` via `IJSRuntime`; falha em contexto sem permissão exibe erro acessível e mantém a chave visível para cópia manual.
- Fake em Development é singleton em memória: confirmações e config não persistem entre reinícios do processo.
- Sem projeto de testes automatizados no repositório.

## Pendências De Integração

- Substituir o fake pela implementação Backend real após revisão das duas frentes.
- Migration de `Empresa`/`Pedido` (Pix) continua fora do escopo desta frente.
- Smoke E2E no navegador (copiar chave, confirmar, breakpoints) recomendado no Principal após integração.

## Declaração

- [x] Não alterei Backend, DI ou contrato
- [x] Usei somente o fake fornecido
- [x] Revisei o diff
