using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mppurge", "MobProgram", Hidden = true)]
[Syntax("syntax mob purge {target}")]
[Help(
@"Lets the mobile purge all objects and other npcs in the room,
or purge a specified object or mob in the room. The mobile cannot
purge itself for safety reasons.")]
public class Purge : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [];

    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }

    public Purge(ICharacterManager characterManager, IItemManager itemManager, IWiznet wiznet)
    {
        ItemManager = itemManager;
        CharacterManager = characterManager;
    }

    private IEntity Target { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // no parameter -> room
        if (actionInput.Parameters.Length == 0)
        {
            Target = Actor.Room;
            return null;
        }

        // search npc (including actor)
        var npc = FindHelpers.FindByName(Actor.Room.NonPlayableCharacters, actionInput.Parameters[0])!;
        if (npc != null)
        {
            if (npc.ActFlags.IsSet("NoPurge"))
                return "It can't be purged.";
            Target = npc;
            return null;
        }

        // search item
        var item = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0])!;
        if (item == null)
            return StringHelpers.ItemNotFound;
        if (item.ItemFlags.IsSet("NoPurge"))
            return "It can't be purged.";
        Target = item;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Target)
        {
            case IRoom room:
                PurgeRoom(room);
                break;
            case INonPlayableCharacter nonPlayableCharacter:
                PurgeNonPlayableCharacter(nonPlayableCharacter);
                break;
            case IItem item:
                PurgeItem(item);
                break;
        }
    }

    private void PurgeRoom(IRoom room)
    {
        // purge npcs (except Actor) (without NoPurge flag) (TODO: what if npc was wearing NoPurge items?)
        var npcs = room.NonPlayableCharacters.Where(x => x != Actor && !x.ActFlags.IsSet("NoPurge")).ToArray(); // clone
        foreach (var nonPlayableCharacter in npcs)
            CharacterManager.RemoveCharacter(nonPlayableCharacter);
        // purge items (without NoPurge flag)
        var items = room.Content.Where(x => !x.ItemFlags.IsSet("NoPurge")).ToArray(); // clone
        foreach (var itemToPurge in items)
            ItemManager.RemoveItem(itemToPurge);
    }

    private void PurgeNonPlayableCharacter(INonPlayableCharacter npc)
        => CharacterManager.RemoveCharacter(npc);

    private void PurgeItem(IItem item)
        => ItemManager.RemoveItem(item);
}
