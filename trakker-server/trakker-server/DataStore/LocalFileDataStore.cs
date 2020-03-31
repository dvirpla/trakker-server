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

        // CR: (Kfir) Why is Serializer a public property?
        public IObjectSerializer Serializer { get; set; }

        // CR: This function has several references and the return value doesn't change.
        //     I would add a new property and initialize this property in the constructor
        private static string GetLocalWorkingFolderPath() =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                // CR: (Kfir) Make this a const
                "TrakkerServer"
            );

        // CR: Add function docstring
        public User GetUser(Guid userId)
        {
            var userFolder = Path.Combine(GetLocalWorkingFolderPath(), userId.ToString());
            if (!Directory.Exists(userFolder))
            {
                // CR: Consider implementing custom Exceptions
                // CR: (Kfir) Include the userId in the exception message
                throw new InvalidOperationException("User doesn't exists");
            }

            var snapshotIds = Directory.EnumerateFiles(userFolder).Select(snapshot => new Guid(Path.GetFileNameWithoutExtension(snapshot))).ToList();
            
            // CR: (Kfir) Write another constructor for User that accepts the snapshotIds list
            return new User(userId) {SnapshotIds = snapshotIds};
        }

        // CR: Add function docstring
        public Snapshot GetSnapshot(Guid snapshotId, User snapshotOwner)
        {
            // CR: (Kfir) Remove redundant parentheses
            // CR: (Kfir) This code duplicates the code in the beginning of GetUser
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
                // CR: (Kfir) Include the snapshotId in the exception message
                throw new InvalidOperationException("Snapshot doesn't exist");
            }

            // CR: (Kfir) Use a "using" statement to create the FileStream and auto-close it (it's the equivalent of python's "with")
            var snapshotFileStream = new FileStream(snapshotFilePath, FileMode.Open);
            var snapshot = (Snapshot)this.Serializer.Deserialize(snapshotFileStream);
            snapshotFileStream.Close();
            return snapshot;
        }

        // CR: (Kfir) Add function docstring
        public void SaveSnapshot(Snapshot snapshot, User snapshotOwner)
        {
            // CR: (Kfir) Remove redundant parentheses
            var userFolder = (Path.Combine(GetLocalWorkingFolderPath(), snapshotOwner.Uuid.ToString()));
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            // CR: I would move the extension string to const
            var snapshotFilePath = Path.Combine(userFolder, snapshot.Uuid + ".snp");
            // CR: (Kfir) It's not necessary to create the file ahead of time. You can use FileMode.OpenOrCreate
            File.Create(snapshotFilePath).Close();
            // CR: (Kfir) Use a "using" statement to create the FileStream and auto-close it (it's the equivalent of python's "with")
            var snapshotFileStream = new FileStream(snapshotFilePath, FileMode.Open);
            this.Serializer.Serialize(snapshotFileStream, snapshot);
            snapshotOwner.SnapshotIds.Add(snapshot.Uuid);
            snapshotFileStream.Close();
        }
    }
}
