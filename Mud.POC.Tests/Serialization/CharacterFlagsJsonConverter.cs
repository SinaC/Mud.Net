using Mud.Server.Flags.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.POC.Tests.Serialization
{
    public class CharacterFlagsJsonConverter : JsonConverter<ICharacterFlags>
    {
        private IFlagFactory FlagFactory { get; }

        public CharacterFlagsJsonConverter(IFlagFactory flagFactory)
        {
            FlagFactory = flagFactory;
        }

        public override ICharacterFlags Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => FlagFactory.CreateInstance<ICharacterFlags, ICharacterFlagValues>(reader.GetString());

        public override void Write(Utf8JsonWriter writer, ICharacterFlags flags, JsonSerializerOptions options)
            => writer.WriteStringValue(string.Join(",", flags.Values));
    }
}
