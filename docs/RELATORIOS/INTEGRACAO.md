# Relatório De Integração — PIX-MANUAL-001

## Revisão Atual

- Data: 2026-07-24
- Base comum: `00f3e49`
- Backend revisado: `6d2f8e8` (commit legado do executor, anterior à política atual)
- Frontend revisado: `bce517c` + relatório do gestor `743d2a9`
- Merge realizado: não

## Classificação

### Backend — AJUSTES

O código compila e o escopo está correto, mas o commit não contém os dois ajustes obrigatórios:

1. `SalvarConfiguracaoAsync` ainda não valida os limites de 140 caracteres da chave e 200 do beneficiário antes de persistir.
2. `MontarDto` ainda considera o Pix disponível sem exigir beneficiário preenchido.

Validações do gestor:

- `dotnet build SalgaFacil.slnx --no-restore`: 0 erros, 11 avisos preexistentes.
- Nenhuma migration criada.
- Nenhum arquivo de Frontend ou contrato alterado.
- Nova rodada de ajustes registrada na tarefa; executor não deve criar commit.

### Frontend — APROVADA

Os dois ajustes foram aplicados corretamente:

- a página pública usa mensagem genérica e não expõe `Exception.Message`;
- a ação `Confirmar novamente` e o estado de repetição foram removidos da interface paga.

Validações do gestor:

- `dotnet build SalgaFacil.slnx --no-restore`: 0 erros, 9 avisos preexistentes.
- Smoke HTTP com banco temporário isolado: respostas 200 em `/`, `/login`, `/configuracoes`, `/loja/consucruz/meus-pedidos` e `/pedidos/1`.
- Smoke visual não executado: o navegador integrado falhou por erro do ambiente do Codex. Devolver ao Cursor não resolveria essa limitação; o teste visual fica obrigatório após a integração.
- O banco e o servidor temporários foram removidos após o teste.
- Relatório final registrado pelo gestor em `743d2a9`.

## Integração

- Não há sobreposição de arquivos entre Backend e Frontend.
- Não há migration nas frentes, conforme o contrato.
- Frontend pode ser integrado somente junto do Backend aprovado ou depois dele, para evitar publicar uma interface sem persistência real.
- Ordem recomendada: corrigir e aprovar Backend, integrar Backend, integrar Frontend, criar migration no Principal e executar smoke ponta a ponta.

## Riscos Conhecidos

- Não há projeto de testes automatizados.
- Permanecem avisos NU1903 de dependências com vulnerabilidades conhecidas e avisos CS8602 preexistentes; devem entrar em tarefa técnica separada.
- Smoke visual completo continua pendente para a integração.

## Resultado Geral

- [ ] Integrado
- [ ] Reprovado
- [x] Requer ajustes no Backend
- [x] Frontend aprovado

## Próximo Passo

1. No Claude Code da worktree Backend, executar apenas `executar`.
2. Quando ele concluir sem commit, executar novamente `revisar entregas` no gestor.
3. Nenhum merge foi autorizado nesta revisão.