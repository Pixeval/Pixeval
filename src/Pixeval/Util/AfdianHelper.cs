using System.Threading.Tasks;
using Pixeval.Utilities;

namespace Pixeval.Util;

public static class AfdianHelper
{
    public const string UserId = "984915f621f011ea981552540025c377";

    public const string Api = $"https://afdian.net/api/creator/get-sponsors?user_id={UserId}&type={{0}}&page={{1}}";

    public static async Task<Sponsor[]> QueryByAmount()
    {
        var a = Api.Format(QueryType.Amount, 0);
        return [];
    }
}

public enum QueryType
{
    Amount,
    Old
}

public record Sponsor(string Name, string Avatar, string Amount, string Date);
