using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using NSubstitute;
using WebApplication3;
using WebApplication3.Controllers;
using Xunit;
using WebApplication3.MongoDBHelper;

namespace WebApplication3UnitTests
{
    public class UserControllerUnitTests
    {
        private UserController userController;
        private ILogger<UserController> _logger = Substitute.For<ILogger<UserController>>();

        public UserControllerUnitTests()
        {
            userController = new UserController(_logger);
            MongoDBHelperSingleton.instance = Substitute.For<IMongoDBHelperSingleton>();
        }

        [Fact]
        public void ValidInput_AddUser()
        {
            AddUserInput input = new AddUserInput
            {
                email = "testemail@email.com",
                name = "Neil",
                phone = "4323432",
                password = "fskdlfjir",
                username = "neilbd"
            };

            MongoDBHelperSingleton.instance.CreateRecord(Arg.Any<BsonDocument>(), "UserCollection")
                .Returns(false);
            MongoDBHelperSingleton.instance.CreateRecord(Arg.Any<BsonDocument>(), "AccountCollection")
                .Returns(false);
            MongoDBHelperSingleton.instance.GetRecords("UserCollection", Arg.Any<FilterDefinition<BsonDocument>[]>())
                .Returns(new List<BsonDocument>());

            object result = userController.AddUser(input);
            var jsonToReturn = JObject.FromObject(result);

            Assert.True(jsonToReturn.Value<bool>("success"));
        }

        [Fact]
        public void UserExists_AddUser()
        {
            AddUserInput input = new AddUserInput
            {
                email = "testemail@email.com",
                name = "Neil",
                phone = "4323432",
                password = "fskdlfjir",
                username = "neilbd"
            };

            MongoDBHelperSingleton.instance.CreateRecord(Arg.Any<BsonDocument>(), "UserCollection")
                .Returns(true);

            var result = userController.AddUser(input);

            var jsonToReturn = JObject.FromObject(result);

            Assert.False(jsonToReturn.Value<bool>("success"));
        }
    }
}
