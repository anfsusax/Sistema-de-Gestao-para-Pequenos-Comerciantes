using SalgaFacil.Desktop.Enums;

namespace SalgaFacil.Desktop.Helpers;

/// <summary>Cores e estilos do protótipo HTML clássico Windows.</summary>
public static class WinStyles
{
    public static readonly Color FundoGeral = Color.FromArgb(236, 233, 216);
    public static readonly Color MenuBar = Color.FromArgb(240, 240, 240);
    public static readonly Color MenuAtivo = Color.FromArgb(205, 228, 247);
    public static readonly Color PainelBranco = Color.White;
    public static readonly Color Borda = Color.FromArgb(204, 204, 204);
    public static readonly Color BotaoPrimario = Color.FromArgb(74, 144, 217);
    public static readonly Color BotaoPrimarioHover = Color.FromArgb(90, 159, 232);
    public static readonly Color GridHeader = Color.FromArgb(240, 240, 240);
    public static readonly Color GridAlternada = Color.FromArgb(250, 250, 250);
    public static readonly Color Rodape = Color.FromArgb(240, 240, 240);
    public static readonly Font FontePadrao = new("Segoe UI", 9F);
    public static readonly Font FonteTitulo = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font FonteLogo = new("Segoe UI", 10F, FontStyle.Bold);

    public static Button CriarBotao(string texto, bool primario = false)
    {
        var btn = new Button
        {
            Text = texto,
            FlatStyle = FlatStyle.Flat,
            Font = FontePadrao,
            Height = 28,
            AutoSize = true,
            Padding = new Padding(12, 2, 12, 2),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = primario ? Color.FromArgb(42, 106, 176) : Color.FromArgb(171, 171, 171);
        btn.BackColor = primario ? BotaoPrimario : Color.FromArgb(240, 240, 240);
        btn.ForeColor = primario ? Color.White : Color.Black;
        btn.FlatAppearance.MouseOverBackColor = primario ? BotaoPrimarioHover : Color.FromArgb(232, 232, 232);
        return btn;
    }

    public static DataGridView CriarGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = PainelBranco,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 28,
            RowTemplate = { Height = 26 },
            Font = FontePadrao,
            EnableHeadersVisualStyles = false
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeader;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 254);
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        grid.AlternatingRowsDefaultCellStyle.BackColor = GridAlternada;
        return grid;
    }

    public static Panel CriarCardMetrica(string titulo, string valor, Color? corValor = null)
    {
        var p = new Panel { BackColor = PainelBranco, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(12, 8, 12, 8), Margin = new Padding(0, 0, 8, 8) };
        var lblT = new Label { Text = titulo, ForeColor = Color.FromArgb(85, 85, 85), Font = new Font("Segoe UI", 8F), Dock = DockStyle.Top, AutoSize = false, Height = 18 };
        var lblV = new Label { Text = valor, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = corValor ?? Color.FromArgb(30, 58, 95), Dock = DockStyle.Fill };
        p.Controls.Add(lblV);
        p.Controls.Add(lblT);
        return p;
    }

    public static Color CorStatusPedido(StatusPedido status) => status switch
    {
        StatusPedido.Produzindo => Color.FromArgb(219, 234, 254),
        StatusPedido.Aguardando => Color.FromArgb(254, 243, 199),
        StatusPedido.Pronto => Color.FromArgb(209, 250, 229),
        StatusPedido.Entregue => Color.FromArgb(229, 231, 235),
        _ => Color.White
    };

    public static Color CorTextoStatusPedido(StatusPedido status) => status switch
    {
        StatusPedido.Produzindo => Color.FromArgb(30, 64, 175),
        StatusPedido.Aguardando => Color.FromArgb(146, 64, 14),
        StatusPedido.Pronto => Color.FromArgb(6, 95, 70),
        StatusPedido.Entregue => Color.FromArgb(75, 85, 99),
        _ => Color.Black
    };

    public static Color CorStatusProducao(StatusProducao status) => status switch
    {
        StatusProducao.Produzindo => Color.FromArgb(219, 234, 254),
        StatusProducao.NaoIniciado => Color.FromArgb(229, 231, 235),
        StatusProducao.Finalizado => Color.FromArgb(209, 250, 229),
        _ => Color.White
    };

    public static Color CorTextoProducao(StatusProducao status) => status switch
    {
        StatusProducao.Produzindo => Color.FromArgb(30, 64, 175),
        StatusProducao.NaoIniciado => Color.FromArgb(75, 85, 99),
        StatusProducao.Finalizado => Color.FromArgb(6, 95, 70),
        _ => Color.Black
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
