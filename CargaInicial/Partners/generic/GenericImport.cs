using DSS.Common.Extensions;
using DSS.Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaInicial.Partners.generic
{
    class GenericImport
    {



        private int tc = 0;
        private int lc = 0;
        GenericReader _reader;
        MongoDB _mongo = new MongoDB();
        long idCompany = 0;
        const string c_bucketName = "predify-smarket";
        long _errorsCount = 0;
        string _prefix;
        string _separator = ";";


        public async Task Import(string[] args)
        {
            string fileName = "";
            string collection = "";
            string importedFilesCollection = "";


            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower() == "--file")
                    {
                        if (i > args.Length - 2)
                        {
                            Console.WriteLine($"Não foi possível localizar o nome do arquivo.");
                            return;
                        }

                        fileName = args[i + 1];
                    }
                    else if (args[i].ToLower() == "--collection")
                    {
                        if (i > args.Length - 2)
                        {
                            Console.WriteLine("Collection não informada, utilize --collection <nome da collection> para informar");
                            return;
                        }
                        _prefix = args[i + 1];
                    }
                    else if (args[i] == "--separator")
                    {
                        if (i > args.Length - 2)
                        {
                            Console.WriteLine("Separador não informada, utilize --collection <nome da collection> para informar");
                            return;
                        }
                        _separator = args[i + 1];

                    }
                }
            }

            if (_prefix.IsNullEmptyOrWhiteSpaces())
            {
                Console.WriteLine("Collection não informada, utilize --collection <nome da collection> para informar");
                return;
            }

            _mongo.Connect()
                .SelectDatabase("enterprise");

            importedFilesCollection = _prefix + "_imported_files";
            collection = _prefix + "_sales_history";

            Console.WriteLine($"Prefixo: {_prefix}");
            Console.WriteLine($"Imported Files Collection: {importedFilesCollection}");
            Console.WriteLine($"Sales History Collection: {collection}");


            (await _mongo.CreateCollectionIfNotExist(collection))
                .SelectCollection(collection);

            var importedFiles = await _mongo.SelectCollection(importedFilesCollection)
                .MongoCollection
                .Find(new BsonDocument())
                .ToListAsync();


            _mongo.SelectCollection(collection);

            _errorsCount = 0;
            Console.WriteLine(new String('=', Console.WindowWidth));
            Console.WriteLine($"Nome arquivo: {fileName}");

            using (_reader = new GenericReader(fileName,separator: _separator))
            {
                
                _reader.Callback = InsertOnMongo;
                Console.Write("Importando: ");
                tc = Console.CursorTop;
                lc = Console.CursorLeft;
                await _reader.ProcessarArquivo();
            }

            await _mongo.SelectCollection(importedFilesCollection)
                .MongoCollection.InsertOneAsync(new BsonDocument(new List<BsonElement>()
                {
                        new BsonElement("file", fileName)
                        , new BsonElement("date", DateTime.Now)
                }));

            File.Delete(fileName);
        }

        public async Task InsertOnMongo(List<BsonDocument> history)
        {
            await _mongo.InsertRange(history);

            Console.SetCursorPosition(lc, tc);
            Console.Write($"{_reader.Percentage.ToString("0.0000")}% - Erros: {_errorsCount}");
        }


    }
}
