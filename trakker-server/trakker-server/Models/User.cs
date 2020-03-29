using System;
using System.Collections.Generic;
using System.Text;
using TrakkerModels;

namespace TrakkerServer.Models
{
    public class User
    {
        public Guid Uuid { get; set; }
        public List<Guid> SnapshotIds { get; set; }
    }
}
