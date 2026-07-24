# Plano E Registro De Integração

Atualizado em: 2026-07-24

## Estado

- Branch principal: `main`
- Frentes registradas: `docs/FRENTES.md`
- Merge autorizado: não
- Migration autorizada: não

## Regra Geral

As frentes executam em paralelo a partir de um contrato congelado. A revisão ocorre depois das entregas, sem merge automático.

## Entrega PIX-MANUAL-001

1. Claude Code implementa `IPagamentoPixService` real na branch Backend.
2. Cursor implementa as páginas reais usando o fake Development na branch Frontend.
3. Ambos compilam, validam, escrevem relatórios exclusivos e criam commits independentemente.
4. O gestor executa `revisar entregas` e verifica contrato, escopo, testes e conflitos.
5. Após autorização, integrar Backend numa worktree de integração.
6. Gerar e revisar a migration somente após autorização específica.
7. Integrar Frontend; o registro DI real do Backend substitui o fake automaticamente.
8. Executar build e smoke test ponta a ponta.
9. Atualizar estado, backlog, frentes e relatório de integração.

## Checklist

- [ ] Contrato congelado respeitado
- [ ] Arquivos das frentes não se sobrepõem
- [ ] Fake não está ativo fora de Development
- [ ] Implementação real aplica isolamento por empresa e telefone
- [ ] Cliente não confirma pagamento
- [ ] Migration revisada e autorizada
- [ ] Build e smoke tests aprovados

Nenhuma integração desta entrega foi executada ainda.