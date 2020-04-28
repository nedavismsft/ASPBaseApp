using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Text.Json;
using MongoDB.Driver;
using System.Linq.Expressions;
using WebApplication3.MongoDBHelper;

namespace WebApplication3.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ManageAccountController : ControllerBase
    {

        private readonly ILogger<ManageAccountController> _logger;
        public ManageAccountController(ILogger<ManageAccountController> logger)
        {
            _logger = logger;
        }

        [HttpPost("{id?}/deposit")]
        public object Deposit([FromBody] DepositPayload input)
        {
            decimal newBalance = Decimal.Parse(input.deposit) + input.currentDeposit;
            Dictionary<string, object> fieldsToUpdate = new Dictionary<string, object>();
            fieldsToUpdate.Add("balance", newBalance);
            fieldsToUpdate.Add("modifiedDate", DateTime.UtcNow.ToString());
            string xcv = Guid.NewGuid().ToString();

            try
            {
                using (_logger.BeginScope(new Dictionary<string, object>() { { "xcv", xcv } }))
                {
                    _logger.LogInformation("Started Deposit");
                }

                BsonDocument result = MongoDBHelperSingleton.instance.GetRecordAndUpdate(input.accountId, fieldsToUpdate, "AccountCollection");
                if (result != null)
                {
                    return new
                    {
                        success = true,
                        data = new { newBalance }
                    };
                }
                else
                {
                    using (_logger.BeginScope(new Dictionary<string, object>()
                    { { "xcv", xcv }, { "message", "Unexpected error updating the record" }, { "errorCode", "500" } }))
                    {
                        _logger.LogError(new Exception(), "Unexpected exception in updating the record");
                    }
                    return new
                    {
                        success = false,
                        message = "Unexpected error updating the record",
                        errorCode = "500"
                    };
                }
            }
            catch (Exception e)
            {
                using (_logger.BeginScope(new Dictionary<string, object>()
                    { { "xcv", xcv }, { "message", e.Message }, { "errorCode", "500" } }))
                {
                    _logger.LogError(e, "Unexpected exception in depositing balance");
                }
                return new
                {
                    success = false,
                    message = "Unexpected error updating the record",
                    errorCode = "500"
                };
            }
        }
    }

    public class DepositPayload
    {
        public string deposit { get; set; }
        public decimal currentDeposit { get; set; }
        public string accountId { get; set; }
    }
}
