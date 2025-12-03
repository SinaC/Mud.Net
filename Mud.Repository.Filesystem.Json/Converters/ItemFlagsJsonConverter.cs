using Mud.Server.Flags.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.Repository.Filesystem.Json.Converters
{
    public class ItemFlagsJsonConverter : JsonConverter<IItemFlags>
    {
        private IFlagFactory FlagFactory { get; }

        public ItemFlagsJsonConverter(IFlagFactory flagFactory)
        {
            FlagFactory = flagFactory;
        }

        public override IItemFlags Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => FlagFactory.CreateInstance<IItemFlags, IItemFlagValues>(reader.GetString());

        public override void Write(Utf8JsonWriter writer, IItemFlags flags, JsonSerializerOptions options)
            => writer.WriteStringValue(string.Join(",", flags.Values));
    }
}
