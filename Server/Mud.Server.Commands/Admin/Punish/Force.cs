using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
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
    private ICommandParser CommandParser { get; }
    private ICharacterManager CharacterManager { get; }
    private IWiznet Wiznet { get; }

    public Force(ICommandParser commandParser, ICharacterManager characterManager, IWiznet wiznet)
    {
        CommandParser = commandParser;
        CharacterManager = characterManager;
        Wiznet = wiznet;
    }

    protected ICharacter Whom { get; set; } = default!;
    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length < 2)
            return BuildCommandSyntax();

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
            Wiznet.Log($"{Actor.DisplayName} forces {Whom.DebugName} to {What}", WiznetFlags.Punish);

            ForceOneCharacter(Whom);
        }
        else
        {
            Actor.Send("You force everyone to '{0}'.", What);
            Wiznet.Log($"{Actor.DisplayName} forces everyone to {What}", WiznetFlags.Punish);

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
