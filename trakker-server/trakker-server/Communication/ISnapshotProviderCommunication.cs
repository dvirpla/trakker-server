using System;
using System.Collections.Generic;
using System.Text;
using TrakkerModels;

namespace TrakkerServer.Communication
{
    public interface ISnapshotProviderCommunication
    {
        List<System.IO.DriveInfo> GetDrivesMetadata();

        DriveInfo GetDriveInfo(string driveName);

    }
}
