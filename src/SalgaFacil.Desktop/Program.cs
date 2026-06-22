using SalgaFacil.Desktop.Forms;

namespace SalgaFacil.Desktop;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FrmLogin());
    }
}
