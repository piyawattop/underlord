using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using UnderlordLeagueTables.Models;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace UnderlordLeagueTables.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public UnderlordTable TableData { get; set; }

        [BindProperty]
        public List<Player> ListPlayer { get; set; }
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            string json = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "data.json"));

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
    }
}
