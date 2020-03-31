using System;
using System.Collections.Generic;
using System.Text;
using TrakkerModels;

namespace TrakkerServer.Models
{
    public class User
    {
        public User(Guid uuid)
        {
            this.Uuid = uuid;
            this.SnapshotIds = new List<Guid>();
        }

        public User(Guid uuid, List<Guid> snapshotIds)
        {
            this.Uuid = uuid;
            this.SnapshotIds = snapshotIds;
        }

        public Guid Uuid { get; }

        public List<Guid> SnapshotIds { get; }
    }
}
