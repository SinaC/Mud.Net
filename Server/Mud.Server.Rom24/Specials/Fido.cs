using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Special;

namespace Mud.Server.Rom24.Specials;

[SpecialBehavior("spec_fido")]
public class Fido : ISpecialBehavior
{
    private IItemManager ItemManager { get; }

    public Fido(IItemManager itemManager)
    {
        ItemManager = itemManager;
    }

    public bool Execute(INonPlayableCharacter npc)
    {
        // must be awake
        if (!npc.IsValid || npc.Room == null
            || npc.Position <= Positions.Sleeping)
            return false;

        // devours npc corpse
        var npcCorpse = npc.Room.Content.OfType<IItemCorpse>().FirstOrDefault(x => !x.IsPlayableCharacterCorpse);
        if (npcCorpse == null)
            return false;

        npc.Act(ActOptions.ToRoom, "{0} savagely devours a corpse.", npc);
        // move corpse content to the ground and destroy corpse
        var corpseContent = npcCorpse.Content.ToArray();
        foreach (var item in corpseContent)
            item.ChangeContainer(npc.Room);
        ItemManager.RemoveItem(npcCorpse);

        return true;
    }
}
