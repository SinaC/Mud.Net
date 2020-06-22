using Mud.Domain;
using Mud.Logger;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races
{
    public class Human : PlayableRaceBase
    {
        public Human(IAbilityManager abilityManager)
            : base(abilityManager)
        {
        }

        #region IRace

        public override string Name => "human";
        public override string ShortName => "Hum";

        public override Sizes Size => Sizes.Medium;

        public override CharacterFlags CharacterFlags => CharacterFlags.None;

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.None;

        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Sentient | BodyForms.Biped | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Arms | BodyParts.Legs | BodyParts.Head | BodyParts.Brains | BodyParts.Guts | BodyParts.Hands | BodyParts.Feet | BodyParts.Fingers | BodyParts.Ear | BodyParts.Eye | BodyParts.Body;

        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.None;

        public override int GetStartAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 13;
                case CharacterAttributes.Intelligence: return 13;
                case CharacterAttributes.Wisdom: return 13;
                case CharacterAttributes.Dexterity: return 13;
                case CharacterAttributes.Constitution: return 13;
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
                case CharacterAttributes.Strength: return 18;
                case CharacterAttributes.Intelligence: return 18;
                case CharacterAttributes.Wisdom: return 18;
                case CharacterAttributes.Dexterity: return 18;
                case CharacterAttributes.Constitution: return 18;
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

        #endregion
    }
}
