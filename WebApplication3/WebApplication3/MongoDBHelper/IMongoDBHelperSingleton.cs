using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Authentication;
using System.Runtime.InteropServices;

namespace WebApplication3.MongoDBHelper
{
    public interface IMongoDBHelperSingleton
    {
        public bool CreateRecord(BsonDocument record, string collectionName);
        public List<BsonDocument> GetRecords(string collectionName, FilterDefinition<BsonDocument>[] filters);
        public BsonDocument GetRecordAndUpdate(string id, Dictionary<string, object> fieldsToUpdate, string collectionName);
    }
}
