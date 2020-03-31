using System;
using TrakkerModels;
using TrakkerServer.Models;

namespace TrakkerServer.DataStore
{
    public interface IDataStore
    {
        User GetUser(Guid userId);
        Snapshot GetSnapshot(Guid snapshotId, User snapshotOwner);
        void SaveSnapshot(Snapshot snapshot, User snapshotOwner);
    }
}
