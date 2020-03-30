﻿using System;
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
        public Guid Uuid { get; set; }
        public List<Guid> SnapshotIds { get; set; }
    }
}