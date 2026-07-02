using SalgaFacil.Desktop.Enums;

namespace SalgaFacil.Desktop.Helpers;

/// <summary>
/// Paleta e estilos alinhados ao design system do SalgadosPro Web (src/SalgaFacil.Web/wwwroot/app.css):
/// navy #1a1d2e (sidebar), terracota #d4500f (acento/marca), fundo bege #f5f0eb.
/// Fonte mantida em "Segoe UI" (o Web usa "Plus Jakarta Sans", fonte do Google não instalada por
/// padrão no Windows — embutir o .ttf é possível, mas exige distribuir o arquivo junto do app;
/// não fiz isso aqui para não adicionar mais uma fonte de fragilidade nesta rodada).
/// Sombras suaves (box-shadow) e cantos arredondados (border-radius) do Web não têm equivalente
/// nativo no WinForms sem desenho customizado (GDI+); aproximados aqui com bordas finas simples.
/// </summary>
public static class WinStyles
{
    // Fundo geral bege (era #ECE9D8 estilo Windows clássico — agora bate com --bg do Web)
    public static readonly Color FundoGeral = Color.FromArgb(245, 240, 235);
    // Navy da sidebar (--navy)
    public static readonly Color Navy = Color.FromArgb(26, 29, 46);
    public static readonly Color NavyClaro = Color.FromArgb(37, 40, 64);
    // Terracota, cor de marca/acento (--terracota / --terracota-hover / --terracota-light)
    public static readonly Color Terracota = Color.FromArgb(212, 80, 15);
    public static readonly Color TerracotaHover = Color.FromArgb(184, 68, 12);
    public static readonly Color TerracotaClara = Color.FromArgb(253, 238, 230);
    public static readonly Color PainelBranco = Color.White;
    public static readonly Color Borda = Color.FromArgb(232, 228, 223);
    // Compat: nomes antigos apontando para as cores novas (evita quebrar código que ainda os usa)
    public static readonly Color MenuBar = Color.FromArgb(26, 29, 46);
    public static readonly Color MenuAtivo = Color.FromArgb(212, 80, 15);
    public static readonly Color BotaoPrimario = Color.FromArgb(212, 80, 15);
    public static readonly Color BotaoPrimarioHover = Color.FromArgb(184, 68, 12);
    public static readonly Color GridHeader = Color.White;
    public static readonly Color GridAlternada = Color.FromArgb(245, 240, 235);
    public static readonly Color Rodape = Color.FromArgb(245, 240, 235);
    // Textos (--text / --text-muted / --text-light)
    public static readonly Color TextoPrincipal = Color.FromArgb(26, 29, 46);
    public static readonly Color TextoMuted = Color.FromArgb(107, 114, 128);
    public static readonly Color TextoClaro = Color.FromArgb(156, 163, 175);
    // Verde (--green / --green-light / --green-text)
    public static readonly Color Verde = Color.FromArgb(13, 148, 136);
    public static readonly Color VerdeClaro = Color.FromArgb(209, 250, 229);
    public static readonly Color VerdeTexto = Color.FromArgb(6, 95, 70);
    // Azul (--blue-light / --blue-text)
    public static readonly Color AzulClaro = Color.FromArgb(219, 234, 254);
    public static readonly Color AzulTexto = Color.FromArgb(29, 78, 216);
    // Amarelo (--yellow-light / --yellow-text)
    public static readonly Color AmareloClaro = Color.FromArgb(254, 243, 199);
    public static readonly Color AmareloTexto = Color.FromArgb(180, 83, 9);
    // Cinza neutro (--gray-light / --gray-text)
    public static readonly Color CinzaClaro = Color.FromArgb(243, 244, 246);
    public static readonly Color CinzaTexto = Color.FromArgb(107, 114, 128);

    public static readonly Font FontePadrao = new("Segoe UI", 9F);
    public static readonly Font FonteTitulo = new("Segoe UI", 15F, FontStyle.Bold);
    public static readonly Font FonteLogo = new("Segoe UI", 10F, FontStyle.Bold);

    /// <remarks>
    /// BUG CORRIGIDO (2026-07-01): tinha "Height = 28" fixo junto com "AutoSize = true" — combinação
    /// redundante/conflitante (o AutoSize recalcula a altura sozinho) que deixava pouca folga para
    /// acentos com descendente, tipo o "ç" de "Produção" (botões "Iniciar Produção"/"Finalizar
    /// Produção"), cortando a base da letra. Removido o Height fixo; Padding vertical aumentado.
    /// </remarks>
    public static Button CriarBotao(string texto, bool primario = false)
    {
        var btn = new Button
        {
            Text = texto,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(16, 6, 16, 6),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = primario ? TerracotaHover : Borda;
        btn.BackColor = primario ? Terracota : PainelBranco;
        btn.ForeColor = primario ? Color.White : TextoPrincipal;
        btn.FlatAppearance.MouseOverBackColor = primario ? TerracotaHover : FundoGeral;
        return btn;
    }

    public static DataGridView CriarGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = PainelBranco,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 32,
            RowTemplate = { Height = 30 },
            Font = FontePadrao,
            EnableHeadersVisualStyles = false,
            GridColor = Borda
        };
        // Header sem fundo diferenciado (igual ao Web: mesmo branco do card, só com texto cinza claro
        // uppercase e borda inferior separando) em vez do cinza sólido do estilo clássico Windows.
        grid.ColumnHeadersDefaultCellStyle.BackColor = PainelBranco;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextoClaro;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        grid.DefaultCellStyle.SelectionBackColor = TerracotaClara;
        grid.DefaultCellStyle.SelectionForeColor = TextoPrincipal;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        // Sem zebra striping — o Web usa fundo uniforme e só destaca a linha no hover.
        grid.AlternatingRowsDefaultCellStyle.BackColor = PainelBranco;
        return grid;
    }

    /// <summary>
    /// Empilha um rótulo acima de um campo dentro de um FlowLayoutPanel vertical (TopDown).
    /// Substitui o padrão fragil de Location manual (ex.: "y += 18; y += 32") usado nas telas de cadastro:
    /// o próprio FlowLayoutPanel calcula a posição de cada elemento a partir do anterior, então
    /// mudanças de fonte/DPI não geram sobreposição.
    /// </summary>
    /// <remarks>
    /// IMPORTANTE: defina "campo.Width" explicitamente ANTES de chamar este método.
    /// O fallback "larguraCampo" só é aplicado quando Width &lt;= 0, mas o valor padrão de um
    /// TextBox novo já é 100 (nunca &lt;= 0) — então o fallback NÃO dispara se você esquecer de
    /// definir a largura, e o campo fica visualmente cortado (bug real encontrado em 2026-07-01
    /// em UcConfiguracoes.cs: "Nome da Empresa" cortando o texto por falta de Width).
    /// </remarks>
    public static FlowLayoutPanel AdicionarCampo(FlowLayoutPanel destino, string rotulo, Control campo, int larguraCampo = 320)
    {
        var lbl = new Label
        {
            Text = rotulo,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = TextoMuted,
            AutoSize = true,
            Margin = new Padding(0, destino.Controls.Count == 0 ? 0 : 10, 0, 3)
        };
        campo.Margin = new Padding(0);
        if (campo.Width <= 0) campo.Width = larguraCampo;
        destino.Controls.Add(lbl);
        destino.Controls.Add(campo);
        return destino;
    }

    /// <summary>
    /// Cria uma linha horizontal (FlowLayoutPanel LeftToRight) contendo dois campos lado a lado,
    /// cada um com seu próprio rótulo acima. Usada para pares como "Categoria / Tipo" ou
    /// "Preço Venda / Custo Estimado", sem precisar calcular coordenadas X manualmente.
    /// </summary>
    public static FlowLayoutPanel CriarLinhaDupla(string rotulo1, Control campo1, string rotulo2, Control campo2, int espacoEntre = 24)
    {
        var linha = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 10, 0, 0)
        };

        var col1 = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, espacoEntre, 0) };
        AdicionarCampo(col1, rotulo1, campo1);

        var col2 = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0) };
        AdicionarCampo(col2, rotulo2, campo2);

        linha.Controls.Add(col1);
        linha.Controls.Add(col2);
        return linha;
    }

    /// <summary>
    /// Painel de ações (toolbar) ancorado à direita, usado para os botões Novo/Editar/Excluir
    /// nas telas de listagem. Substitui Location fixo (ex.: x=700/820/900) que cortava os
    /// botões quando a janela era redimensionada para o MinimumSize.
    /// </summary>
    /// <remarks>
    /// Adicione os botões em ordem normal de leitura (ex.: Novo, Editar, Excluir) —
    /// o painel fica ancorado à direita (Dock=Right) mas o fluxo interno é LeftToRight,
    /// então o primeiro adicionado fica mais à esquerda do grupo, como esperado.
    /// </remarks>
    public static FlowLayoutPanel CriarBarraAcoes()
    {
        return new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Right,
            Margin = new Padding(0)
        };
    }

    public static Panel CriarCardMetrica(string titulo, string valor, Color? corValor = null) =>
        CriarCardMetrica(titulo, valor, corValor, out _);

    /// <remarks>
    /// BUG CORRIGIDO (2026-07-01, 2ª rodada): a 1ª correção (AutoEllipsis) não resolveu — o real
    /// problema é estrutural: "lblT" tinha Height=18 fixo e "lblV" tinha Dock=Fill dentro de um
    /// Panel com Padding fixo (20,14,20,14), dentro de uma linha de TableLayoutPanel com Height
    /// fixo (80/70px). Sobrando pouco espaço pro valor (fonte 18pt bold), qualquer variação de
    /// DPI/scaling do Windows cortava o texto pela metade. Fix: título e valor agora ficam num
    /// FlowLayoutPanel TopDown com AutoSize=true (Dock=Top, nunca Fill), então a altura real vem
    /// do conteúdo renderizado, não de um número mágico em pixels.
    ///
    /// BUG CORRIGIDO (2026-07-01, 3ª rodada): UcProducao.cs precisa atualizar o valor do card
    /// depois (Carregar() recalcula totais), e fazia isso com "(Label)cardTotal.Controls[0]" —
    /// um cast cego no índice de filho direto do Panel, que quebrou (InvalidCastException) assim
    /// que a estrutura interna mudou de "Panel > Label, Label" pra "Panel > FlowLayoutPanel >
    /// Label, Label" na correção acima. Reflexão em índice de Controls é frágil por definição.
    /// Fix: expor a Label do valor via parâmetro "out", sem o chamador precisar conhecer a
    /// árvore de controles interna do card.
    /// </remarks>
    public static Panel CriarCardMetrica(string titulo, string valor, Color? corValor, out Label lblValor)
    {
        var p = new Panel { BackColor = PainelBranco, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(18, 12, 18, 12), Margin = new Padding(0, 0, 12, 0), Dock = DockStyle.Fill };

        var conteudo = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };

        var lblT = new Label { Text = titulo.ToUpperInvariant(), ForeColor = TextoMuted, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 6) };
        var lblV = new Label { Text = valor, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = corValor ?? TextoPrincipal, AutoSize = true, Margin = new Padding(0) };

        conteudo.Controls.Add(lblT);
        conteudo.Controls.Add(lblV);
        p.Controls.Add(conteudo);
        lblValor = lblV;
        return p;
    }

    public static Color CorStatusPedido(StatusPedido status) => status switch
    {
        StatusPedido.Produzindo => AzulClaro,
        StatusPedido.Aguardando => AmareloClaro,
        StatusPedido.Pronto => VerdeClaro,
        StatusPedido.Entregue => CinzaClaro,
        _ => Color.White
    };

    public static Color CorTextoStatusPedido(StatusPedido status) => status switch
    {
        StatusPedido.Produzindo => AzulTexto,
        StatusPedido.Aguardando => AmareloTexto,
        StatusPedido.Pronto => VerdeTexto,
        StatusPedido.Entregue => CinzaTexto,
        _ => TextoPrincipal
    };

    public static Color CorStatusProducao(StatusProducao status) => status switch
    {
        StatusProducao.Produzindo => AzulClaro,
        StatusProducao.NaoIniciado => CinzaClaro,
        StatusProducao.Finalizado => VerdeClaro,
        _ => Color.White
    };

    public static Color CorTextoProducao(StatusProducao status) => status switch
    {
        StatusProducao.Produzindo => AzulTexto,
        StatusProducao.NaoIniciado => CinzaTexto,
        StatusProducao.Finalizado => VerdeTexto,
        _ => TextoPrincipal
    };

    public static string StatusPedidoTexto(StatusPedido s) => s switch
    {
        StatusPedido.Aguardando => "Aguardando",
        StatusPedido.Produzindo => "Produzindo",
        StatusPedido.Pronto => "Pronto",
        StatusPedido.Entregue => "Entregue",
        _ => s.ToString()
    };

    public static string StatusProducaoTexto(StatusProducao s) => s switch
    {
        StatusProducao.NaoIniciado => "Não iniciado",
        StatusProducao.Produzindo => "Produzindo",
        StatusProducao.Finalizado => "Finalizado",
        _ => s.ToString()
    };
}
