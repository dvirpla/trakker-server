using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TrakkerServer.DataStore
{
    public class FileObjectSerializer<T> : IObjectSerializer<T>
    {
        public void Serialize(Stream stream, T objectToSerialize)
        {
            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            var formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(stream, objectToSerialize);
            }
            catch (SerializationException e)
            {
                Debug.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
        }

        public T Deserialize(Stream stream)
        {
            var formatter = new BinaryFormatter();
            try
            {
                // Deserialize the hashtable from the file and return it
                return (T)formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Debug.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }

        }
    }
}
