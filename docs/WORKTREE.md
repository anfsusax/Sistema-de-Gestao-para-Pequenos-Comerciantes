# Git Worktree — SalgaFacil (SalgadosFácil)

Guia oficial para desenvolvimento paralelo do projeto usando `git worktree`.
Este documento não altera nenhuma regra de negócio, arquitetura ou código —
é só o mapa para várias frentes de trabalho (humanas ou IA) editarem o mesmo
repositório ao mesmo tempo com o mínimo de conflito.

---

## 1. Objetivo

Permitir que múltiplas frentes de trabalho (cada uma em uma branch, cada uma
em uma pasta de worktree separada) desenvolvam módulos diferentes do
SalgaFacil ao mesmo tempo, sem:

- editar o mesmo arquivo em paralelo sem saber;
- gerar duas migrations EF Core conflitantes;
- quebrar o `Program.cs`/DI por dois merges simultâneos;
- transformar o monólito em vários sistemas soltos.

O projeto continua sendo **um monólito modular**: uma solução, um banco,
um deploy. Worktree só resolve o problema de "várias pessoas/IAs editando
a mesma working copy ao mesmo tempo" — não cria novos limites de deploy.

---

## 2. Arquitetura Real Do Projeto (leia antes de dividir módulos)

A missão que originou este documento descreve uma arquitetura com camadas
`Domain / Application / Infrastructure / API / Web`. **Isso não bate com o
código real.** A solução hoje tem 4 projetos:

| Projeto | Papel | Depende de |
|---|---|---|
| `SalgaFacil.Domain` | Entidades, Enums, regras de domínio puras (`Domain/Services/*`) | nenhum |
| `SalgaFacil.Infrastructure` | `SalgaFacilDbContext`, Migrations EF Core, `DbSeeder`, `DependencyInjection.cs` | Domain |
| `SalgaFacil.Web` | Blazor Server (UI + `Services/*` com `DbContext` injetado direto — sem Repository/UnitOfWork, decisão já tomada no projeto) | Infrastructure (→ Domain) |
| `SalgaFacil.Desktop` | WinForms, protótipo isolado, dados em memória própria | **nenhum** (zero `ProjectReference`) |

Não existe projeto de API REST nem Controllers, nem camada `Application`
separada. Este documento organiza os módulos em cima da estrutura real
(pastas dentro de `SalgaFacil.Web`), não da estrutura hipotética. Se no
futuro o projeto realmente precisar de uma API separada, isso é uma decisão
de arquitetura à parte — não faz parte deste guia de worktree.

---

## 3. Mapa De Módulos

Módulos definidos a partir da organização de pastas já existente
(`Components/Pages/*`, `Services/*`, `Domain/Entities/*`). Nenhum módulo foi
inventado — cada um corresponde a uma pasta ou a um agrupamento de arquivos
que já colaboram entre si no código atual.

| Módulo | Arquivos principais | Dependências | Isolado? | Risco de conflito |
|---|---|---|---|---|
| **Catálogo (admin)** | `Pages/Produtos/*`, `Pages/Categorias/*`, `Pages/Unidades/*`, `Services/ProdutoService.cs`, `CategoriaService.cs`, `UnidadeMedidaService.cs`, `ProdutoImagemService.cs`, `PacoteService.cs` | `Domain/Entities/Produto.cs`, `CategoriaProduto.cs`, `UnidadeMedida.cs`, `Pacote*.cs` (leitura/escrita); `IEmpresaContext` | Sim | Baixo — só sobe se mexer no schema de `Produto`/`Categoria` (migration) |
| **Loja Pública (Catálogo + Carrinho + Checkout)** | `Pages/Loja/Index.razor`, `Carrinho.razor`, `MeusPedidos.razor`, `Shared/SeletorQuantidade.razor`, `Shared/LojaNav.razor`, `Services/LojaPublicaService.cs`, `CarrinhoSessao.cs`, `Layout/PublicLayout.razor` | Lê `Produto`/`CategoriaProduto` (Catálogo); **chama `ClienteService`** (módulo Clientes) para achar/criar cliente; grava direto em `Pedido`/`PedidoItem` (mesma entidade do módulo Pedidos, caminho de escrita separado) | Parcial | **Médio** — depende de `ClienteService` e da entidade `Pedido`; mudança de assinatura em qualquer um dos dois quebra este módulo |
| **Clientes** | `Pages/Clientes/Index.razor`, `Form.razor`, `Duplicados.razor`, `Services/ClienteService.cs`, `ClienteManutencaoService.cs`, `Domain/Entities/Cliente.cs`, `EnderecoCliente.cs`, `Domain/Services/TelefoneNormalizador.cs` | `IEmpresaContext` | Sim | **Médio** — é dependência de entrada da Loja Pública; mexer na assinatura de `ClienteService` exige coordenar com quem estiver em Loja Pública |
| **Pedidos (gestão administrativa)** | `Pages/Pedidos/Index.razor`, `Detalhe.razor`, `Novo.razor`, `Services/PedidoService.cs`, `Domain/Entities/Pedido.cs`, `PedidoItem.cs`, `Enums/StatusPedido.cs`, `FormaPagamento.cs`, `FormaPagamentoExtensions.cs` | `IEmpresaContext`; entidade `Pedido` também é escrita por Loja Pública (caminho diferente, mesma tabela) | Parcial | **Médio** — duas frentes (Pedidos admin e Loja Pública) gravam na mesma entidade por serviços diferentes |
| **PDV / Caixa** | `Pages/Pdv/Index.razor`, `Caixa.razor`, `Historico.razor`, `Shared/PdvNav.razor`, `Services/VendaService.cs`, `CaixaService.cs`, `Domain/Entities/Venda.cs`, `VendaItem.cs`, `SessaoCaixa.cs`, `MovimentoCaixa.cs` | `IEmpresaContext`; independente de Pedidos (usa `Venda`, não `Pedido`) | Sim | Baixo |
| **Produção** | `Pages/Producao/Index.razor` | Lê `PedidoService.ObterProducaoPorProdutoAsync()` (módulo Pedidos) | Parcial | Baixo — só leitura, tela pequena |
| **Custos** | `Pages/Custos/Index.razor`, `Services/CustosService.cs` | Lê `Produto.CustoEstimado`/`PrecoVenda` | Sim | Baixo |
| **Dashboard** | `Pages/Home.razor`, `Services/DashboardService.cs` | Lê `Pedido` inteiro (agregador cross-módulo) | Não (é sempre o último a integrar) | **Alto indireto** — qualquer mudança de schema em `Pedido`/`Produto` pode quebrar o dashboard sem tocar nele diretamente |
| **Administração / Auth** | `Pages/Configuracoes/Index.razor`, `Pages/Login.razor`, `Services/AuthService.cs` (contém também `IEmpresaContext`), `EmpresaService.cs`, `Domain/Entities/Empresa.cs`, `Usuario.cs` | Usado por **todos** os outros módulos administrativos (via `IEmpresaContext`) | Não | **Alto** — é tronco compartilhado, não módulo de feature (ver seção 4) |
| **Desktop** | `src/SalgaFacil.Desktop/**` (WinForms) | Nenhuma (zero `ProjectReference`) | **Sim, total** | Nenhum — projeto fisicamente separado |

---

## 4. Arquivos Compartilhados (tronco comum)

Estes arquivos **não pertencem a nenhum módulo** — são o tronco que todo
módulo atravessa. Só devem ser alterados na branch `main`/worktree principal,
ou quando a tarefa exigir estritamente (ex.: registrar um novo `Service` no
DI). Qualquer PR que toque neles precisa de revisão extra.

| Arquivo/pasta | Por que é compartilhado |
|---|---|
| `src/SalgaFacil.Web/Program.cs` | Único ponto de registro de DI (`AddScoped<...>`) e pipeline HTTP |
| `src/SalgaFacil.Web/Services/AuthService.cs` | Define `IEmpresaContext`, usado por quase todo `Service` administrativo |
| `src/SalgaFacil.Infrastructure/DependencyInjection.cs` | Registro do `DbContext`/Npgsql |
| `src/SalgaFacil.Infrastructure/Data/SalgaFacilDbContext.cs` | Mapeamento EF Core de **todas** as entidades |
| `src/SalgaFacil.Infrastructure/Data/Migrations/**` (inclui `SalgaFacilDbContextModelSnapshot.cs`) | Sequencial por natureza — duas migrations criadas em paralelo em branches diferentes **sempre** vão conflitar no snapshot |
| `src/SalgaFacil.Infrastructure/Data/DbSeeder.cs` | Seed único, usado por todo módulo em ambiente de dev |
| `src/SalgaFacil.Web/Components/_Imports.razor` | `@using` globais — qualquer módulo pode precisar adicionar um |
| `src/SalgaFacil.Web/Components/App.razor`, `Routes.razor` | Bootstrap de rotas/render mode |
| `src/SalgaFacil.Web/Components/Layout/*` (`MainLayout`, `PublicLayout`, `EmptyLayout`, `ReconnectModal.*`) | Layout visual usado por todas as páginas |
| `src/SalgaFacil.Web/Components/Shared/{StatusBadge,TipoBadge,UserAvatar}.razor` | Componentes de UI reaproveitados por vários módulos |
| `src/SalgaFacil.Web/wwwroot/app.css` | Design system global (variáveis de cor, `.card`, `.btn`, `.form-control`, etc. — usados por todas as telas) |
| `appsettings*.json`, `launchSettings.json` | Configuração de ambiente/conexão |
| `SalgaFacil.slnx`, `*.csproj` | Estrutura da solução e dependências de pacote |
| `.gitignore`, `.gitattributes` (se criado — ver seção 9) | Regras de repositório |
| `_ia/**` | Memória do protocolo CENTRAL-ROBO — já é `.gitignore`d, mas é compartilhada entre todas as sessões de IA no mesmo checkout |

---

## 5. Estrutura De Worktrees Recomendada

```
D:\CENTRAL-ROBO\PROJETOS\Comercial\
├── Sistema de Gestão para Pequenos Comerciantes\        ← worktree principal (main)
├── SalgadosFacil-Catalogo\        ← branch feature/catalogo
├── SalgadosFacil-LojaPublica\     ← branch feature/loja-publica
├── SalgadosFacil-Clientes\        ← branch feature/clientes
├── SalgadosFacil-Pedidos\         ← branch feature/pedidos
├── SalgadosFacil-Pdv\             ← branch feature/pdv
└── SalgadosFacil-Admin\           ← branch feature/admin (Configurações/Auth/Dashboard)
```

Regras da estrutura:

- A pasta atual do projeto (`Sistema de Gestão para Pequenos Comerciantes`)
  continua sendo o worktree da `main` — não precisa ser recriada.
- Cada worktree novo é uma pasta **irmã**, fora da pasta principal (nunca
  dentro dela — evita a IDE/`dotnet` indexar os dois ao mesmo tempo por
  engano).
- Um worktree por módulo em desenvolvimento ativo. Não crie worktree para
  módulo parado — remova (`git worktree remove`, seção 7) quando o PR for
  mergeado.
- Produção e Custos são pequenos demais para justificar worktree próprio —
  desenvolva junto do módulo que os usa (Produção junto de Pedidos, Custos
  junto de Catálogo) ou direto na `main` se for ajuste pontual.
- Desktop pode ganhar worktree próprio (`SalgadosFacil-Desktop`) a qualquer
  momento sem risco, já que não compartilha nenhum arquivo com o Web.

---

## 6. Convenção De Branches

```
feature/catalogo
feature/loja-publica
feature/clientes
feature/pedidos
feature/pdv
feature/admin
feature/desktop
```

- Prefixo `feature/` para desenvolvimento de módulo; `fix/` para correção
  pontual; `chore/` para tarefas de infraestrutura (ex.: `.gitattributes`).
- Uma branch por módulo/frente, não por tarefa individual — reduz o número
  de merges na `main` e o risco de duas branches tocarem `Program.cs` ao
  mesmo tempo.
- Nomeie a pasta do worktree igual ao módulo da branch (`SalgadosFacil-<Modulo>`)
  para não haver ambiguidade sobre o que está rodando em cada janela do
  Visual Studio/Cursor.

---

## 7. Comandos Git (mostrados, não executados)

### Criar uma nova frente de trabalho

```bash
# 1. Atualizar a main no worktree principal
git checkout main
git pull origin main

# 2. Criar a branch a partir da main atualizada
git branch feature/catalogo main

# 3. Criar o worktree (pasta irmã, fora do projeto principal)
git worktree add ../SalgadosFacil-Catalogo feature/catalogo
```

### Ver worktrees ativos

```bash
git worktree list
```

### Finalizar uma frente (depois do merge do PR)

```bash
# Dentro do worktree principal, após o PR ser mergeado na main:
git worktree remove ../SalgadosFacil-Catalogo

# Limpa referências de worktrees removidos manualmente (ex.: apagou a pasta na mão)
git worktree prune

# Apagar a branch local já mergeada (opcional)
git branch -d feature/catalogo
```

### Sincronizar um worktree existente com a main (durante o desenvolvimento)

```bash
# Dentro do worktree do módulo:
git fetch origin
git merge origin/main
# ou, se o time preferir histórico linear:
git rebase origin/main
```

---

## 8. Fluxo Oficial De Desenvolvimento

```
Atualizar main
      ↓
Criar branch (feature/<modulo>)
      ↓
Criar worktree (git worktree add)
      ↓
Abrir a pasta do worktree no Visual Studio e/ou Cursor
      ↓
Executar a IA/desenvolvedor no escopo do módulo
      ↓
Rodar o projeto localmente e testar o módulo
      ↓
Gestor revisa e cria o commit (mensagem clara, escopo do módulo)
      ↓
Push da branch (origin/feature/<modulo>)
      ↓
Abrir Pull Request contra a main
      ↓
Revisão (checar se tocou em arquivo compartilhado sem necessidade — seção 4)
      ↓
Merge na main
      ↓
Sincronizar os outros worktrees ativos (git merge origin/main)
      ↓
Remover o worktree do módulo finalizado (git worktree remove)
```

---

## 9. Pontos De Conflito E Como Evitar

| Arquivo/área | Por que conflita | Como evitar |
|---|---|---|
| `Program.cs` (DI) | Duas branches registrando `Service` novo na mesma região do arquivo | Adicionar o novo `AddScoped<...>` sempre no **fim** da lista de registros do próprio módulo; nunca reordenar linhas existentes só por estética |
| `Migrations/**` + `SalgaFacilDbContextModelSnapshot.cs` | EF Core gera o snapshot inteiro a cada migration — duas migrations em paralelo sempre colidem no mesmo arquivo | **Regra dura:** só uma branch por vez gera migration. Antes de rodar `dotnet ef migrations add`, sincronizar com a `main` primeiro. Preferir nomear a migration com prefixo de data (`AAAAMMDDHHmmss_Nome`, já é o padrão do projeto) para ordenação previsível |
| `IEmpresaContext` (dentro de `AuthService.cs`) | Toda mudança em auth obriga recompilar/revisar todo módulo administrativo | Ver sugestão de extração na seção 10 — não é um problema de worktree, é acoplamento estrutural |
| `_Imports.razor` | Dois módulos adicionando `@using` diferentes na mesma região | Adicionar sempre no fim do arquivo, uma linha por `using`, nunca reescrever o bloco inteiro |
| `app.css` (design system) | Dois módulos adicionando classes novas na mesma faixa de linhas | Adicionar blocos novos sempre no **fim do arquivo**, com comentário de seção (`/* ── Nome Do Módulo ── */`), nunca inserir no meio de blocos existentes |
| `Layout/*` | Mudança visual de layout afeta todas as telas ao mesmo tempo | Mudança de layout compartilhado só na `main`, nunca dentro de uma branch de módulo |
| `appsettings*.json` | Motivo real de conflito costuma ser line-ending (CRLF/LF), não conteúdo — ver observação abaixo | Resolver o `.gitattributes` (seção 10) antes de abrir muitas branches em paralelo |
| Entidade `Pedido` (escrita por dois módulos: Pedidos admin e Loja Pública) | Mudança de campo em `Pedido`/`PedidoItem` quebra os dois módulos ao mesmo tempo | Mudança de schema em `Pedido` deve ser coordenada entre as duas frentes antes de começar, não descoberta no PR |

**Observação sobre CRLF/LF:** o repositório hoje mistura arquivos CRLF e LF
(alguns arquivos foram reescritos por ferramentas que gravam LF, outros
mantêm o CRLF original do Windows). Isso já foi identificado como risco
pendente no `_ia/TAREFAS.md` ("Decidir sobre diffs CRLF antigos /
`.gitattributes` antes de pushes maiores"). Antes de abrir múltiplos
worktrees, **vale resolver isso primeiro** — do contrário, todo merge entre
branches vai mostrar diffs enormes de arquivos que na prática não mudaram
de conteúdo, só de terminador de linha. Ver sugestão na seção 10.

---

## 10. Acoplamentos Identificados (sugestões — não implementadas)

Estas são observações de arquitetura para avaliação futura. Nenhuma foi
aplicada neste documento, conforme a missão pediu ("apenas documente").

1. **`IEmpresaContext` dentro de `AuthService.cs`.**
   A interface é consumida por praticamente todo `Service` administrativo,
   mas vive no mesmo arquivo que a lógica de login. Extrair `IEmpresaContext`
   (e só a interface — a implementação `EmpresaContext` pode continuar perto
   do `AuthService`) para um arquivo próprio (`IEmpresaContext.cs`) reduziria
   o "raio de explosão" de qualquer alteração em `AuthService.cs` sobre o
   restante do sistema. Baixo risco, mudança puramente mecânica.

2. **`Pedido` escrito por dois caminhos diferentes.**
   `PedidoService.CriarAsync` (admin/PDV/encomenda por telefone) e
   `LojaPublicaService.CriarPedidoVisitanteAsync` (cardápio público) fazem a
   mesma coisa — criar `Pedido` + `PedidoItem` — com regras de validação
   parcialmente diferentes e código duplicado (cálculo de `Total`, criação de
   itens). Não é um bug hoje, mas é o tipo de duplicação que tende a divergir
   com o tempo. Poderia ser unificado em um método de domínio único no
   futuro — fora do escopo desta tarefa de worktree.

3. **`.gitattributes` ausente.**
   Não existe hoje. Seria a correção estrutural do problema de CRLF/LF
   citado na seção 9 (ex.: `* text=auto eol=crlf` ou equivalente). Como o
   projeto já tem esse risco registrado em `_ia/RISCOS.md`/`TAREFAS.md`,
   fica como recomendação explícita aqui também, mas a decisão de qual
   convenção adotar é do time, não desta análise.

4. **`Home.razor` (Dashboard) sem módulo dono.**
   Por natureza agrega dados de `Pedido` (e futuramente poderia agregar
   `Venda`, `Cliente` etc.). Não deveria ganhar worktree próprio nem ser
   tratado como módulo de feature — sugestão é sempre revisá-lo por último,
   depois que os módulos que ele lê estiverem estáveis na `main`.

---

## 11. Checklist Para Criar Uma Nova Frente De Trabalho

- [ ] A `main` local está atualizada (`git pull`) antes de criar a branch?
- [ ] A branch tem nome `feature/<modulo>` correspondente a um módulo da
      seção 3 (ou `fix/`/`chore/` se não for uma feature de módulo)?
- [ ] O worktree foi criado como pasta **irmã**, fora da pasta do projeto
      principal?
- [ ] A tarefa da IA/dev deixa claro quais arquivos do módulo ela pode tocar
      livremente e quais são compartilhados (seção 4) — e que mudança em
      compartilhado exige registro no relatório para decisão do gestor?
- [ ] Se a tarefa envolve migration EF Core: foi confirmado que nenhuma outra
      branch está gerando migration ao mesmo tempo?
- [ ] Se a tarefa envolve `Program.cs`/DI: o novo registro vai no fim da
      lista, sem reordenar o que já existe?
- [ ] Antes do PR: `git diff` foi revisado para confirmar que só os arquivos
      do módulo (mais, no máximo, uma linha nova em algum compartilhado)
      foram alterados?
- [ ] Depois do merge: o worktree foi removido (`git worktree remove`) e a
      branch local apagada?

---

## 12. Regras

- Nunca alterar regra de negócio ao mexer em estrutura de worktree/branch.
- Nunca criar módulo que não corresponda a uma pasta/agrupamento já existente
  no código (ver seção 3).
- Nunca mover ou excluir arquivo do projeto para "organizar" — isso é uma
  decisão de refatoração separada, não faz parte deste guia.
- Nunca gerar duas migrations EF Core em branches diferentes sem sincronizar
  primeiro.
- Nunca alterar `Program.cs`, `AuthService.cs`, `SalgaFacilDbContext.cs`,
  `app.css` (global) ou `Layout/*` dentro de uma branch de módulo, a menos
  que a tarefa exija estritamente — e, mesmo assim, avisar as outras frentes
  ativas.
- Sempre preferir várias branches pequenas e sequenciais a uma branch grande
  tocando vários módulos.

---

## 13. Modelo Operacional Vigente Para A Gestão Técnica

Em 2026-07-24, a missão de gestão técnica definiu três ambientes permanentes
para a fase atual do MVP:

| Ambiente | Branch | Pasta irmã | Responsabilidade |
|---|---|---|---|
| Principal | `main` | `Sistema de Gestão para Pequenos Comerciantes` | Planejamento, documentação, revisão, integração, smoke tests e migrations autorizadas |
| Backend | `feature/backend` | `SalgadosFacil-Backend` | Domain, Services, EF Core, persistência, regras, consultas e testes de backend |
| Frontend | `feature/frontend` | `SalgadosFacil-Frontend` | Páginas e componentes Blazor, formulários, modais, CSS, responsividade e experiência do usuário |

Esta decisão adapta a recomendação por módulo das seções 5 e 6 para a
coordenação entre as ferramentas atuais. O mapa de módulos e os riscos deste
documento continuam válidos para definir o escopo de cada tarefa.

Regras adicionais:

- Backend e Frontend não recebem tarefas sobre o mesmo arquivo ao mesmo tempo.
- Mudanças em arquivos compartilhados exigem coordenação registrada antes do início da tarefa.
- A branch Backend não autoriza migrations automaticamente.
- A branch Frontend não autoriza mudanças em entidades, persistência ou regras de negócio.
- O contrato de cada entrega conjunta deve estar registrado em `docs/CONTRATOS.md` antes da execução.
- A integração continua sendo revisada no ambiente Principal, normalmente na ordem Backend, Frontend, ajustes, testes e documentação.

---

## 14. Modelo Escalável Vigente

A partir de 2026-07-24, as pastas de worktree são ambientes reutilizáveis, mas as branches são curtas e específicas por entrega. Esta seção substitui os nomes fixos de branch da seção 13.

Exemplo atual:

- `SalgadosFacil-Backend` → `feature/pix-manual-backend`
- `SalgadosFacil-Frontend` → `feature/pix-manual-frontend`

Fluxo:

1. Gestor congela contrato documental e, quando necessário, contrato de código na `main`.
2. Registra cada branch, tarefa e relatório em `docs/FRENTES.md`.
3. Cada executor recebe apenas o comando `executar` e trabalha autonomamente.
4. Dependências ainda inexistentes usam adapters/fakes compatíveis com o mesmo contrato, nunca formatos inventados.
5. Cada executor entrega código, validações possíveis e relatório exclusivo no working tree, sem stage ou commit.
6. O gestor usa `revisar entregas`, executa as validações finais, classifica a entrega e cria o commit somente quando aprovada.
7. Migrations e merges continuam centralizados e autorizados.

Para equipes maiores, criar uma branch e uma tarefa por entrega ou módulo, mantendo propriedade de arquivos sem sobreposição. Não acumular várias entregas simultâneas numa branch genérica de Backend ou Frontend.