using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup_Restore
{
    public class ConfiguradorProcesso
    {
        /// <summary>
        /// Cria a base do processo em segundo plano e injeta a senha se necessário.
        /// </summary>
     
        public static ProcessStartInfo CriarBase(string caminhoexecutavel, string argumentos, string variavelSenha = null, string senha = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = caminhoexecutavel,
                Arguments = argumentos,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            if (!string.IsNullOrEmpty(variavelSenha) && !string.IsNullOrEmpty(senha))
            {
                psi.EnvironmentVariables[variavelSenha] = senha;
            } 

            return psi;
        }
    }
}
