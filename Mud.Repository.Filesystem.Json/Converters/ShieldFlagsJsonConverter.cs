using Mud.Server.Flags.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.Repository.Filesystem.Json.Converters
{
    public class ShieldFlagsJsonConverter : JsonConverter<IShieldFlags>
    {
        private IFlagFactory FlagFactory { get; }

        public ShieldFlagsJsonConverter(IFlagFactory flagFactory)
        {
            FlagFactory = flagFactory;
        }

        public override IShieldFlags Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => FlagFactory.CreateInstance<IShieldFlags, IShieldFlagValues>(reader.GetString());

        public override void Write(Utf8JsonWriter writer, IShieldFlags flags, JsonSerializerOptions options)
            => writer.WriteStringValue(string.Join(",", flags.Values));
    }
}
