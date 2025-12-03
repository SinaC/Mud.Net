using Mud.Server.Flags.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.Repository.Filesystem.Json.Converters
{
    public class IRVFlagsJsonConverter : JsonConverter<IIRVFlags>
    {
        private IFlagFactory FlagFactory { get; }

        public IRVFlagsJsonConverter(IFlagFactory flagFactory)
        {
            FlagFactory = flagFactory;
        }

        public override IIRVFlags Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>(reader.GetString());

        public override void Write(Utf8JsonWriter writer, IIRVFlags flags, JsonSerializerOptions options)
            => writer.WriteStringValue(string.Join(",", flags.Values));
    }
}
