using Mud.Domain;

namespace Mud.Server.Common.Extensions;

public static class CharacterAttributesExtensions
{
    public static string ShortName(this CharacterAttributes attribute)
    {
        switch (attribute)
        {
            case CharacterAttributes.Strength: return "Str";
            case CharacterAttributes.Intelligence: return "Int";
            case CharacterAttributes.Wisdom: return "Wis";
            case CharacterAttributes.Dexterity: return "Dex";
            case CharacterAttributes.Constitution: return "Con";
            case CharacterAttributes.SavingThrow: return "Saves";
            case CharacterAttributes.HitRoll: return "Hit";
            case CharacterAttributes.DamRoll: return "Dam";
            case CharacterAttributes.ArmorBash: return "AcB";
            case CharacterAttributes.ArmorPierce: return "AcP";
            case CharacterAttributes.ArmorSlash: return "AcS";
            case CharacterAttributes.ArmorExotic: return "AcE";
            default:
                return attribute.ToString();
        }
    }
}
