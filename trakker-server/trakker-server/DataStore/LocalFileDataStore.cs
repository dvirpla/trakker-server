using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TrakkerModels;
using TrakkerServer.Exceptions;
using TrakkerServer.Models;

namespace TrakkerServer.DataStore
{
    public class LocalFileDataStore : IDataStore
    {
        private const string WorkingFolderName = "TrakkerServer";

        private const string SnapshotFileExtension = ".snp";
        public LocalFileDataStore(IObjectSerializer<Snapshot> serializer)
        {
            this.Serializer = serializer;
            this.LocalWorkingFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                WorkingFolderName);
        }

        private IObjectSerializer<Snapshot> Serializer { get; set; }

        private string LocalWorkingFolder { get; set; }

        private string GetUserFolderPath(Guid userId)
        {
            return Path.Combine(this.LocalWorkingFolder, userId.ToString());
        }

        /// <summary>
        /// this function returns the user from the user id
        /// </summary>
        /// <param name="userId">the id of the user</param>
        /// <returns>the user</returns>
        public User GetUser(Guid userId)
        {
            var userFolder = this.GetUserFolderPath(userId);
            if (!Directory.Exists(userFolder))
            {
                throw new UserNotFoundException($"User {userId} doesn't exists");
            }

            var snapshotIds = Directory.EnumerateFiles(userFolder).Select(snapshot => new Guid(Path.GetFileNameWithoutExtension(snapshot))).ToList();
            
            return new User(userId, snapshotIds);
        }

        /// <summary>
        /// Get a user snapshot from the disk using the serializer.
        /// </summary>
        /// <param name="snapshotId">the id of the snapshot to get</param>
        /// <param name="snapshotOwner">the user that owns the snapshot</param>
        /// <returns>the snapshot</returns>
        public Snapshot GetSnapshot(Guid snapshotId, User snapshotOwner)
        {
            var userFolder = this.GetUserFolderPath(snapshotOwner.Uuid);
            if (!Directory.Exists(userFolder))
            {
                throw new UserNotFoundException($"User {snapshotOwner.Uuid} doesn't exists");
            }

            var snapshotFilePath = Path.Combine(userFolder, snapshotId + SnapshotFileExtension);
            if (!File.Exists(snapshotFilePath))
            {
                throw new SnapshotNotFoundException($"Snapshot {snapshotId} of User {snapshotOwner.Uuid} doesn't exists");
            }

            Snapshot snapshot;
            using (var snapshotFileStream = new FileStream(snapshotFilePath, FileMode.Open))
            {
                snapshot = (Snapshot)this.Serializer.Deserialize(snapshotFileStream);

            }
            return snapshot;
        }

        /// <summary>
        /// Save a snapshot to the disk using the serializer.
        /// </summary>
        /// <param name="snapshot">The snapshot to save</param>
        /// <param name="snapshotOwner">The user to save the snapshot for</param>
        public void SaveSnapshot(Snapshot snapshot, User snapshotOwner)
        {
            var userFolder = Path.Combine(this.LocalWorkingFolder, snapshotOwner.Uuid.ToString());
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            var snapshotFilePath = Path.Combine(userFolder, snapshot.Uuid + SnapshotFileExtension);

            // ReSharper disable once ConvertToUsingDeclaration
            using (var snapshotFileStream = new FileStream(snapshotFilePath, FileMode.OpenOrCreate))
            {
                this.Serializer.Serialize(snapshotFileStream, snapshot);
                snapshotOwner.SnapshotIds.Add(snapshot.Uuid);
            }
        }
    }
}
