using DSS.Common.Extensions;
using DSS.Common.Models;
using DSS.Model.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Predify.Models.V2.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaInicial
{
    class Program
    {

        static void Main(string[] args)
        {

            try
            {
                if (args.Any(an => an.ToLower().Trim() == "--horus"))
                {
                    Partners.horus.HorusImport horusImport = new Partners.horus.HorusImport();
                    horusImport.Import(args).GetAwaiter().GetResult();

                }
                else if (args.Any(an => an.ToLower().Trim() == "--smarket"))
                {
                    Partners.smarket.SmarketImport smarketImport = new Partners.smarket.SmarketImport();
                    smarketImport.Import(args).GetAwaiter().GetResult();
                }
                else if (args.Any(an => an.ToLower().Trim() == "--saleshistory"))
                {
                    SalesHistory.IntegrationManager im = new SalesHistory.IntegrationManager(args);
                    im.Import().GetAwaiter().GetResult();
                }
                else if (args.Any(an => an.ToLower().Trim() == "--generic"))
                {
                    Partners.generic.GenericImport genericImport = new Partners.generic.GenericImport();
                    genericImport.Import(args).GetAwaiter().GetResult();
                }
                else
                {
                    Console.WriteLine("Argumentos não informados.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //string bigFile = "";
            //string smallFile = "";

            //foreach (var i in TesteYield())
            //    Console.WriteLine(i);


            //using (StreamReader sr = new StreamReader("d:\\rofe_dump.txt"))
            //{
            //    for (int i = 0; i < 5; i++)
            //    {
            //        Console.WriteLine(sr.ReadLine());
            //    }
            //}



            //using (var f = new StreamReader("D:\\predify_daily.dump"))
            //{
            //    for (int i = 0; i < 1; i++)
            //    {
            //        smallFile = f.ReadLine();
            //    }
            //}

            //MongoDB m = new MongoDB();
            //m.Connect()
            //    .SelectDatabase("horus")
            //    .SelectCollection("products");

            ////var ownerFilter = Builders<Documents>.Filter.ElemMatch(x => x.ArtifactAttributes, p => p.AttributeName.Equals("Owner"));
            //var ownerValueFieldDefinition = new StringFieldDefinition<BsonDocument, string>("cnpj");
            ////var distinctItems = _projectArtifacts.Distinct(ownerValueFieldDefinition, ownerFilter).ToList();

            //var result = m.MongoCollection.DistinctAsync(ownerValueFieldDefinition, new BsonDocument()).GetAwaiter().GetResult();

            //StringBuilder sb = new StringBuilder();

            //using (FileStream f = new FileStream("cnpjs.txt", FileMode.Create))
            //{
            //    using (System.IO.StreamWriter sw = new StreamWriter(f))
            //    {
            //        var li = result.ToList();
            //        long count = 0;
            //        foreach (var r in li)
            //        {
            //            var item = m.MongoCollection.Find(new BsonDocument("cnpj", r)).FirstOrDefaultAsync().GetAwaiter().GetResult();
            //            Console.WriteLine($"{++count} de {li.Count}");
            //            sb.Append("\"").Append(item["cnpj"]).Append("\"");
            //            sb.Append("\"").Append(item["brand"]).Append("\"");
            //            sb.Append("\"").Append(item["uf"]).Append("\"");
            //            sb.Append("\"").Append(item["city"]).AppendLine("\"");
            //        }
            //    }
            //}


            //var ind = Builders<BsonDocument>.IndexKeys;
            //var indexModel = new CreateIndexModel<BsonDocument>(ind.Ascending("dt_venda").Ascending("seqproduto").Ascending("seqloja"));

            //var result = m.MongoCollection.Indexes.CreateOneAsync(indexModel).GetAwaiter().GetResult();

            //var result = m.MongoCollection.CountDocumentsAsync(new BsonDocument()).GetAwaiter().GetResult();
            //Console.WriteLine(result);


            //m.CleanCollection().GetAwaiter().GetResult();

            //            var teste = m.ListCollections().GetAwaiter().GetResult();


            //var teste = m.MongoCollection.Find(new BsonDocument()).FirstOrDefaultAsync().GetAwaiter().GetResult();
            //Console.WriteLine(teste);
            //foreach (var db in teste)
            //{
            //    Console.WriteLine(db);
            //}

            //object t = new
            //{
            //    nome = "Gustavo",
            //    teste = true
            //};

            //BsonDocument document = t.ToBsonDocument();

            //m.InsertRange(new List<BsonDocument>() { document }).GetAwaiter().GetResult();


            //ImportFiles.HorusFiles hf = new ImportFiles.HorusFiles(null);

            //FileStream fs = File.OpenRead("d:\\predify_2022-07-29.zip");
            //ICSharpCode.SharpZipLib.Zip.ZipFile file = new ICSharpCode.SharpZipLib.Zip.ZipFile(fs); 
            //byte[] buffer = new byte[4096];
            //int offSet = 0;
            //int size = 4096;
            //int read = 0;

            //foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry in file)
            //{
            //    if (!zipEntry.IsFile)
            //    {
            //        continue;
            //    }
            //    Stream zipStream = file.GetInputStream(zipEntry);
            //    do
            //    {
            //        read = ICSharpCode.SharpZipLib.Core.StreamUtils.ReadRequestedBytes(zipStream, buffer, offSet, size);
            //        if (read > 0)
            //        {
            //            //string[] v = Encoding.UTF8.GetString(buffer).Split(new char[] { '\r', '\n' });
            //            //foreach (var l in v)
            //            //{
            //            hf.ProcessarString(Encoding.UTF8.GetString(buffer), false).GetAwaiter().GetResult();
            //            //    break;
            //            //}
            //            break;
            //        }
            //    }
            //    while (read == size);
            //    break;
            //}


            Console.WriteLine();
            Console.WriteLine("Finalizado");
            Console.ReadKey();


        }

        static IEnumerable<string> TesteYield()
        {
            int i = 0;
            while (i < 10)
            {
                yield return (i++).ToString();

            }
        }


        static async Task ProcessHorus()
        {

            

            Dictionary<string, string> values = new Dictionary<string, string>();
            DSS.Common.Utils.Files.Process.ArquivoGenericoReader columnSplitter = new DSS.Common.Utils.Files.Process.ArquivoGenericoReader();
            int total = 0;

            Console.Write("Processados: ");
            int cleft = Console.CursorLeft;
            int ctop = Console.CursorTop;

            using (var fr = new FileStream("D:\\prices_predify_historical.csv\\prices_predify_historical.csv", FileMode.Open, FileAccess.Read))
            using (var f = new StreamReader(fr))
            {
                using (var fw = new FileStream("d:\\prices_predify_historical.csv\\by_company.csv", FileMode.Create))
                {
                    using (var w = new StreamWriter(fw, Encoding.UTF8))
                    {
                        bool isBlank = false;
                        bool first = true;
                        string line = "";
                        string lineValue = "";
                        DateTime lastTime = DateTime.Now;
                        long lastPos = 0;

                        while (!f.EndOfStream)
                        {
                            line = f.ReadLine();
                            if (first)
                            {
                                first = false;
                                continue;
                            }
                            var colunas = columnSplitter.SplitColumns(line, out isBlank);

                            if (!isBlank)
                            {
                                if (colunas[10].ToLower().Trim() == "Papelaria e Material Escolar".ToLower())
                                {
                                    if (!values.ContainsKey(colunas[4]))
                                    {
                                        lineValue = $"\"{colunas[4]}\";" +
                                            $"\"{colunas[6]}\";" +
                                            $"\"{colunas[5]}\";" +
                                            $"\"{colunas[7]}\";" +
                                            $"\"{colunas[8]}\";" +
                                            $"\"{colunas[10]}\";";

                                        values.Add(colunas[4], lineValue);
                                        w.WriteLine(lineValue);
                                    }
                                }
                            }

                            if (++total >= 100000)
                            {
                                var dif = DateTime.Now - lastTime;
                                string leftTime = $"{dif.Hours}:{dif.Minutes}:{dif.Seconds}";

                                double totalMS = (f.BaseStream.Length * dif.TotalMilliseconds) / (f.BaseStream.Position - lastPos);

                                dif = TimeSpan.FromMilliseconds(totalMS);

                                lastTime = DateTime.Now;

                                Console.SetCursorPosition(cleft, ctop);
                                Console.Write($"{(f.BaseStream.Position * 100F / f.BaseStream.Length).ToString("0.0000") }% - Encontrados: {values.Count} - Tempo Restante: {dif}");
                                total = 0;
                                lastPos = f.BaseStream.Position;
                            }
                        }
                    }
                }
            }
        }



    }
}
