using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace CargaInicial
{
    class MongoDB
    {
        public MongoDB()
        {
            //BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
        }

        public MongoClient MongoClient { get; set; } = null;
        public IMongoDatabase MongoDatabase { get; set; } = null;
        public IMongoCollection<BsonDocument> MongoCollection { get; set; } = null;

        public MongoDB Connect()
        {
            string pass = HttpUtility.UrlEncode("=7m@MpU&1e3zCD5b");
            MongoClient = new MongoClient($"mongodb://mongoAdmin:{pass}@172.31.46.114:27017");

            return this;
            //            var dbList = _mongoClient.ListDatabases().ToList();
            //Console.WriteLine("The list of databases on this server is: ");
            //foreach (var db in dbList)
            //{
            //    Console.WriteLine(db);
            //}
        }

        public async Task<List<BsonDocument>> ListDatabases()
        {
            return (await MongoClient.ListDatabasesAsync()).ToList();
        }

        public Task CreateCollection(string collectionName)
        {
            return MongoDatabase.CreateCollectionAsync(collectionName);
        }

        public MongoDB SelectDatabase(string database)
        {
            MongoDatabase = MongoClient.GetDatabase(database);
            return this;
        }

        public async Task<List<BsonDocument>> ListCollections()
        {
            return (await MongoDatabase.ListCollectionsAsync()).ToList();
        }

        public async Task<MongoDB> CreateCollectionIfNotExist(string collection)
        {
            bool exist = (await ListCollections()).Any(an => an["name"].ToString().ToLower() == collection.ToLower());
            if (!exist)
                await CreateCollection(collection);
            return this;
        }

        public MongoDB SelectCollection(string collection)
        {
            MongoCollection = MongoDatabase.GetCollection<BsonDocument>(collection);
            return this;
        }

        public Task InsertRange(IEnumerable<BsonDocument> documents)
        {
            return MongoCollection.InsertManyAsync(documents);
        }

        public Task CleanCollection()
        {
            return MongoCollection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        }
    }
}
