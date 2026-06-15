using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Backup_Restore.Services
{
    internal class StatusFirebird
    {
        private const string NOME_GUARDIAN = "FirebirdGuardianDefaultInstance";
        private const string NOME_SERVER = "FirebirdServerDefaultInstance";
        public static void DesativaFirebird()
        {
            string[] servicos =
            {
                    NOME_GUARDIAN,
                    NOME_SERVER
            };

            foreach (string nomeServico in servicos)
            {
                try
                {

                    using (ServiceController servico = new ServiceController(nomeServico))
                    {
                        servico.Refresh();

                        if (servico.Status == ServiceControllerStatus.Running)
                        {
                            servico.Stop();
                            servico.WaitForStatus(ServiceControllerStatus.Stopped);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // firebird 4.0 não possui guardian, então ignora
                    continue;
                }
            }
        }
        public static void AtivaFirebird() 
        {
            string[] servicos =
            {
                NOME_GUARDIAN,
                NOME_SERVER
            };

            foreach (string nomeServico in servicos)
            {
                try
                {
                    using (ServiceController servico = new ServiceController(nomeServico))
                    {
                        servico.Refresh();

                        if (servico.Status == ServiceControllerStatus.Stopped)
                        {
                            servico.Start();
                            servico.WaitForStatus(ServiceControllerStatus.Running);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // firebird 4.0 não possui guardian, então ignora
                    continue;
                }
            }
        }
    }
}
