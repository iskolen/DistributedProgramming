using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly ConnectionMultiplexer _redis;

    public SummaryModel(ILogger<SummaryModel> logger)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        IDatabase db = _redis.GetDatabase();

        string rankKey = "RANK-" + id;
        string similarityKey = "SIMILARITY-" + id;

        if (db.KeyExists(rankKey) && db.KeyExists(similarityKey))
        {
            Rank = ((double)db.StringGet(rankKey));
            Similarity = ((double)db.StringGet(similarityKey));
        }
        else
        {
            _logger.LogError("Values not found for ID: {id}", id);
        }
    }
}

