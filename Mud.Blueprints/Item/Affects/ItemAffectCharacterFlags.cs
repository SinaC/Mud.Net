using Mud.Server.Flags.Interfaces;

namespace Mud.Blueprints.Item.Affects;

public class ItemAffectCharacterFlags : ItemAffectBase // used for WhereToAttributeOrResource
{
    public ICharacterFlags CharacterFlags { get; set; } = default!;
}
