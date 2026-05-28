using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Backup_Restore
{
    public class AgendaManutencao
    {
        public void Executar(string senha)
        {
            string pastaArquivo = ".hos\\config";
            string pastaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string arquivoConfig = System.IO.Path.Combine(pastaUsuario, pastaArquivo);

            if (System.IO.File.Exists(arquivoConfig))
            {
                string textoArquivo = System.IO.File.ReadAllText(arquivoConfig);
                ConfigAutomatica configCarregada = System.Text.Json.JsonSerializer.Deserialize<ConfigAutomatica>(textoArquivo);

                string versaoBancoFbAut = configCarregada.Configuracoes.BANCO_DADOS_VERSAO.Split('.')[0];
                string extBancoAut = System.IO.Path.GetExtension(configCarregada.Configuracoes.NOME_BANCODADOS).ToLower();
                string caminhoBancoAut = System.IO.Path.Combine(configCarregada.Configuracoes.BANCO_REMOTO, configCarregada.Configuracoes.NOME_BANCODADOS);
                string bancoPostgresAut = configCarregada.Configuracoes.NOME_BANCODADOS;

                using (TaskService ts = new TaskService())
                {

                }
            }
        }
    }
}