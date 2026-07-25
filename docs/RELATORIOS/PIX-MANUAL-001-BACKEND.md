# Relatório PIX-MANUAL-001 — Backend

- Situação: APROVADA PELO GESTOR
- Branch: `feature/pix-manual-backend`
- Commit: topo de `feature/pix-manual-backend` (pai: `7b6ae07`) — hash não fixado aqui de propósito, pois `--amend` o alteraria; confira com `git log -1`.

## Resumo

Implementação real de `IPagamentoPixService`, substituindo o fake de desenvolvimento no registro
de DI. Cliente consulta o pagamento Pix do pedido; comerciante autenticado confirma o recebimento
manualmente, de forma idempotente. Nenhuma alteração em contrato, DTOs, Frontend ou migration.

## Arquivos Alterados

- `src/SalgaFacil.Domain/Enums/StatusPagamento.cs` (novo) — enum persistido, espelha
  `StatusPagamentoPix` do contrato (Aguardando=1, Pago=2).
- `src/SalgaFacil.Domain/Entities/Empresa.cs` — campos `PixAtivo` (bool, padrão false),
  `PixChave` (string?), `PixNomeBeneficiario` (string?).
- `src/SalgaFacil.Domain/Entities/Pedido.cs` — campos `StatusPagamento` (StatusPagamento?),
  `PagamentoConfirmadoEm` (DateTime?, UTC).
- `src/SalgaFacil.Infrastructure/Data/SalgaFacilDbContext.cs` — mapeamento de tamanho máximo
  para `PixChave` (140) e `PixNomeBeneficiario` (200) em `Empresa`. Nenhuma migration criada.
- `src/SalgaFacil.Web/Services/PagamentoPixService.cs` (novo) — implementação real do contrato.
- `src/SalgaFacil.Web/Services/LojaPublicaService.cs` — `CriarPedidoVisitanteAsync` agora inicia
  `StatusPagamento = Aguardando` quando `FormaPagamento == Pix` (nulo nos demais casos, como já era).
- `src/SalgaFacil.Web/Program.cs` — troca do registro condicional (fake em Development /
  indisponível fora dele) pelo registro único da implementação real, scoped.

Não alterado: `PagamentoPixDesenvolvimentoService.cs` (fake), `PagamentoPixContracts.cs`
(contrato), qualquer `.razor`/CSS/JS, `SalgaFacil.Desktop`.

## Contrato

Cobertura da interface `IPagamentoPixService`:

- `ObterConfiguracaoAsync` / `SalvarConfiguracaoAsync`: leem/gravam `Empresa.PixAtivo/PixChave/
  PixNomeBeneficiario` do usuário autenticado (`IEmpresaContext.RequireEmpresaId()`). Ativar exige
  chave e beneficiário preenchidos — mesma regra do fake.
- `ObterParaClienteAsync(empresaId, pedidoId, telefone)`: busca o pedido por `Id` + `EmpresaId`;
  compara o telefone normalizado (`TelefoneNormalizador`, com fallback legado igual ao usado em
  `ClienteService`) contra o do cliente do pedido. Qualquer divergência retorna `null` sem
  distinguir a causa (pedido inexistente, empresa errada ou telefone errado), como exige o
  contrato. Nunca oferece confirmação (`PodeConfirmar = false`).
- `ObterParaAdministracaoAsync(pedidoId)`: exige empresa autenticada; isola por `EmpresaId`.
- `ConfirmarRecebimentoAsync(pedidoId)`: exige empresa autenticada; rejeita pedido não-Pix;
  idempotente (chamada repetida em pedido já pago é no-op, não sobrescreve `PagamentoConfirmadoEm`);
  altera exclusivamente `StatusPagamento` e `PagamentoConfirmadoEm` — não toca `Pedido.Status`
  (operacional), `Total`, `FormaPagamento` nem estoque de produto.
- Pix antigo com `StatusPagamento` nulo é interpretado como `Aguardando` em `MontarDto`.
- Mensagem de indisponibilidade usa exatamente o texto do contrato.

`docs/CONTRATOS/PIX-MANUAL-001.md` e `PagamentoPixContracts.cs` não foram tocados.

## Build E Testes

**Não executado neste ambiente — pendência real, não uma formalidade.** O sandbox onde rodei
esta tarefa não tem o SDK do .NET instalado e o acesso de rede está bloqueado por allowlist
(`dot.net`, `api.nuget.org` retornam 403; `apt-get install dotnet-sdk-8.0` falha por falta de
permissão de root). Não consegui rodar `dotnet build SalgaFacil.slnx --no-restore` nem qualquer
teste automatizado (não há projeto de testes no repositório).

O que fiz em vez disso: revisão manual linha a linha de tipos, assinaturas e nomes de propriedade
contra o código real (`Empresa`, `Pedido`, `Cliente`, `IEmpresaContext`, `SalgaFacilDbContext`,
`PagamentoPixContracts.cs`), e raciocínio explícito sobre cada cenário obrigatório:

- Pix inativo → `Disponivel=false`, chave/beneficiário ocultos, mensagem padrão.
- Configuração incompleta → `SalvarConfiguracaoAsync` lança exceção antes de persistir.
- Telefone correto → retorna DTO; incorreto → `null` (mesmo comportamento para "outra empresa").
- Pedido não Pix → `null` em consulta, exceção em confirmação.
- Confirmação inicial → grava status e data; repetida → no-op idempotente.
- Invariância → `ConfirmarRecebimentoAsync` só escreve em `StatusPagamento`/`PagamentoConfirmadoEm`.

Isso reduz mas não elimina o risco de erro de compilação (por exemplo, um typo que a leitura não
pegasse). **Recomendo fortemente que isto seja compilado e testado manualmente antes de integrar.**

## Limitações E Riscos

- **Build não verificado.** O sandbox onde rodei esta tarefa não tem o SDK do .NET instalado e o
  acesso de rede está bloqueado por allowlist (`dot.net`, `api.nuget.org`, `dotnetcli.azureedge.net`,
  `download.microsoft.com` etc. retornam 403; `apt-get install dotnet-sdk-8.0` falha por falta de
  permissão de root). Não consegui rodar `dotnet build SalgaFacil.slnx --no-restore` nem qualquer
  teste automatizado (não há projeto de testes no repositório). Validei por revisão manual
  linha a linha de tipos/assinaturas contra o código real e por raciocínio explícito sobre cada
  cenário obrigatório (Pix inativo, config incompleta, telefone correto/incorreto, outra empresa,
  pedido não Pix, confirmação inicial/repetida, invariância de status/estoque) — ver seção acima
  do histórico desta entrega. **Recomendo compilar antes de aprovar.**
- **Git só ficou acessível após pedir acesso à pasta pai** (`.git` real desta worktree vive em
  `Sistema de Gestão para Pequenos Comerciantes/.git/worktrees/SalgadosFacil-Backend`, fora da
  pasta originalmente montada). Com acesso concedido, rodei `git --git-dir=... --work-tree=...`
  manualmente: `status`/`diff`/`add`/`commit` funcionaram.
- **Achado de ambiente, não de código**: sem configurar `core.autocrlf=true`, `git diff` mostrava
  ~150 arquivos como "modificados" — toda a worktree está em CRLF mas os blobs do HEAD estão em
  LF (conversão automática do Git for Windows, ausente neste sandbox Linux). Configurei
  `core.autocrlf=true` só nesta sessão antes de `add`/`commit`; o diff final ficou limpo, restrito
  aos 8 arquivos desta tarefa (revisado abaixo). Nada foi normalizado nos outros ~150 arquivos.
- O mount desta pasta bloqueia `unlink` (delete) em alguns arquivos internos do `.git` (lock/temp
  files), mas permite `rename`; git preferiu o caminho de rename e completou `add`/`commit` sem
  corromper o repositório (verificado com `git log`/`git show --stat` depois). Ficaram alguns
  arquivos temporários órfãos em `.git/objects/*/tmp_obj_*` — inofensivos (git os recria conforme
  necessário), mas registro aqui caso apareçam em uma limpeza futura.
- Nenhum segredo foi introduzido (chave Pix é dado do lojista, não credencial de gateway).
- `PixChave`/`PixNomeBeneficiario` sem validação de formato de chave Pix (CPF/CNPJ/e-mail/telefone/
  aleatória) — só valida "não vazio quando ativo", igual ao fake. Validar formato de chave está
  fora do escopo desta tarefa (não estava nos critérios de aceite).

## Pendências De Integração

- Migration para `Empresa.PixAtivo/PixChave/PixNomeBeneficiario` e `Pedido.StatusPagamento/
  PagamentoConfirmadoEm` (autorizada apenas no worktree de integração, conforme contrato).
- Rodar `dotnet build SalgaFacil.slnx --no-restore` e os cenários de teste listados acima antes
  de aprovar.

## Declaração

- [x] Não alterei Frontend ou contrato
- [x] Não gerei migration
- [x] Revisei o diff (`git diff` com `core.autocrlf=true`, restrito aos 8 arquivos desta tarefa)


## Validação Final Do Gestor

- Limites aplicados após `Trim()`: chave Pix com máximo de 140 caracteres e beneficiário com máximo de 200.
- Disponibilidade exige Pix ativo, chave preenchida e beneficiário preenchido.
- `dotnet build SalgaFacil.slnx --no-restore`: sucesso, 0 erros e 11 avisos preexistentes.
- Nenhuma migration, arquivo de Frontend ou contrato foi alterado nesta correção.
- Backend aprovado para integração.