 using LiteDB;

namespace Pixeval.Database.Managers;

public class SearchHistoryPersistentManager : SimplePersistentManager<SearchHistoryEntry>
{
    public SearchHistoryPersistentManager(ILiteDatabase db, int maximumRecords) : base(db, maximumRecords)
    {

    }
}