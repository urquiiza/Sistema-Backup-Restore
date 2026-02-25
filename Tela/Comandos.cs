using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Backup_Restore
{
    internal static class Comandos
    {
        public static string NomeCaminho(string caminhoOrigem, string caminhoDestino)
        {
            string nomeOriginal = Path.GetFileName(caminhoOrigem);
            string extensao = Path.GetExtension(caminhoOrigem).ToUpper();

            string novoNome;
            string caminhoFinal;

            if (extensao == ".FDB")
            {
                novoNome = Path.ChangeExtension(nomeOriginal, ".FBK");
            }
            else if (extensao == ".FBK")
            {
                novoNome = Path.ChangeExtension(nomeOriginal, ".FDB");
            }
            else
            {
                throw new Exception("Extensão não suportada.");
            }

            caminhoFinal = Path.Combine(caminhoDestino, novoNome);
            return caminhoFinal;
        }

        public static string Comando(string bancoOrigem, string bancoDestino)
        {
            string caminhoDestino = NomeCaminho(bancoOrigem, bancoDestino);

            string extensao = Path.GetExtension(bancoOrigem).ToUpper();
            string argumentos = "";

            if (extensao == ".FDB")
            {
                //backup
                argumentos = $"-b -v -user sysdba -pass masterkey \"{bancoOrigem}\" \"{caminhoDestino}\"";
            }
            else if (extensao == ".FBK")
            {
                //restore
                argumentos = $"-c -v -user sysdba -pass masterkey -rep -FIX_FSS_D win1252 -FIX_FSS_M win1252 -P 8192 \"{bancoOrigem}\" \"{caminhoDestino}\"";
            }
            else { throw new Exception("Extensão não suportada"); }

            return argumentos;
        }
    }
}