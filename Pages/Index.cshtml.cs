using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using UnderlordLeagueTables.Models;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace UnderlordLeagueTables.Pages
{
    public class IndexModel : PageModel
    {
        private const string BlobConatiner = "insights-log-data";

        [BindProperty]
        public UnderlordTable TableData { get; set; }

        [BindProperty]
        public List<Player> ListPlayer { get; set; }
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
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

                foreach (var round in collection.competition)
                {
                    foreach (var match in round.result)
                    {
                        var player = collection.player.Where(e => e.name == match.name).FirstOrDefault();
                        if (player != null)
                        {
                            player.point += match.point;
                            if (player.form == null)
                            {
                                player.form = new List<ResultRound>();
                            }
                            player.form.Add(new ResultRound
                            {
                                round = round.round,
                                point = match.point
                            });
                        }
                    }
                }
                ListPlayer = collection.player.AsEnumerable().OrderByDescending(e => e.point)
                .Select((row, index) => new Player { name = row.name, point = row.point, form = row.form, position = index + 1 })
                .ToList();
            }
            System.IO.File.Delete(fileName);
        }
    }
}
