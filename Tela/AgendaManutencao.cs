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
        public void Executar(string banco, string acao, string senha, string versao = null, string nomeBanco = null, string caminhoOrigem = null, string caminhoDestino = null)
        {
            if (banco == "2")
            {
                string pastaBin = System.IO.Path.Combine(versao, "bin");
                var listaComandos = ComandosPostgres.ManutencaoPostgres(pastaBin, nomeBanco, senha);

                string pastaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string arquivoLog = System.IO.Path.Combine(pastaUsuario, ".hos", "historico_manutencao.txt");

                try
                {
                    foreach (var comando in listaComandos)
                    {
                        using (System.Diagnostics.Process processos = new System.Diagnostics.Process())
                        {
                            comando.RedirectStandardOutput = true;
                            comando.RedirectStandardError = true;
                            comando.UseShellExecute = false;

                            string mensagemErro = "";

                            processos.StartInfo = comando;

                            processos.OutputDataReceived += (s, args) => { };
                            processos.ErrorDataReceived += (s, args) => 
                            { 
                                if (!string.IsNullOrEmpty(args.Data))
                                {
                                    mensagemErro += args.Data + "\n";
                                }
                            };

                            processos.Start();
                            processos.BeginOutputReadLine();
                            processos.BeginErrorReadLine();
                            processos.WaitForExit();
                            if (processos.ExitCode != 0)
                            {
                                throw new Exception($"{mensagemErro}");
                            }
                        }
                    }
                    string mensagemSucesso = $"{DateTime.Now} Manutenção do banco '{nomeBanco}' concluída com sucesso.\n";
                    System.IO.File.AppendAllText(arquivoLog, mensagemSucesso);
                    return;
                }
                catch (Exception ex)
                {
                    string mensagemFalha = $"\n{DateTime.Now} ERRO na manutenção do banco '{nomeBanco}'.\n\n{ex}\n";
                    System.IO.File.AppendAllText(arquivoLog, mensagemFalha);
                    System.Windows.MessageBox.Show($"{DateTime.Now} ERRO: Manutenção automática falhou.\nVerifique os logs de erros para mais informações.\n\nCaminho: {arquivoLog}");
                    return;
                }
            }
            else
            {
                if (banco == "0" || banco == "1")
                {
                    string pastaFb = banco == "0" ? @"C:\Program Files\Firebird\Firebird_2_5\bin" : @"C:\Program Files\Firebird\Firebird_4_0";
                    var listaComandos = ComandosFirebird.ManutencaoFirebird(pastaFb, caminhoOrigem, caminhoDestino, senha);
                    string pastaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string arquivoLog = System.IO.Path.Combine(pastaUsuario, ".hos", "historico_manutencao.txt");

                    try
                    {
                        foreach (var comando in listaComandos)
                        {
                            using (System.Diagnostics.Process processos = new System.Diagnostics.Process())
                            {
                                comando.RedirectStandardOutput = true;
                                comando.RedirectStandardError = true;
                                comando.UseShellExecute = false;

                                string mensagemErro = "";

                                processos.StartInfo = comando;

                                processos.OutputDataReceived += (s, args) => { };
                                processos.ErrorDataReceived += (s, args) =>
                                {
                                    if (!string.IsNullOrEmpty(args.Data))
                                    {
                                        mensagemErro += args.Data + "\n";
                                    }
                                };

                                processos.Start();
                                processos.BeginOutputReadLine();
                                processos.BeginErrorReadLine();

                                processos.WaitForExit();
                                if (processos.ExitCode != 0)
                                {
                                    throw new Exception("Código Exitado.");
                                }
                            }
                        }
                        string mensagemSucesso = $"{DateTime.Now} Manutenção do banco '{nomeBanco}' concluída com sucesso.\n";
                        System.IO.File.AppendAllText(arquivoLog, mensagemSucesso);
                        return;
                    }
                    catch (Exception ex)
                    {
                        string mensagemFalha = $"{DateTime.Now} ERRO na manutenção do banco '{nomeBanco}'.\n{ex}\n";
                        System.IO.File.AppendAllText(arquivoLog, mensagemFalha);
                        System.Windows.MessageBox.Show($"{DateTime.Now} ERRO: Manutenção automática falhou.\nVerifique os logs de erros para mais informações.\n\nCaminho: {arquivoLog}");
                        return;
                    }
                }
            }
        }
    }
}