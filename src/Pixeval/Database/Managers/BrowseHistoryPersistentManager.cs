using LiteDB;

namespace Pixeval.Database.Managers;

public class BrowseHistoryPersistentManager : SimplePersistentManager<BrowseHistoryEntry>
{
    public BrowseHistoryPersistentManager(ILiteDatabase db, int maximumRecords) : base(db, maximumRecords)
    {

    }
}