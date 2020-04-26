using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrakkerModels;
using TrakkerServer;
using DirectoryInfo = TrakkerModels.DirectoryInfo;
using DriveInfo = TrakkerModels.DriveInfo;
using FileInfo = TrakkerModels.FileInfo;
using SnapshotProvider = SnapshotProvider.SnapshotProvider;

namespace TrakkerServerTests
{
    [TestClass]
    public class TreeComparisonTests
    {
     

        [TestMethod]
        public void CompareTreeTestUsingSnapshot()
        {

            var snapshotProvider = new global::SnapshotProvider.SnapshotProvider();
            var drive1 = snapshotProvider.GetDriveInfo(@"C:\");
            var drive2 = snapshotProvider.GetDriveInfo(@"C:\");
            var diff = SnapshotComparator.CompareListsRecursive(drive1.Children, drive2.Children).ToList();
        }

        [TestMethod]
        public void CompareTreeTest()
        {
            // Arrange
            var root1 = new TrakkerModels.DirectoryInfo("C:\\root", new List<FileSystemNode>()
            {
                new TrakkerModels.DirectoryInfo("C:\\root\\unchanged", new List<FileSystemNode>()
                {
                    new FileInfo(1, "C:\\root\\unchanged\\removed.txt"),
                    new FileInfo(1, "C:\\root\\unchanged\\modified.txt")
                }, 2),
                new TrakkerModels.DirectoryInfo("C:\\root\\unchanged2"),
                new TrakkerModels.DirectoryInfo("C:\\root\\charlie"),
                new TrakkerModels.FileInfo(300, "C:\\root\\modified.txt"),
                new TrakkerModels.FileInfo(2, "C:\\root\\unchanged.txt")
            });

            var root2 = new TrakkerModels.DirectoryInfo("C:\\root", new List<FileSystemNode>()
            {
                new TrakkerModels.DirectoryInfo("C:\\root\\unchanged", new List<FileSystemNode>()
                {
                    new FileInfo(2, "C:\\root\\unchanged\\new.png"),
                    new FileInfo(5, "C:\\root\\unchanged\\modified.txt")
                }, 7),
                new TrakkerModels.DirectoryInfo("C:\\root\\new"),
                new TrakkerModels.DirectoryInfo("C:\\root\\unchanged2"),
                new TrakkerModels.FileInfo(350, "C:\\root\\modified.txt"),
                new TrakkerModels.FileInfo(2, "C:\\root\\unchanged.txt")
            });
            var snapshotOne = new Snapshot() { Time = DateTime.Now, Uuid = Guid.NewGuid(), Drive = new DriveInfo(@"C:\", root1)};
            var snapshotTwo = new Snapshot() { Time = DateTime.Now, Uuid = Guid.NewGuid(), Drive = new DriveInfo(@"C:\", root2)};

            // Act
            var comparedSnapshot = SnapshotComparator.CompareSnapshots(snapshotOne, snapshotTwo);
            // Assert
            Assert.IsInstanceOfType(comparedSnapshot, typeof(ChangedDirectory));
            if (comparedSnapshot is ChangedDirectory comparedSnapshotAsDirectory)
            {
                // Check New
                var newDir = comparedSnapshotAsDirectory.Children.Find(x => x.FullPath == "C:\\root\\new"); 
                var newDirChanged = newDir as ChangedDirectory;
                Assert.IsInstanceOfType(newDir, typeof(ChangedDirectory));
                Assert.AreEqual(newDirChanged.Status, ChangedSystemNodeStatus.New);
                // Check Modified
                var modifiedFile = comparedSnapshotAsDirectory.Children.Find(x => x.FullPath == "C:\\root\\modified.txt");
                var modifiedFileChanged = modifiedFile as ChangedFile;
                Assert.IsInstanceOfType(modifiedFile, typeof(ChangedFile));
                Assert.AreEqual(modifiedFileChanged.Status, ChangedSystemNodeStatus.Modified);
            }
        }
    }
}
