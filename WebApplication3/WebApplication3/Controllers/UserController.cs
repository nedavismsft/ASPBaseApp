using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApplication3.Models;
using MongoDB.Bson.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using WebApplication3.MongoDBHelper;

namespace WebApplication3.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ILogger<UserController> _logger;
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpPost("adduser")]
        public object AddUser([FromBody] AddUserInput input)
        {
            string timeCreated = DateTime.UtcNow.ToString();
            string userId = Guid.NewGuid().ToString();
            string xcv = Guid.NewGuid().ToString();

            try
            {
                using (_logger.BeginScope(new Dictionary<string, object>() { { "xcv", xcv } }))
                {
                    _logger.LogInformation("Started user creation");
                }

                FilterDefinition<BsonDocument>[] filters = new FilterDefinition<BsonDocument>[]
                { Builders<BsonDocument>.Filter.Eq("email", input.email) };
                var userRecord = MongoDBHelperSingleton.instance.GetRecords("UserCollection", filters);

                if (userRecord.Count > 0)
                {
                    return new { success = false, message = "User already exists. Please use a different email.", errorCode = "409" };
                }

                User newUser = new User
                {
                    _id = userId,
                    email = input.email,
                    name = input.name,
                    username = input.username,
                    password = input.password,
                    phone = input.phone,
                    createdDate = timeCreated,
                    modifiedDate = timeCreated
                };

                Account newAccount = new Account
                {
                    _id = Guid.NewGuid().ToString(),
                    balance = 0,
                    userId = userId,
                    createdDate = timeCreated,
                    modifiedDate = timeCreated
                };

                BsonDocument userAsBson = newUser.ToBsonDocument();
                BsonDocument accountAsBson = newAccount.ToBsonDocument();

                bool recordExists = MongoDBHelperSingleton.instance.CreateRecord(userAsBson, "UserCollection");
                if (recordExists)
                {
                    return new { success = false, message = "User already exists. Please use a new email" };
                }

                recordExists = MongoDBHelperSingleton.instance.CreateRecord(accountAsBson, "AccountCollection");
                if (recordExists)
                {

                    return new { success = false, message = "Record already exists. Please try a new email" };
                }
            }
            catch (Exception e)
            {
                var errorObject = new { success = false, message = e.Message, stackTrace = e.StackTrace };
                return errorObject;
            }

            return new { success = true, data = new { id = userId } };
        }

        [HttpGet("{id?}/getaccounts")]
        public object GetAccounts(string id)
        {
            FilterDefinition<BsonDocument>[] filters = new FilterDefinition<BsonDocument>[]
                { Builders<BsonDocument>.Filter.Eq("userId", id) };
            string xcv = Guid.NewGuid().ToString();

            try
            {
                using (_logger.BeginScope(new Dictionary<string, object>()
                    { { "xcv", xcv }, { "userId", id } }))
                {
                    _logger.LogInformation("Started obtaining user's account information");
                }
                List<BsonDocument> accountRecordsMongo = MongoDBHelperSingleton.instance.GetRecords("AccountCollection", filters);

                List<Account> accountRecords = new List<Account>();

                foreach (BsonDocument account in accountRecordsMongo)
                {
                    accountRecords.Add(BsonSerializer.Deserialize<Account>(account));
                }

                filters = new FilterDefinition<BsonDocument>[]
                   { Builders<BsonDocument>.Filter.Eq("_id", id) };

                User userRecord = BsonSerializer.Deserialize<User>(MongoDBHelperSingleton.instance.GetRecords("UserCollection", filters).First());

                return new
                {
                    success = true,
                    data = new { name = userRecord.name, accounts = accountRecords.ToArray() }
                };
            }
            catch (Exception e)
            {
                return new
                {
                    success = false,
                    message = e.Message,
                    errorCode = "500"
                };
            }
        }
    }

    public class AddUserInput
    {
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
    }

}