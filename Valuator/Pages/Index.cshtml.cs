using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ConnectionMultiplexer _redis;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        IDatabase db = _redis.GetDatabase();
        db.StringSet(textKey, text);

        string rankKey = "RANK-" + id;
        double rank = TextEvaluator.CalculateRank(text);
        db.StringSet(rankKey, rank);

        string similarityKey = "SIMILARITY-" + id;
        int similarity = TextEvaluator.CalculateSimilarity(text, db, textKey);
        db.StringSet(similarityKey, similarity);

        return Redirect($"summary?id={id}");
    }
}
