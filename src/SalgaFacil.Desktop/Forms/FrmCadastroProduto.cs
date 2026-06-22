using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Models;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Forms;

public class FrmCadastroProduto : Form
{
    private readonly Produto _produto;
    private readonly ProdutoService _service = new();
    private TextBox _txtNome = null!;
    private ComboBox _cmbCategoria = null!;
    private RadioButton _rbFrito = null!;
    private RadioButton _rbAssado = null!;
    private TextBox _txtPreco = null!;
    private TextBox _txtCusto = null!;
    private TextBox _txtDescricao = null!;
    private CheckBox _chkAtivo = null!;

    public FrmCadastroProduto(Produto? existente)
    {
        _produto = existente != null ? new Produto
        {
            Id = existente.Id, Nome = existente.Nome, Categoria = existente.Categoria,
            Tipo = existente.Tipo, PrecoVenda = existente.PrecoVenda, CustoEstimado = existente.CustoEstimado,
            Descricao = existente.Descricao, Status = existente.Status
        } : new Produto();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Cadastro de Produto";
        Size = new Size(500, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(245, 245, 245);
        Font = WinStyles.FontePadrao;

        int y = 16;
        Controls.Add(CriarLabel("Nome", 16, y)); y += 18;
        _txtNome = new TextBox { Location = new Point(16, y), Width = 450, Text = _produto.Nome }; Controls.Add(_txtNome); y += 32;

        Controls.Add(CriarLabel("Categoria", 16, y));
        _cmbCategoria = new ComboBox { Location = new Point(16, y + 18), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbCategoria.Items.AddRange(["Salgado", "Doce", "Bebida"]);
        _cmbCategoria.SelectedItem = _produto.Categoria;
        if (_cmbCategoria.SelectedIndex < 0) _cmbCategoria.SelectedIndex = 0;
        Controls.Add(_cmbCategoria);

        Controls.Add(CriarLabel("Tipo", 240, y));
        _rbFrito = new RadioButton { Text = "Frito", Location = new Point(240, y + 20), AutoSize = true, Checked = _produto.Tipo == TipoProduto.Frito };
        _rbAssado = new RadioButton { Text = "Assado", Location = new Point(320, y + 20), AutoSize = true, Checked = _produto.Tipo == TipoProduto.Assado };
        Controls.AddRange([_rbFrito, _rbAssado]); y += 52;

        Controls.Add(CriarLabel("Preço Venda", 16, y));
        Controls.Add(CriarLabel("Custo Estimado", 240, y)); y += 18;
        _txtPreco = new TextBox { Location = new Point(16, y), Width = 200, Text = _produto.PrecoVenda > 0 ? _produto.PrecoVenda.ToString("F2") : "" };
        _txtCusto = new TextBox { Location = new Point(240, y), Width = 200, Text = _produto.CustoEstimado > 0 ? _produto.CustoEstimado.ToString("F2") : "" };
        Controls.AddRange([_txtPreco, _txtCusto]); y += 32;

        Controls.Add(CriarLabel("Descrição", 16, y)); y += 18;
        _txtDescricao = new TextBox { Location = new Point(16, y), Width = 450, Height = 60, Multiline = true, Text = _produto.Descricao };
        Controls.Add(_txtDescricao); y += 72;

        _chkAtivo = new CheckBox { Text = "Status Ativo", Location = new Point(16, y), Checked = _produto.Status == StatusProduto.Ativo, AutoSize = true };
        Controls.Add(_chkAtivo); y += 36;

        var btnSalvar = WinStyles.CriarBotao("Salvar", true);
        btnSalvar.Location = new Point(280, y);
        btnSalvar.Click += BtnSalvar_Click;
        var btnCancelar = WinStyles.CriarBotao("Cancelar");
        btnCancelar.Location = new Point(370, y);
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.AddRange([btnSalvar, btnCancelar]);
    }

    private static Label CriarLabel(string t, int x, int y) => new() { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 8F) };

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtNome.Text)) { MessageBox.Show("Nome é obrigatório."); return; }
        if (_cmbCategoria.SelectedItem == null) { MessageBox.Show("Categoria é obrigatória."); return; }
        if (!decimal.TryParse(_txtPreco.Text, out var preco)) { MessageBox.Show("Preço de venda inválido."); return; }

        _produto.Nome = _txtNome.Text.Trim();
        _produto.Categoria = _cmbCategoria.SelectedItem!.ToString()!;
        _produto.Tipo = _rbFrito.Checked ? TipoProduto.Frito : TipoProduto.Assado;
        _produto.PrecoVenda = preco;
        decimal.TryParse(_txtCusto.Text, out var custo);
        _produto.CustoEstimado = custo;
        _produto.Descricao = _txtDescricao.Text;
        _produto.Status = _chkAtivo.Checked ? StatusProduto.Ativo : StatusProduto.Inativo;

        _service.Salvar(_produto);
        DialogResult = DialogResult.OK;
        Close();
    }
}
