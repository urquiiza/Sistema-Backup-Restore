using Backup_Restore.Services;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace Backup_Restore.Views
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 1 && e.Args[0] == "/manutencao")
            {
                    AgendaManutencao agendaManutencao = new AgendaManutencao();
                    agendaManutencao.Executar(e.Args[1], e.Args[2], e.Args[3], e.Args[4], e.Args[5], e.Args[6], e.Args[7]);
                    App.Current.Shutdown();
            }
            else
            {
                MainWindow telaPrincipal = new MainWindow();
                telaPrincipal.Show();
            }
        }
    }
}