using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaInicial.Partners.horus
{
    sealed class HorusImport
    {

        const string c_bucketName = "predify-horus";
        string _collection = "products";
        bool _clean = false;
        MongoDB _mdb = new MongoDB();


        public async Task Import(string[] args)
        {
            List<DateTime> startDate = new List<DateTime>();
            Console.WriteLine("Iniciando importação Horus");
            string fileName = null;
            bool downloadFile = true;

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower() == "--date")
                    {
                        if (i <= args.Length - 2)
                        {
                            DateTime tp;
                            if (DateTime.TryParseExact(args[i + 1], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out tp))
                            {
                                startDate.Add(tp.Date);

                                if (args.Any(a => a.ToLower() == "--date-to-now"))
                                {
                                    while (tp < DateTime.Now.Date)
                                    {
                                        tp = tp.AddDays(1);
                                        startDate.Add(tp.Date);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Não foi possível converter a data {args[i + 1]}, a mesma deve estar no formato YYYY/MM/DD");
                                return;
                            }
                        }
                    }
                    else if (args[i].ToLower() == "--file" && i <= args.Length - 2)
                    {
                        fileName = args[i + 1];
                    }
                    else if (args[i].ToLower() == "--no-download")
                    {
                        downloadFile = false;
                    }
                    else if (args[i].ToLower() == "--clean-db")
                    {
                        _clean = true;
                    }
                    else if (args[i].ToLower() == "--collection" && i <= args.Length - 2)
                    {
                        _collection = args[i + 1];
                    }
                }
            }

            if (_collection == null)
            {
                Console.WriteLine("Collection não definida, para definir digite: --collection <nome da collection>");
                return;
            }    

            S3 s3 = new S3();
            var filesOnS3 = await s3.ListFilesAsync("predify-horus");
            Console.WriteLine();
            Console.WriteLine();

            var importedFiles = await _mdb.Connect()
                .SelectDatabase("horus")
                .SelectCollection("imported_files")
                .MongoCollection
                .Find(new BsonDocument())
                .ToListAsync();

            if (fileName != null)
            {
                await ProcessFileAsync(fileName, filesOnS3, downloadFile, s3);
            }
            else 
            {
                Console.WriteLine($"Data inicial: {(startDate.Count > 0 ? startDate[0].ToString("yyyy-MM-dd") : "NÃO INFORMADA.")}");
                Console.WriteLine($"Data Final: {(startDate.Count > 0 ? startDate[startDate.Count - 1].ToString("yyyy-MM-dd") : "NÃO INFORMADA.")}");
                Console.WriteLine($"Total de datas: {startDate.Count}");
                Console.WriteLine();
                Console.WriteLine();


                foreach (var file in filesOnS3)
                {

                    if (file.Size == 0 || !file.Key.ToLower().EndsWith(".zip"))
                        continue;

                    if (importedFiles.Any(an => an["file"].ToString().ToLower() == file.Key.ToLower()))
                    {
                        Console.WriteLine($"Ignorando arquivo {file.Key}, pois o mesmo já foi importado");
                        continue;
                    }

                    _mdb.SelectCollection(_collection);

                    fileName = file.Key;
                    await ProcessFileAsync(fileName, filesOnS3, downloadFile, s3);
                }
            }
        }

        private async Task ProcessFileAsync(string fileName, List<Amazon.S3.Model.S3Object> filesOnS3, bool downloadFile, S3 s3)
        {
            if (fileName != null)
            {
                Console.WriteLine(new String('=', Console.WindowWidth));
                Console.WriteLine($"Nome arquivo: {fileName}");


                var s3File = filesOnS3.FirstOrDefault(f => f.Key.ToLower() == fileName.ToLower().Trim());

                if (s3File == null)
                {
                    Console.WriteLine("Arquivo não encontrado...");
                }
                else
                {
                    try
                    {
                        if (downloadFile)
                            await s3.DownloadAsync(s3File, c_bucketName);

                        await ImportingAsync(s3File.Key);

                        _mdb.SelectCollection("imported_files")
                            .MongoCollection
                            .InsertOne(new BsonDocument(new List<BsonElement>()
                            {
                                new BsonElement("file", fileName)
                                , new BsonElement("date", DateTime.Now)
                            }));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar arquivo: {s3File.Key}");
                        Console.WriteLine($"Error message: {ex.Message}");
                    }
                }

                File.Delete(fileName);
            }
            else
            {
                Console.WriteLine("Processo abortado...");
                Console.WriteLine("Informar a data: --date <yyyy-mm-dd>");
                Console.WriteLine("ou");
                Console.WriteLine("Informar nome arquivo: --file <file name>");
            }

        }

        private async Task ImportingAsync(string fileName)
        {
            HorusFiles hf = new HorusFiles(null);


            _mdb.SelectDatabase("horus")
                .SelectCollection(_collection);

            if (_clean)
            {
                await _mdb.CleanCollection();
                _clean = false;
            }


            hf.Callback = (list_files) =>
            {
                return Task.Run(async () =>
                {
                    await _mdb.InsertRange(list_files);
                });
            };




            using (FileStream fs = File.OpenRead(fileName))
            {
                fs.Position = 0;

                Console.Write("Processing: ");

                int leftPos = Console.CursorLeft;
                int topPos = Console.CursorTop;

                using (ICSharpCode.SharpZipLib.Zip.ZipFile file = new ICSharpCode.SharpZipLib.Zip.ZipFile(fs))
                {
                    byte[] buffer = new byte[4096];
                    int offSet = 0;
                    int size = 4096;
                    int read = 0;
                    long processedSize = 0;

                    foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry in file)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue;
                        }
                        using (Stream zipStream = file.GetInputStream(zipEntry))
                        {
                            do
                            {
                                read = ICSharpCode.SharpZipLib.Core.StreamUtils.ReadRequestedBytes(zipStream, buffer, offSet, size);
                                if (read > 0)
                                {
                                    await hf.ProcessarString(Encoding.UTF8.GetString(buffer), false);
                                    processedSize += read;
                                    Console.SetCursorPosition(leftPos, topPos);
                                    Console.Write((processedSize * 100f / zipEntry.Size).ToString("0.00") + "%");
                                }
                            }
                            while (read == size);
                        }
                        Console.SetCursorPosition(leftPos, topPos);
                        Console.WriteLine("100.0%");
                        break;
                    }
                }

            }

        }






    }
}
