using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrakkerModels;
using TrakkerServer.DataStore;
using TrakkerServer.Models;

namespace TrakkerServerTests
{
    [TestClass]
    public class LocalFileDataStoreTests
    {
        [TestMethod]
        public void SnapshotSaveAndLoadTest()
        {
            // Arrange
            var localFileDataStore = new LocalFileDataStore(new FileObjectSerializer<Snapshot>());
            var timeNow = DateTime.Now;
            var snapshotUuid = Guid.NewGuid();
            var snapshotToSave = new Snapshot() {Time = timeNow, Uuid = snapshotUuid};
            var user = new User(Guid.NewGuid());

            //Act
            localFileDataStore.SaveSnapshot(snapshotToSave, user);
            var loadedSnapshot = localFileDataStore.GetSnapshot(snapshotUuid, user);

            // Assert
            Assert.AreEqual(snapshotToSave.Uuid, loadedSnapshot.Uuid);
            Assert.AreEqual(snapshotToSave.Time, loadedSnapshot.Time);
            Assert.AreEqual(snapshotToSave.Drive?.FullPath, loadedSnapshot.Drive?.FullPath);
        }

        [TestMethod]
        public void GetUserMultipleSnapshotsTest()
        {
            // Arrange
            var localFileDataStore = new LocalFileDataStore(new FileObjectSerializer<Snapshot>());
            var timeNow = DateTime.Now;
            var snapshotOneUuid = Guid.NewGuid();
            var snapshotTwoUuid = Guid.NewGuid();
            var snapshotThreeUuid = Guid.NewGuid();
            var snapshotOne = new Snapshot() { Time = timeNow, Uuid = snapshotOneUuid };
            var snapshotTwo = new Snapshot() { Time = timeNow, Uuid = snapshotTwoUuid };
            var snapshotThree = new Snapshot() { Time = timeNow, Uuid = snapshotThreeUuid };
            var userId = Guid.NewGuid();
            var user = new User(userId);

            //Act
            localFileDataStore.SaveSnapshot(snapshotOne, user);
            localFileDataStore.SaveSnapshot(snapshotTwo, user);
            localFileDataStore.SaveSnapshot(snapshotThree, user);
            var loadedUser = localFileDataStore.GetUser(userId);

            // Assert
            Assert.AreEqual(loadedUser.Uuid, user.Uuid);
            Assert.AreEqual(loadedUser.SnapshotIds.Count, user.SnapshotIds.Count);
            foreach (var snapshot in user.SnapshotIds)
            {
                Assert.IsTrue(loadedUser.SnapshotIds.Contains(snapshot));
            }

        }
    }
}
