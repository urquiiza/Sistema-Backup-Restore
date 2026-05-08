using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup_Restore
{
    public class ComandosPostgres
    {
        private const string VARIAVEL_SENHA = "PGPASSWORD";

        public static ProcessStartInfo BackupPostgres(string pastaExecutavel, string nomeBanco, string caminhoDestino, string senha)
        {
            string arquivoDestino = caminhoDestino;

            if (Directory.Exists(caminhoDestino))
            {
                string nomeBase = string.IsNullOrWhiteSpace(nomeBanco) ? "Backup_Postgres" : nomeBanco;

                arquivoDestino = Path.Combine(caminhoDestino, nomeBase + ".backup");
            }

            string executavel = Path.Combine(pastaExecutavel, "pg_dump.exe");
            string argumentos = $"-U postgres -F c -d \"{nomeBanco}\" -f \"{arquivoDestino}\" -v";

            return ConfiguradorProcesso.CriarBase(executavel, argumentos, VARIAVEL_SENHA, senha);
        }

        public static ProcessStartInfo RestorePostgres(string pastaExecutavel, string nomeBanco, string caminhoOrigem, string senha)
        {
            string executavel = Path.Combine(pastaExecutavel, "pg_restore.exe");
            string argumentos = $"-U postgres -d \"{nomeBanco}\" -v \"{caminhoOrigem}\"";

            return ConfiguradorProcesso.CriarBase(executavel, argumentos, VARIAVEL_SENHA, senha);
        }

        public static List<ProcessStartInfo> ManutencaoPostgres(string pastaExecutavel, string nomeBanco, string senha)
        {
            var processos = new List<ProcessStartInfo>();

            string execVacuum = Path.Combine(pastaExecutavel, "vacuumdb.exe");
            string argvacuum = $"-U postgres -d \"{nomeBanco}\" -f -v";
            processos.Add(ConfiguradorProcesso.CriarBase(execVacuum, argvacuum, VARIAVEL_SENHA, senha));

            string execReindex = Path.Combine(pastaExecutavel, "reindexdb.exe");
            string argReindex = $"-U postgres -d \"{nomeBanco}\" -e";
            processos.Add(ConfiguradorProcesso.CriarBase(execReindex, argReindex, VARIAVEL_SENHA, senha));

            return processos;
        }
    }
}