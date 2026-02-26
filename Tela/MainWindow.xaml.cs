using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Backup_Restore;
using Microsoft.Win32;
using Tela.ViewModels;

namespace Tela
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process processoAtual;
        private bool canceladoPelousuario = false;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void btnBuscarOrigem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Arquivos FDB/FBK|*.FDB;*.FBK";
            var arquivoSelecao = dlg.ShowDialog();

            if (arquivoSelecao == true)
            {
                txtOrigemPath.Text = dlg.FileName;
            }
        }

        private void btnBuscarDestino_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dlg = new OpenFolderDialog();
            dlg.Title = "Selecione...";
            var arquivoSelecao = dlg.ShowDialog();

            if (arquivoSelecao == true)
            {
                txtDestinoPath.Text = dlg.FolderName;
            }
        }
        private async void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            if (btnIniciar.Content.ToString() == "Cancelar")
            {
                canceladoPelousuario = true;
                processoAtual.Kill();
                btnIniciar.Content = "Iniciar";
                btnSair.IsEnabled = true;
                return;
            }

            canceladoPelousuario = false;

            if (string.IsNullOrEmpty(txtOrigemPath.Text))
            {
                MessageBox.Show("Selecione um arquivo de banco de dados!");
                return;
            }
            if (string.IsNullOrEmpty(txtDestinoPath.Text))
            {
                MessageBox.Show("Selecione uma pasta destino!");
                return;
            }

            string caminhoGbak = "";

            if (rbFirebird25.IsChecked == true)
            {
                caminhoGbak = "C:\\Program Files\\Firebird\\Firebird_2_5\\bin\\gbak.exe";
            }
            else
            {
                caminhoGbak = "C:\\Program Files\\Firebird\\Firebird_4_0\\gbak.exe";
            }

            string origem = txtOrigemPath.Text;
            string destino = txtDestinoPath.Text;

            string argumentos = Comandos.Comando(origem, destino);

            var config = new ProcessStartInfo
            {
                FileName = caminhoGbak,
                Arguments = argumentos,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            try
            {
                JanelaCarregamento telaLoading = new JanelaCarregamento();
                processoAtual = Process.Start(config);
                
                btnIniciar.Content = "Cancelar";
                btnSair.IsEnabled = false;
                
                telaLoading.Show();
                await processoAtual.WaitForExitAsync();
                
                telaLoading.Close();
                btnSair.IsEnabled = true;

                if (canceladoPelousuario == true)
                {
                    return;
                }

                if (processoAtual.ExitCode != 0)
                {
                    btnIniciar.Content = "Iniciar";
                    MessageBox.Show("O GBAK falhou. Verifique se a versão selecionada corresponde a versão do banco!");
                    return;
                }
                btnIniciar.Content = "Iniciar";
                MessageBox.Show("Processo finalizado com sucesso!");
            }
            catch (Exception ex)
            {
                btnSair.IsEnabled = true;
                MessageBox.Show($"ERRO: nao foi possivel encontrar o arquivo GBAK. \n{ex.Message}");
                return;
            }
        }
        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void rbFirebird25_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbFirebird40_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}