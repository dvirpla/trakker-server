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

        // CR: (Kfir) Make the property readonly (no setter), I don't think the values ever changes
        public Guid Uuid { get; set; }

        // CR: (Kfir) Make the property readonly (no setter), I don't think the values ever changes
        public List<Guid> SnapshotIds { get; set; }
    }
}
