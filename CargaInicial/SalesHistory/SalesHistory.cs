using DSS.Common.Enums;
using DSS.Common.Models;
using Predify.Models.V2;
using Predify_Integracoes.Degust;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaInicial.SalesHistory
{
    sealed class SalesHistory
    {

        public SalesHistory()
        {

        }

        private void WriteLog(string message, string description, e_LogExceptionTipo logType)
        {
            Console.WriteLine(message);
            Console.WriteLine($"\t{description}");
        }

        private DateTimeRange Data { get; set; }

        public async Task Run(Integracao_Config Integracao_Config, DegustManager manager, DateTime dataInicial)
        {
            Data = new DateTimeRange(dataInicial);

            manager.UpdateToken = async () =>
            {
                using (DSS.WebApiService.Services.IntegracaoConfigService ic = new DSS.WebApiService.Services.IntegracaoConfigService(new DSS.Model.Contexts.DssContext()))
                {
                    var idConfig = await ic.QueryForGet(Integracao_Config.IdEmpresa, e_Integracoes_Tipos.Sales_History).FirstOrDefaultAsync();

                    var context = ic.Context as DSS.Model.Contexts.DssContext;
                    var databaseConfig = await context.Integracao_Config_Database.Where(we => we.IdERPConfig == idConfig.IdERPConfig && !we.IsDeletado).FirstOrDefaultAsync();

                    databaseConfig.Token = Integracao_Config.DatabaseModel.Token;
                    databaseConfig.TokenExpiration = Integracao_Config.DatabaseModel.TokenExpiration;

                    (ic.Context as DSS.Model.Contexts.DssContext).Entry(databaseConfig).State = EntityState.Modified;
                    await (ic.Context as DSS.Model.Contexts.DssContext).SaveChangesAsync();
                }
            };

            Action<string, string, e_LogExceptionTipo> externalLog = (message, stack, log) =>
            {
                Console.WriteLine(log.ToString());
                if (message != null)
                    Console.WriteLine(message);
                if (stack != null)
                    Console.WriteLine("\t" + stack);

            };

            manager.ExternalLog = externalLog;

            WriteLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - IdCompany {Integracao_Config.IdEmpresa}",
                $"Processo Sales History iniciado: Start Date: { Data.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss") } End Date: {Data.EndDateTime.ToString("yyyy-MM-dd HH:mm:ss")}",
                 e_LogExceptionTipo.Integracao);

            if ((DateTime.Now.Date - Data.StartDateTime).TotalMinutes < Integracao_Config.salesHistorySyncInterval)
                return;

            WriteLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - IdCompany {Integracao_Config.IdEmpresa}",
                $"Getting Sales History from Client",
                 e_LogExceptionTipo.Integracao);


            var registros = await manager.GetSalesHistoryFromClient(Data);

            WriteLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - IdCompany {Integracao_Config.IdEmpresa}",
                $"Modeling Sales History from Client",
                 e_LogExceptionTipo.Integracao);
            var historicos = await manager.ModelEnterpriseSalesHistory(registros);



            using (DSS.WebApiService.V2_1.Services.Clientes.Petrobahia.PetrobahiaSalesHistoryService service = new DSS.WebApiService.V2_1.Services.Clientes.Petrobahia.PetrobahiaSalesHistoryService(new Predify.DBO.Contexts.PrfContext() ))
            {

                service.Context.Database.CommandTimeout = 600;

                //WriteLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - IdCompany {Integracao_Config.IdEmpresa}",
                //    $"Deleting sales history for input",
                //     e_LogExceptionTipo.Integracao);

                //await service.Context.Database.Connection.OpenAsync();
                //await service.Context.Database.ExecuteSqlCommandAsync($"DELETE FROM Enterprise_Sales_History where Issuance >= '{Data.StartDateTime.ToString("yyyy/MM/dd")}'  AND idCompany = {Integracao_Config.IdEmpresa}");
                //service.Context.Database.Connection.Close();

                WriteLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - IdCompany {Integracao_Config.IdEmpresa}",
                    $"Adding sales history",
                     e_LogExceptionTipo.Integracao);

                service.EntityDBSet.AddRange(historicos);

                WriteLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - IdCompany {Integracao_Config.IdEmpresa}",
                    $"Saving sales history",
                     e_LogExceptionTipo.Integracao);

                await service.Context.SaveChangesAsync();
            }

            DateTime ultimaData = Data.StartDateTime;

            if (historicos != null && historicos.Any())
                ultimaData = historicos.Max(p => p.Issuance);

            WriteLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - IdCompany {Integracao_Config.IdEmpresa}",
                $"Saving last date {ultimaData.ToString("yyyy-MM-dd HH:mm:ss")}",
                 e_LogExceptionTipo.Integracao);

            await SalvarDataAtualizacao(Integracao_Config.IdEmpresa, ultimaData, e_TipoDadoExterno.Get_Sales_History);

        }

        private async Task SalvarDataAtualizacao(long idEmpresa, DateTime ultimaData, e_TipoDadoExterno tipoDadoExterno)
        {
            if (ultimaData != new DateTime(1, 1, 1))
            {

                var Context = new DSS.Model.Contexts.DssContext();

                //Pego a ultima data do banco de dados que foi sincronizado.
                DSS.Model.Models.Integracao_Dados dadosIntegracao = await Context.DadosImportados.FirstOrDefaultAsync(e => e.IdEmpresa == idEmpresa && e.TipoDado == tipoDadoExterno);



                if (dadosIntegracao == null)
                {
                    dadosIntegracao = new DSS.Model.Models.Integracao_Dados()
                    {
                        IdEmpresa = idEmpresa,
                        TipoDado = tipoDadoExterno,
                        ExternalLastUpdateData = ultimaData
                    };

                    Context.Entry(dadosIntegracao).State = EntityState.Added;
                }
                else
                {
                    if (dadosIntegracao.ExternalLastUpdateData < ultimaData)
                    {
                        dadosIntegracao.ExternalLastUpdateData = ultimaData;

                        Context.Entry(dadosIntegracao).State = EntityState.Modified;
                    }
                }

                await Context.SaveChangesAsync();

            }
        }







    }
}
