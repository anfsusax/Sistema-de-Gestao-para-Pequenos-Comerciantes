# Relatório PIX-MANUAL-002 — Fluxo Completo de Pagamento Pix Manual

- Situação: PRONTA PARA REVISÃO (não commitada — conforme instrução explícita do contrato)
- Branch: `main` (feito direto no Principal, a pedido do usuário — não passou pelo fluxo
  `executar`/`FRENTES.md`, pois não havia frente cadastrada para esta tarefa)
- Commit: nenhum. **Nada foi commitado, staged ou enviado** — só arquivos no working tree.
- Migration: preparada, **não aplicada**. Ver seção "Migration".

## Resumo Da Implementação

Implementação de ponta a ponta do fluxo de pagamento Pix manual: cliente escolhe Pix no checkout,
recebe QR Code (padrão BR Code/EMV, com CRC16 calculado em C#) e Pix Copia e Cola, anexa um
comprovante (JPG/PNG/PDF), acompanha o status em "Meus pedidos", e a loja confere manualmente no
extrato antes de confirmar — nunca automaticamente. Reaproveita a base já existente do
PIX-MANUAL-001 (que só cobria consulta e confirmação simples, sem QR, sem comprovante, sem os
estados intermediários) em vez de recriar telas ou fluxo paralelo.

Os 5 estados do pagamento (`Aguardando`, `ComprovanteEnviado`, `EmAnalise`, `Pago`, `Rejeitado`)
substituem os 2 estados do v1 (`Aguardando`, `Pago`). O contrato `IPagamentoPixService` mudou de
6 para 8 métodos — é uma mudança incompatível (breaking change) documentada e propagada a todos os
call sites (ver "Arquivos Alterados").

## Fluxo Implementado (17 Passos Do Contrato)

1–5. Cardápio → escolher produto/quantidade → adicionar ao carrinho → abrir carrinho/revisar →
confirmar compra: **sem alteração** (fluxo já existia; bug de badge do carrinho corrigido em
sessão anterior, fora do escopo Pix).
6. Login/cadastro se não autenticado: já existia em `Carrinho.razor`; reaproveitado sem alteração
   de lógica (só corrigido o call site do Pix que dependia do cliente autenticado).
7. Escolher entrega/retirada + forma de pagamento: **já ficava depois** da etapa de
   login/cadastro no fluxo existente (`ConfirmarCarrinho()` só libera a etapa `Pagamento` se
   `ClienteAuth.EstaAutenticado`) — satisfaz o critério "forma de pagamento não aparece antes do
   login" sem precisar de mudança estrutural.
8. Pix: pedido é criado com o cliente autenticado (`LojaPublicaService.CriarPedidoClienteAsync`,
   inalterado) e o QR Code/Copia e Cola são gerados (`PagamentoPixService.MontarDto` →
   `PixPayloadGerador` + `QrCodeGerador`).
9. QR Code + Copia e Cola + chave + beneficiário + valor exato + identificação do pedido:
   implementado em `PagamentoPixClienteCard.razor` (imagem do QR, botão "Copiar Pix Copia e
   Cola", botão "Copiar chave", valor e identificador `PED{pedidoId}` embutidos no payload).
10. Cliente paga no app do banco: fora do sistema, por definição (sem gateway).
11. Cliente anexa comprovante (JPG/PNG/PDF): `InputFile` em `PagamentoPixClienteCard.razor` →
    `ComprovanteArmazenamentoService` (validação) → `PagamentoPixService.EnviarComprovanteAsync`.
12. Status vira "Comprovante enviado"/"Em análise": `EnviarComprovanteAsync` grava
    `StatusPagamento.ComprovanteEnviado`; `MarcarEmAnaliseAsync` (opcional, botão do admin) move
    para `EmAnalise`.
13. Painel admin recebe alerta visual, abre/baixa o comprovante, confere com o extrato:
    `Pedidos/Index.razor` (chip/ponto amarelo por linha e no cabeçalho) +
    `ConfirmacaoPagamentoPixCard.razor` (botão "Ver/baixar comprovante", que busca os bytes via
    `ObterComprovanteParaAdministracaoAsync` e abre no navegador via Blob/JS — ver seção
    "Entrega Do Comprovante Ao Admin").
14. Admin confirma pagamento / rejeita com motivo / mantém em análise:
    `ConfirmarRecebimentoAsync`/`RejeitarComprovanteAsync`/`MarcarEmAnaliseAsync`, todos exigindo
    `usuarioId` do funcionário autenticado (`AuthService.UsuarioAtual.Id`, injetado em
    `Pedidos/Detalhe.razor`).
15. Confirmação registra data/hora/usuário responsável: `PagamentoConfirmadoEm` +
    `PagamentoConfirmadoPorUsuarioId`; rejeição registra `ComprovanteRevisadoEm` +
    `ComprovanteRevisadoPorUsuarioId` + `ComprovanteMotivoRejeicao`. Rejeição libera reenvio
    (`PodeEnviarComprovante` volta a `true` em status `Rejeitado`).
16. Cliente acompanha tudo em "Meus pedidos": `MeusPedidos.razor` reescrito para exigir login
    (`ClienteAuthService`) em vez de busca só por telefone (ver "Decisão: Meus Pedidos Exige
    Login"), reutilizando `PagamentoPixClienteCard.razor` por pedido Pix.
17. Botão auxiliar de WhatsApp com o número do pedido: `WhatsAppLinkBuilder.MontarLinkPedido`,
    exibido em `PagamentoPixClienteCard.razor` (Carrinho e Meus Pedidos) quando a loja tem
    WhatsApp cadastrado (`Empresa.WhatsApp`).

## Arquivos Criados

- `src/SalgaFacil.Domain/Services/PixPayloadGerador.cs` — payload BR Code/EMV + CRC16-CCITT-FALSE.
- `src/SalgaFacil.Domain/Services/WhatsAppLinkBuilder.cs` — link `wa.me` com número do pedido.
- `src/SalgaFacil.Web/Services/ComprovanteArmazenamentoService.cs` — validação (assinatura
  binária/tamanho/extensão) e armazenamento privado (fora de `wwwroot`) do comprovante.
- `src/SalgaFacil.Web/Services/QrCodeGerador.cs` — PNG do QR a partir do payload (QRCoder).
- `src/SalgaFacil.Web/wwwroot/js/comprovante.js` — abre/baixa o comprovante no navegador do admin
  a partir de bytes em base64 (via Blob), sem endpoint HTTP público.
- `src/SalgaFacil.Infrastructure/Data/Migrations/20260725120000_ComprovantePixManual.cs` (+
  `.Designer.cs`) — migration hand-crafted (sem SDK disponível — ver "Migration").
- `src/SalgaFacil.Tests/` — projeto de testes novo (`SalgaFacil.Tests.csproj` + 3 arquivos de
  teste + suporte). Ver "Testes".

## Arquivos Alterados

**Domain**
- `Enums/StatusPagamento.cs` — de 2 para 5 estados (`ComprovanteEnviado`, `EmAnalise`,
  `Rejeitado` adicionados).
- `Entities/Pedido.cs` — 9 propriedades novas (comprovante + revisão + confirmação por usuário).

**Infrastructure**
- `Data/SalgaFacilDbContext.cs` — mapeamento das 9 colunas novas + 2 FKs para `Usuarios`.
- `Data/Migrations/SalgaFacilDbContextModelSnapshot.cs` — atualizado para refletir o modelo atual.

**Web — contrato e serviços**
- `Contracts/Pagamentos/PagamentoPixContracts.cs` — v2 do contrato (breaking change, ver acima).
- `Services/PagamentoPixService.cs` — reescrito para os 8 métodos do contrato v2.
- `Services/PagamentoPixDesenvolvimentoService.cs` — fake de Development (não registrado em
  `Program.cs` hoje) e serviço "indisponível" atualizados para não quebrar a build.
- `Services/LojaPublicaService.cs` — novo método `ListarPedidosDoClienteAsync(empresaId,
  clienteId)` para `MeusPedidos.razor` autenticado.
- `Program.cs` — registro de `ComprovanteArmazenamentoService`.
- `SalgaFacil.Web.csproj` — pacote `QRCoder 1.6.0`.

**Web — telas**
- `Components/App.razor` — inclui `js/comprovante.js`.
- `Components/Pages/Loja/Carrinho.razor` — corrigido call site quebrado pela mudança de
  contrato (`ObterParaClienteAsync` agora recebe `ClienteId`, não `telefone`); adicionado handler
  de upload de comprovante e link do WhatsApp.
- `Components/Pages/Loja/MeusPedidos.razor` — reescrito: login/cadastro via `ClienteAuthService`
  em vez de busca por telefone (mudança de comportamento deliberada, ver decisão abaixo).
- `Components/Pages/Pedidos/Detalhe.razor` — injeta `AuthService`/`IJSRuntime`; novos handlers
  `MarcarPixEmAnaliseAsync`, `RejeitarPixAsync`, `VerComprovantePixAsync`; `ConfirmarPixAsync`
  agora envia o `usuarioId` do funcionário logado.
- `Components/Pages/Pedidos/Index.razor` — indicador visual de comprovante pendente (linha e
  cabeçalho da lista).
- `Components/Shared/PagamentoPix/PagamentoPixClienteCard.razor(.css)` — QR Code, Copia e Cola,
  upload de comprovante, estados de análise/rejeição, botão WhatsApp.
- `Components/Shared/PagamentoPix/ConfirmacaoPagamentoPixCard.razor(.css)` — ver/baixar
  comprovante, marcar em análise, rejeitar com motivo (formulário inline), destaque visual quando
  há comprovante pendente.
- `Components/Shared/PagamentoPix/StatusPagamentoPixBadge.razor(.css)` — 5 estados.
- `wwwroot/app.css` — estilos do indicador de pendência em `Pedidos/Index.razor`.

- `SalgaFacil.slnx` — inclui `SalgaFacil.Tests`.

## Regras Do Contrato — Onde Cada Uma Foi Implementada

- **Comprovante não confirma sozinho**: `EnviarComprovanteAsync` só grava o arquivo e move o
  status; `PodeConfirmar` em `MontarDto` não depende de haver comprovante (comentário explícito no
  código sobre essa decisão, herdada do v1 e preservada de propósito).
- **Só a loja confirma, após checar a conta**: `ConfirmarRecebimentoAsync` exige
  `IEmpresaContext.RequireEmpresaId()` (usuário administrativo autenticado) + `usuarioId`
  explícito; não há caminho de confirmação automática em lugar nenhum do código novo.
- **5 estados mínimos**: `StatusPagamento` (Domain) e `StatusPagamentoPix` (contrato) —
  `Aguardando/ComprovanteEnviado/EmAnalise/Pago/Rejeitado`.
- **QR Code EMV/BR Code com CRC16 correto**: `PixPayloadGerador.GerarPayload` + `CalcularCrc16`
  (CRC-16/CCITT-FALSE, poly 0x1021, init 0xFFFF) — testado contra o vetor padrão
  `"123456789"` → `"29B1"` e contra auto-consistência do payload gerado (ver "Testes").
- **Geração em C#, sem gateway**: `PixPayloadGerador` (Domain, puro C#) e `QrCodeGerador`
  (QRCoder — só renderiza PNG a partir do texto, não fala com nenhum serviço externo).
- **Validar tipo/extensão/assinatura/tamanho**: `ComprovanteArmazenamentoService.Validar` — lista
  branca de extensões, checagem de assinatura binária (magic bytes) por cima da extensão
  declarada, limite de 8 MB.
- **Armazenamento privado, sem caminho público exposto**: `App_Data/comprovantes/` (fora de
  `wwwroot`), sem rota estática apontando para lá; entrega ao navegador feita dentro do circuito
  Blazor autenticado, nunca por URL pública (ver "Entrega Do Comprovante Ao Admin").
- **Autorização (cliente só vê seus próprios comprovantes; admin só da própria empresa)**:
  `ObterParaClienteAsync`/`EnviarComprovanteAsync`/`ObterComprovanteParaClienteAsync` conferem
  `EmpresaId` **e** `ClienteId` do pedido contra os parâmetros recebidos, retornando `null`/lançando
  sem distinguir "não existe" de "não é seu" (evita enumeração de Id); `ObterParaAdministracaoAsync`
  e afins usam `IEmpresaContext.RequireEmpresaId()`.
- **Proteção contra troca de identificador, upload malicioso, confirmação duplicada**: coberto por
  testes dedicados (`EnviarComprovanteAsync_ClienteNaoDonoTentandoTrocarIdentificador_Lanca`,
  `EnviarComprovanteAsync_ArquivoDisfarcado_LancaENaoAlteraStatus`,
  `ConfirmarRecebimentoAsync_ChamadoDuasVezes_NaoSobrescreveDataNemUsuarioDaPrimeiraConfirmacao`).
- **Isolamento multi-tenant preservado**: todo acesso a `Pedido` passa por `EmpresaId` (explícito
  no cliente, via `IEmpresaContext` no admin) — nenhuma consulta nova ignora esse filtro.
- **Pix Copia e Cola não removido ao adicionar QR Code**: `PayloadCopiaECola` continua sendo
  campo próprio do DTO, exibido e copiável independente do QR (`QrCodePngBase64` é um campo
  adicional, não uma substituição).
- **Login admin não alterado para autenticar clientes**: `ClienteAuthService` e `AuthService`
  continuam duas classes/sessões completamente separadas; nenhuma tela administrativa foi tocada
  em termos de autenticação, só `Detalhe.razor` passou a *ler* `AuthService.UsuarioAtual.Id`
  (já autenticado por fluxo existente) para registrar quem confirmou/rejeitou.
- **Sem segredo/dado bancário real no código**: chaves/valores de teste são todos fictícios
  (`loja@teste.com.br`, `CHAVE-PIX-SIMULADA`, etc.), consistente com o que já existia no fake de
  Development.

## Decisões Importantes (E Por Quê)

**"Meus Pedidos" passou a exigir login, não mais só telefone.** O comportamento antigo
(`ListarPedidosPorTelefoneAsync`) deixava qualquer pessoa que soubesse o telefone de um cliente
ver o pedido completo dele. Isso já era uma fragilidade antes, mas ficou inaceitável com o
PIX-MANUAL-002: o mesmo fluxo agora expõe QR Code, Pix Copia e Cola e o comprovante de pagamento
anexado. Telefone sem senha não é prova de posse suficiente para autorizar isso. A tela foi
reescrita para pedir login/cadastro (mesmo componente reaproveitado do checkout) antes de mostrar
qualquer pedido. O método antigo por telefone (`ListarPedidosPorTelefoneAsync`) foi mantido no
`LojaPublicaService` (não removido, para não quebrar nada que ainda dependa dele fora deste
fluxo), mas `MeusPedidos.razor` não o usa mais.

**Entrega do comprovante ao admin não usa endpoint HTTP.** Não existe endpoint minimal API
separado para "baixar o comprovante do pedido X" — a autenticação de `AuthService`/
`ClienteAuthService` vive no circuito Blazor Server (memória do circuito), não em cookie/token
HTTP, então um endpoint REST separado não teria como validar a mesma sessão sem reimplementar
autenticação. Em vez disso, o botão "Ver/baixar comprovante" busca os bytes via
`ObterComprovanteParaAdministracaoAsync` (já dentro do circuito autenticado), manda para o
navegador em base64 via `IJSRuntime`, e um pequeno script (`wwwroot/js/comprovante.js`) monta um
`Blob` e dispara o download/abre em nova aba. Isso elimina de saída a categoria de bug "esqueci de
checar auth no endpoint", ao custo de não ter uma URL direta e de carregar o arquivo inteiro
(até 8 MB) via SignalR — aceitável no volume esperado de uma loja pequena, mas vale reavaliar se o
volume de comprovantes crescer muito (ver "Riscos").

**Confirmar recebimento não exige comprovante enviado.** Preservei a decisão já tomada no
PIX-MANUAL-001 (documentada em comentário no código): a loja confirma depois de checar a própria
conta bancária, independente de o cliente ter anexado comprovante ou não. Cheguei a implementar
uma versão mais restrita (só permitir confirmar a partir de `ComprovanteEnviado`/`EmAnalise`) e
revertida de propósito — o contrato deste PIX-MANUAL-002 não pede essa restrição, e adicioná-la
mudaria um comportamento já em produção sem necessidade.

## Migration

`src/SalgaFacil.Infrastructure/Data/Migrations/20260725120000_ComprovantePixManual.cs` (+
`.Designer.cs`) — **preparada, não aplicada**, conforme instrução do contrato.

- `Up()`: adiciona 9 colunas em `Pedidos` (`ComprovanteCaminho` varchar(300),
  `ComprovanteNomeOriginal` varchar(255), `ComprovanteContentType` varchar(100),
  `ComprovanteTamanhoBytes` bigint, `ComprovanteEnviadoEm` timestamptz,
  `ComprovanteMotivoRejeicao` varchar(500), `ComprovanteRevisadoPorUsuarioId` int,
  `ComprovanteRevisadoEm` timestamptz, `PagamentoConfirmadoPorUsuarioId` int), 2 índices e 2 FKs
  para `Usuarios(Id)` com `ON DELETE RESTRICT` (não deixa apagar um funcionário que já confirmou
  ou revisou algum pagamento).
- `Down()`: reverte tudo, na ordem inversa.
- Escrita à mão (hand-crafted), espelhando o padrão dos arquivos `Designer.cs`/
  `ModelSnapshot.cs` já existentes no repositório — **não foi gerada por `dotnet ef migrations
  add`** porque o sandbox não tem o SDK do .NET instalado (ver "Limitações Do Ambiente"). O risco
  real disso é o EF Core, ao rodar `dotnet ef migrations add` de verdade depois, detectar um
  "diff" entre o modelo e o snapshot que eu não previ manualmente.

**Esta migration está pronta para ser aplicada, mas não foi.** Antes de aplicar em qualquer
ambiente, recomendo fortemente:

```
dotnet ef migrations add VerificacaoComprovantePixManual --project src/SalgaFacil.Infrastructure --startup-project src/SalgaFacil.Web
```

para confirmar que o EF Core, com o SDK de verdade, não gera nenhuma migration adicional (ou seja,
que a migration hand-crafted já deixou o snapshot em sincronia com o modelo). Se gerar uma
migration vazia, está tudo certo. Se gerar algo com conteúdo, ajuste antes de aplicar em produção.

## Testes

Projeto novo `src/SalgaFacil.Tests` (xUnit), adicionado a `SalgaFacil.slnx`:

- `Domain/PixPayloadGeradorTests.cs` — 11 casos: CRC16 contra o vetor padrão
  (`"123456789"`→`"29B1"`), auto-consistência do CRC no payload completo, presença dos campos
  obrigatórios do BR Code (moeda 986, país BR, valor formatado, chave), normalização de acento/
  maiúscula do nome do beneficiário, sanitização do identificador de transação, truncamento em 25
  caracteres, e 4 casos de validação (`ArgumentException` para chave/nome vazios e valor ≤ 0).
- `Web/ComprovanteArmazenamentoServiceTests.cs` — 13 casos: aceitação de JPG/PNG/PDF com
  assinatura correta, rejeição de arquivo vazio, maior que 8 MB, extensão não suportada, e o caso
  central de segurança (arquivo PDF disfarçado de `.jpg`, rejeitado pela assinatura binária, não
  pela extensão); salvar/ler round-trip; reenvio substitui o arquivo anterior; 3 tentativas de
  path traversal (`../../../etc/passwd` etc.) retornando `null` sem lançar; `ContentTypePara` por
  extensão.
- `Web/PagamentoPixServiceTests.cs` — 15 casos cobrindo autorização (dono do pedido, outro
  cliente da mesma empresa, cliente de outro tenant, admin de outra empresa — todos retornando
  `null` sem distinguir a causa), upload (sucesso, arquivo disfarçado, pedido já pago, reenvio
  após rejeição limpa o motivo antigo), transições de status (`EmAnalise` só a partir de
  `ComprovanteEnviado`, no-op silencioso fora disso; rejeição exige motivo, só a partir de
  `ComprovanteEnviado`/`EmAnalise`), e confirmação idempotente (duas chamadas seguidas com
  usuários diferentes não sobrescrevem data/usuário da primeira confirmação; confirmação registra
  data/hora/usuário mesmo sem comprovante).

Usa Sqlite em memória (`TestSupport/SqliteContexto.cs`) para exercitar o `SalgaFacilDbContext`
real (Includes, FKs, índices) em vez de mockar `DbSet`, e stubs simples para `IWebHostEnvironment`
(`FakeWebHostEnvironment`) e `IEmpresaContext` (`FakeEmpresaContext`) — sem framework de mock, para
não adicionar uma dependência nova só para isso.

**Resultado da execução: não executado neste ambiente.** O sandbox onde fiz esta tarefa não tem o
SDK do .NET instalado nem acesso de rede liberado para `dotnet restore` (confirmado por `which
dotnet` vazio e `curl` a domínios da Microsoft retornando bloqueio de allowlist) — a mesma
limitação já registrada no relatório do PIX-MANUAL-001-BACKEND. Não consegui rodar `dotnet build`,
`dotnet test` nem os testes já existentes no repositório (não havia nenhum antes desta tarefa).
Validei os arquivos novos por revisão manual linha a linha contra as assinaturas reais dos
métodos/entidades (`Pedido`, `Empresa`, `Cliente`, `Usuario`, `IPagamentoPixService`,
`PagamentoPixService`, `ComprovanteArmazenamentoService`, `SalgaFacilDbContext`), mas isso não
substitui rodar `dotnet test` de verdade. **Recomendo fortemente rodar antes de aprovar:**

```
dotnet restore SalgaFacil.slnx
dotnet build SalgaFacil.slnx --no-restore
dotnet test src/SalgaFacil.Tests/SalgaFacil.Tests.csproj --no-restore
```

## Riscos E Pendências Reais

- **Build/testes não verificados por execução real** (ver acima) — é o maior risco desta entrega.
  Revisão manual reduz mas não elimina a chance de erro de compilação (typo, assinatura errada,
  `using` faltando).
- **Migration hand-crafted, não gerada pelo `dotnet ef`** — rodar a verificação sugerida na seção
  "Migration" antes de aplicar em qualquer banco.
- **Download de comprovante via SignalR/base64** carrega o arquivo inteiro (até 8 MB) pela conexão
  do circuito Blazor a cada clique em "Ver/baixar" — sem cache, sem streaming. Para o volume
  esperado (poucas dezenas de pedidos Pix por dia numa loja pequena) é aceitável; se o volume
  crescer muito, vale considerar um endpoint autenticado por token de curta duração no lugar disso.
- **`ListarPedidosPorTelefoneAsync` continua existindo** em `LojaPublicaService` mesmo não sendo
  mais usado por `MeusPedidos.razor` — mantido por não ter certeza de que nada mais depende dele;
  vale um grep dedicado numa limpeza futura para confirmar que é seguro remover.
- **`PagamentoPixDesenvolvimentoService`/`PagamentoPixIndisponivelService`** foram atualizados só
  para não quebrar a build (implementam a interface v2), mas o fake não é exercitado por nenhum
  teste novo — ele já não estava registrado em `Program.cs` antes desta tarefa (achado herdado,
  não introduzido agora).
- **Sem validação de formato da chave Pix** (CPF/CNPJ/e-mail/telefone/aleatória) — herdado do
  PIX-MANUAL-001, fora do escopo desta tarefa.
- **Sem rate limiting no upload de comprovante** — um cliente autenticado pode reenviar comprovante
  repetidamente (cada envio grava no disco e substitui o anterior); não é um vetor de dano sério
  dado que exige estar logado como o dono do pedido, mas não há limite de tentativas.
- Nenhum segredo real foi commitado nem chegou a existir em nenhum arquivo desta entrega.

## Roteiro De Validação Manual (Fluxo Completo)

1. Habilitar Pix em Configurações (chave + nome do beneficiário) para uma empresa de teste.
2. No cardápio público da loja, adicionar um produto ao carrinho e ir para o checkout.
3. Confirmar que a etapa de pagamento só aparece **depois** de logar ou criar conta — tentar
   acessar `/loja/{slug}/carrinho` direto na etapa de pagamento sem login não deve ser possível.
4. Escolher Pix, finalizar o pedido, e conferir: QR Code aparece, "Copiar Pix Copia e Cola" e
   "Copiar chave" funcionam, valor bate com o total do pedido, botão do WhatsApp abre com o número
   do pedido na mensagem (se a loja tiver WhatsApp cadastrado).
5. Anexar um arquivo `.jpg`/`.png`/`.pdf` como comprovante; status deve virar "Comprovante
   enviado". Tentar anexar um arquivo `.txt` ou maior que 8 MB deve ser rejeitado com mensagem
   clara.
6. No painel administrativo (`/pedidos`), confirmar que o pedido aparece com o indicador de
   comprovante pendente (ponto/chip amarelo).
7. Abrir o pedido (`/pedidos/{id}`), clicar em "Ver/baixar comprovante" e confirmar que o arquivo
   correto abre.
8. Clicar em "Rejeitar comprovante", informar um motivo, confirmar. Voltar como cliente em "Meus
   pedidos" e conferir que o motivo aparece e que é possível reenviar um novo comprovante.
9. Reenviar o comprovante como cliente; como admin, clicar em "Confirmar recebimento Pix".
   Conferir que o status vira "Pago" nos dois lados (admin e "Meus pedidos" do cliente), com
   data/hora e (se exposto na tela) o nome de quem confirmou.
10. Clicar em "Confirmar recebimento Pix" uma segunda vez (ou recarregar e confirmar de novo) e
    verificar que nada quebra e a data/usuário da confirmação não mudam (idempotência).
11. Testar isolamento: logar como cliente de uma segunda empresa/loja e tentar acessar a URL de um
    pedido Pix da primeira empresa — deve se comportar como se o pedido não existisse.

## Declaração

- [x] Não fiz `git add`/`commit`/`push`/`merge`/`rebase` em nenhum momento desta tarefa.
- [x] Migration preparada e **não aplicada** — pronta para revisão e aplicação mediante
      autorização explícita (ver seção "Migration" para o passo de verificação recomendado antes).
- [x] Nenhum segredo ou dado bancário real foi incluído em qualquer arquivo.
- [x] Login administrativo (`AuthService`) não foi alterado; `Detalhe.razor` só passou a *ler*
      `AuthService.UsuarioAtual.Id` de uma sessão já existente.
- [x] Pix Copia e Cola preservado como campo próprio ao lado do QR Code (nenhum removido).
- [x] Isolamento multi-tenant conferido em todo acesso novo a `Pedido` (client-side por
      `ClienteId`+`EmpresaId`, admin-side por `IEmpresaContext`).
- [ ] Build/testes executados de verdade — **não fiz**, ambiente sem SDK do .NET/rede liberada.
      Ver "Testes" para o comando exato a rodar antes de aprovar.
