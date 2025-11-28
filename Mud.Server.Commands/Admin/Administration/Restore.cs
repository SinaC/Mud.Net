using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("restore", "Admin")]
[Syntax(
        "[cmd] <character>",
        "[cmd] all",
        "[cmd] room",
        "[cmd] (if impersonated)")]
[Help(
@"[cmd] restores full hit points, mana points, and movement points to the
target character.  It also heals poison, plague, and blindness.

[cmd] room (or restore with no argument) performs a restore on every player
in the room, restore all does the same for all connected players. [cmd] all
is only usable by creators and implementors.  [cmd] should be used sparingly
or not at all.")]
public class Restore : AdminGameAction
{
    private ICharacterManager CharacterManager { get; }
    private IPlayerManager PlayerManager { get; }
    private IWiznet Wiznet { get; }

    public Restore(ICharacterManager characterManager, IPlayerManager playerManager, IWiznet wiznet)
    {
        CharacterManager = characterManager;
        PlayerManager = playerManager;
        Wiznet = wiznet;
    }

    protected bool IsRoom { get; set; }
    protected bool IsAll { get; set; }
    protected IPlayableCharacter Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0 || actionInput.Parameters[0].Value == "room")
        {
            if (Actor.Impersonating == null)
                return "Restore what?";
            IsRoom = true;
            return null;
        }
        if (actionInput.Parameters[0].IsAll)
        {
            IsAll = true;
            return null;
        }
        Whom = FindHelpers.FindByName(PlayerManager.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (IsRoom)
        {
            foreach (ICharacter loopVictim in Impersonating.Room.People)
                RestoreOneCharacter(loopVictim);
            Wiznet.Log($"{Actor.DisplayName} has restored room {Impersonating.Room.Blueprint.Id}.", WiznetFlags.Restore);
            Actor.Send("Room restored.");
            return;
        }

        if (IsAll)
        {
            foreach (IPlayableCharacter loopVictim in CharacterManager.PlayableCharacters)
                RestoreOneCharacter(loopVictim);
            Wiznet.Log($"{Actor.DisplayName} has restored all active players.", WiznetFlags.Restore);
            Actor.Send("All active players restored.");
            return;
        }

        RestoreOneCharacter(Whom);
        Wiznet.Log($"{Actor.DisplayName} has restored {Whom.DisplayName}.", WiznetFlags.Restore);
        Actor.Send("Ok.");
    }

    private void RestoreOneCharacter(ICharacter victim)
    {
        victim.RemoveAuras(x => !x.AuraFlags.HasFlag(AuraFlags.NoDispel) && !x.AuraFlags.HasFlag(AuraFlags.Permanent) && !x.Affects.OfType<ICharacterFlagsAffect>().Any(a => a.Modifier.IsSet("Charm")), true); // TODO: harmful auras only ?
        victim.UpdateHitPoints(victim.MaxHitPoints);
        victim.UpdateMovePoints(victim.MaxMovePoints);
        foreach (ResourceKinds resource in victim.CurrentResourceKinds)
            victim.UpdateResource(resource, victim.MaxResource(resource));
        victim.Send("{0} has restored you.", Actor.Impersonating?.DisplayName ?? Actor.DisplayName);
    }
}
