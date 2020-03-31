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

        // CR: This function has several references and the return value doesn't change.
        // I would add a new property and initialize this property in the constructor
        private static string GetLocalWorkingFolderPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrakkerServer");

        // CR: Add function comment
        public User GetUser(Guid userId)
        {
            var userFolder = Path.Combine(GetLocalWorkingFolderPath(), userId.ToString());
            if (!Directory.Exists(userFolder))
            {
                // CR: Consider implementing custom Exceptions
                throw new InvalidOperationException("User doesn't exists");
            }

            var snapshotIds = Directory.EnumerateFiles(userFolder).Select(snapshot => new Guid(Path.GetFileNameWithoutExtension(snapshot))).ToList();
            
            return new User(userId) {SnapshotIds = snapshotIds};
        }

        // CR: Add function comment
        public Snapshot GetSnapshot(Guid snapshotId, User snapshotOwner)
        {
            var userFolder = (Path.Combine(GetLocalWorkingFolderPath(), snapshotOwner.Uuid.ToString()));
            if (!Directory.Exists(userFolder))
            {
                // CR: Consider implementing custom Exceptions
                throw new InvalidOperationException("User doesn't exists");
            }

            // CR: I would move the extension string to const
            var snapshotFilePath = Path.Combine(userFolder, snapshotId + ".snp");
            if (!File.Exists(snapshotFilePath))
            {
                // CR: Consider implementing custom Exceptions
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

            // CR: I would move the extension string to const
            var snapshotFilePath = Path.Combine(userFolder, snapshot.Uuid + ".snp");
            File.Create(snapshotFilePath).Close();
            var snapshotFileStream = new FileStream(snapshotFilePath, FileMode.Open);
            this.Serializer.Serialize(snapshotFileStream, snapshot);
            snapshotOwner.SnapshotIds.Add(snapshot.Uuid);
            snapshotFileStream.Close();
        }
    }
}
