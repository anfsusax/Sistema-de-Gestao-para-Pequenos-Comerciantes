using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Models;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Forms;

public class FrmNovoPedido : Form
{
    private readonly PedidoService _pedidoService = new();
    private ComboBox _cmbCliente = null!;
    private ComboBox _cmbProduto = null!;
    private TextBox _txtQtd = null!;
    private DataGridView _gridItens = null!;
    private Label _lblTotal = null!;
    private DateTimePicker _dtpEntrega = null!;
    private readonly List<ItemPedido> _itens = [];

    public FrmNovoPedido()
    {
        _itens.AddRange([
            new ItemPedido { Produto = "Coxinha", Quantidade = 100, ValorUnitario = 4.50m },
            new ItemPedido { Produto = "Esfiha", Quantidade = 100, ValorUnitario = 4.00m }
        ]);
        InitializeComponent();
        AtualizarItens();
    }

    private void InitializeComponent()
    {
        Text = "Novo Pedido";
        Size = new Size(500, 480);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(245, 245, 245);
        Font = WinStyles.FontePadrao;

        int y = 16;
        Controls.Add(new Label { Text = "Cliente", Location = new Point(16, y), AutoSize = true }); y += 18;
        _cmbCliente = new ComboBox { Location = new Point(16, y), Width = 450, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbCliente.Items.AddRange(DataStore.Clientes.Select(c => c.Nome).ToArray());
        if (_cmbCliente.Items.Count > 0) _cmbCliente.SelectedIndex = 0;
        Controls.Add(_cmbCliente); y += 36;

        Controls.Add(new Label { Text = "Produto", Location = new Point(16, y), AutoSize = true }); y += 18;
        var pnlProd = new Panel { Location = new Point(16, y), Size = new Size(450, 28) };
        _cmbProduto = new ComboBox { Location = new Point(0, 2), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbProduto.Items.AddRange(DataStore.Produtos.Where(p => p.Status == StatusProduto.Ativo).Select(p => p.Nome).ToArray());
        if (_cmbProduto.Items.Count > 0) _cmbProduto.SelectedIndex = 0;
        _txtQtd = new TextBox { Location = new Point(210, 2), Width = 60, Text = "1" };
        var btnAdd = WinStyles.CriarBotao("+");
        btnAdd.Location = new Point(280, 0);
        btnAdd.Click += (_, _) => AdicionarItem();
        pnlProd.Controls.AddRange([_cmbProduto, _txtQtd, btnAdd]);
        Controls.Add(pnlProd); y += 36;

        _gridItens = WinStyles.CriarGrid();
        _gridItens.Location = new Point(16, y);
        _gridItens.Size = new Size(450, 100);
        _gridItens.Columns.AddRange([
            new DataGridViewTextBoxColumn { HeaderText = "Produto", Name = "Produto", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = "Qtd", Name = "Qtd", Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Valor", Name = "Valor", Width = 120 }
        ]);
        Controls.Add(_gridItens); y += 110;

        _lblTotal = new Label { Text = "Total: R$ 850,00", Location = new Point(16, y), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true };
        Controls.Add(_lblTotal); y += 28;

        Controls.Add(new Label { Text = "Data de Entrega", Location = new Point(16, y), AutoSize = true }); y += 18;
        _dtpEntrega = new DateTimePicker { Location = new Point(16, y), Width = 200 };
        Controls.Add(_dtpEntrega); y += 40;

        var btnConfirmar = WinStyles.CriarBotao("Confirmar Pedido", true);
        btnConfirmar.Location = new Point(250, y);
        btnConfirmar.Click += BtnConfirmar_Click;
        var btnCancelar = WinStyles.CriarBotao("Cancelar");
        btnCancelar.Location = new Point(370, y);
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.AddRange([btnConfirmar, btnCancelar]);
    }

    private void AdicionarItem()
    {
        if (_cmbProduto.SelectedItem == null || !int.TryParse(_txtQtd.Text, out var qtd) || qtd <= 0) return;
        var nome = _cmbProduto.SelectedItem.ToString()!;
        var prod = DataStore.Produtos.FirstOrDefault(p => p.Nome == nome);
        _itens.Add(new ItemPedido { Produto = nome, Quantidade = qtd, ValorUnitario = prod?.PrecoVenda ?? 0 });
        AtualizarItens();
    }

    private void AtualizarItens()
    {
        _gridItens.Rows.Clear();
        var pt = new System.Globalization.CultureInfo("pt-BR");
        decimal total = 0;
        foreach (var i in _itens)
        {
            _gridItens.Rows.Add($"{i.Produto} x{i.Quantidade}", i.Quantidade, i.Total.ToString("C", pt));
            total += i.Total;
        }
        _lblTotal.Text = $"Total: {total.ToString("C", pt)}";
    }

    private void BtnConfirmar_Click(object? sender, EventArgs e)
    {
        var pedido = new Pedido
        {
            Numero = DataStore.NovoNumeroPedido(),
            Cliente = _cmbCliente.SelectedItem?.ToString() ?? "",
            Data = DateTime.Today,
            DataEntrega = _dtpEntrega.Value,
            Valor = _itens.Sum(i => i.Total),
            Status = StatusPedido.Aguardando,
            Itens = _itens.ToList()
        };
        _pedidoService.Salvar(pedido);
        MessageBox.Show("Pedido confirmado com sucesso.", "SalgaPro");
        DialogResult = DialogResult.OK;
        Close();
    }
}
