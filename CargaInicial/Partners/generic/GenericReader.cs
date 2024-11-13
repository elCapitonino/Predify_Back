using DSS.Common.Exceptions;
using DSS.Common.Extensions;
using DSS.Common.Interfaces;
using DSS.Common.Utils.Files.Process;
using DSS.Common.Utils.Files.Process.Converters;
using DSS.Model.Models;
using DSS.WebApiService.Models.ImportarDados;
using DSS.WebApiService.Services.Arquivos;
using DSS.WebApiService.Services.Arquivos.Planilha;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Predify.Models.V2.ImportacaoPlanilha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CargaInicial.Partners.generic
{
    public class GenericReader : IGenericReader, IPercentage
    {
        /// <summary>
        /// Hash Set é uma lista muito rápida para consultas.
        /// </summary>
        HashSet<DateTime> _datas = new HashSet<DateTime>();
        string[] _columnsFormats = null;

        #region Construtores

        public GenericReader()
        {
        }

        public GenericReader(string filePath, string[] columnsFormats = null, string separator = ";")
        {
            _reader = new ArquivoGenericoReader(null, 500, filePath, separator);
            _reader.AutoDetectColumns = true;
            _reader.AutoSelectConverter = true;

            _reader.GetConverter = (colName) =>
            {
                if (colName == "pbcost" 
                || colName == "quantity"
                || colName == "total"
                || colName == "saleprice")
                    return typeof(decimal);
                return typeof(string);
            };


            if (_reader != null)
            {
                _reader.HasCabecalho = true;
                _reader.LinhaProcessadaCallback = LinhasProcessadas;
            }
            _reader.UseHeaderFromFile = true;
            int index = 0;
            //foreach (var s in columnsFormats)
            //{
            //    _reader.Colunas.Add($"col_{index}", new ColunasModelBase()
            //    {
            //        Index = index,
            //        Conversor = GetConverter(s)
            //    });

            //    index++;
            //}

            //_columnsFormats = columnsFormats;
        }

        private ConversorBase GetConverter(string Format)
        {
            if (!Format.IsNullEmptyOrWhiteSpaces())
            {
                if (Format == "0.00" || Format == "0,00")
                    return new ConversorDecimal() { SeparadorDecimal = Format.Replace("0","").Trim() };
                else if (Format == "yyyy/MM/dd" || Format == "dd/MM/yyyy" || Format == "yyyy-MM-dd" || Format == "dd-MM-yyyy")
                    return new ConversorData() { FormatoData = Format };
            }

            return new ConversorString();
        }


        #endregion

        ProcessarArquivoBase _reader;

        public Func<List<BsonDocument>, Task> Callback { get; set; }

        public ulong TotalRegisters
        {
            get
            {
                if (_reader != null)
                    return _reader.TotalRegisters;

                return 0;
            }
        }

        public long Total
        {
            get
            {
                var c = _reader as IPercentage;
                return c != null ? c.Total : 0;
            }
        }

        public long Current
        {
            get
            {
                var c = _reader as IPercentage;
                return c != null ? c.Current : 0;
            }
        }

        public float Percentage
        {
            get
            {
                var c = _reader as IPercentage;
                return c != null ? c.Percentage : 0;
            }
        }

        public Task ProcessarArquivo()
        {
            if (_reader != null)
                return _reader.ProcessarArquivo();

            throw new DssErrorException("Reader não definído.");
        }

        protected async Task LinhasProcessadas(Dictionary<string, object>[] valores, ulong len)
        {
            if (valores.Length == 1 && valores[0].Count == 0)
                return;
            if (len > 0)
            {
                if (Callback != null)
                {
                    List<BsonDocument> result = new List<BsonDocument>();
                    for (ulong i = 0; i < len; i++)
                    {
                        var valorCorrente = valores[i];
                        if (valorCorrente.Count == 0)
                            continue;


                        BsonDocument documento = new BsonDocument();

                        var c = 0;
                        foreach (var vc in valorCorrente)
                        {
                            var col = _reader.Colunas[vc.Key];
                            if (col.Conversor != null)
                            {
                                if (col.Conversor.GetType() == typeof(ConversorDecimal))
                                    documento.Add(new BsonElement(vc.Key, (decimal)vc.Value));
                                else if (col.Conversor.GetType() == typeof(ConversorData))
                                    documento.Add(new BsonElement(vc.Key, (DateTime)vc.Value));
                                else
                                    documento.Add(new BsonElement(vc.Key, vc.Value != null ? vc.Value.ToString(): null));
                            }
                            c++;
                        }

                        result.Add(documento);
                    }

                        await Callback(result);
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