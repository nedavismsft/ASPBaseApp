﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApplication3.MongoDBHelper
{
    public class MongoDBHelperSingleton : IMongoDBHelperSingleton
    {
        public static IMongoDBHelperSingleton instance = null;
        private MongoClient client;
        private string password;
        private string userName;
        private string host;
        private string dbName;

        private MongoDBHelperSingleton()
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            password = keyVaultClient.GetSecretAsync("<insert your keyvault URI>/secrets/DBPassword").Result.Value;
            userName = keyVaultClient.GetSecretAsync("<insert your keyvault URI>/secrets/DBUserName").Result.Value;
            host = keyVaultClient.GetSecretAsync("<insert your keyvault URI>/secrets/DBHost").Result.Value;
            dbName = keyVaultClient.GetSecretAsync("<insert your keyvault URI>/secrets/DBName").Result.Value;

            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseTls = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;
            settings.RetryWrites = false;
            settings.RetryReads = false;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            client = new MongoClient(settings);
        }
        
        public static void InitializeSingleton()
        {
            if (instance == null)
                instance = new MongoDBHelperSingleton();
        }

        private IMongoCollection<BsonDocument> GetCollection(string collectionName)
        {
            var database = client.GetDatabase(dbName);
            var todoTaskCollection = database.GetCollection<BsonDocument>(collectionName);
            return todoTaskCollection;
        }

        // FilterDefinitions<BsonDocument>[] { new FilterDefinition<BsonDocument("id", "32432") }
        public List<BsonDocument> GetRecords(string collectionName, FilterDefinition<BsonDocument>[] filters)
        {
            var collection = GetCollection(collectionName);
            var filter = Builders<BsonDocument>.Filter.And(filters);

            return collection.Find(filter).ToList<BsonDocument>();
        }

        public bool CreateRecord(BsonDocument record, string collectionName)
        {
            var collection = GetCollection(collectionName);
            FilterDefinition<BsonDocument>[] filters = new FilterDefinition<BsonDocument>[]
                    { Builders<BsonDocument>.Filter.Eq("_id", record.GetValue("_id").AsString) };

            List<BsonDocument> result = GetRecords(collectionName, filters);

            if (result.Count > 0)
            {
                return true;
            }
            collection.InsertOne(record);

            return false;
        }

        public BsonDocument GetRecordAndUpdate(string id, Dictionary<string, object> fieldsToUpdate, string collectionName)
        {
            var collection = GetCollection(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

            List<UpdateDefinition<BsonDocument>> updates = new List<UpdateDefinition<BsonDocument>>();
            foreach (KeyValuePair<string, object> pair in fieldsToUpdate)
            {
                var update = Builders<BsonDocument>.Update.Set(pair.Key, pair.Value);
                updates.Add(update);
            }

            var allUpdates = Builders<BsonDocument>.Update.Combine(updates);
            BsonDocument result = collection.FindOneAndUpdate(filter, allUpdates);

            return result;
        }
    }
}
