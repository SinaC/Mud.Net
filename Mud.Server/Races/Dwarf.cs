using Mud.Domain;

namespace Mud.Server.Races
{
    public class Dwarf : RaceBase
    {
        public Dwarf()
        {
            AddAbility("Berserk");
        }

        #region IRace

        public override string Name => "dwarf";
        public override string ShortName => "Dwa";

        public override Sizes Size => Sizes.Small;

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Poison | IRVFlags.Disease;
        public override IRVFlags Vulnerabilities => IRVFlags.Drowning;

        public override int GetStartAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 14;
                case CharacterAttributes.Intelligence: return 12;
                case CharacterAttributes.Wisdom: return 14;
                case CharacterAttributes.Dexterity: return 10;
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
                    Wiznet.Wiznet($"Unexpected attribute {attribute} for Dwarf", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return 0;
            }
        }

        public override int GetMaxAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 20;
                case CharacterAttributes.Intelligence: return 16;
                case CharacterAttributes.Wisdom: return 19;
                case CharacterAttributes.Dexterity: return 14;
                case CharacterAttributes.Constitution: return 21;
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
                    Wiznet.Wiznet($"Unexpected attribute {attribute} for Dwarf", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return 0;
            }
        }

        public override int ClassExperiencePercentageMultiplier(IClass c) 
        {
            if (c is Classes.Mage)
                return 150;
            if (c is Classes.Cleric)
                return 125;
            return 100;
        }

        #endregion
    }
}
