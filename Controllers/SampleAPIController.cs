using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CASIOProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetUserDetails : ControllerBase
    {
        private readonly IConfiguration _configuration;
        HttpClientHandler handler = new HttpClientHandler();
        public GetUserDetails(IConfiguration configuration)
        {
            _configuration = configuration;
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, ssl) => { return true; };
        }

        [HttpGet(nameof(getUsers))]
        public  async Task<List<string>> getUsers()
        {
            List<string> str = new List<string>();
            Log.Information($"Getusers method called at {DateTime.Now}");
            try
            {
                
                using (var httpClient = new HttpClient(handler))
                {
                    using (var response = await httpClient.GetAsync("https://localhost:5001"))
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        str.Add(result);
                        Log.Information("Response Successfull");
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
            }
            
            return  str;
        }

        [HttpPost(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile(IFormFile files)
        {
            Log.Information($"UploadFile method called at {DateTime.Now}");
            try
            {
                string systemFileName = files.FileName;
                string blobstorageconnection = _configuration.GetValue<string>("BlobConnectionString");
                // Get storage account from connection string.    
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
                // Blob client
                CloudBlobClient _cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer _container = _cloudBlobClient.GetContainerReference(_configuration.GetValue<string>("BlobContainerName"));
                CloudBlockBlob _blockBlob = _container.GetBlockBlobReference(systemFileName);
                await using (var streamData = files.OpenReadStream())
                {
                    await _blockBlob.UploadFromStreamAsync(streamData);
                }
                Log.Information("Call Succeeded");
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
            }
            return Ok();

        }
    }
}
