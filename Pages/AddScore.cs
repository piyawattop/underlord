using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using UnderlordLeagueTables.Models;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace UnderlordLeagueTables.Pages
{
    public class AddScoreModel : PageModel
    {
        private const string BlobConatiner = "insights-log-data";
        [BindProperty]
        public Competition Competition { get; set; }
        [BindProperty]
        public List<Player> Players { get; set; }

        private readonly ILogger<AddScoreModel> _logger;
        private readonly IConfiguration _configuration;

        public AddScoreModel(ILogger<AddScoreModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            BlobStorageService blobService = new BlobStorageService(_configuration.GetValue<string>("BlobStorageConnectionString"));
            await blobService.InitializeAsync(BlobConatiner);
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "data" + DateTime.Now.ToString("HHmmsss") + ".json");
            await blobService.DownloadAsync("data.json", fileName);
            string json = System.IO.File.ReadAllText(fileName);
            if (!string.IsNullOrEmpty(json))
            {

                UnderlordTable collection = JsonConvert.DeserializeObject<UnderlordTable>(json);
                Players = collection.player;
                Competition = new Competition();
                Competition.round = collection.competition.Max(e => e.round) + 1;
                Competition.result = new List<Result>();
            }
            System.IO.File.Delete(fileName);
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            bool isAddNew = false;
            if (Competition.result == null)
            {
                Competition.result = new List<Result>();
            }
            var keys = HttpContext.Request.Form.Keys;
            var keyNames = keys.Where(e => e.Contains("point#")).ToArray();
            for (int i = 0; i < keyNames.Length; i++)
            {
                var formValue = HttpContext.Request.Form[keyNames[i]];
                var playerName = keyNames[i].Split("#", StringSplitOptions.RemoveEmptyEntries);
                Console.WriteLine(formValue[0]);
                Competition.result.Add(new Result
                {
                    name = playerName[1],
                    point = Convert.ToDouble(formValue[0])
                });
                isAddNew = true;
            }
            if (isAddNew)
            {
                BlobStorageService blobService = new BlobStorageService(_configuration.GetValue<string>("BlobStorageConnectionString"));
                await blobService.InitializeAsync(BlobConatiner);
                var fileName = Path.Combine(Directory.GetCurrentDirectory(), "data" + DateTime.Now.ToString("HHmmsss") + ".json");
                await blobService.DownloadAsync("data.json", fileName);
                string json = System.IO.File.ReadAllText(fileName);
                UnderlordTable collection = JsonConvert.DeserializeObject<UnderlordTable>(json);
                collection.competition.Add(Competition);
                string output = JsonConvert.SerializeObject(collection);
                System.IO.File.WriteAllText(fileName, output);
                await blobService.UploadFileAsync(fileName, "data.json");
                System.IO.File.Delete(fileName);
                return RedirectToPage("./Index");
            }
            else
            {
                return Page();
            }
        }
    }
}
