# PIX-MANUAL-001 — Pagamento Pix Manual

- Versão: 1
- Situação: CONGELADO
- Código: `src/SalgaFacil.Web/Contracts/Pagamentos/PagamentoPixContracts.cs`

## Objetivo

Permitir que o cliente consulte os dados Pix do pedido e pague no aplicativo bancário. O comerciante autenticado confirma manualmente o recebimento.

## Limites

- Sem gateway, webhook, cartão, credencial bancária ou confirmação automática.
- Cliente nunca confirma o próprio pagamento.
- Confirmação não altera status operacional, estoque, total ou forma de pagamento.
- Dados são isolados por empresa, pedido e telefone normalizado.

## Interface Compartilhada

`IPagamentoPixService`:

- `ObterConfiguracaoAsync()`
- `SalvarConfiguracaoAsync(ConfiguracaoPixDto configuracao)`
- `ObterParaClienteAsync(int empresaId, int pedidoId, string? telefone)`
- `ObterParaAdministracaoAsync(int pedidoId)`
- `ConfirmarRecebimentoAsync(int pedidoId)`

A assinatura completa e os tipos estão no arquivo de código do contrato. Esse arquivo não pode ser alterado pelas frentes.

## Persistência Real

`Empresa`:

- `PixAtivo: bool`, padrão falso
- `PixChave: string?`, máximo 140
- `PixNomeBeneficiario: string?`, máximo 200

`Pedido`:

- `StatusPagamento: StatusPagamento?`
- `PagamentoConfirmadoEm: DateTime?`, UTC

Pedidos Pix novos começam em `Aguardando`; Pix antigo com status nulo é interpretado como aguardando.

## Regras De Serviço

- Configuração ativa exige chave e beneficiário.
- Consulta pública retorna `null` para pedido inexistente ou não autorizado, sem revelar a causa.
- Pedido não Pix não oferece pagamento Pix.
- Confirmação administrativa exige empresa autenticada e é idempotente.
- Pagamento indisponível usa: `Pagamento Pix indisponível no momento. Entre em contato com a loja.`

## Estados Visuais

- carregando;
- indisponível;
- aguardando;
- chave copiada;
- pago com data de confirmação;
- erro.

## Desenvolvimento Paralelo

A base comum registra um fake apenas em ambiente Development e uma implementação indisponível segura fora dele. O Frontend usa o fake sem alterar o contrato ou `Program.cs`. O Backend substitui o registro pela implementação real.

## Migration

Necessária, mas não autorizada para as frentes. Será gerada no worktree de integração após revisão das duas entregas e autorização específica.