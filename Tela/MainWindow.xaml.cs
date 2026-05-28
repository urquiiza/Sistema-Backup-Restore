using Backup_Restore;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
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
using Microsoft.Win32.TaskScheduler;

namespace Backup_Restore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string pastaArquivo = ".hos\\config";
            string pastaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string arquivoConfig = System.IO.Path.Combine(pastaUsuario, pastaArquivo);
            if (System.IO.File.Exists(arquivoConfig))
            {
                string textoArquivo = System.IO.File.ReadAllText(arquivoConfig);
                ConfigAutomatica configCarregada = System.Text.Json.JsonSerializer.Deserialize<ConfigAutomatica>(textoArquivo);

                string versaoBancoFb = configCarregada.Configuracoes.BANCO_DADOS_VERSAO.Split('.')[0];
                string extBanco = System.IO.Path.GetExtension(configCarregada.Configuracoes.NOME_BANCODADOS).ToLower();
                string caminhoBancoAut = System.IO.Path.Combine(configCarregada.Configuracoes.BANCO_REMOTO, configCarregada.Configuracoes.NOME_BANCODADOS);
                string bancoPostgres = configCarregada.Configuracoes.NOME_BANCODADOS;

                if (versaoBancoFb == "2")
                {
                    cmbBanco.SelectedIndex = 0;
                    txtOrigemPath.Text = caminhoBancoAut;
                }
                else if (versaoBancoFb == "4")
                {
                    cmbBanco.SelectedIndex = 1;
                    txtOrigemPath.Text = caminhoBancoAut;
                }
                else
                {
                    cmbBanco.SelectedIndex = 2;
                    txtNomeBanco.Text = bancoPostgres;
                }
                if (extBanco == ".fdb")
                {
                    cmbAcao.SelectedIndex = 0;
                }
                else if (extBanco == ".fbk")
                {
                    cmbAcao.SelectedIndex = 1;
                }
                else { cmbAcao.SelectedIndex = -1; }
            }
            cmbHora.ItemsSource = Enumerable.Range(0, 24).Select(h => h.ToString("D2")).ToList();
            cmbMinuto.ItemsSource = Enumerable.Range(0, 60).Select(m => m.ToString("D2")).ToList();
        }
        private Process processoAtual;
        private bool canceladoPelousuario = false;
        private System.Windows.Forms.NotifyIcon iconeBandeja;
        private bool fechamentoSeguro = false;
        public MainWindow()
        {
            InitializeComponent();

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
            iconeBandeja.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath);
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

            if (cmbBanco.SelectedIndex == 2)
            {
                dlg.Filter = "Arquivos PostgreSQL (*.dump)|*.dump";
            }
            else
            {
                dlg.Filter = "Arquivos Firebird (*.fbk;*.fdb)|*.fbk;*.fdb";
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
            Microsoft.Win32.OpenFolderDialog dlg = new Microsoft.Win32.OpenFolderDialog();

            dlg.DefaultDirectory = "C:\\Program Files\\";
            dlg.FolderName = dlg.DefaultDirectory + "PostgreSQL";

            if (dlg.ShowDialog() == true)
            {
                txtVersao.Text = dlg.FolderName;
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

            if (cmbBanco.SelectedIndex == -1 || cmbAcao.SelectedIndex == -1)
            {
                System.Windows.MessageBox.Show("Selecione o banco de dados e a acao desejada!");
                return;
            }
            if (cmbBanco.SelectedIndex >= 0)
            {
                if (string.IsNullOrEmpty(txtOrigemPath.Text))
                {
                    System.Windows.MessageBox.Show("Selecione o banco origem!");
                    return;
                }
                else if (string.IsNullOrEmpty(txtDestinoPath.Text) && cmbAcao.SelectedIndex != 2)
                {
                    System.Windows.MessageBox.Show("Selecione o destino!");
                    return;
                }
                if (cmbBanco.SelectedIndex == 2)
                {
                    if (string.IsNullOrEmpty(txtVersao.Text))
                    {
                        System.Windows.MessageBox.Show("Selecione a versão PostgresSQL!");
                        return;
                    }
                    if (string.IsNullOrEmpty(txtNomeBanco.Text))
                    {
                        System.Windows.MessageBox.Show("Informe o nome do banco!");
                        return;
                    }
                    if (string.IsNullOrEmpty(ObterSenhaAtual()))
                    {
                        System.Windows.MessageBox.Show("Informe a senha para prosseguir!");
                        return;
                    }
                }
            }

            canceladoPelousuario = false;
            string origem = txtOrigemPath.Text;
            string destino = txtDestinoPath.Text;
            string senha = ObterSenhaAtual();

            int indexBanco = cmbBanco.SelectedIndex;
            int indexAcao = cmbAcao.SelectedIndex;

            List<ProcessStartInfo> executarProcesso = new List<ProcessStartInfo>();

            if (indexBanco == 0 || indexBanco == 1)
            {
                string pastaFb = indexBanco == 0 ? @"C:\Program Files\Firebird\Firebird_2_5\bin" : @"C:\Program Files\Firebird\Firebird_4_0";

                if (indexAcao == 0) executarProcesso.Add(ComandosFirebird.BackupFirebird(pastaFb, origem, destino, senha));
                else executarProcesso.Add(ComandosFirebird.RestoreFirebird(pastaFb, origem, destino, senha));
            }
            else
            {
                string pastaPg = System.IO.Path.Combine(txtVersao.Text, "bin");
                string nomeBanco = txtNomeBanco.Text;

                if (indexAcao == 0) executarProcesso.Add(ComandosPostgres.BackupPostgres(pastaPg, nomeBanco, destino, senha));
                else if (indexAcao == 1) executarProcesso.AddRange(ComandosPostgres.RestorePostgres(pastaPg, nomeBanco, origem, senha));
                else executarProcesso.AddRange(ComandosPostgres.ManutencaoPostgres(pastaPg, nomeBanco, senha));
            }

            txtTerminal.Clear();
            AbasMenu.SelectedIndex = 1;
            btnIniciar.Content = "Cancelar";
            btnSair.IsEnabled = false;
            btnSair.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            filaLogs.Clear();
            timerTerminal.Start();

            try
            {
                foreach (var config in executarProcesso)
                {
                    processoAtual = new Process();
                    processoAtual.StartInfo = config;

                    processoAtual.OutputDataReceived += (s, args) => { if (!string.IsNullOrEmpty(args.Data)) filaLogs.Enqueue(args.Data); };
                    processoAtual.ErrorDataReceived += (s, args) => { if (!string.IsNullOrEmpty(args.Data)) filaLogs.Enqueue(args.Data); };

                    processoAtual.Start();
                    processoAtual.BeginOutputReadLine();
                    processoAtual.BeginErrorReadLine();

                    await processoAtual.WaitForExitAsync();

                    if (processoAtual.ExitCode != 0 && !canceladoPelousuario && System.IO.Path.GetFileName(config.FileName) != "psql.exe")
                    {
                        btnIniciar.Content = "Iniciar";
                        btnSair.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#315C85");
                        throw new Exception("O comando falhou. Verifique o terminal para mais detalhes.");
                    }
                    if (canceladoPelousuario == true)
                    {
                        btnIniciar.Content = "Iniciar";
                        btnSair.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#315C85");
                        System.Windows.MessageBox.Show("Operacao cancelada!");
                        return;
                    }
                }

                timerTerminal.Stop();

                btnIniciar.Content = "Iniciar";
                btnSair.IsEnabled = true;
                btnSair.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#315C85");
                System.Windows.MessageBox.Show("Processo finalizado com sucesso!");
            }
            catch (Exception ex)
            {
                timerTerminal.Stop();
                btnIniciar.Content = "Iniciar";
                btnSair.IsEnabled = true;
                btnSair.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#315C85");
                System.Windows.MessageBox.Show($"ERRO:\n{ex.Message}");
            }
        }
        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            fechamentoSeguro = true;
            System.Windows.Application.Current.Shutdown();
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
        private void cmbBanco_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAcao == null || lblAcao == null) return;

            lblAcao.Visibility = Visibility.Visible;
            cmbAcao.Visibility = Visibility.Visible;

            if (cmbBanco.SelectedIndex == 2)
            {
                cbiManutencao.Visibility = Visibility.Visible;
            }
            else
            {
                cbiManutencao.Visibility = Visibility.Collapsed;

                if (cmbAcao.SelectedIndex == 2) cmbAcao.SelectedIndex = 0;
            }

            AtualizarVisibilidadeCampos();
        }
        private void cmbAcao_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AtualizarVisibilidadeCampos();
        }
        private void AtualizarVisibilidadeCampos()
        {
            lblDestino.Text = "Pasta de Destino para Salvar:";
            lblDestino.Visibility = Visibility.Visible;
            txtDestinoPath.Visibility = Visibility.Visible;
            btnBuscarDestino.Visibility = Visibility.Visible;

            if (cmbBanco == null || cmbAcao == null) return;

            lblVersao.Visibility = Visibility.Collapsed;
            txtVersao.Visibility = Visibility.Collapsed;
            btnBuscarVersao.Visibility = Visibility.Collapsed;

            lblNomeBanco.Visibility = Visibility.Collapsed;
            txtNomeBanco.Visibility = Visibility.Collapsed;

            lblSenha.Visibility = Visibility.Collapsed;
            panelSenha.Visibility = Visibility.Collapsed;

            if (cmbBanco.SelectedIndex == 2)
            {
                lblVersao.Visibility = Visibility.Visible;
                txtVersao.Visibility = Visibility.Visible;
                btnBuscarVersao.Visibility = Visibility.Visible;

                lblNomeBanco.Visibility = Visibility.Visible;
                txtNomeBanco.Visibility = Visibility.Visible;

                lblSenha.Visibility = Visibility.Visible;
                panelSenha.Visibility = Visibility.Visible;
            }
            if (cmbAcao.SelectedIndex == 2)
            {
                lblDestino.Visibility = Visibility.Collapsed;
                txtDestinoPath.Visibility = Visibility.Collapsed;
                btnBuscarDestino.Visibility = Visibility.Collapsed;

                lblOrigem.Visibility = Visibility.Collapsed;
                txtOrigemPath.Visibility = Visibility.Collapsed;
                btnBuscarOrigem.Visibility = Visibility.Collapsed;
            }
        }
        public (DateTime? data, int? hora, int? minuto) ObterDataHoraAgendamento()
        {
            DateTime? dataSelecionada = dtpAgendaManutencao.SelectedDate;
            int? horaSelecionada = cmbHora.SelectedItem != null ? int.Parse(cmbHora.SelectedItem.ToString()) : null;
            int? minutoSelecionado = cmbMinuto.SelectedItem != null ? int.Parse(cmbMinuto.SelectedItem.ToString()) : null;
            return (dataSelecionada, horaSelecionada, minutoSelecionado);
        }

        public DateTime? DataHoraAgendada()
        {
            var escolha = ObterDataHoraAgendamento();
            if (!escolha.data.HasValue || !escolha.hora.HasValue || !escolha.minuto.HasValue)
            {
                return null;
            }
            return new DateTime(
                escolha.data.Value.Year,
                escolha.data.Value.Month,
                escolha.data.Value.Day,
                escolha.hora.Value,
                escolha.minuto.Value,
                0);
        }
        private void btnAgendar_Click(object sender, RoutedEventArgs e)
        {
            DateTime? agendaTarefa = DataHoraAgendada();
            string senhaInformada = ObterSenhaAtual();

            if (agendaTarefa != null)
            {
                if (agendaTarefa < DateTime.Now)
                {
                    System.Windows.MessageBox.Show("Informe uma data e hora superior a atual.");
                    return;
                }
                if (cmbBanco.SelectedIndex != 2 || cmbAcao.SelectedIndex != 2)
                {
                    System.Windows.MessageBox.Show("Somente é possível agendar manutenção para PostgresSQL. Ação e banco alterados para manutenção e postgreSQL!\n\nManutenção para bancos Firebird em desenvolvimento.");
                    cmbBanco.SelectedIndex = 2;
                    cmbAcao.SelectedIndex = 2;
                    AbasMenu.SelectedIndex = 0;
                    return;
                }
                if (string.IsNullOrEmpty(txtVersao.Text))
                {
                    System.Windows.MessageBox.Show("Selecione a versão PostgresSQL na aba 'Configurações'!");
                    AbasMenu.SelectedIndex = 0;
                    return;
                }
                if (string.IsNullOrEmpty(txtNomeBanco.Text))
                {
                    System.Windows.MessageBox.Show("Informe o nome do banco na aba 'Configurações'!");
                    AbasMenu.SelectedIndex = 0;
                    return;
                }
                if (string.IsNullOrEmpty(senhaInformada))
                {
                    System.Windows.MessageBox.Show("Informe a senha do banco na aba 'Configurações'.");
                    AbasMenu.SelectedIndex = 0;
                    return;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Data, hora ou minuto não foram preenchidos corretamente.");
                return;
            }
            string caminhoexe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string argumentos = $"/manutencao {cmbBanco.SelectedIndex} {cmbAcao.SelectedIndex} \"{txtVersao.Text}\" {txtNomeBanco.Text} {senhaInformada}";
            ExecAction executaAcao = new ExecAction(caminhoexe, argumentos);
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Manutenção Automática de Banco de Dados HOS.";

                    DailyTrigger dailyTrigger = new DailyTrigger();

                    dailyTrigger.StartBoundary = (DateTime)agendaTarefa;

                    td.Triggers.Add(dailyTrigger);
                    td.Actions.Add(executaAcao);

                    ts.RootFolder.RegisterTaskDefinition("ManutencaoHOS", td);

                    System.Windows.MessageBox.Show("Agendamento criado com sucesso!");

                    dtpAgendaManutencao.SelectedDate = null;
                    cmbHora.SelectedIndex = -1;
                    cmbMinuto.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao criar o agendamento: \n{ex.Message}");
            }
        }
        private void btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            dtpAgendaManutencao.SelectedDate = null;
            cmbHora.SelectedIndex = -1;
            cmbMinuto.SelectedIndex = -1;
        }
    }
}