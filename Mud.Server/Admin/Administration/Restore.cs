﻿using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Linq;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Affect;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("restore", "Admin")]
    [Syntax(
            "[cmd] <character>",
            "[cmd] all",
            "[cmd] (if impersonated)")]
    public class Restore : AdminGameAction
    {
        private ICharacterManager CharacterManager { get; }
        private IPlayerManager PlayerManager { get; }
        private IWiznet Wiznet { get; }

        public bool IsRoom { get; set; }
        public bool IsAll { get; set; }
        public IPlayableCharacter Whom { get; protected set; }

        public Restore(ICharacterManager characterManager, IPlayerManager playerManager, IWiznet wiznet)
        {
            CharacterManager = characterManager;
            PlayerManager = playerManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
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
            Whom = FindHelpers.FindByName(PlayerManager.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), actionInput.Parameters[0]);
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
                Wiznet.Wiznet($"{Actor.DisplayName} has restored room {Impersonating.Room.Blueprint.Id}.", WiznetFlags.Restore);
                Actor.Send("Room restored.");
                return;
            }

            if (IsAll)
            {
                foreach (IPlayableCharacter loopVictim in CharacterManager.PlayableCharacters)
                    RestoreOneCharacter(loopVictim);
                Wiznet.Wiznet($"{Actor.DisplayName} has restored all active players.", WiznetFlags.Restore);
                Actor.Send("All active players restored.");
                return;
            }

            RestoreOneCharacter(Whom);
            Wiznet.Wiznet($"{Actor.DisplayName} has restored {Whom.DisplayName}.", WiznetFlags.Restore);
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
}
