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

1. Claude Code executa Backend e Cursor executa componentes Frontend em paralelo.
2. Principal revisa os dois commits independentemente.
3. Principal integra Backend e, mediante autorização, gera e revisa a migration.
4. Principal integra os componentes Frontend, sem conflito de arquivos.
5. Principal conecta os componentes às páginas e Services conforme o contrato.
6. Compilar e executar smoke test ponta a ponta.

As duas frentes estão liberadas agora. Somente a conexão final depende das duas entregas.