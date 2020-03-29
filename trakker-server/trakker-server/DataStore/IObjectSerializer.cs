using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TrakkerServer.DataStore
{
    public interface IObjectSerializer
    {
        public void Serialize(Stream stream, object objectToSerialize);
        public object Deserialize(Stream stream);
    }
}
