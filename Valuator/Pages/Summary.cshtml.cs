using DatabaseService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IDatabaseService _db;

    public SummaryModel(ILogger<SummaryModel> logger, IDatabaseService db)
    {
        _logger = logger;
        _db = db;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public IActionResult OnGet(string id)
    {
        _logger.LogDebug(id);

        string usernameActual = User.Identity.Name;
        if (string.IsNullOrWhiteSpace(usernameActual))
        {
            return Redirect("login");
        }

        string shardKey = _db.Get("MAIN", id);

        string[] values = _db.Get(shardKey, [$"RANK-{id}", $"SIMILARITY-{id}", id]);

        if (usernameActual != values[2])
        {
            return Redirect("index");
        }

        if (double.TryParse(values[0], out double rank))
        {
            Rank = rank;
        }

        if (double.TryParse(values[1], out double similarity))
        {
            Similarity = similarity;
        }

        return Page();
    }
}
