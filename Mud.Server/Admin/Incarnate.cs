﻿using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Linq;
using Mud.Server.Interfaces;

namespace Mud.Server.Admin
{
    [AdminCommand("incarnate", "Admin", CannotBeImpersonated = true)]
    [Syntax(
            "[cmd]",
            "[cmd] room <name|id>",
            "[cmd] item name",
            "[cmd] mob name")]
    public class Incarnate : AdminGameAction
    {
        private IRoomManager RoomManager { get; }
        private ICharacterManager CharacterManager { get; }
        private IItemManager ItemManager { get; }
        private IWiznet Wiznet { get; }

        public IEntity Target { get; protected set; }

        public Incarnate(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IWiznet wiznet)
        {
            RoomManager = roomManager;
            CharacterManager = characterManager;
            ItemManager = itemManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
            {
                if (Actor.Incarnating == null)
                    return BuildCommandSyntax();
                Target = null; // un-incarnate
                return null;
            }

            if (actionInput.Parameters.Length == 1)
                return BuildCommandSyntax();

            string kind = actionInput.Parameters[0].Value;
            if ("room".StartsWith(kind))
            {
                if (actionInput.Parameters[1].IsNumber)
                {
                    int id = actionInput.Parameters[1].AsNumber;
                    Target = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint?.Id == id);
                }
                else
                    Target = FindHelpers.FindByName(RoomManager.Rooms, actionInput.Parameters[1]);
            }
            else if ("item".StartsWith(kind))
                Target = FindHelpers.FindByName(ItemManager.Items, actionInput.Parameters[1]);
            else if ("mob".StartsWith(kind))
                Target = FindHelpers.FindByName(CharacterManager.Characters, actionInput.Parameters[1]);
            if (Target == null)
                return "Target not found.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Target == null)
            {
                Wiznet.Wiznet($"{Actor.DisplayName} stops incarnating {Actor.Incarnating.DebugName}.", Domain.WiznetFlags.Incarnate);

                Actor.Send("%M%You stop incarnating %C%{0}%x%.", Actor.Incarnating.DisplayName);
                Actor.StopIncarnating();

                return;
            }

            bool incarnated = Actor.StartIncarnating(Target);
            if (incarnated)
            {
                string msg = $"{Actor.DisplayName} starts incarnating {Target.DebugName}.";
                Wiznet.Wiznet(msg, Domain.WiznetFlags.Incarnate);

                Actor.Send("%M%You start incarnating %C%{0}%x%.", Target.DisplayName);
            }
            else
                Actor.Send($"Something prevented you from being incarnating {Target.DisplayName}.");
        }
    }
}
