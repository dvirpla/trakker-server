using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TrakkerModels;
using TrakkerServer.Models;

namespace TrakkerServer.DataStore
{
    public class LocalFileDataStore : IDataStore
    {
        public LocalFileDataStore(IObjectSerializer serializer)
        {
            this.Serializer = serializer;
        }
        public IObjectSerializer Serializer { get; set; }
        private static string GetLocalWorkingFolderPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrakkerServer");

        public User GetUser(Guid userId)
        {
            var userFolder = Path.Combine(GetLocalWorkingFolderPath(), userId.ToString());
            if (!Directory.Exists(userFolder))
            {
                throw new InvalidOperationException("User doesn't exists");
            }

            var snapshotIds = Directory.EnumerateFiles(userFolder).Select(snapshot => new Guid(Path.GetFileNameWithoutExtension(snapshot))).ToList();
            
            return new User(userId) {SnapshotIds = snapshotIds};
        }

        public Snapshot GetSnapshot(Guid snapshotId, User snapshotOwner)
        {
            var userFolder = (Path.Combine(GetLocalWorkingFolderPath(), snapshotOwner.Uuid.ToString()));
            if (!Directory.Exists(userFolder))
            {
                throw new InvalidOperationException("User doesn't exists");
            }

            var snapshotFilePath = Path.Combine(userFolder, snapshotId + ".snp");
            if (!File.Exists(snapshotFilePath))
            {
                throw new InvalidOperationException("Snapshot doesn't exists");
            }
            var snapshotFileStream = new FileStream(snapshotFilePath, FileMode.Open);
            var snapshot = (Snapshot)this.Serializer.Deserialize(snapshotFileStream);
            snapshotFileStream.Close();
            return snapshot;
        }

        public void SaveSnapshot(Snapshot snapshot, User snapshotOwner)
        {
            var userFolder = (Path.Combine(GetLocalWorkingFolderPath(), snapshotOwner.Uuid.ToString()));
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            var snapshotFilePath = Path.Combine(userFolder, snapshot.Uuid + ".snp");
            File.Create(snapshotFilePath).Close();
            var snapshotFileStream = new FileStream(snapshotFilePath, FileMode.Open);
            this.Serializer.Serialize(snapshotFileStream, snapshot);
            snapshotOwner.SnapshotIds.Add(snapshot.Uuid);
            snapshotFileStream.Close();
        }
    }
}
