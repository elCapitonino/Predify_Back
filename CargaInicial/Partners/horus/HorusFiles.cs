using DSS.Common.Exceptions;
using DSS.Common.Utils.Files.Process;
using DSS.Common.Utils.Files.Process.Converters;
using DSS.Model.Models;
using DSS.WebApiService.Models.ImportarDados;
using MongoDB.Bson;
using Predify.Models.V2.ImportacaoPlanilha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CargaInicial.Partners.horus
{
    class HorusFiles
    {

        List<DateTime> _datas = new List<DateTime>();

        #region Construtores

        public HorusFiles(TextSplitterBase spliter)
        {

            _reader = new ArquivoGenericoReader(spliter, 500);

            if (_reader != null)
            {
                _reader.HasCabecalho = true;
                _reader.LinhaProcessadaCallback = LinhasProcessadas;
            }

            //CNPJ, BRAND, BRAND_GROUP, UF, CITY, EAN, DESCRIPTION, CATEGORY, DATE, PRICE
            #region CNPJ

            _reader.Colunas.Add("cnpj", new ColunasModelBase()
            {
                Index = 0,
                Conversor = new ConversorString()
            });

            #endregion

            #region BRAND

            _reader.Colunas.Add("brand", new ColunasModelBase()
            {
                Index = 1,
                Conversor = new ConversorString()
            });

            #endregion

            #region BRAND_GROUP

            _reader.Colunas.Add("brand_group", new ColunasModelBase()
            {
                Index = 2,
                Conversor = new ConversorString()
            });

            #endregion

            #region UF

            _reader.Colunas.Add("uf", new ColunasModelBase()
            {
                Index = 3,
                Conversor = new ConversorString()
            });

            #endregion

            #region CITY

            _reader.Colunas.Add("city", new ColunasModelBase()
            {
                Index = 4,
                Conversor = new ConversorString()
            });

            #endregion

            #region EAN

            _reader.Colunas.Add("ean", new ColunasModelBase()
            {
                Index = 5,
                Conversor = new ConversorString()
            });

            #endregion

            #region Description

            _reader.Colunas.Add("description", new ColunasModelBase()
            {
                Index = 6,
                Conversor = new ConversorString()
            });

            #endregion

            #region Category

            _reader.Colunas.Add("category", new ColunasModelBase()
            {
                Index = 7,
                Conversor = new ConversorString()
            });

            #endregion

            #region Date

            _reader.Colunas.Add("date", new ColunasModelBase()
            {
                Index = 8,
                Conversor = new ConversorData() { FormatoData = "yyyy-MM-dd" }
            });

            #endregion

            #region Price

            _reader.Colunas.Add("price", new ColunasModelBase()
            {
                Index = 9,
                Conversor = new ConversorDecimal() { SeparadorDecimal = "." }
            });

            #endregion

        }

        #endregion

        ArquivoGenericoReader _reader;

        public Func<List<BsonDocument>, Task> Callback { get; set; }

        public Task ProcessarString(string texto, bool finished = false)
        {
            return _reader.ProcessarString(texto, finished);
        }

        protected async Task LinhasProcessadas(Dictionary<string, object>[] valores, ulong len)
        {
            if (valores.Length == 1 && valores[0].Count == 0)
                return;
            if (len > 0)
            {
                if (Callback != null)
                {
                    List<BsonDocument> produtos = new List<BsonDocument>();

                    for (ulong i = 0; i < len; i++)
                    {
                        var valorCorrente = valores[i];
                        if (valorCorrente.Count == 0)
                            continue;

                        BsonDocument produto = new BsonDocument();
                        produto.Add(new BsonElement("brand", _reader.GetValor<string>(valorCorrente, "brand", "")));
                        produto.Add(new BsonElement("brand_group", _reader.GetValor<string>(valorCorrente, "brand_group", "")));
                        produto.Add(new BsonElement("category", _reader.GetValor<string>(valorCorrente, "category", "")));
                        produto.Add(new BsonElement("city", _reader.GetValor<string>(valorCorrente, "city", "")));
                        produto.Add(new BsonElement("cnpj", _reader.GetValor<string>(valorCorrente, "cnpj", "")));
                        produto.Add(new BsonElement("date", _reader.GetValor<DateTime?>(valorCorrente, "date", null)));
                        produto.Add(new BsonElement("description", _reader.GetValor<string>(valorCorrente, "description", "")));
                        produto.Add(new BsonElement("ean", _reader.GetValor<string>(valorCorrente, "ean", "")));
                        produto.Add(new BsonElement("uf", _reader.GetValor<string>(valorCorrente, "uf", "")));
                        produto.Add(new BsonElement("price", _reader.GetValor<decimal>(valorCorrente, "price", -1)));

                        produtos.Add(produto);
                    }

                    await Callback(produtos);

                }
            }
        }

        public void Dispose()
        {
            Callback = null;
            _reader?.Dispose();
            _reader = null;
        }


    }
}