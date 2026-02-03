using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Loot.Interfaces;

public interface IItemGenerator
{
    void AddBlueprintId(int id);

    IItem? Generate(INonPlayableCharacter victim, IItemCorpse? corpse, IRoom room);
    void Enchant(IItem item, int level);
}
