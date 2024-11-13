using DSS.Common.Enums;
using DSS.Common.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CargaInicial
{
    public static class IQuerableExtensions
    {

        public static IQueryable<T> IntegrationWhere<T>(this IQueryable<T> values, e_Integracoes_Tipos value)
            where T : DSS.Model.Models.Integracao_Config, new()
        {

            string propName = "";
            bool Ok = false;
            T val = new T();

            switch (value)
            {
                case e_Integracoes_Tipos.Produtos:
                    propName = val.GetNameFromExpression((n) => n.Produto, false);
                    break;
                case e_Integracoes_Tipos.Custo_Produto:
                    propName = val.GetNameFromExpression((n) => n.IntegrarProdutoCusto, false);
                    break;
                case e_Integracoes_Tipos.Preco_Venda_Produto:
                    propName = val.GetNameFromExpression((n) => n.IntegrarProdutoPrecoVenda, false);
                    break;
                case e_Integracoes_Tipos.Servicos:
                    propName = val.GetNameFromExpression((n) => n.Servico, false);
                    break;
                case e_Integracoes_Tipos.Custo_Servicos:
                    propName = val.GetNameFromExpression((n) => n.IntegrarServicoCusto, false);
                    break;
                case e_Integracoes_Tipos.Preco_Venda_Servicos:
                    propName = val.GetNameFromExpression((n) => n.IntegrarServicoPrecoVenda, false);
                    break;
                case e_Integracoes_Tipos.Custos_Fixos_Grupos_E_Itens:
                    propName = val.GetNameFromExpression((n) => n.IntegrarCustosFixosGruposItens, false);
                    break;
                case e_Integracoes_Tipos.Custos_Fixos_Valores:
                    propName = val.GetNameFromExpression((n) => n.IntegrarCustosFixosValor, false);
                    break;
                case e_Integracoes_Tipos.Custos_Variaveis_Config:
                    propName = val.GetNameFromExpression((n) => n.IntegrarCustosVariaveisConfig, false);
                    break;
                case e_Integracoes_Tipos.Custos_Variaveis_Valores:
                    propName = val.GetNameFromExpression((n) => n.IntegrarCustosVariaveis, false);
                    break;
                case e_Integracoes_Tipos.SendPrice:
                    propName = val.GetNameFromExpression((n) => n.IntegrarEnvioDePrecoVenda, false);
                    break;
                case e_Integracoes_Tipos.Sales_History:
                    propName = val.GetNameFromExpression((n) => n.IntegrateSalesHistory);
                    break;
                case e_Integracoes_Tipos.Stock_Product:
                    propName = val.GetNameFromExpression((n) => n.IntegrateStock);
                    break;
                case e_Integracoes_Tipos.Input_Cost:
                    propName = val.GetNameFromExpression((n) => n.Integrate_InputCost);
                    break;
                default:
                    throw new Exception($"Esse tipo de integração, {value.ToString()} , não foi configurada na extension \"IntegrationWhere\".");
            }

            ParameterExpression param = Expression.Parameter(typeof(T), "x");

            Expression member = Expression.Property(param, propName);
            ConstantExpression vc = Expression.Constant(true);
            BinaryExpression e = Expression.Equal(member, vc);

            Expression<Func<T, bool>> lambda1 = Expression.Lambda<Func<T, bool>>((Expression)e, param);

            return values.Where(lambda1);
        }

    }
}
