using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup_Restore
{
    internal class ComandosFirebird
    {
        private const string VARIAVEL_SENHA = "ISC_PASSWORD";
        public static ProcessStartInfo BackupFirebird(string pastaExecutavel, string caminhoOrigem, string caminhoDestino, string senha)
        {
            string executavel = Path.Combine(pastaExecutavel, "gbak.exe");
            string argumentos = $"-b -v -user sysdba \"{caminhoOrigem}\" \"{caminhoDestino}\"";

            return ConfiguradorProcesso.CriarBase(executavel, argumentos, VARIAVEL_SENHA, senha);
        }
        public static ProcessStartInfo ObterRestore(string pastaExecutavel, string caminhoOrigem, string caminhoDestino, string senha)
        {
            string executavel = Path.Combine(pastaExecutavel, "gbak.exe");
            string argumentos = $"-c -v -user SYSDBA \"{caminhoOrigem}\" \"{caminhoDestino}\"";

            return ConfiguradorProcesso.CriarBase(executavel, argumentos, VARIAVEL_SENHA, senha);
        }
    }
}
