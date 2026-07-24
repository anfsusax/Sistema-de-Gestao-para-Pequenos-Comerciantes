# Contratos Entre Backend E Frontend

## Situação Atual

Não há contrato novo liberado para implementação nesta primeira entrega.

O projeto usa Blazor Server com Services internos. A ausência de API REST, Controllers ou camada Application deve ser preservada. Um contrato só será adicionado quando uma entrega conjunta Backend + Frontend for priorizada.

## Modelo Obrigatório Para Um Contrato Ativo

### Identificação

- Entrega:
- Status: Rascunho | Aprovado | Implementado | Validado
- Responsáveis:
- Data:

### Serviço E Método

- Serviço:
- Método:
- Entrada:
- Retorno:

### Modelo

| Propriedade | Tipo | Obrigatória | Observação |
|---|---|---|---|
| A definir | A definir | A definir | Não inventar propriedades |

### Validações E Mensagens

- Validações:
- Mensagens esperadas:

### Estados De Interface

- Carregando:
- Sucesso:
- Vazio:
- Erro:

### Comportamento Esperado

- Pré-condições:
- Resultado:
- Efeitos colaterais:

### Compatibilidade

- Consumidores atuais:
- Arquivos compartilhados envolvidos:
- Migration necessária: Não | Sim, somente após autorização

### Critérios De Aceite

- [ ] Assinatura confirmada no código ou aprovada antes da implementação
- [ ] Backend implementado e testado
- [ ] Frontend consumindo exatamente o contrato
- [ ] Estados de erro e carregamento validados
- [ ] Integração revisada no ambiente Principal

---

## PIX-MANUAL-001 — Pagamento Pix Manual Em Meus Pedidos

### Identificação

- Entrega: pagamento Pix manual por empresa
- Status: Aprovado para implementação do Backend
- Responsáveis: Claude Code (Backend), Cursor (Frontend), GPT (integração e migration autorizada)
- Data: 2026-07-24

### Limites

- Não existe gateway, webhook ou confirmação bancária automática.
- Nenhuma credencial bancária será armazenada.
- O cliente copia a chave Pix e paga no aplicativo do próprio banco.
- Somente o comerciante autenticado confirma o recebimento.
- Confirmar o pagamento não altera o status operacional do pedido.

### Modelos Persistidos

`Empresa`:

| Propriedade | Tipo | Obrigatória | Regra |
|---|---|---|---|
| `PixAtivo` | `bool` | Sim | Padrão `false` |
| `PixChave` | `string?` | Quando ativo | Trim, máximo 140 caracteres |
| `PixNomeBeneficiario` | `string?` | Quando ativo | Trim, máximo 200 caracteres |

`Pedido`:

| Propriedade | Tipo | Obrigatória | Regra |
|---|---|---|---|
| `StatusPagamento` | `StatusPagamento?` | Não | `null` em pedidos antigos; Pix novo inicia em `Aguardando` |
| `PagamentoConfirmadoEm` | `DateTime?` | Não | UTC, preenchido somente ao confirmar |

`StatusPagamento`:

- `Aguardando = 1`
- `Pago = 2`

### LojaPublicaService

Método:

`Task<PagamentoPixPublicoDto?> ObterPagamentoPixAsync(int empresaId, int pedidoId, string? telefone)`

Regras:

- Validar empresa, pedido e cliente pelo mesmo telefone normalizado usado em `ListarPedidosPorTelefoneAsync`.
- Nunca retornar dados de pedido pertencente a outro telefone ou empresa.
- Retornar `null` para pedido inexistente ou não autorizado, sem revelar qual validação falhou.
- Aceitar somente pedido cuja `FormaPagamento` seja `Pix`.
- Tratar `StatusPagamento == null` de pedido Pix antigo como `Aguardando`.
- Não alterar qualquer dado.

Retorno `PagamentoPixPublicoDto`:

| Propriedade | Tipo | Regra |
|---|---|---|
| `PedidoId` | `int` | Pedido autorizado |
| `Valor` | `decimal` | Total persistido do pedido |
| `Disponivel` | `bool` | Pix ativo e chave configurada |
| `Chave` | `string?` | Preenchida somente quando disponível |
| `NomeBeneficiario` | `string?` | Preenchido somente quando disponível |
| `Status` | `StatusPagamento` | `Aguardando` ou `Pago` |
| `ConfirmadoEm` | `DateTime?` | UTC |
| `MensagemIndisponibilidade` | `string?` | Mensagem pública sem detalhe interno |

Mensagem quando indisponível:

`Pagamento Pix indisponível no momento. Entre em contato com a loja.`

### PedidoService

Método:

`Task ConfirmarPagamentoPixAsync(int pedidoId)`

Regras:

- Exigir empresa autenticada por `IEmpresaContext`.
- Localizar pedido exclusivamente na empresa atual.
- Recusar pedido que não use Pix com `Este pedido não utiliza pagamento Pix.`
- Ser idempotente quando o pedido já estiver pago.
- Definir `StatusPagamento = Pago`.
- Definir `PagamentoConfirmadoEm = DateTime.UtcNow`.
- Não alterar `StatusPedido`, estoque, valor ou forma de pagamento.

### EmpresaService

O método existente `SalvarAsync(Empresa dados)` passa a persistir os três campos Pix.

Validação:

- Se `PixAtivo` for `true`, chave e nome do beneficiário são obrigatórios.
- Mensagens: `Informe a chave Pix.` e `Informe o nome do beneficiário do Pix.`

### Estados Do Frontend

- Carregando: botão desabilitado e texto `Carregando dados do Pix...`
- Disponível e aguardando: mostrar valor, beneficiário, chave e botão `Copiar chave Pix`
- Copiado: `Chave Pix copiada. Abra seu banco para concluir o pagamento.`
- Pago: badge `Pagamento confirmado` e data, sem botão de pagamento
- Indisponível: mostrar a mensagem pública do contrato
- Erro: `Não foi possível carregar o pagamento. Tente novamente.`

### Migration

Necessária para os campos de `Empresa` e `Pedido`, mas não autorizada nesta tarefa. Deve ser gerada no ambiente Principal somente depois da revisão e integração do Backend.

### Critérios De Aceite

- [ ] Nenhuma credencial ou segredo bancário é armazenado ou exibido
- [ ] Isolamento por empresa e telefone validado
- [ ] Pedido não Pix não oferece pagamento Pix
- [ ] Cliente não consegue confirmar o próprio pagamento
- [ ] Confirmação do comerciante é idempotente
- [ ] Status de pagamento não muda o status operacional do pedido
- [ ] Backend integrado antes do início do Frontend
- [ ] Migration gerada somente após autorização