using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TrakkerServer.DataStore
{
    public interface IObjectSerializer
    {
        // CR: I think that a generic method will be better in this case
        public void Serialize(Stream stream, object objectToSerialize);

        // CR: I think that a generic method will be better in this case
        public object Deserialize(Stream stream);
    }
}
