using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Controls;

public class UcDashboard : UserControl
{
    private DataGridView _grid = null!;

    public UcDashboard()
    {
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;
        Dock = DockStyle.Fill;

        var titulo = new Label { Text = "Painel Principal", Font = WinStyles.FonteTitulo, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 12) };

        var pnlCards = new TableLayoutPanel { Dock = DockStyle.Top, Height = 80, ColumnCount = 4, RowCount = 1 };
        pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        pnlCards.Controls.Add(WinStyles.CriarCardMetrica("Pedidos Hoje", "12", Color.FromArgb(29, 78, 216)), 0, 0);
        pnlCards.Controls.Add(WinStyles.CriarCardMetrica("Em Produção", "340", Color.FromArgb(234, 88, 12)), 1, 0);
        pnlCards.Controls.Add(WinStyles.CriarCardMetrica("Aguardando", "5", Color.FromArgb(202, 138, 4)), 2, 0);
        pnlCards.Controls.Add(WinStyles.CriarCardMetrica("Valor Vendido", "R$ 2.450", Color.FromArgb(21, 128, 61)), 3, 0);

        var pnlEntregas = new Panel { Dock = DockStyle.Fill, BackColor = WinStyles.PainelBranco, Padding = new Padding(12), Margin = new Padding(0, 12, 0, 0) };
        pnlEntregas.BorderStyle = BorderStyle.FixedSingle;
        var lblEnt = new Label { Text = "Próximas Entregas", Font = new Font("Segoe UI", 9F, FontStyle.Bold), Dock = DockStyle.Top, Height = 24 };

        _grid = WinStyles.CriarGrid();
        _grid.Columns.AddRange([
            new DataGridViewTextBoxColumn { HeaderText = "Pedido", Name = "Pedido" },
            new DataGridViewTextBoxColumn { HeaderText = "Cliente", Name = "Cliente" },
            new DataGridViewTextBoxColumn { HeaderText = "Entrega", Name = "Entrega" },
            new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status" }
        ]);
        _grid.CellFormatting += Grid_CellFormatting;
        _grid.Dock = DockStyle.Fill;

        pnlEntregas.Controls.Add(_grid);
        pnlEntregas.Controls.Add(lblEnt);

        Controls.Add(pnlEntregas);
        Controls.Add(pnlCards);
        Controls.Add(titulo);

        CarregarDados();
    }

    private void CarregarDados()
    {
        _grid.Rows.Clear();
        foreach (var e in DataStore.Entregas)
            _grid.Rows.Add(e.Pedido, e.Cliente, e.Entrega, WinStyles.StatusPedidoTexto(e.Status));
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grid.Columns[e.ColumnIndex].Name != "Status" || e.RowIndex < 0) return;
        var texto = e.Value?.ToString() ?? "";
        var status = texto switch
        {
            "Produzindo" => StatusPedido.Produzindo,
            "Aguardando" => StatusPedido.Aguardando,
            "Pronto" => StatusPedido.Pronto,
            "Entregue" => StatusPedido.Entregue,
            _ => StatusPedido.Aguardando
        };
        e.CellStyle.BackColor = WinStyles.CorStatusPedido(status);
        e.CellStyle.ForeColor = WinStyles.CorTextoStatusPedido(status);
        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    }
}
