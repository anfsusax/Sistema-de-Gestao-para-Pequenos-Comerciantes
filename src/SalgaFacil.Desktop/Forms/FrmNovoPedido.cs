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
        Size = new Size(500, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;

        // BUG CORRIGIDO: "corpo" estava com Dock=Fill + AutoSize=true ao mesmo tempo — combinação
        // contraditória em WinForms que cortava o conteúdo que não coubesse (grid de itens e
        // botões "Confirmar"/"Cancelar" podiam ficar fora da área visível). Dock=Top fixa a
        // Largura e deixa a Altura livre pro AutoSize calcular a partir do conteúdo real.
        var corpo = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(16) };

        _cmbCliente = new ComboBox { Width = 450, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbCliente.Items.AddRange(DataStore.Clientes.Select(c => c.Nome).ToArray());
        if (_cmbCliente.Items.Count > 0) _cmbCliente.SelectedIndex = 0;
        WinStyles.AdicionarCampo(corpo, "Cliente", _cmbCliente);

        corpo.Controls.Add(new Label { Text = "Produto", AutoSize = true, Margin = new Padding(0, 10, 0, 2) });
        var pnlProd = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0) };
        _cmbProduto = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbProduto.Items.AddRange(DataStore.Produtos.Where(p => p.Status == StatusProduto.Ativo).Select(p => p.Nome).ToArray());
        if (_cmbProduto.Items.Count > 0) _cmbProduto.SelectedIndex = 0;
        _txtQtd = new TextBox { Width = 60, Text = "1", Margin = new Padding(8, 3, 0, 0) };
        var btnAdd = WinStyles.CriarBotao("+");
        btnAdd.Margin = new Padding(8, 0, 0, 0);
        btnAdd.Click += (_, _) => AdicionarItem();
        pnlProd.Controls.AddRange([_cmbProduto, _txtQtd, btnAdd]);
        corpo.Controls.Add(pnlProd);

        _gridItens = WinStyles.CriarGrid();
        _gridItens.Size = new Size(450, 100);
        _gridItens.Margin = new Padding(0, 12, 0, 0);
        _gridItens.Columns.AddRange([
            new DataGridViewTextBoxColumn { HeaderText = "Produto", Name = "Produto", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = "Qtd", Name = "Qtd", Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Valor", Name = "Valor", Width = 120 }
        ]);
        corpo.Controls.Add(_gridItens);

        _lblTotal = new Label { Text = "Total: R$ 850,00", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
        corpo.Controls.Add(_lblTotal);

        _dtpEntrega = new DateTimePicker { Width = 200 };
        WinStyles.AdicionarCampo(corpo, "Data de Entrega", _dtpEntrega);

        var pnlBotoes = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 16, 0, 0) };
        var btnConfirmar = WinStyles.CriarBotao("Confirmar Pedido", true);
        btnConfirmar.Click += BtnConfirmar_Click;
        var btnCancelar = WinStyles.CriarBotao("Cancelar");
        btnCancelar.Margin = new Padding(8, 0, 0, 0);
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBotoes.Controls.AddRange([btnConfirmar, btnCancelar]);
        corpo.Controls.Add(pnlBotoes);

        Controls.Add(corpo);
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
