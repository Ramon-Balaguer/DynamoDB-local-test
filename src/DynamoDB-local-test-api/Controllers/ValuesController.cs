using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using DynamoDB_local_test_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThirdParty.Json.LitJson;

namespace DynamoDB_local_test_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class ValuesController : ControllerBase
    {
        private const string TableName = "SampleData";

        private readonly IAmazonDynamoDB _amazonDynamoDb;

        public ValuesController(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        // GET api/values/init
        [HttpGet]
        [Route("init")]
        public async Task Initialise()
        {
            var request = new ListTablesRequest
            {
                Limit = 10
            };

            var response = await _amazonDynamoDb.ListTablesAsync(request);

            var results = response.TableNames;

            if (!results.Contains(TableName))
            {
                var createRequest = new CreateTableRequest
                {
                    TableName = TableName,
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "Id",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "Id",
                            KeyType = "HASH" //Partition key
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 2
                    }
                };

                await _amazonDynamoDb.CreateTableAsync(createRequest);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var request = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue> { { "Id", new AttributeValue { N = id.ToString() } } }
            };

            var response = await _amazonDynamoDb.GetItemAsync(request);

            if (!response.IsItemSet)
                return NotFound();

            return response.Item["Title"].S;
        }

        [HttpPost]
        public async Task Post([FromBody] PostInput input)
        {
            var request = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { N = input.Id.ToString() }},
                    { "Title", new AttributeValue { S = input.Title }}
                }
            };

            await _amazonDynamoDb.PutItemAsync(request);
        }

        
        
        [HttpPost]
        [Route("UploadFile")]
        public async Task UploadFile(IFormFile zipFile)
        {
            var streamFile = Request.Form.Files[0];
            await using (var fileContentStream = new MemoryStream())
            {
                await streamFile.CopyToAsync(fileContentStream);
                var json = System.Text.Encoding.Default.GetString(fileContentStream.ToArray());
                var jsonArray = (JArray) JArray.Parse(json);
                
                var table = Table.LoadTable(_amazonDynamoDb, TableName);
                var batch = table.CreateBatchWrite();
                foreach (var jsonItem in jsonArray)
                {
                    var item = Document.FromJson(jsonItem.ToString());
                    batch.AddDocumentToPut(item);
                }
                
                await batch.ExecuteAsync();
                //await table.PutItemAsync(item);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = new DeleteItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue> { { "Id", new AttributeValue { N = id.ToString() } } }
            };

            var response = await _amazonDynamoDb.DeleteItemAsync(request);

            return StatusCode((int)response.HttpStatusCode);
        }
    }
}
