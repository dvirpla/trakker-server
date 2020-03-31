using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TrakkerServer.DataStore
{
    public interface IObjectSerializer<T>
    {
        void Serialize(Stream stream, T objectToSerialize);

        T Deserialize(Stream stream);
    }
}
