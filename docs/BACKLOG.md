# Backlog Do MVP

Atualizado em: 2026-07-24

| ID | Prioridade | Entrega | Dependências | Responsável | Status |
|---|---|---|---|---|---|
| MVP-001 | P0 | Validar o fluxo público ponta a ponta em runtime | PostgreSQL local, dados de teste e navegador | Principal | Pronta |
| MVP-002 | P0 | Validar clientes duplicados no banco real | MVP-001 e backup do banco | Backend, sem migration | Bloqueada |
| MVP-003 | P0 | Decidir e, se autorizado, criar índice UNIQUE de telefone normalizado | MVP-002 sem duplicidades e exclusividade de migration | Principal/Backend | Bloqueada |
| MVP-004 | P0 | Executar smoke test de administração do pedido e PDV | MVP-001 | Principal | Pendente |
| MVP-005 | P1 | Criar cobertura automatizada inicial para regras puras críticas | Definir projeto de testes sem mudar arquitetura | Backend | Pendente |
| MVP-006 | P1 | Corrigir riscos de nulidade apontados no PDV | Reproduzir cenário e definir aceite | Backend | Pendente |
| MVP-007 | P1 | Avaliar e tratar dependências com vulnerabilidades reportadas | Análise de compatibilidade e decisão formal | Principal/Backend | Pendente |
| MVP-008 | P1 | Persistir autenticação após recarga/reinício | Contrato e decisão de segurança | Backend + Frontend | Pendente |
| MVP-009 | P1 | Implementar página de detalhes do produto | Contrato de leitura do produto | Backend + Frontend | Pendente |
| MVP-010 | P1 | Implementar ícone flutuante do carrinho | MVP-001 validado | Frontend | Pendente |
| MVP-011 | P2 | Criar tela de pacotes de venda | Validar serviço existente e contrato | Frontend | Pendente |
| MVP-012 | P2 | Decidir convenção de line endings e `.gitattributes` | Revisão dos impactos no repositório | Principal | Pendente |

## Regras De Priorização

- P0 protege o fluxo principal e deve ser concluída antes de novas features.
- Migration só pode ser iniciada após autorização explícita e confirmação de que nenhuma outra branch está gerando migration.
- Entregas Backend + Frontend exigem contrato ativo em `CONTRATOS.md`.
- Qualquer defeito encontrado no baseline deve virar uma tarefa pequena, verificável e vinculada ao cenário que falhou.
