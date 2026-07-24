# Plano E Registro De Integração

Atualizado em: 2026-07-24

## Estado

- Branch principal: `main`
- Última base diagnosticada: `9f1a61b`
- Backend pronto para integrar: não
- Frontend pronto para integrar: não
- Merge autorizado: não

## Ordem Padrão

1. Revisar relatório e commit Backend.
2. Validar escopo, contrato, build, testes e arquivos compartilhados.
3. Integrar Backend.
4. Atualizar Frontend com a `main`.
5. Revisar relatório e commit Frontend.
6. Integrar Frontend.
7. Resolver conflitos no Principal.
8. Compilar a solução.
9. Executar smoke tests e baseline ponta a ponta.
10. Atualizar `ESTADO.md`, `BACKLOG.md` e tarefas concluídas.

## Checklist De Revisão

- [ ] Tarefa entregue corresponde ao escopo autorizado
- [ ] Nenhum arquivo fora do escopo foi alterado
- [ ] Contrato foi respeitado
- [ ] Arquivos compartilhados foram identificados
- [ ] Nenhuma migration foi criada sem autorização
- [ ] Build passou
- [ ] Testes aplicáveis passaram
- [ ] Relatório informa arquivos, riscos e validações
- [ ] Ordem de integração foi confirmada

## Conflitos E Resultados

Nenhuma integração foi executada nesta primeira entrega.

## Entrega PIX-MANUAL-001

Ordem obrigatória:

1. Claude Code implementa Backend sem migration.
2. Principal revisa contrato, diff, build e relatório.
3. Backend é integrado somente após aprovação.
4. Principal gera e revisa a migration, mediante autorização específica.
5. Frontend é sincronizado com a nova base.
6. Cursor implementa as três telas sem alterar Backend.
7. Principal integra Frontend e executa smoke test ponta a ponta.

O Frontend permanece bloqueado até a conclusão dos passos 1 a 5.