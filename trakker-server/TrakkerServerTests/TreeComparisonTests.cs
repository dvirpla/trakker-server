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
                            yield return new ChangedDirectory(item2.FullPath, item2.Size, new List<FileSystemNode>(
                                    CompareFlatListsRecursive(item1dir.Children, item2dir.Children)),
                                ChangedSystemNodeStatus.Modified, item1dir.Size);
                        }
                        else
                        {
                            yield return new ChangedFile(existingItem.FullPath, item2.Size,
                                ChangedSystemNodeStatus.Modified, existingItem.Size);
                        }
                    }
                }
                // New
                else
                {
                    if (isDir)
                    {
                        yield return new ChangedDirectory(item2.FullPath, item2.Size,
                            ChangeStatusForChildren(item2dir.Children, ChangedSystemNodeStatus.New).ToList(),
                            ChangedSystemNodeStatus.New);
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
                        yield return new ChangedDirectory(item1.FullPath, 0,
                            ChangeStatusForChildren(item1dir.Children, ChangedSystemNodeStatus.Deleted).ToList(),
                            ChangedSystemNodeStatus.Deleted, item1.Size);
                    }
                    else
                    {
                        yield return new ChangedFile(item1.FullPath, 0, ChangedSystemNodeStatus.Deleted, item1.Size);
                    }
                }
            }
        }

        private static IEnumerable<FileSystemNode> ChangeStatusForChildren(List<FileSystemNode> children,
            ChangedSystemNodeStatus statusToChange)
        {
            foreach (var child in children)
            {
                var childDir = child as DirectoryInfo;
                var isDir = childDir != null;
                if (isDir)
                {
                    if (statusToChange == ChangedSystemNodeStatus.New)
                    {
                        yield return new ChangedDirectory(childDir.FullPath, childDir.Size,
                            ChangeStatusForChildren(childDir.Children, statusToChange).ToList(), statusToChange);
                    }
                    else
                    {
                        yield return new ChangedDirectory(childDir.FullPath, childDir.Size,
                            ChangeStatusForChildren(childDir.Children, statusToChange).ToList(), statusToChange,
                            childDir.Size);
                    }
                }
                else
                {
                    if (statusToChange == ChangedSystemNodeStatus.New)
                    {
                        yield return new ChangedFile(child.FullPath, child.Size, statusToChange);
                    }
                    else
                    {
                        yield return new ChangedFile(child.FullPath, child.Size, statusToChange, child.Size);
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
            var z = ChangeStatusForChildren(root1.Children, ChangedSystemNodeStatus.New).ToList();
            var x = CompareFlatListsRecursive(root1.Children, root2.Children).ToList();
            var y = 1;
        }
    }
}
