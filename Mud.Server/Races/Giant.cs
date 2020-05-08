using Mud.Domain;
using Mud.Logger;

namespace Mud.Server.Races
{
    public class Giant : RaceBase
    {
        public Giant()
        {
            AddAbility("Bash");
            AddAbility("Fast healing");
        }

        #region IRace

        public override string Name => "giant";

        public override string ShortName => "Gia";

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Fire | IRVFlags.Cold;
        public override IRVFlags Vulnerabilities => IRVFlags.Mental | IRVFlags.Lightning;

        public override int GetStartAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 16;
                case CharacterAttributes.Intelligence: return 11;
                case CharacterAttributes.Wisdom: return 13;
                case CharacterAttributes.Dexterity: return 11;
                case CharacterAttributes.Constitution: return 14;
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
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected attribute {0} for Giant", attribute);
                    return 0;
            }
        }

        public override int GetMaxAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 22;
                case CharacterAttributes.Intelligence: return 15;
                case CharacterAttributes.Wisdom: return 18;
                case CharacterAttributes.Dexterity: return 15;
                case CharacterAttributes.Constitution: return 20;
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
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected attribute {0} for Giant", attribute);
                    return 0;
            }
        }

        public override int ClassExperiencePercentageMultiplier(IClass c)
        {
            if (c is Classes.Mage)
                return 200;
            if (c is Classes.Priest)
                return 150;
            if (c is Classes.Thief)
                return 150;
            if (c is Classes.Warrior)
                return 105;
            return 100;
        }

        #endregion
    }
}
