using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Forms;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Controls;

public class UcPedidos : UserControl
{
    private DataGridView _grid = null!;
    private ComboBox _cmbStatus = null!;
    private DateTimePicker _dtpData = null!;
    private readonly PedidoService _service = new();

    public UcPedidos()
    {
        BackColor = WinStyles.FundoGeral;
        Dock = DockStyle.Fill;
        BuildUi();
        Carregar();
    }

    private void BuildUi()
    {
        var pnlTopo = new Panel { Dock = DockStyle.Top, Height = 36 };
        var titulo = new Label { Text = "Lista de Pedidos", Font = WinStyles.FonteTitulo, AutoSize = true, Location = new Point(0, 4) };
        var btnNovo = WinStyles.CriarBotao("Novo Pedido", true);
        btnNovo.Location = new Point(820, 4);
        btnNovo.Click += (_, _) => { using var f = new FrmNovoPedido(); if (f.ShowDialog(FindForm()) == DialogResult.OK) Carregar(); };
        pnlTopo.Controls.AddRange([titulo, btnNovo]);

        var pnlFiltro = new Panel { Dock = DockStyle.Top, Height = 32 };
        pnlFiltro.Controls.Add(new Label { Text = "Status:", Location = new Point(0, 8), AutoSize = true });
        _cmbStatus = new ComboBox { Location = new Point(50, 4), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbStatus.Items.AddRange(["Todos", "Aguardando", "Em produção", "Pronto", "Entregue"]);
        _cmbStatus.SelectedIndex = 0;
        _cmbStatus.SelectedIndexChanged += (_, _) => Carregar();
        pnlFiltro.Controls.Add(new Label { Text = "Data:", Location = new Point(200, 8), AutoSize = true });
        _dtpData = new DateTimePicker { Location = new Point(240, 4), Width = 120, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
        _dtpData.ValueChanged += (_, _) => Carregar();
        pnlFiltro.Controls.AddRange([_cmbStatus, _dtpData]);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = WinStyles.PainelBranco, Margin = new Padding(0, 8, 0, 0) };
        pnlGrid.BorderStyle = BorderStyle.FixedSingle;
        _grid = WinStyles.CriarGrid();
        _grid.Columns.AddRange([
            new DataGridViewTextBoxColumn { HeaderText = "#", Name = "Numero" },
            new DataGridViewTextBoxColumn { HeaderText = "Cliente", Name = "Cliente" },
            new DataGridViewTextBoxColumn { HeaderText = "Data", Name = "Data" },
            new DataGridViewTextBoxColumn { HeaderText = "Valor", Name = "Valor" },
            new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status" }
        ]);
        _grid.CellFormatting += GridStatusFormat;
        pnlGrid.Controls.Add(_grid);

        Controls.Add(pnlGrid);
        Controls.Add(pnlFiltro);
        Controls.Add(pnlTopo);
    }

    private void Carregar()
    {
        var status = _cmbStatus.SelectedItem?.ToString();
        DateTime? data = _dtpData.Checked ? _dtpData.Value.Date : null;
        _grid.Rows.Clear();
        foreach (var p in _service.Listar(status, data))
        {
            var statusTxt = p.Status switch
            {
                StatusPedido.Produzindo => "Produzindo",
                StatusPedido.Aguardando => "Aguardando",
                _ => PedidoService.StatusLabel(p.Status)
            };
            if (status == "Em produção" && p.Status == StatusPedido.Produzindo) { }
            _grid.Rows.Add(p.Numero.ToString("D4"), p.Cliente, p.Data.ToString("dd/MM"), p.Valor.ToString("C", new System.Globalization.CultureInfo("pt-BR")), statusTxt);
        }
    }

    private void GridStatusFormat(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grid.Columns[e.ColumnIndex].Name != "Status" || e.RowIndex < 0) return;
        var s = e.Value?.ToString() ?? "";
        StatusPedido st = s switch
        {
            "Pronto" => StatusPedido.Pronto,
            "Aguardando" => StatusPedido.Aguardando,
            "Produzindo" => StatusPedido.Produzindo,
            "Entregue" => StatusPedido.Entregue,
            _ => StatusPedido.Aguardando
        };
        e.CellStyle.BackColor = WinStyles.CorStatusPedido(st);
        e.CellStyle.ForeColor = WinStyles.CorTextoStatusPedido(st);
        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    }
}
