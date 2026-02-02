using Mud.Flags;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Punish;

[AdminCommand("force", "Punish")]
[Syntax(
        "[cmd] <character> <command>",
        "[cmd] all <command>")]
[Help(
@"[cmd] forces one character to execute a command, except of course delete.

[cmd] 'all' forces all player characters to execute a command.
This is typically used for 'force all save'.")]
// TODO: check if force mob murder player is possible (should not be)
public class Force : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastTwoArguments()];

    private ICommandParser CommandParser { get; }
    private ICharacterManager CharacterManager { get; }
    private IWiznet Wiznet { get; }

    public Force(ICommandParser commandParser, ICharacterManager characterManager, IWiznet wiznet)
    {
        CommandParser = commandParser;
        CharacterManager = characterManager;
        Wiznet = wiznet;
    }

    private ICharacter Whom { get; set; } = default!;
    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters[1].Value == "delete" || actionInput.Parameters[1].Value == "deleteavatar")
            return "That will NOT be done.";

        if (!actionInput.Parameters[0].IsAll)
        {
            Whom = Impersonating == null
                ? FindHelpers.FindByName(CharacterManager.Characters, actionInput.Parameters[0])!
                : FindHelpers.FindChararacterInWorld(CharacterManager, Impersonating, actionInput.Parameters[0])!;
            if (Whom == null)
                return StringHelpers.CharacterNotFound;
            if (Whom == Impersonating || Whom == Actor.Incarnating)
                return "Aye aye, right away!";
        }
        What = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Whom != null)
        {
            Actor.Send("You force {0} to '{1}'.", Whom.DebugName, What);
            Wiznet.Log($"{Actor.DisplayName} forces {Whom.DebugName} to {What}", new WiznetFlags("Punish"));

            ForceOneCharacter(Whom);
        }
        else
        {
            Actor.Send("You force everyone to '{0}'.", What);
            Wiznet.Log($"{Actor.DisplayName} forces everyone to {What}", new WiznetFlags("Punish"));

            foreach (ICharacter victim in CharacterManager.Characters.Where(x => x != Impersonating))
                ForceOneCharacter(victim);
        }
    }

    private void ForceOneCharacter(ICharacter victim)
    {
        victim.Send("{0} forces you to '{1}'.", Actor.DisplayName, What);
        switch (victim)
        {
            case INonPlayableCharacter npc:
                npc.ProcessInput(What);
                break;
            case IPlayableCharacter pc:
                if (pc.ImpersonatedBy != null)
                    pc.ImpersonatedBy.ProcessInput(What);
                else
                    pc.ProcessInput(What);
                break;
        }
    }
}
