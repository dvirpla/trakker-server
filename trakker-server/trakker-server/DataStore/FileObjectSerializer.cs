using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TrakkerServer.DataStore
{
    public class FileObjectSerializer : IObjectSerializer
    {
        public void Serialize(Stream stream, object objectToSerialize)
        {
            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            var formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(stream, objectToSerialize);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
        }

        public object Deserialize(Stream stream)
        {
            var formatter = new BinaryFormatter();
            try
            {
                // Deserialize the hashtable from the file and returns it.
                return formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }

        }
    }
}
