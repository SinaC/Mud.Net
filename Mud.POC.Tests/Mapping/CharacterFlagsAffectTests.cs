using Microsoft.Extensions.DependencyInjection;
using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Riok.Mapperly.Abstractions;

namespace Mud.POC.Tests.Mapping
{
    [TestClass]
    public class CharacterFlagsAffectTests
    {
        [TestMethod]
        public void CharacterFlagsAffect_CharacterFlagsAffectData()
        {
            var characterFlags = new CharacterFlags(new Rom24CharacterFlags());
            characterFlags.Set("Calm", "Invisible");
            var characterFlagsAffect = new CharacterFlagsAffect { Modifier = characterFlags, Operator = AffectOperators.Or };

            var mapper = CreateMapper();
            var characterFlagsAffectData = mapper.ToData(characterFlagsAffect);

            Assert.AreEqual("Calm,Invisible", characterFlagsAffectData.Modifier);
            Assert.AreEqual(AffectOperators.Or, characterFlagsAffectData.Operator);
        }

        [TestMethod]
        public void CharacterFlagsAffectData_CharacterFlagsAffect()
        {
            var characterFlagsAffectData = new CharacterFlagsAffectDataNew { Modifier = "DetectEvil,Curse", Operator = AffectOperators.Assign };

            var mapper = CreateMapper();
            var characterFlagsAffect = mapper.FromData(characterFlagsAffectData);

            Assert.HasCount(2, characterFlagsAffect.Modifier.Values);
            Assert.Contains("DetectEvil", characterFlagsAffect.Modifier.Values);
            Assert.Contains("Curse", characterFlagsAffect.Modifier.Values);
            Assert.AreEqual(AffectOperators.Assign, characterFlagsAffect.Operator);
        }

        private ICharacterFlagsAffectMapper CreateMapper()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IFlagFactory<ICharacterFlags, ICharacterFlagValues>, FlagFactory>();
            services.AddSingleton<ICharacterFlagsAffectMapper, CharacterFlagsAffectMapper>();

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<ICharacterFlagsAffectMapper>();
        }
    }

    public class CharacterFlagsAffectDataNew
    {
        public required AffectOperators Operator { get; set; } // Add and Or are identical

        public required string Modifier { get; set; }
    }

    public interface ICharacterFlagsAffectMapper
    {
        CharacterFlagsAffectDataNew ToData(CharacterFlagsAffect characterFlagsAffect);

        CharacterFlagsAffect FromData(CharacterFlagsAffectDataNew characterFlagsAffect);
    }

    [Mapper]
    public partial class CharacterFlagsAffectMapper : ICharacterFlagsAffectMapper
    {
        private IFlagFactory<ICharacterFlags, ICharacterFlagValues> FlagFactory { get; }

        public CharacterFlagsAffectMapper(IFlagFactory<ICharacterFlags, ICharacterFlagValues> flagFactory)
        {
            FlagFactory = flagFactory;
        }

        [MapProperty(nameof(CharacterFlagsAffect.Modifier), nameof(CharacterFlagsAffectDataNew.Modifier), Use = nameof(MapModifierToData))]
        public partial CharacterFlagsAffectDataNew ToData(CharacterFlagsAffect characterFlagsAffect);

        [MapProperty(nameof(CharacterFlagsAffectDataNew.Modifier), nameof(CharacterFlagsAffect.Modifier), Use = nameof(MapModifierFromData))]
        public partial CharacterFlagsAffect FromData(CharacterFlagsAffectDataNew characterFlagsAffect);

        [UserMapping]
        private string MapModifierToData(ICharacterFlags flags)
            => string.Join(",", flags.Values);

        [UserMapping]
        private ICharacterFlags MapModifierFromData(string flags)
            => FlagFactory.CreateInstance(flags);
    }

    public class FlagFactory : IFlagFactory<ICharacterFlags, ICharacterFlagValues>
    {
        public ICharacterFlags CreateInstance(string? flags)
        {
            var instance = new CharacterFlags(new Rom24CharacterFlags());
            instance.Set(flags);
            return instance;
        }

        public ICharacterFlags CreateInstance(params string[] flags)
        {
            var instance = new CharacterFlags(new Rom24CharacterFlags());
            instance.Set(flags);
            return instance;
        }
    }

    public class Rom24CharacterFlags : FlagValuesBase<string>, ICharacterFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "Blind",
            "Invisible",
            "DetectEvil",
            "DetectInvis",
            "DetectMagic",
            "DetectHidden",
            "DetectGood",
            "FaerieFire",
            "Infrared",
            "Curse",
            "Poison",
            "Sneak",
            "Hide",
            "Sleep",
            "Charm",
            "Flying",
            "PassDoor",
            "Haste",
            "Calm",
            "Plague",
            "Weaken",
            "DarkVision",
            "Berserk",
            "Swim",
            "Regeneration",
            "Slow",
            "Test", // TEST PURPOSE
        };

        protected override HashSet<string> HashSet => Flags;

        public Rom24CharacterFlags()
        {
        }

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            // NOP
        }

        public string PrettyPrint(string flag, bool shortDisplay)
            => flag.ToString();
    }
}
