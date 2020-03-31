using System;
using System.Collections.Generic;
using System.Text;

namespace TrakkerServer.Exceptions
{
    public class SnapshotNotFoundException : Exception
    {
        public SnapshotNotFoundException()
        {
        }

        public SnapshotNotFoundException(string message)
            : base(message)
        {
        }

        public SnapshotNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
