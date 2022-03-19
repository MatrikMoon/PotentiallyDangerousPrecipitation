using ProtoBuf;
using System.IO;

/**
 * Created by Moon on 9/9/2021
 * Extension methods for working with these proto packets
 * Particularly, this helper came around when the need arose for custom equality between proto packets
 */

namespace PotentiallyDangerousPrecipitation.Extensions
{
    public static class ProtobufExtensions
    {
        public static byte[] ProtoSerialize<T>(this T record) where T : class
        {
            if (null == record) return null;

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, record);
                return stream.ToArray();
            }
        }

        public static T ProtoDeserialize<T>(this byte[] data) where T : class
        {
            if (null == data) return null;

            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }
    }
}
