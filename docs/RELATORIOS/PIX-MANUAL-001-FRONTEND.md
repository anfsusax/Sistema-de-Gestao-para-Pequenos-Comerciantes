# Relatório PIX-MANUAL-001 — Frontend

- Situação: ENTREGUE (rodada de ajustes concluída)
- Branch: `feature/pix-manual-frontend`
- Base inicial: `00f3e49`
- Entrega inicial: `2f52a98`
- Pedido de ajustes: `e74e6c4`
- Ajustes aplicados em: `bce517c` (commit criado antes da mudança de protocolo em `ac73fc2`; a partir desta rodada nenhum commit foi criado pelo executor)
- Data: 2026-07-24

## Resumo

Rodada de ajustes do gestor aplicada sobre a integração Pix manual:

1. Em `MeusPedidos`, falhas da consulta Pix pública exibem somente a mensagem genérica `Não foi possível carregar o pagamento. Tente novamente.` — nenhum `Exception.Message` chega ao cliente (`MeusPedidos.razor`, catch de `CarregarPixDosPedidosAsync`).
2. Em `ConfirmacaoPagamentoPixCard`, estado `Pago` mostra apenas confirmação e data; a ação `Confirmar novamente` e o parâmetro `ConfirmacaoRepetida` foram removidos do componente e de `Detalhe.razor`. Idempotência permanece responsabilidade do serviço.

Verificação objetiva nesta rodada: `rg` não encontra `Confirmar novamente` nem `ConfirmacaoRepetida` em `src/SalgaFacil.Web`; a mensagem genérica está presente em `MeusPedidos.razor`.

## Arquivos Alterados

Rodada de ajustes (já no histórico da branch em `bce517c`):

- `src/SalgaFacil.Web/Components/Pages/Loja/MeusPedidos.razor`
- `src/SalgaFacil.Web/Components/Shared/PagamentoPix/ConfirmacaoPagamentoPixCard.razor`
- `src/SalgaFacil.Web/Components/Pages/Pedidos/Detalhe.razor`

Nesta rodada, somente este relatório foi alterado e permanece no working tree para validação do gestor.

Entrega inicial (em `2f52a98`): componentes em `Components/Shared/PagamentoPix/` (4 componentes + CSS isolado), `Configuracoes/Index.razor`, `Detalhe.razor.css`.

## Estados E Breakpoints

Estados representados no markup/CSS (revisão estática):

- carregando, indisponível, aguardando, copiado, pago, erro

Breakpoints no CSS isolado:

- `max-width: 389px` (≈390) — tipografia/padding compactos
- `min-width: 768px` — ações em linha, grid de dados do admin, detalhe em 2 colunas
- `min-width: 1366px` — espaçamento ampliado

### Smoke Visual — Evidência Objetiva Da Limitação

Não executado. Tentativa real nesta sessão:

- Não há ferramenta de navegador/automação disponível no ambiente.
- Tentei subir a aplicação com `dotnet run --project src/SalgaFacil.Web` em Development para inspecionar as páginas: o processo aborta no startup com `Npgsql.NpgsqlException: No password has been provided but the backend requires one (in SASL/SCRAM-SHA-256)` em `DbSeeder.SeedAsync` (`Program.cs:40`), antes de o servidor escutar.
- Causa: não existe `appsettings.Development.json` nesta worktree (apenas o `.example` com senha placeholder) e a connection string padrão não tem senha. Criar config local com segredo está fora do escopo do executor.

Portanto, os estados e breakpoints foram validados apenas por revisão estática de markup e CSS; a validação visual em runtime fica para o gestor no ambiente com banco configurado.

## Contrato

- Sem alteração de Backend, DI (`Program.cs`), fake ou `PagamentoPixContracts.cs`.
- Diff desta rodada: apenas este relatório. Histórico anterior revisado: somente páginas Frontend e componentes `Shared/PagamentoPix`.

## Build E Testes

- `dotnet build SalgaFacil.slnx --no-restore` (executado nesta rodada, 2026-07-24) → sucesso, **0 erros**
- Avisos pré-existentes: CS8602 em `Pdv/Index.razor` e NU1903 de dependências (SQLitePCLRaw, System.Security.Cryptography.Xml)
- Não há projeto de testes automatizados no repositório

## Limitações E Riscos

- Smoke visual em runtime pendente (evidência da limitação acima).
- Cards Pix só aparecem em pedidos com `FormaPagamento == Pix`.
- Fake em memória (singleton) não persiste entre reinícios do processo.

## Pendências De Integração

- Substituir o fake pela implementação Backend real após revisão das frentes; migration segue não autorizada para esta frente.
- Smoke visual com fake em Development (estados carregando, indisponível, aguardando, copiado, pago, erro + breakpoints 390/768/1366) no ambiente do gestor.

## Declaração

- [x] Não alterei Backend, DI ou contrato
- [x] Usei somente o fake fornecido
- [x] Revisei o diff
- [x] Ajustes obrigatórios do gestor aplicados
- [x] Nenhum stage ou commit criado nesta rodada
