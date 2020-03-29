using System;
using TrakkerModels;
using TrakkerServer.Models;

namespace TrakkerServer.DataStore
{
    public interface IDataStore
    {
        public User GetUser(Guid userId);
        public Snapshot GetSnapshot(Guid snapshotId, User snapshotOwner);
        public void SaveSnapshot(Snapshot snapshot, User snapshotOwner);

    }
}
