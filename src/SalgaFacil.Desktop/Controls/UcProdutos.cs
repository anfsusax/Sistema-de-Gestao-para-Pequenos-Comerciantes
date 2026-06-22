using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Forms;
using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Models;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Controls;

public class UcProdutos : UserControl
{
    private DataGridView _grid = null!;
    private ComboBox _cmbTipo = null!;
    private CheckBox _chkAtivos = null!;
    private readonly ProdutoService _service = new();

    public UcProdutos()
    {
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;
        Dock = DockStyle.Fill;
        BuildUi();
        Carregar();
    }

    private void BuildUi()
    {
        var pnlTopo = new Panel { Dock = DockStyle.Top, Height = 36 };
        var titulo = new Label { Text = "Gestão de Produtos", Font = WinStyles.FonteTitulo, AutoSize = true, Location = new Point(0, 4) };

        var btnNovo = WinStyles.CriarBotao("Novo Produto", true);
        btnNovo.Location = new Point(700, 4);
        btnNovo.Click += (_, _) => AbrirModal(null);

        var btnEditar = WinStyles.CriarBotao("Editar");
        btnEditar.Location = new Point(820, 4);
        btnEditar.Click += (_, _) => EditarSelecionado();

        var btnExcluir = WinStyles.CriarBotao("Excluir");
        btnExcluir.Location = new Point(900, 4);
        btnExcluir.Click += (_, _) => ExcluirSelecionado();

        pnlTopo.Controls.AddRange([titulo, btnNovo, btnEditar, btnExcluir]);

        var pnlFiltro = new Panel { Dock = DockStyle.Top, Height = 32 };
        pnlFiltro.Controls.Add(new Label { Text = "Tipo:", AutoSize = true, Location = new Point(0, 8) });
        _cmbTipo = new ComboBox { Location = new Point(40, 4), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbTipo.Items.AddRange(["Todos", "Fritos", "Assados"]);
        _cmbTipo.SelectedIndex = 0;
        _cmbTipo.SelectedIndexChanged += (_, _) => Carregar();
        _chkAtivos = new CheckBox { Text = "Apenas ativos", Location = new Point(160, 6), Checked = true, AutoSize = true };
        _chkAtivos.CheckedChanged += (_, _) => Carregar();
        pnlFiltro.Controls.AddRange([_cmbTipo, _chkAtivos]);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = WinStyles.PainelBranco, Padding = new Padding(0), Margin = new Padding(0, 8, 0, 0) };
        pnlGrid.BorderStyle = BorderStyle.FixedSingle;

        _grid = WinStyles.CriarGrid();
        _grid.Columns.AddRange([
            new DataGridViewTextBoxColumn { HeaderText = "Nome", Name = "Nome" },
            new DataGridViewTextBoxColumn { HeaderText = "Categoria", Name = "Categoria" },
            new DataGridViewTextBoxColumn { HeaderText = "Tipo", Name = "Tipo" },
            new DataGridViewTextBoxColumn { HeaderText = "Preço", Name = "Preco" },
            new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status" },
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false }
        ]);
        _grid.CellFormatting += (_, e) =>
        {
            if (_grid.Columns[e.ColumnIndex].Name == "Status" && e.RowIndex >= 0)
            {
                var ativo = e.Value?.ToString() == "Ativo";
                e.CellStyle.BackColor = ativo ? Color.FromArgb(209, 250, 229) : Color.FromArgb(243, 244, 246);
                e.CellStyle.ForeColor = ativo ? Color.FromArgb(6, 95, 70) : Color.FromArgb(75, 85, 99);
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        };
        pnlGrid.Controls.Add(_grid);

        Controls.Add(pnlGrid);
        Controls.Add(pnlFiltro);
        Controls.Add(pnlTopo);
    }

    private void Carregar()
    {
        var tipo = _cmbTipo.SelectedItem?.ToString();
        if (tipo == "Todos") tipo = null;
        _grid.Rows.Clear();
        foreach (var p in _service.Listar(tipo, _chkAtivos.Checked))
            _grid.Rows.Add(p.Nome, p.Categoria, p.Tipo.ToString(), p.PrecoVenda.ToString("C", new System.Globalization.CultureInfo("pt-BR")), p.Status == StatusProduto.Ativo ? "Ativo" : "Inativo", p.Id);
    }

    private int? IdSelecionado() => _grid.CurrentRow?.Cells["Id"].Value is int id ? id : null;

    private void AbrirModal(Produto? produto)
    {
        using var frm = new FrmCadastroProduto(produto);
        if (frm.ShowDialog(FindForm()) == DialogResult.OK) Carregar();
    }

    private void EditarSelecionado()
    {
        var id = IdSelecionado();
        if (!id.HasValue) { MessageBox.Show("Selecione um produto.", "Aviso"); return; }
        AbrirModal(_service.Obter(id.Value));
    }

    private void ExcluirSelecionado()
    {
        var id = IdSelecionado();
        if (!id.HasValue) { MessageBox.Show("Selecione um produto.", "Aviso"); return; }
        if (MessageBox.Show("Deseja excluir o produto selecionado?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            _service.Excluir(id.Value);
            Carregar();
        }
    }
}
