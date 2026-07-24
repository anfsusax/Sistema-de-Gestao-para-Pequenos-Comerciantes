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
## RODADA DE AJUSTES DO GESTOR — 2026-07-24

Esta seção tem precedência para a retomada da entrega após a primeira revisão.

### Regra Excepcional De Retomada

A worktree contém a implementação da rodada anterior ainda sem commit. Essas alterações são esperadas e pertencem a esta tarefa. Não pare por falta de limpeza, não descarte, não restaure e não recomece do zero. Antes de editar, confirme que os arquivos modificados são somente os arquivos previstos nesta tarefa e o relatório. Se houver qualquer outro arquivo, pare e informe.

### Ajustes Obrigatórios

1. Em `PagamentoPixService.SalvarConfiguracaoAsync`, normalize com `Trim()` e valide antes de persistir:
   - chave Pix com no máximo 140 caracteres;
   - nome do beneficiário com no máximo 200 caracteres.
   Use mensagens de validação claras e não deixe a restrição do banco virar erro técnico para a interface.
2. Em `MontarDto`, considere o Pix disponível somente quando `PixAtivo` estiver verdadeiro e chave e beneficiário estiverem preenchidos.
3. Revise o diff completo, incluindo `StatusPagamento.cs` e `PagamentoPixService.cs`, que ainda estão sem rastreamento.
4. Execute `dotnet restore SalgaFacil.slnx` se necessário e `dotnet build SalgaFacil.slnx --no-restore`.
5. Atualize o relatório: substitua a limitação antiga de build/Git pelo resultado real desta rodada, registre os ajustes e marque a revisão do diff.
6. Crie um único commit com toda a implementação e o relatório. Não faça migration, merge, rebase ou push.

### Critério De Nova Entrega

A entrega só retorna para revisão quando os dois ajustes de regra estiverem concluídos, o build estiver sem erros, todos os arquivos da implementação estiverem no commit e a worktree estiver limpa.
### Modo Obrigatório Neste Sandbox

O sandbox desta IA já demonstrou que não acessa os metadados Git da pasta irmã e não possui SDK .NET. Isso não é mais um bloqueio nem uma decisão do usuário.

- Não tente baixar ou instalar o SDK novamente.
- Não peça acesso à pasta pai e não peça que o usuário escolha quem fará o commit.
- Aplique agora todos os ajustes de código desta rodada e atualize o relatório.
- Registre `BUILD, DIFF E COMMIT PENDENTES DO GESTOR` apenas como transferência automática de responsabilidade.
- Encerre com `EXECUTOR CONCLUÍDO — AGUARDANDO VALIDAÇÃO DO GESTOR`.

O gestor executará build, revisão do diff e commit localmente assim que receber essa conclusão.