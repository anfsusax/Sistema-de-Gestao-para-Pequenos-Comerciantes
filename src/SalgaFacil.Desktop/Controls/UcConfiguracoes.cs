using SalgaFacil.Desktop.Helpers;
using SalgaFacil.Desktop.Services;

namespace SalgaFacil.Desktop.Controls;

public class UcConfiguracoes : UserControl
{
    private TextBox _txtNome = null!;
    private TextBox _txtCnpj = null!;
    private TextBox _txtTelefone = null!;

    public UcConfiguracoes()
    {
        BackColor = WinStyles.FundoGeral;
        Font = WinStyles.FontePadrao;
        Dock = DockStyle.Fill;
        BuildUi();
    }

    private void BuildUi()
    {
        var titulo = new Label { Text = "Configurações", Font = WinStyles.FonteTitulo, Dock = DockStyle.Top, Height = 32 };

        // Wrapper Dock=Top (altura automática) segurando o GroupBox com Anchor em vez de Dock:
        // isso evita que o card estique para a largura inteira da janela (bug da correção anterior,
        // onde Dock=Top no proprio GroupBox forçava Width = largura total do conteudo).
        var pnlGrpWrap = new Panel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

        var grp = new GroupBox
        {
            Text = "Dados da Empresa",
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            Width = 420,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(16, 20, 16, 16),
            BackColor = WinStyles.PainelBranco,
            FlatStyle = FlatStyle.Standard
        };

        // BUG CORRIGIDO: "corpo" estava com Dock=Fill + AutoSize=true ao mesmo tempo — combinação
        // contraditória (Dock=Fill trava o tamanho ao espaço do pai, e como o pai "grp" também é
        // AutoSize, cria uma dependência circular). Resultado: o botão "Salvar" podia ficar cortado
        // fora da área visível, sem erro. Trocado para Dock=Top: fixa a Largura (igual ao "grp") mas
        // deixa a Altura livre para o AutoSize calcular a partir do conteúdo real.
        var corpo = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        // Largura explícita nos 3 campos: WinStyles.AdicionarCampo só aplica a largura padrão (320)
        // quando Width <= 0, e o valor padrão de um TextBox novo é 100 (nunca <= 0) — sem isso,
        // os campos ficavam com 100px e cortavam o texto (bug visto no print "Salgados ç").
        _txtNome = new TextBox { Text = DataStore.Empresa.Nome, Width = 320 };
        _txtCnpj = new TextBox { Text = DataStore.Empresa.Cnpj, Width = 320 };
        _txtTelefone = new TextBox { Text = DataStore.Empresa.Telefone, Width = 320 };

        WinStyles.AdicionarCampo(corpo, "Nome da Empresa", _txtNome);
        WinStyles.AdicionarCampo(corpo, "CNPJ", _txtCnpj);
        WinStyles.AdicionarCampo(corpo, "Telefone", _txtTelefone);

        var btnSalvar = WinStyles.CriarBotao("Salvar", true);
        btnSalvar.Margin = new Padding(0, 14, 0, 0);
        btnSalvar.Click += (_, _) =>
        {
            DataStore.Empresa.Nome = _txtNome.Text;
            DataStore.Empresa.Cnpj = _txtCnpj.Text;
            DataStore.Empresa.Telefone = _txtTelefone.Text;
            MessageBox.Show("Configurações salvas com sucesso.", "SalgaPro");
        };
        corpo.Controls.Add(btnSalvar);

        grp.Controls.Add(corpo);
        pnlGrpWrap.Controls.Add(grp);

        Controls.Add(pnlGrpWrap);
        Controls.Add(titulo);
    }
}
