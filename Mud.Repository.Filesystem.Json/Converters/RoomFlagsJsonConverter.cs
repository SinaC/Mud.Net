using Mud.Server.Flags.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.Repository.Filesystem.Json.Converters
{
    public class RoomFlagsJsonConverter : JsonConverter<IRoomFlags>
    {
        private IFlagFactory FlagFactory { get; }

        public RoomFlagsJsonConverter(IFlagFactory flagFactory)
        {
            FlagFactory = flagFactory;
        }

        public override IRoomFlags Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => FlagFactory.CreateInstance<IRoomFlags, IRoomFlagValues>(reader.GetString());

        public override void Write(Utf8JsonWriter writer, IRoomFlags flags, JsonSerializerOptions options)
            => writer.WriteStringValue(string.Join(",", flags.Values));
    }
}
