using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SnapshotProvider;

namespace TrakkerServer.Communication
{
    public class LocalSnapshotProviderCommunication : ISnapshotProviderCommunication
    {
        public LocalSnapshotProviderCommunication(ISnapshotProvider snapshotProvider)
        {
            this.SnapshotProvider = snapshotProvider;
        }

        private ISnapshotProvider SnapshotProvider { get; }

        public List<DriveInfo> GetDrivesMetadata()
        {
            return this.SnapshotProvider.GetDrivesMetadata();
        }

        public TrakkerModels.DriveInfo GetDriveInfo(string driveName)
        {
            return this.SnapshotProvider.GetDriveInfo(driveName);
        }
    }
}
