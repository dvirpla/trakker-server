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
            var x = 1;
            var drive2 = snapshotProvider.GetDriveInfo(@"C:\");
            var z = 1;
            var diff = SnapshotComparator.CompareListsRecursive(drive1.Children, drive2.Children).ToList();
            var y = 1;
        }

        [TestMethod]
        public void CompareTreeTest()
        {
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
            var x = SnapshotComparator.CompareListsRecursive(root1.Children, root2.Children).ToList();
            var y = 1;
        }
    }
}
