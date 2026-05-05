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

            if (rbPostgres.IsChecked == true)
            {
                dlg.Filter = "Arquivos PostgreSQL (*.dump)|*.dump";
            }
            else
            {
                dlg.Filter = "Arquivos Firebird (*.fbk;*.fdb)|*.fbk;*.fdb|Todos os arquivos|*.*";
            }

            if (dlg.ShowDialog() == true)
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

            string caminhoExecutavel = "";
            string argumentosProcesso = "";

            string origem = txtOrigemPath.Text;
            string destino = txtDestinoPath.Text;

            if (rbFirebird25.IsChecked == true)
            {
                caminhoExecutavel = "C:\\Program Files\\Firebird\\Firebird_2_5\\bin\\gbak.exe";
                argumentosProcesso = Comandos.Comando(origem, destino);
            }
            else if (rbFirebird40.IsChecked == true)
            {
                caminhoExecutavel = "C:\\Program Files\\Firebird\\Firebird_4_0\\gbak.exe";
                argumentosProcesso = Comandos.Comando(origem, destino);
            }
            else if (rbPostgres.IsChecked == true)
            {
                string versao = txtVersao.Text;
                string nomeBanco = txtNomeBanco.Text;
                string senha = txtSenha.Text;

                caminhoExecutavel = $"{versao}\\bin\\pg_restore.exe";

                argumentosProcesso = Comandos.RestorePostgres(versao, nomeBanco, senha, origem);
            }

            var config = new ProcessStartInfo
            {
                FileName = caminhoExecutavel,
                Arguments = argumentosProcesso,
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
                    MessageBox.Show("O processo falhou. Verifique se a versão selecionada corresponde a versão do banco!");
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
        private void rbPostgres_Checked(object sender, RoutedEventArgs e)
        {
            lblVersao.Visibility = Visibility.Visible;
            txtVersao.Visibility = Visibility.Visible;
            btnBuscarVersao.Visibility = Visibility.Visible;

            lblSenha.Visibility = Visibility.Visible;   
            txtSenha.Visibility = Visibility.Visible;

            lblNomeBanco.Visibility = Visibility.Visible;
            txtNomeBanco.Visibility = Visibility.Visible;

            lblDestino.Visibility = Visibility.Collapsed;
            txtDestinoPath.Visibility = Visibility.Collapsed;
            btnBuscarDestino.Visibility = Visibility.Collapsed;
        }
        private void rbPostgres_Unchecked(object sender, RoutedEventArgs e)
        {
            lblVersao.Visibility = Visibility.Collapsed;
            txtVersao.Visibility = Visibility.Collapsed;
            btnBuscarVersao.Visibility = Visibility.Collapsed;

            lblSenha.Visibility = Visibility.Collapsed;
            txtSenha.Visibility = Visibility.Collapsed;

            lblNomeBanco.Visibility = Visibility.Collapsed;
            txtNomeBanco.Visibility = Visibility.Collapsed;

            lblDestino.Visibility = Visibility.Visible;
            txtDestinoPath.Visibility = Visibility.Visible;
            btnBuscarDestino.Visibility = Visibility.Visible;
        }
        private void btnBuscarVersao_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}