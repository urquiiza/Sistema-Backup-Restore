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
            string arquivoDestinoBackup = caminhoDestino;

            if (Directory.Exists(caminhoDestino))
            {
                string nomeBase = Path.GetFileNameWithoutExtension(caminhoOrigem);
                if (string.IsNullOrWhiteSpace(nomeBase)) nomeBase = "Backup_Firebird";

                arquivoDestinoBackup = Path.Combine(caminhoDestino, nomeBase + ".FBK");
            }

            string executavel = Path.Combine(pastaExecutavel, "gbak.exe");
            string argumentos = $"-b -v -user SYSDBA \"{caminhoOrigem}\" \"{arquivoDestinoBackup}\"";

            return ConfiguradorProcesso.CriarBase(executavel, argumentos, VARIAVEL_SENHA, senha);
        }
        public static ProcessStartInfo RestoreFirebird(string pastaExecutavel, string caminhoOrigem, string caminhoDestino, string senha)
        {
            string arquivoDestinoBanco = caminhoDestino;

            if (Directory.Exists(caminhoDestino))
            {
                string nomeBase = Path.GetFileNameWithoutExtension(caminhoOrigem);

                if (string.IsNullOrWhiteSpace(nomeBase)) nomeBase = "BancoRestaurado";

                arquivoDestinoBanco = Path.Combine(caminhoDestino, nomeBase + ".FDB");
            }

            string executavel = Path.Combine(pastaExecutavel, "gbak.exe");

            string argumentos = $"-rep -v -user SYSDBA \"{caminhoOrigem}\" \"{arquivoDestinoBanco}\"";

            return ConfiguradorProcesso.CriarBase(executavel, argumentos, VARIAVEL_SENHA, senha);
        }
        public static List<ProcessStartInfo> ManutencaoFirebird(string pastaExecutavel, string caminhoOrigem, string caminhoDestino, string senha)
        {
            List<ProcessStartInfo> manutencaoFire = new List<ProcessStartInfo>();
            
            string destinoBackup = @"C:\MANUTENÇÃO";
            if (!System.IO.Directory.Exists(destinoBackup))
            {
                System.IO.Directory.CreateDirectory(destinoBackup);
            }
            
            string arquivoTemporario = System.IO.Path.GetFileNameWithoutExtension(caminhoOrigem) + ".FBK";
            string arquivoFbk = System.IO.Path.Combine(destinoBackup, arquivoTemporario);

            manutencaoFire.Add(BackupFirebird(pastaExecutavel, caminhoOrigem, arquivoFbk, senha));
            manutencaoFire.Add(RestoreFirebird(pastaExecutavel, arquivoFbk, caminhoOrigem, senha));

            return manutencaoFire;
        }
    }
}