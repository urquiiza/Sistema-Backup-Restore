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
        private System.Windows.Forms.NotifyIcon iconeBandeja;
        private bool fechamentoSeguro = false;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();

            timerTerminal = new System.Windows.Threading.DispatcherTimer();
            timerTerminal.Interval = TimeSpan.FromMilliseconds(100); // 10 vezes por segundo
            timerTerminal.Tick += (s, args) =>
            {
                if (filaLogs.IsEmpty) return;

                StringBuilder sb = new StringBuilder();
                while (filaLogs.TryDequeue(out string linha))
                {
                    sb.AppendLine(linha);
                }
                txtTerminal.AppendText(sb.ToString());
                txtTerminal.ScrollToEnd();
            };

            iconeBandeja = new System.Windows.Forms.NotifyIcon();
            iconeBandeja.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            iconeBandeja.Text = "HOS - Backup e Restore";

            iconeBandeja.DoubleClick += (s, args) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                iconeBandeja.Visible = false;
            };
        }

        private System.Windows.Threading.DispatcherTimer timerTerminal;
        private System.Collections.Concurrent.ConcurrentQueue<string> filaLogs = new System.Collections.Concurrent.ConcurrentQueue<string>();

        private void btnBuscarOrigem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

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

        private void btnBuscarVersao_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Selecione o pg_restore.exe";
            dlg.Filter = "Executável do PostgreSQL (pg_restore.exe)|pg_restore.exe|Arquivos Executáveis (*.exe)|*.exe";

            if (dlg.ShowDialog() == true)
            {
                string caminhoCompleto = dlg.FileName;
                string pastaBin = System.IO.Path.GetDirectoryName(caminhoCompleto);
                string pastaVersao = System.IO.Path.GetDirectoryName(pastaBin);

                txtVersao.Text = pastaVersao;
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
                System.Windows.MessageBox.Show("Selecione um arquivo de banco de dados!");
                return;
            }
            if (string.IsNullOrEmpty(txtDestinoPath.Text) && rbPostgres.IsChecked == false)
            {
                System.Windows.MessageBox.Show("Selecione uma pasta destino!");
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
            else
            {
                string versao = txtVersao.Text;
                string nomeBanco = txtNomeBanco.Text;
                string senha = ObterSenhaAtual();

                caminhoExecutavel = $"{versao}\\bin\\pg_restore.exe";

                argumentosProcesso = Comandos.RestorePostgres(versao, nomeBanco, senha, origem);
            }

            var config = new ProcessStartInfo
            {
                FileName = caminhoExecutavel,
                Arguments = argumentosProcesso,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (rbPostgres.IsChecked == true)
            {
                config.EnvironmentVariables["PGPASSWORD"] = ObterSenhaAtual();
            }

            txtTerminal.Clear();

            try
            {
                processoAtual = new Process();
                processoAtual.StartInfo = config;

                filaLogs.Clear();
                timerTerminal.Start();

                processoAtual.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        filaLogs.Enqueue(e.Data);
                    }
                };

                processoAtual.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        filaLogs.Enqueue(e.Data);
                    }
                };

                processoAtual.Start();

                processoAtual.BeginOutputReadLine();
                processoAtual.BeginErrorReadLine();

                AbasMenu.SelectedIndex = 1;

                btnIniciar.Content = "Cancelar";
                btnSair.IsEnabled = false;

                await processoAtual.WaitForExitAsync();

                timerTerminal.Stop();

                StringBuilder resto = new StringBuilder();
                while (filaLogs.TryDequeue(out string linha))
                {
                    resto.Append(linha);
                }
                if (resto.Length > 0)
                {
                    txtTerminal.AppendText(resto.ToString());
                    txtTerminal.ScrollToEnd();
                }

                btnSair.IsEnabled = true;

                if (canceladoPelousuario == true)
                {
                    return;
                }

                if (processoAtual.ExitCode != 0)
                {
                    btnIniciar.Content = "Iniciar";
                    System.Windows.MessageBox.Show($"O processo falhou!\n\nVerifique o Terminal para ver o motivo exato do erro.");
                    return;
                }
                btnIniciar.Content = "Iniciar";
                System.Windows.MessageBox.Show("Processo finalizado com sucesso!");
            }
            catch (Exception ex)
            {
                btnSair.IsEnabled = true;
                System.Windows.MessageBox.Show($"ERRO ao executar o processo. \n{ex.Message}");
                return;
            }
        }
        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            fechamentoSeguro = true;
            System.Windows.Application.Current.Shutdown();
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

            lblNomeBanco.Visibility = Visibility.Visible;
            txtNomeBanco.Visibility = Visibility.Visible;

            lblSenha.Visibility = Visibility.Visible;
            panelSenha.Visibility = Visibility.Visible;

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
            panelSenha.Visibility = Visibility.Collapsed;

            lblNomeBanco.Visibility = Visibility.Collapsed;
            txtNomeBanco.Visibility = Visibility.Collapsed;

            lblDestino.Visibility = Visibility.Visible;
            txtDestinoPath.Visibility = Visibility.Visible;
            btnBuscarDestino.Visibility = Visibility.Visible;
        }
        private void btnVerSenha_Click(object sender, RoutedEventArgs e)
        {
            if (btnVerSenha.IsChecked == true)
            {
                txtSenhaVisivel.Text = pbSenha.Password;
                pbSenha.Visibility = Visibility.Collapsed;
                txtSenhaVisivel.Visibility = Visibility.Visible;
            }
            else
            {
                pbSenha.Password = txtSenhaVisivel.Text;
                txtSenhaVisivel.Visibility = Visibility.Collapsed;
                pbSenha.Visibility = Visibility.Visible;
            }
        }
        private string ObterSenhaAtual()
        {
            return pbSenha.Visibility == Visibility.Visible ? pbSenha.Password : txtSenhaVisivel.Text;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!fechamentoSeguro)
            {
                e.Cancel = true;
                this.Hide();                 
                iconeBandeja.Visible = true;

                iconeBandeja.ShowBalloonTip(2000, "Minimizado", "O sistema continua rodando em segundo plano. Clique duplo para abrir.", System.Windows.Forms.ToolTipIcon.Info);
            }
        }
    }
}