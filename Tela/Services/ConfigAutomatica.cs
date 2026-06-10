using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows;

namespace Backup_Restore.Services
{
    public class ConfigAutomatica
    {
        public HosConfig Configuracoes { get; set; }
    }
    public class HosConfig
    {
        public string BANCO_DADOS { get; set; }
        public string BANCO_REMOTO { get; set; }
        public string NOME_BANCODADOS { get; set; }
        public string BANCO_DADOS_VERSAO { get; set; }
        
    }
}
