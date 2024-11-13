using DSS.Common.Enums;
using Predify.Models.V2.ImportacaoPlanilha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.WebApiService.V2_1.Services.Clientes.Petrobahia.Importacao
{

    /// <summary>
    /// Classe utlizada para atualizar somente o valor do Custo de um Custo de Produção.
    /// </summary>
    public class SmarketSheet : SheetBase
    {

        public ColunaImportacaoModel product { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel description { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel pbCost { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel store { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel quantity { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel salePrice { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel customer { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel customerName { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel issuance { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel affiliate { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel affiliateName { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel document { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel nameSegment { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel shippingValue { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel productGroup { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel economicGroupCode { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel economicGroup { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel total { get; set; } = new ColunaImportacaoModel();
        public ColunaImportacaoModel invoceNumber { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel clientSalesPrice { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel supplierCode { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel supplierName { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel packageDescription { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel packageUnity { get; set; } = new ColunaImportacaoModel();

        public ColunaImportacaoModel nbm { get; set; } = new ColunaImportacaoModel();

    }
}
