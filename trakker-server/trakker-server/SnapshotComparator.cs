using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TrakkerModels;
using DirectoryInfo = TrakkerModels.DirectoryInfo;
using DriveInfo = TrakkerModels.DriveInfo;

namespace TrakkerServer
{
    public class SnapshotComparator
    {
        public static FileSystemNode CompareSnapshots(Snapshot snapshotOne, Snapshot snapshotTwo)
        {
            var childrenDiff = CompareListsRecursive(snapshotOne.Drive.Children, snapshotTwo.Drive.Children).ToList();
            var childrenDiffSize = childrenDiff.Aggregate<FileSystemNode, ulong>(0, (current, child) => current + child.Size);
            var isChanged = childrenDiff.OfType<ChangedFileSystemNode>().Count() != 0;
            if (isChanged)
            {
                return new ChangedDirectory(snapshotTwo.Drive.FullPath, childrenDiffSize, childrenDiff, ChangedSystemNodeStatus.Modified);
            }
            return new DirectoryInfo(snapshotTwo.Drive.FullPath, childrenDiff, childrenDiffSize);
        }
        public static IEnumerable<FileSystemNode> CompareListsRecursive(List<FileSystemNode> firstList,
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

            #region Check for New Modified and UnChanged

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
                                    CompareListsRecursive(item1dir.Children, item2dir.Children)),
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
            #endregion

            #region Check for Deleted
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
            #endregion
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
    }
}
