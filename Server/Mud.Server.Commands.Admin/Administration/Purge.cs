using Mud.Flags;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("purge", "Admin", Priority = 999, NoShortcut = true)]
[Syntax(
    "[cmd] all",
    "[cmd] <character>",
    "[cmd] <item>")]
[Help(
@"[cmd] is used to clean up the world.
[cmd] with no arguments removes all the NPC's and objects in the current room.
[cmd] with an argument purges one character from anywhere in the world.

Purge will not get rid of PC's, or objects or mobiles with a NOPURGE flag
set (i.e. the pit, the fountain, shopkeepers, Hassan).  Mobiles may be
purged if they are called directly by name.")]
public class Purge : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new MustBeImpersonated(), new RequiresAtLeastOneArgument()];

    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }
    private IWiznet Wiznet { get; }

    public Purge(ICharacterManager characterManager, IItemManager itemManager, IWiznet wiznet)
    {
        ItemManager = itemManager;
        CharacterManager = characterManager;
        Wiznet = wiznet;
    }

    private IEntity Target { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // All -> room
        if (actionInput.Parameters[0].IsAll)
        {
            Target = Impersonating.Room;
            return null;
        }

        // Search NPC
        var npc = FindHelpers.FindNonPlayableChararacterInWorld(CharacterManager, Actor.Impersonating!, actionInput.Parameters[0])!;
        if (npc != null)
        {
            if (npc.ActFlags.IsSet("NoPurge"))
                return "It can't be purged.";
            Target = npc;
            return null;
        }

        // Search Item
        var item = FindHelpers.FindItemHere(Actor.Impersonating!, actionInput.Parameters[0])!;
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
        Wiznet.Log($"{Actor.DisplayName} purges room {room.Blueprint.Id}.", new WiznetFlags("Punish"));

        // Purge non playable characters (without NoPurge flag) (TODO: what if npc was wearing NoPurge items?)
        var nonPlayableCharacters = room.NonPlayableCharacters.Where(x => !x.ActFlags.IsSet("NoPurge")).ToArray(); // clone
        foreach (var nonPlayableCharacter in nonPlayableCharacters)
            CharacterManager.RemoveCharacter(nonPlayableCharacter);
        // Purge items (without NoPurge flag)
        var items = room.Content.Where(x => !x.ItemFlags.IsSet("NoPurge")).ToArray(); // clone
        foreach (var itemToPurge in items)
            ItemManager.RemoveItem(itemToPurge);
        Impersonating.Act(ActOptions.ToRoom, "{0} purge{0:v} the room!", Actor.Impersonating!);
        Actor.Send("Ok.");
    }

    private void PurgeNonPlayableCharacter(INonPlayableCharacter npc)
    {
        Wiznet.Log($"{Actor.DisplayName} purges npc {npc.DebugName}.", new WiznetFlags("Punish"));

        npc.Act(ActOptions.ToRoom, "{0} purge{0:v} {1}.", Actor.Impersonating!, npc);
        CharacterManager.RemoveCharacter(npc);
        Actor.Send("Ok.");
    }

    private void PurgeItem(IItem item)
    {
        Wiznet.Log($"{Actor.DisplayName} purges item {item.DebugName}.", new WiznetFlags("Punish"));

        Impersonating.Act(ActOptions.ToAll, "{0:N} purge{0:v} {1}.", Actor.Impersonating!, item);
        ItemManager.RemoveItem(item);
        Actor.Send("Ok.");
    }
}
