using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrakkerModels;
using DirectoryInfo = TrakkerModels.DirectoryInfo;
using FileInfo = TrakkerModels.FileInfo;
using SnapshotProvider = SnapshotProvider.SnapshotProvider;

namespace TrakkerServerTests
{
    [TestClass]
    public class TreeComparisonTests
    {
        private static IEnumerable<FileSystemNode> CompareFlatListsRecursive(List<FileSystemNode> firstList,
            List<FileSystemNode> secondList)
        {
            #region Lists initializing
            var itemMapping = new Dictionary<string, FileSystemNode>();
            var visited = new Dictionary<string, bool>();

            foreach (var item1 in firstList)
            {
                visited[item1.FullPath] = false;
                itemMapping[item1.FullPath] = item1;
            }
            #endregion

            foreach (var item2 in secondList)
            {
                var item2dir = item2 as DirectoryInfo;
                var isDir = item2dir != null;

                if (itemMapping.TryGetValue(item2.FullPath, out var existingItem))
                {
                    visited[item2.FullPath] = true;
                    // UnChanged
                    if (existingItem.Size == item2.Size)
                    {
                        yield return existingItem;
                    }
                    // Modified
                    else
                    {
                        if (isDir)
                        {
                            var item1dir = existingItem as DirectoryInfo;
                            yield return new ChangedDirectory(item2.FullPath, item2.Size,
                                ChangedSystemNodeStatus.Modified, new List<FileSystemNode>(
                                    CompareFlatListsRecursive(item1dir?.Children, item2dir.Children)), item1dir);
                        }
                        else
                        {
                            yield return new ChangedFile(existingItem.FullPath, item2.Size,
                                ChangedSystemNodeStatus.Modified, existingItem);
                        }
                    }
                }
                // New
                else
                {
                    if (isDir)
                    {
                        // TODO: Make all children New.
                        yield return new ChangedDirectory(item2.FullPath, item2.Size, ChangedSystemNodeStatus.New,
                            item2dir.Children);
                    }
                    else
                    {
                        yield return new ChangedFile(item2.FullPath, item2.Size, ChangedSystemNodeStatus.New);
                    }
                }
            }

            // Deleted
            foreach (var item1 in firstList)
            {
                var item1dir = item1 as DirectoryInfo;
                var isDir = item1dir != null;

                if (!visited[item1.FullPath])
                {
                    if (isDir)
                    {
                        // TODO: Make all children deleted.
                        yield return new ChangedDirectory(item1.FullPath, 0, ChangedSystemNodeStatus.Deleted,
                            item1dir.Children, item1);
                    }
                    else
                    {
                        yield return new ChangedFile(item1.FullPath, 0, ChangedSystemNodeStatus.Deleted, item1);
                    }
                }
            }
        }

        [TestMethod]
        public void CompareTreeTestUsingSnapshot()
        {
            
            var snapshotProvider = new global::SnapshotProvider.SnapshotProvider();
            var drive1 = snapshotProvider.GetDriveInfo(@"C:\");
            var x = 1;
            var drive2 = snapshotProvider.GetDriveInfo(@"C:\");
            var z = 1;
            var diff = CompareFlatListsRecursive(drive1.Children, drive2.Children).ToList();
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

            var x = CompareFlatListsRecursive(root1.Children, root2.Children).ToList();
            var y = 1;
        }
    }
}
