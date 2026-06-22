using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Controls;

public class UcCustos : UserControl
{
    private readonly CustoService _service = new();

    public UcCustos()
    {
        BackColor = WinStyles.FundoGeral;
        Dock = DockStyle.Fill;
        BuildUi();
    }

    private void BuildUi()
    {
        var titulo = new Label { Text = "Controle de Custos", Font = WinStyles.FonteTitulo, Dock = DockStyle.Top, Height = 32 };

        var pnlCards = new TableLayoutPanel { Dock = DockStyle.Top, Height = 70, ColumnCount = 2 };
        pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlCards.Controls.Add(WinStyles.CriarCardMetrica("Custo Mensal", _service.CustoMensal.ToString("C", new System.Globalization.CultureInfo("pt-BR")), Color.FromArgb(220, 38, 38)), 0, 0);
        pnlCards.Controls.Add(WinStyles.CriarCardMetrica("Venda Mensal", _service.VendaMensal.ToString("C", new System.Globalization.CultureInfo("pt-BR")), Color.FromArgb(21, 128, 61)), 1, 0);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = WinStyles.PainelBranco, Margin = new Padding(0, 8, 0, 0) };
        pnlGrid.BorderStyle = BorderStyle.FixedSingle;
        var grid = WinStyles.CriarGrid();
        grid.Columns.AddRange([
            new DataGridViewTextBoxColumn { HeaderText = "Produto", Name = "Produto" },
            new DataGridViewTextBoxColumn { HeaderText = "Custo Unit.", Name = "Custo" },
            new DataGridViewTextBoxColumn { HeaderText = "Preço Venda", Name = "Preco" },
            new DataGridViewTextBoxColumn { HeaderText = "Lucro Est.", Name = "Lucro" },
            new DataGridViewTextBoxColumn { HeaderText = "Margem", Name = "Margem" }
        ]);
        grid.CellFormatting += (_, e) =>
        {
            if (_gridCol(grid, e.ColumnIndex) == "Margem" && e.RowIndex >= 0)
            {
                e.CellStyle.ForeColor = Color.FromArgb(21, 128, 61);
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        };
        var pt = new System.Globalization.CultureInfo("pt-BR");
        foreach (var c in _service.Listar())
            grid.Rows.Add(c.Produto, c.CustoUnitario.ToString("C", pt), c.PrecoVenda.ToString("C", pt), c.LucroEstimado.ToString("C", pt), $"{c.MargemPercentual}%");
        pnlGrid.Controls.Add(grid);

        Controls.Add(pnlGrid);
        Controls.Add(pnlCards);
        Controls.Add(titulo);
    }

    private static string _gridCol(DataGridView g, int i) => g.Columns[i].Name;
}
