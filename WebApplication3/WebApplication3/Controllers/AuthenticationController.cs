using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApplication3.MongoDBHelper;

namespace WebApplication3.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        ILogger<AuthenticationController> _logger;
        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
        }

        [HttpPost("signin")]
        public object SignIn([FromBody] SignInInput input)
        {
            Guid xcv = Guid.NewGuid();

            try
            {
                using (_logger.BeginScope(new Dictionary<string, object>() { { "xcv", xcv } }))
                {
                    _logger.LogError(new Exception(), "An error occurred");
                }

                FilterDefinition<BsonDocument>[] filters = new FilterDefinition<BsonDocument>[]
                    { Builders<BsonDocument>.Filter.Eq("email", input.email),
                    Builders<BsonDocument>.Filter.Eq("password", input.password)};

                List<BsonDocument> userRecords = MongoDBHelperSingleton.instance.GetRecords("UserCollection", filters);

                if (userRecords.Count == 0)
                {
                    using (_logger.BeginScope(new Dictionary<string, object>() { { "xcv", xcv }, { "errorCode", "404" } }))
                    {
                        _logger.LogWarning("User doesn't exist");
                    }
                    return new
                    {
                        success = false,
                        message = "User not found",
                        errorCode = "404"
                    };
                }
                var objectTest = new
                {
                    success = true,
                    data = new { userId = userRecords[0].GetValue("_id").AsString, signInComplete = true }
                };

                return new
                {
                    success = true,
                    data = new { userId = userRecords[0].GetValue("_id").AsString, signInComplete = true }
                };
            }
            catch (Exception e)
            {
                using (_logger.BeginScope(new Dictionary<string, object>()
                    { { "xcv", xcv }, { "message", e.Message }, { "errorCode", "500" } }))
                {
                    _logger.LogError(e, "Started Sign In");
                }
                return new
                {
                    success = true,
                    message = e.Message,
                    errorCode = "500"
                };
            }
        }
    }

    public class SignInInput
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}