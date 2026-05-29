using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Backup_Restore
{
    public class AgendaManutencao
    {
        public void Executar(string banco, string acao, string versao, string nomeBanco, string senha)
        {
            string pastaBin = System.IO.Path.Combine(versao, "bin");
            var listaComandos = ComandosPostgres.ManutencaoPostgres(pastaBin, nomeBanco, senha);

            string pastaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string arquivoLog = System.IO.Path.Combine(pastaUsuario, ".hos", "historico_manutencao.txt");

            foreach (var comando in listaComandos)
            {
                try
                {
                    using (System.Diagnostics.Process processos = new System.Diagnostics.Process())
                    {
                        processos.StartInfo = comando;
                        processos.Start();
                        processos.WaitForExit();
                        if (processos.ExitCode != 0)
                        {
                            throw new Exception("Código Exitado.");
                        }
                        else
                        {
                            string mensagemSucesso = $"{DateTime.Now} Manutenção do banco '{nomeBanco}' concluída com sucesso.\n";
                            System.IO.File.AppendAllText(arquivoLog, mensagemSucesso);
                            System.Windows.MessageBox.Show($"{DateTime.Now} Manutenção automática concluída com sucesso!");
                        }
                    }
                }
                catch (Exception ex)
                { 
                    string mensagemFalha= $"{DateTime.Now} ERRO na manutenção do banco '{nomeBanco}'.\n{ex}\n";
                    System.IO.File.AppendAllText(arquivoLog, mensagemFalha);        
                    System.Windows.MessageBox.Show($"{DateTime.Now} ERRO: Manutenção automática falhou.\nVerifique os logs de erros para mais informações.\n\nCaminho: {arquivoLog}");
                }
            }
        }
    }
}