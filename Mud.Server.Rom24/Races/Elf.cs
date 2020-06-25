using Mud.Domain;
using Mud.Logger;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races
{
    public class Elf : PlayableRaceBase
    {
        public Elf(IAbilityManager abilityManager)
            : base(abilityManager)
        {
            AddAbility("Sneak");
            AddAbility("Hide");
        }

        #region IRace

        public override string Name => "elf";
        public override string ShortName => "Elf";

        public override Sizes Size => Sizes.Medium;

        public override ICharacterFlags CharacterFlags => new CharacterFlags();

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Charm;
        public override IRVFlags Vulnerabilities => IRVFlags.Iron;

        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Sentient | BodyForms.Biped | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Arms | BodyParts.Legs | BodyParts.Head | BodyParts.Brains | BodyParts.Guts | BodyParts.Hands | BodyParts.Feet | BodyParts.Fingers | BodyParts.Ear | BodyParts.Eye | BodyParts.Body;

        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.None;

        public override int GetStartAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 12;
                case CharacterAttributes.Intelligence: return 14;
                case CharacterAttributes.Wisdom: return 13;
                case CharacterAttributes.Dexterity: return 15;
                case CharacterAttributes.Constitution: return 11;
                case CharacterAttributes.MaxHitPoints: return 100;
                case CharacterAttributes.SavingThrow: return 0;
                case CharacterAttributes.HitRoll: return 0;
                case CharacterAttributes.DamRoll: return 0;
                case CharacterAttributes.MaxMovePoints: return 100;
                case CharacterAttributes.ArmorBash: return 100;
                case CharacterAttributes.ArmorPierce: return 100;
                case CharacterAttributes.ArmorSlash: return 100;
                case CharacterAttributes.ArmorExotic: return 100;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected start attribute {0} for {1}", attribute, Name);
                    return 0;
            }
        }

        public override int GetMaxAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 16;
                case CharacterAttributes.Intelligence: return 20;
                case CharacterAttributes.Wisdom: return 18;
                case CharacterAttributes.Dexterity: return 21;
                case CharacterAttributes.Constitution: return 15;
                case CharacterAttributes.MaxHitPoints: return 100;
                case CharacterAttributes.SavingThrow: return 0;
                case CharacterAttributes.HitRoll: return 0;
                case CharacterAttributes.DamRoll: return 0;
                case CharacterAttributes.MaxMovePoints: return 100;
                case CharacterAttributes.ArmorBash: return 100;
                case CharacterAttributes.ArmorPierce: return 100;
                case CharacterAttributes.ArmorSlash: return 100;
                case CharacterAttributes.ArmorExotic: return 100;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected max attribute {0} for {1}", attribute, Name);
                    return 0;
            }
        }

        public override int ClassExperiencePercentageMultiplier(IClass c) => c is Classes.Cleric ? 125 : 100;

        #endregion
    }
}
