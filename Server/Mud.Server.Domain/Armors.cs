using Mud.Domain;

namespace Mud.Server.Domain;

public enum Armors // must have the same values as CharacterAttributes
{
    Bash        = CharacterAttributes.ArmorBash,
    Pierce      = CharacterAttributes.ArmorPierce,
    Slash       = CharacterAttributes.ArmorSlash,
    Exotic      = CharacterAttributes.ArmorExotic,
}
