# TÍTULO

PIX-MANUAL-001 — Implementação real do Backend Pix manual

# OBJETIVO

Implementar `IPagamentoPixService` com regras e persistência reais, mantendo integralmente o contrato congelado.

# CONTEXTO

O contrato, DTOs e fake de desenvolvimento já existem na base comum. Frontend trabalha em paralelo consumindo a mesma interface.

# ESCOPO PERMITIDO

- Domain: campos de Empresa e Pedido e enum persistido de pagamento.
- Infrastructure: mapeamentos no DbContext.
- Web: novo `PagamentoPixService`, ajuste de criação de pedido Pix e troca do registro DI para a implementação real.

# FORA DO ESCOPO

- Qualquer `.razor`, CSS ou JavaScript.
- Alterar o contrato compartilhado ou o fake.
- Migration ou snapshot.
- Gateway, webhook, cartão, comprovante ou credencial bancária.
- Nova camada, API, Controller, Repository ou Unit of Work.

# ARQUIVOS PROVÁVEIS

- `src/SalgaFacil.Domain/Entities/Empresa.cs`
- `src/SalgaFacil.Domain/Entities/Pedido.cs`
- novo `src/SalgaFacil.Domain/Enums/StatusPagamento.cs`
- `src/SalgaFacil.Infrastructure/Data/SalgaFacilDbContext.cs`
- novo `src/SalgaFacil.Web/Services/PagamentoPixService.cs`
- `src/SalgaFacil.Web/Services/LojaPublicaService.cs`
- `src/SalgaFacil.Web/Program.cs`

# CONTRATO

`docs/CONTRATOS/PIX-MANUAL-001.md` e `PagamentoPixContracts.cs` são imutáveis nesta tarefa.

# REGRAS DE NEGÓCIO

- Validar configuração, tenant, pedido, forma Pix e telefone.
- Cliente somente consulta.
- Comerciante autenticado confirma de forma idempotente.
- Não alterar status operacional nem estoque.
- Não armazenar segredos.

# CRITÉRIOS DE ACEITE

- [ ] Implementação real cobre todos os métodos da interface
- [ ] Isolamento por empresa e telefone
- [ ] Configuração Pix validada
- [ ] Confirmação idempotente
- [ ] Build sem erros
- [ ] Nenhum arquivo Frontend ou contrato alterado
- [ ] Nenhuma migration criada

# TESTES OBRIGATÓRIOS

Validar Pix inativo, configuração incompleta, telefone correto/incorreto, outra empresa, pedido não Pix, confirmação inicial/repetida e invariância de status/estoque. Executar `dotnet build SalgaFacil.slnx --no-restore`.

# RESTRIÇÕES

Não esperar o Frontend. Não fazer merge, push ou migration. Não adicionar pacotes. Criar commit pequeno com o relatório.

# RESULTADO ESPERADO

Backend real pronto para revisão e integração.

# FORMATO DO RELATÓRIO FINAL

Preencher `docs/RELATORIOS/PIX-MANUAL-001-BACKEND.md`.