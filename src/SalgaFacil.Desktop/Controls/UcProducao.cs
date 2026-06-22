using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Controls;

public class UcProducao : UserControl
{
    private DataGridView _grid = null!;
    private Label _lblTotalCard = null!;
    private Label _lblFritoCard = null!;
    private Label _lblAssadoCard = null!;
    private readonly ProducaoService _service = new();

    public UcProducao()
    {
        BackColor = WinStyles.FundoGeral;
        Dock = DockStyle.Fill;
        BuildUi();
        Carregar();
    }

    private void BuildUi()
    {
        var titulo = new Label { Text = "Controle de Produção", Font = WinStyles.FonteTitulo, Dock = DockStyle.Top, Height = 32 };

        var pnlCards = new TableLayoutPanel { Dock = DockStyle.Top, Height = 70, ColumnCount = 3 };
        for (int i = 0; i < 3; i++) pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        var cardTotal = WinStyles.CriarCardMetrica("Total a Produzir", "540 un");
        var cardFrito = WinStyles.CriarCardMetrica("Fritos", "320 un");
        var cardAssado = WinStyles.CriarCardMetrica("Assados", "220 un");
        _lblTotalCard = (Label)cardTotal.Controls[0];
        _lblFritoCard = (Label)cardFrito.Controls[0];
        _lblAssadoCard = (Label)cardAssado.Controls[0];
        pnlCards.Controls.Add(cardTotal, 0, 0);
        pnlCards.Controls.Add(cardFrito, 1, 0);
        pnlCards.Controls.Add(cardAssado, 2, 0);

        var pnlBotoes = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 4, 0, 4) };
        var btnIniciar = WinStyles.CriarBotao("Iniciar Produção", true);
        var btnFinalizar = WinStyles.CriarBotao("Finalizar Produção", true);
        var btnPronto = WinStyles.CriarBotao("Marcar Pedido como Pronto", true);
        var btnAtualizar = WinStyles.CriarBotao("Atualizar");
        btnIniciar.Click += (_, _) => AlterarStatus(StatusProducao.Produzindo);
        btnFinalizar.Click += (_, _) => AlterarStatus(StatusProducao.Finalizado);
        btnPronto.Click += (_, _) => MessageBox.Show("Pedido marcado como pronto.", "Produção");
        btnAtualizar.Click += (_, _) => Carregar();
        pnlBotoes.Controls.AddRange([btnIniciar, btnFinalizar, btnPronto, btnAtualizar]);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = WinStyles.PainelBranco, Margin = new Padding(0, 8, 0, 0) };
        pnlGrid.BorderStyle = BorderStyle.FixedSingle;
        _grid = WinStyles.CriarGrid();
        _grid.Columns.AddRange([
            new DataGridViewTextBoxColumn { HeaderText = "Pedido", Name = "Pedido" },
            new DataGridViewTextBoxColumn { HeaderText = "Produto", Name = "Produto" },
            new DataGridViewTextBoxColumn { HeaderText = "Qtd", Name = "Qtd" },
            new DataGridViewTextBoxColumn { HeaderText = "Tipo", Name = "Tipo" },
            new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status" }
        ]);
        _grid.CellFormatting += (_, e) =>
        {
            if (_grid.Columns[e.ColumnIndex].Name != "Status" || e.RowIndex < 0) return;
            var s = e.Value?.ToString() ?? "";
            StatusProducao st = s.Contains("Produzindo") ? StatusProducao.Produzindo : s.Contains("Finalizado") ? StatusProducao.Finalizado : StatusProducao.NaoIniciado;
            e.CellStyle.BackColor = WinStyles.CorStatusProducao(st);
            e.CellStyle.ForeColor = WinStyles.CorTextoProducao(st);
            e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        };
        pnlGrid.Controls.Add(_grid);

        Controls.Add(pnlGrid);
        Controls.Add(pnlBotoes);
        Controls.Add(pnlCards);
        Controls.Add(titulo);
    }

    private void Carregar()
    {
        var (total, fritos, assados) = _service.ObterTotais();
        _lblTotalCard.Text = $"{total} un";
        _lblFritoCard.Text = $"{fritos} un";
        _lblAssadoCard.Text = $"{assados} un";
        _grid.Rows.Clear();
        foreach (var i in _service.Listar())
            _grid.Rows.Add(i.Pedido, i.Produto, i.Quantidade, i.Tipo.ToString(), WinStyles.StatusProducaoTexto(i.Status));
    }

    private void AlterarStatus(StatusProducao novo)
    {
        if (_grid.CurrentRow == null) return;
        var pedido = _grid.CurrentRow.Cells["Pedido"].Value?.ToString();
        var produto = _grid.CurrentRow.Cells["Produto"].Value?.ToString();
        var item = DataStore.Producao.FirstOrDefault(p => p.Pedido == pedido && p.Produto == produto);
        if (item != null) item.Status = novo;
        Carregar();
    }
}
