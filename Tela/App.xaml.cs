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
        private static Mutex _mutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            const string nomeAplicativo = "BackupRestore_InstanciaUnica";
            bool criadoNovo;

            _mutex = new Mutex(true, nomeAplicativo, out criadoNovo);

            if (e.Args.Length > 1 && e.Args[0] == "/manutencao")
            {
                    AgendaManutencao agendaManutencao = new AgendaManutencao();
                    agendaManutencao.Executar(e.Args[1], e.Args[2], e.Args[3], e.Args[4], e.Args[5], e.Args[6], e.Args[7]);
                    App.Current.Shutdown();
            }
            else
            {
                if (!criadoNovo)
                {
                    System.Windows.MessageBox.Show("O aplicativo já está em execução!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    App.Current.Shutdown();
                    return;
                }
                else
                {
                    MainWindow telaPrincipal = new MainWindow();
                    telaPrincipal.Show();
                }
                base.OnStartup(e);
            }
        }
    }
}