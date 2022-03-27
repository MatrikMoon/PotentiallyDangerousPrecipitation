using ProtoBuf;
using Protos.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/**
 * Created by Moon on 9/9/2021
 * Extension methods for working with these proto packets
 * Particularly, this helper came around when the need arose for custom equality between proto packets
 */

namespace PotentiallyDangerousPrecipitation.Extensions
{
    public static class ProtobufExtensions
    {
        public static bool UserEquals(this User firstUser, User secondUser)
        {
            if ((firstUser == null) ^ (secondUser == null)) return false;
            else if ((firstUser == null) && (secondUser == null)) return true;
            return firstUser.Id == secondUser.Id;
        }

        public static bool ContainsUser(this IEnumerable<User> users, User user)
        {
            return users.Any(x => x.UserEquals(user));
        }

        public static bool PlayerEquals(this Player firstPlayer, Player secondPlayer)
        {
            return firstPlayer.User.UserEquals(secondPlayer.User);
        }

        public static bool ContainsPlayer(this IEnumerable<Player> players, Player player)
        {
            return players.Any(x => x.PlayerEquals(player));
        }

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
