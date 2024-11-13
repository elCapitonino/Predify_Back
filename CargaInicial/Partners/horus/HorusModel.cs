using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaInicial.Partners.horus
{

    class HorusModel
    {
        public HorusModel()
        {
            _id = ObjectId.GenerateNewId();
        }


        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId? _id { get; set; }

        public string cnpj { get; set; }
        public string brand { get; set; }
        public string brand_group { get; set; }
        public string uf { get; set; }
        public string city { get; set; }
        public string ean { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public DateTime? date { get; set; }
        public decimal price { get; set; }


    }
}
