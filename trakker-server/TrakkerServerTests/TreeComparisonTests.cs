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

namespace TrakkerServerTests
{
    [TestClass]
    public class TreeComparisonTests
    {
        private static IEnumerable<FileSystemNode> CompareFlatListsRecursive(List<FileSystemNode> list1,
            List<FileSystemNode> list2)
        {
            var itemMapping = new Dictionary<string, FileSystemNode>();
            var visited = new Dictionary<string, bool>();

            foreach (var item1 in list1)
            {
                visited[item1.FullPath] = false;
                itemMapping[item1.FullPath] = item1;
            }

            foreach (var item2 in list2)
            {
                var item2dir = item2 as DirectoryInfo;
                var isDir = item2dir != null;
                var item2file = item2 as FileInfo;

                // Modified
                if (itemMapping.TryGetValue(item2.FullPath, out var existingItem))
                {
                    visited[item2.FullPath] = true;

                    if (existingItem.Size == item2.Size)
                    {
                        yield return existingItem;
                    }
                    else
                    {
                        if (isDir)
                        {
                            var item1dir = existingItem as DirectoryInfo;
                            yield return new ChangedDirectory()
                            {
                                FullPath = item2dir.FullPath,
                                Children = new List<FileSystemNode>(
                                    CompareFlatListsRecursive(item1dir.Children, item2dir.Children)),
                                OldFileSystemNode = item1dir,
                                Size = item2dir.Size,
                                Status = ChangedSystemNodeStatus.Modified
                            };
                        }
                        else
                        {
                            yield return new ChangedFile()
                            {
                                FullPath = existingItem.FullPath,
                                OldFileSystemNode = existingItem,
                                Size = item2.Size,
                                Status = ChangedSystemNodeStatus.Modified
                            };
                        }
                    }
                }
                // New
                else
                {
                    if (isDir)
                    {
                        yield return new ChangedDirectory()
                        {
                            FullPath = item2.FullPath,
                            Size = item2.Size,
                            Status = ChangedSystemNodeStatus.New,
                            Children = item2dir.Children
                        };
                    }
                    else
                    {
                        yield return new ChangedFile()
                        {
                            FullPath = item2.FullPath,
                            Size = item2.Size,
                            Status = ChangedSystemNodeStatus.New
                        };
                    }
                }
            }

            // Deleted
            foreach (var item1 in list1)
            {
                var item1dir = item1 as DirectoryInfo;
                var isDir = item1dir != null;

                if (!visited[item1.FullPath])
                {
                    if (isDir)
                    {
                        yield return new ChangedDirectory()
                        {
                            FullPath = item1.FullPath,
                            OldFileSystemNode = item1,
                            Size = 0,
                            Status = ChangedSystemNodeStatus.Deleted,
                            Children = item1dir.Children
                        };
                    }
                    else
                    {
                        yield return new ChangedFile()
                        {
                            FullPath = item1.FullPath,
                            OldFileSystemNode = item1,
                            Size = 0,
                            Status = ChangedSystemNodeStatus.Deleted
                        };
                    }
                }
            }
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
