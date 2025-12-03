using Mud.Server.Flags.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.Repository.Filesystem.Json.Converters
{
    public class WeaponFlagsJsonConverter : JsonConverter<IWeaponFlags>
    {
        private IFlagFactory FlagFactory { get; }

        public WeaponFlagsJsonConverter(IFlagFactory flagFactory)
        {
            FlagFactory = flagFactory;
        }

        public override IWeaponFlags Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => FlagFactory.CreateInstance<IWeaponFlags, IWeaponFlagValues>(reader.GetString());

        public override void Write(Utf8JsonWriter writer, IWeaponFlags flags, JsonSerializerOptions options)
            => writer.WriteStringValue(string.Join(",", flags.Values));
    }
}
