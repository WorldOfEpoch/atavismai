using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public class SerializationSurrogateUtility
    {
        public static BinaryFormatter GetBinaryFormatter()
        {
            BinaryFormatter bf = new();
            SurrogateSelector surrogateSelector = new();
            Vector3SerializationSurrogate vector3Ss = new();
            surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Ss);

            QuaternionSerializationSurrogate quaterions = new();
            surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaterions);
            bf.SurrogateSelector = surrogateSelector;

            return bf;
        }
    }

    public class Vector3SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)  // called to serialize a Vector3 object
        {
            Vector3 v3 = (Vector3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)    // method called to deserialize a Vector3 object
        {
            Vector3 v3 = (Vector3)obj;
            v3.x = (float)info.GetValue("x", typeof(float));
            v3.y = (float)info.GetValue("y", typeof(float));
            v3.z = (float)info.GetValue("z", typeof(float));
            obj = v3;
            return obj;
        }
    }

    public class QuaternionSerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)  // method called to serialize a Vector3 object
        {
            Quaternion q = (Quaternion)obj;
            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("z", q.z);
            info.AddValue("w", q.w);
        }

        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)    // method called to deserialize a Vector3 object
        {
            Quaternion q = (Quaternion)obj;
            q.x = (float)info.GetValue("x", typeof(float));
            q.y = (float)info.GetValue("y", typeof(float));
            q.z = (float)info.GetValue("z", typeof(float));
            q.w = (float)info.GetValue("w", typeof(float));
            obj = q;
            return obj;
        }
    }
}