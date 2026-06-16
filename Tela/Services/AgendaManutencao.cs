using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Backup_Restore.Services
{
    public class AgendaManutencao
    {
        public void Executar(string banco, string acao, string senha, string versao = null, string nomeBanco = null, string caminhoOrigem = null, string caminhoDestino = null)
        {
            string pastaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string arquivoLog = Path.Combine(pastaUsuario, ".hos", "manutencao.txt");

            if (banco == "2")
            {
                string pastaBin = Path.Combine(versao, "bin");
                var listaComandos = ComandosPostgres.ManutencaoPostgres(pastaBin, nomeBanco, senha);

                try
                {
                    foreach (var comando in listaComandos)
                    {
                        using (Process processos = new Process())
                        {
                            comando.RedirectStandardOutput = true;
                            comando.RedirectStandardError = true;
                            comando.UseShellExecute = false;

                            string mensagemErro = "";

                            processos.StartInfo = comando;

                            processos.OutputDataReceived += (s, args) => { };
                            processos.ErrorDataReceived += (s, args) => { if (!string.IsNullOrEmpty(args.Data)) {mensagemErro += args.Data + "\n";}};

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
                    File.AppendAllText(arquivoLog, mensagemSucesso);
                    return;
                }
                catch (Exception ex)
                {
                    string mensagemFalha = $"\n{DateTime.Now} ERRO na manutenção do banco '{nomeBanco}'.\n\n{ex}\n";
                    File.AppendAllText(arquivoLog, mensagemFalha);
                    System.Windows.MessageBox.Show($"{DateTime.Now} ERRO: Manutenção automática falhou.\nVerifique os logs de erros para mais informações.\n\nCaminho: {arquivoLog}");
                    return;
                }
            }
            else
            {
                if (banco == "0" || banco == "1")
                {
                    string pastaFb = banco == "0" ? @"C:\Program Files\Firebird\Firebird_2_5\bin" : @"C:\Program Files\Firebird\Firebird_4_0";
                    string copiaBancoManutencao = Path.Combine(Path.GetDirectoryName(caminhoOrigem), Path.GetFileNameWithoutExtension(caminhoOrigem) + "-Copia.FDB");
                    string copiaBancoGuardar = Path.Combine(@"C:\MANUTENÇÃO", DateTime.Now.ToString("dd.MM.yyyy") + Path.GetFileName(caminhoOrigem));

                    StatusFirebird.DesativaFirebird();
                    File.Copy(caminhoOrigem, copiaBancoManutencao, true);
                    File.Copy(caminhoOrigem, copiaBancoGuardar, true);
                    StatusFirebird.AtivaFirebird();

                    var listaComandos = ComandosFirebird.ManutencaoFirebird(pastaFb, caminhoOrigem, caminhoDestino, senha);

                    List<string> arquivoExclusao = new List<string>();

                    try
                    {
                        foreach (var comando in listaComandos)
                        {
                            using (Process processos = new Process())
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
                                    throw new Exception(mensagemErro);
                                }

                                if (comando.Arguments.Contains("-b"))
                                {
                                    string arquivoFbk = Path.Combine(@"C:\MANUTENÇÃO", DateTime.Now.ToString("dd.MM.yyyy") + Path.GetFileNameWithoutExtension(caminhoOrigem) + ".FBK");
                                    string arquivoZip = Path.Combine(@"C:\MANUTENÇÃO", DateTime.Now.ToString("dd.MM.yyyy") + Path.GetFileNameWithoutExtension(caminhoOrigem) + ".zip");

                                    if (File.Exists(copiaBancoGuardar))
                                    {
                                        using (FileStream zipParaCriar = new FileStream(arquivoZip, FileMode.Create))
                                        {
                                            using (ZipArchive zip = new ZipArchive(zipParaCriar, ZipArchiveMode.Create))
                                            {
                                                zip.CreateEntryFromFile(copiaBancoGuardar, Path.GetFileName(copiaBancoGuardar));
                                            }
                                        }
                                        arquivoExclusao.Add(copiaBancoGuardar);
                                        arquivoExclusao.Add(arquivoFbk);
                                    }
                                    else
                                    {
                                        throw new FileNotFoundException($"Arquivo de backup não encontrado: {arquivoFbk}");
                                    }
                                }

                            }
                        }

                        foreach (string arquivo in arquivoExclusao)
                        {
                            if (File.Exists(arquivo))
                            {
                                File.Delete(arquivo);
                            }
                        }
                        StatusFirebird.DesativaFirebird();
                        File.Move(copiaBancoManutencao, caminhoOrigem, true);
                        StatusFirebird.AtivaFirebird();

                        string mensagemSucesso = $"{DateTime.Now} Manutenção do banco '{nomeBanco}' concluída com sucesso.\n";
                        File.AppendAllText(arquivoLog, mensagemSucesso);
                        return;
                    }
                    catch (Exception ex)
                    {
                        string mensagemFalha = $"{DateTime.Now} ERRO na manutenção do banco '{nomeBanco}'.\n{ex}\n";
                        File.AppendAllText(arquivoLog, mensagemFalha);
                        if (File.Exists(copiaBancoManutencao))
                        {
                            File.Delete(copiaBancoManutencao);
                        }
                        return;
                    }
                }
            }
        }
    }
}