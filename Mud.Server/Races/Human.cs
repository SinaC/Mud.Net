using Mud.Domain;

namespace Mud.Server.Races
{
    public class Human : RaceBase
    {
        #region IRace

        public override string Name => "human";
        public override string ShortName => "Hum";

        public override Sizes Size => Sizes.Medium;

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.None;

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
                    Wiznet.Wiznet($"Unexpected attribute {attribute} for Human", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                    Wiznet.Wiznet($"Unexpected attribute {attribute} for Human", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return 0;
            }
        }

        #endregion
    }
}
