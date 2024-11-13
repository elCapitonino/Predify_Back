using DSS.Common.Extensions;
using DSS.Common.Models;
using DSS.Model.Models;
using Predify.Models.V2.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaInicial.SalesHistory
{
    class IntegrationManager
    {

        public IntegrationManager(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "--idcompany" && i <= args.Length - 2)
                {
                    long.TryParse(args[i + 1], out _idCompany);
                }
                else if (args[i].ToLower() == "--startdate" && i <= args.Length - 2)
                {
                    DateTime.TryParseExact(args[i + 1], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _startDate);
                }
                else if (args[i].ToLower() == "--enddate" && i <= args.Length - 2)
                {
                    DateTime.TryParseExact(args[i + 1], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _endDate);
                }
                else if (args[i].ToLower() == "--externalids" && i <=args.Length -2)
                {
                    externalIds = args[i + 1];
                }
            }
        }

        Integracao_Config Integracao_Config;
        private long _idCompany = 0;

        DateTime _startDate = DSS.Common.Extensions.DateTimeExtensions.c_DataDefault;
        DateTime _endDate = DateTimeExtensions.c_DataDefault;
        string externalIds = null;


        public async Task Import()
        {
            while (_idCompany == 0)
            {
                Console.Write("Informe o id da Empresa: ");
                string id = Console.ReadLine();
                if (!long.TryParse(id, out _idCompany))
                {
                    Console.WriteLine("Processo abortado");
                    return;
                }    
            }

            var xiRetorno = QueryForGet(_idCompany, DSS.Common.Enums.e_Integracoes_Tipos.Sales_History).FirstOrDefault();

            if (xiRetorno != null)
            {
                xiRetorno.Integracao_Queries = xiRetorno.Integracao_Queries.Where(we => !we.IsDeletado).ToList();
                xiRetorno.Integracao_Config_CFOPs = xiRetorno.Integracao_Config_CFOPs.Where(we => !we.IsDeleted).ToList();
            }
            else
            {
                Console.WriteLine("Não encontrado configurações para a empresa selecionada");
                Console.ReadKey();
                return;
            }


            try
            {
                Integracao_Config = xiRetorno;
                var integracao = xiRetorno;

                if (externalIds != null)
                    integracao.IdEmpresaExterno = externalIds.Replace(",",";");

                Console.WriteLine($"Empresas que serão importadas: {integracao.IdEmpresaExterno}");

                var integracaoEM = integracao.EntityToModel();

                Predify_Integracoes.Degust.DegustManager degustManager = new Predify_Integracoes.Degust.DegustManager(integracaoEM);

                var dataRef =(await PrepararDatas(DSS.Common.Extensions.DateTimeExtensions.c_DataDefault, DSS.Common.Extensions.DateTimeExtensions.c_DataDefault, DSS.Common.Enums.e_TipoDadoExterno.Get_Sales_History)).StartDateTime;

                if (_startDate != DateTimeExtensions.c_DataDefault)
                    dataRef = _startDate.AddDays(-1);

                if (_endDate != DateTimeExtensions.c_DataDefault)
                    _endDate = _endDate.AddDays(1);
                else
                    _endDate = DateTime.Now.Date;



                SalesHistory sh = new SalesHistory();

                //while (DateTime.Now.AddDays(-1) > dataRef)
                while (dataRef < _endDate)
                {
                    dataRef = dataRef.AddDays(1);
                    Console.WriteLine(new string('=', Console.WindowWidth));
                    Console.WriteLine("Importando Data: " + dataRef.ToString("yyyy-MM-dd"));
                    Console.WriteLine(new string('=', Console.WindowWidth));
                    await sh.Run(integracaoEM, degustManager, dataRef);
                }

                Console.WriteLine("Criando vinculos entre sales history e produtos.");

                using (var nc = new Predify.DBO.Contexts.PrfContext())
                {
                    nc.Database.CommandTimeout = 1800;
                    using (DSS.WebApiService.V2_1.Services.Enterprise.EnterpriseSalesHistoryService esh = new DSS.WebApiService.V2_1.Services.Enterprise.EnterpriseSalesHistoryService(nc))
                    {
                        esh.GenerateProductsFromSaleHistory(_idCompany, false).GetAwaiter().GetResult();
                    }
                }

                Console.WriteLine("Importação finalizada");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
            Console.ReadKey();
            Console.WriteLine("Finalizado");
            Console.ReadKey();
        }


        IQueryable<Integracao_Config> QueryForGet(long? idEmpresa, DSS.Common.Enums.e_Integracoes_Tipos? dataType)
        {
            var Context = new DSS.Model.Contexts.DssContext();


            var xiQuery = Context.Integracao_Config.AsNoTracking().Where(we => !we.IsDeletado);

            if (idEmpresa != null)
                xiQuery = xiQuery.Where(we => we.IdEmpresa == idEmpresa.Value);

            if (dataType != null)
                xiQuery = xiQuery.IntegrationWhere(dataType.Value);

            xiQuery = xiQuery.Include(ie => ie.Empresa)
                .Include(ifq => ifq.Integracao_Queries)
                .Include(e => e.Integracao_Config_CFOPs);

            return xiQuery;
        }


        #region Preparar Datas

        /// <summary>
        /// Prepara as datas a serem utilizadas.
        /// 
        /// Segue as seguintes regras:
        /// 
        /// Quando passada uma dataInicial diferente da data default, essa data sera utilizada.
        /// Caso contrário, segue o nível de prioridade definida no código.
        /// 
        /// 1º - Data já configurada na propriedade Data.
        /// 2º - Data da última sincronização com o ERP.
        /// 3º - Data de configuração da integração, esse é utilizado quando nunca ocorreu uma sincronização e existe uma data de referência no banco de dados.
        /// 4º - Se nenhuma condição anterior for suprida, pega o primero dia do mês corrente.
        /// 
        /// Para a data final, segue as regras:
        /// Se for passada uma dataFinal diferente da Default, essa data sera utilizada.
        /// Caso contrário, segue o nível de prioridade definida no código.
        /// 
        /// 1º - Data já configurada na propriedade Data
        /// 2º - Se a dataFinal for Default e a DataInicial não for default, pega o último dia do mesa da DataInicial.
        /// 3º - Se a dataFinal for Default e a DataInicial for default, pega o último dia do mês corrente.
        /// 
        /// </summary>
        /// <param name="dataInicial">Data Inicial</param>
        /// <param name="dataFinal">Data Final</param>
        /// <param name="tipoDadoExterno">Tipo de dados que serão importados.</param>
        async Task<DSS.Common.Models.DateTimeRange> PrepararDatas(DateTime dataInicial, DateTime dataFinal, DSS.Common.Enums.e_TipoDadoExterno tipoDadoExterno)
        {

            DateTime dataInicialConfig = DSS.Common.Extensions.DateTimeExtensions.c_DataDefault;

            if (Integracao_Config != null && Integracao_Config.DataInicial.HasValue)
                dataInicialConfig = Integracao_Config.DataInicial.Value;

            var Context = new DSS.Model.Contexts.DssContext();

            //Pego a ultima data do banco de dados que foi sincronizado.
            Integracao_Dados lastDate = await Context.DadosImportados.FirstOrDefaultAsync(e => e.IdEmpresa == Integracao_Config.IdEmpresa && e.TipoDado == tipoDadoExterno);

            if (dataInicial.IsDefaultDate())
            {
                if (lastDate != null)
                    dataInicial = lastDate.ExternalLastUpdateData; //Pega a data da ultima sincronização
                else if (!dataInicialConfig.IsDefaultDate())
                    dataInicial = dataInicialConfig; //Data inicial da configuração de integração.
                else
                    dataInicial = DateTime.Now.NormalizarPrimeiroDiaMes(); //Pega o primeiro dia do mês corrente.
            }

            if (dataFinal.IsDefaultDate())
                dataFinal = !dataInicial.IsDefaultDate() ? dataInicial.NormalizarUltimoDiaMes() : DateTime.Now.NormalizarUltimoDiaMes();

            //SetData(new DateTimeRange(dataInicial, dataFinal));


            return new DateTimeRange(dataInicial, dataFinal);
        }

        #endregion
    }
}
