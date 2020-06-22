using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Linq;
using Mud.Server.Interfaces;

namespace Mud.Server.Admin.Punish
{
    [AdminCommand("force", "Punish")]
    [Syntax(
            "[cmd] <character> <command>",
            "[cmd] all <command>")]
    public class Force : AdminGameAction
    {
        private ICharacterManager CharacterManager { get; }
        private IWiznet Wiznet { get; }

        public ICharacter Whom { get; protected set; }
        public string What { get; protected set; }

        public Force(ICharacterManager characterManager, IWiznet wiznet)
        {
            CharacterManager = characterManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length < 2)
                return BuildCommandSyntax();

            if (actionInput.Parameters[1].Value == "delete")
                return "That will NOT be done.";

            if (!actionInput.Parameters[0].IsAll)
            {
                Whom = Impersonating == null
                    ? FindHelpers.FindByName(CharacterManager.Characters, actionInput.Parameters[0])
                    : FindHelpers.FindChararacterInWorld(Impersonating, actionInput.Parameters[0]);
                if (Whom == null)
                    return StringHelpers.CharacterNotFound;
                if (Whom == Impersonating || Whom == Actor.Incarnating)
                    return "Aye aye, right away!";
            }
            What = CommandHelpers.JoinParameters(actionInput.Parameters.Skip(1));
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Whom == null)
            {
                Actor.Send("You force everyone to '{1}'.", What);
                Wiznet.Wiznet($"{Actor.DisplayName} forces everyone to {What}", Domain.WiznetFlags.Punish);

                foreach (ICharacter victimLoop in CharacterManager.Characters.Where(x => x != Impersonating))
                {
                    victimLoop.Send("{0} forces you to '{1}'.", Actor.DisplayName, What);
                    victimLoop.ProcessCommand(What);
                }
            }
        }
    }
}
