# Relatório PIX-MANUAL-001 — Frontend

- Situação: ENTREGUE (rodada de ajustes)
- Branch: `feature/pix-manual-frontend`
- Base inicial: `00f3e49`
- Entrega anterior: `2f52a98`
- Pedido de ajustes: `e74e6c4`
- Data: 2026-07-24

## Resumo

Rodada de ajustes do gestor aplicada sobre a integração Pix manual:

1. Em `MeusPedidos`, falhas da consulta Pix pública passam a exibir somente a mensagem genérica `Não foi possível carregar o pagamento. Tente novamente.` — sem `Exception.Message`.
2. Em `ConfirmacaoPagamentoPixCard`, estado `Pago` mostra confirmação e data, sem ação `Confirmar novamente`. Idempotência permanece no serviço.

## Arquivos Alterados Nesta Rodada

- `src/SalgaFacil.Web/Components/Pages/Loja/MeusPedidos.razor`
- `src/SalgaFacil.Web/Components/Shared/PagamentoPix/ConfirmacaoPagamentoPixCard.razor`
- `src/SalgaFacil.Web/Components/Pages/Pedidos/Detalhe.razor` (remove `ConfirmacaoRepetida`)
- `docs/RELATORIOS/PIX-MANUAL-001-FRONTEND.md` (este relatório)

Arquivos da entrega anterior (ainda na branch): componentes em `Components/Shared/PagamentoPix/`, `Configuracoes/Index.razor`, `Detalhe.razor.css`.

## Estados E Breakpoints

Estados representados no markup/CSS:

- carregando, indisponível, aguardando, copiado, pago, erro

Breakpoints no CSS isolado (revisão estática do código):

- `390` / `max-width: 389px` — tipografia/padding
- `768px` — layout em linha / grid admin / detalhe em 2 colunas
- `1366px` — espaçamento/padding ampliado

### Smoke Visual

Não executado. Evidência objetiva: nesta sessão não há ferramenta de navegador/automação disponível para abrir a app com o fake em Development e validar estados/breakpoints em runtime. Limitação registrada sem afirmar validação visual.

## Contrato

- Sem alteração de Backend, DI (`Program.cs`), fake ou `PagamentoPixContracts.cs`.
- Diff revisado: apenas páginas Frontend, componentes Shared/PagamentoPix e este relatório.

## Build E Testes

- `dotnet build SalgaFacil.slnx --no-restore` → sucesso, **0 erros**
- Avisos pré-existentes em `Pdv/Index.razor` e NU1903 de dependências

## Limitações E Riscos

- Smoke E2E/visual pendente no Principal ou com navegador disponível.
- Cards Pix só em pedidos com `FormaPagamento == Pix`.
- Fake em memória não persiste entre reinícios.

## Pendências De Integração

- Backend real + migration após revisão das frentes.
- Smoke visual com fake em Development (estados + breakpoints).

## Declaração

- [x] Não alterei Backend, DI ou contrato
- [x] Usei somente o fake fornecido
- [x] Revisei o diff
- [x] Ajustes obrigatórios do gestor aplicados
