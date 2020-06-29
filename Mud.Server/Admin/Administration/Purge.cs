using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Server.Interfaces;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("purge", "Admin", Priority = 999, NoShortcut = true, MustBeImpersonated = true)]
    [Syntax(
    "[cmd] all",
    "[cmd] <character>",
    "[cmd] <item>")]
    public class Purge : AdminGameAction
    {
        private ICharacterManager CharacterManager { get; }
        private IItemManager ItemManager { get; }
        private IWiznet Wiznet { get; }

        public IEntity Target { get; protected set; }

        public Purge(ICharacterManager characterManager, IItemManager itemManager, IWiznet wiznet)
        {
            ItemManager = itemManager;
            CharacterManager = characterManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            // All -> room
            if (actionInput.Parameters[0].IsAll)
            {
                Target = Impersonating.Room;
                return null;
            }

            // Search NPC
            Target = FindHelpers.FindNonPlayableChararacterInWorld(CharacterManager, Actor.Impersonating, actionInput.Parameters[0]);
            if (Target != null)
                return null;

            // Search PC
            Target = FindHelpers.FindPlayableChararacterInWorld(CharacterManager, Actor.Impersonating, actionInput.Parameters[0]);
            if (Target != null)
            {
                if (Target == Actor.Impersonating)
                    return "Ho ho ho.";
                return null;
            }

            // Search Item
            Target = FindHelpers.FindItemHere(Actor.Impersonating, actionInput.Parameters[0]);
            if (Target == null)
                return StringHelpers.ItemNotFound;
            if (((IItem) Target).ItemFlags.IsSet("NoPurge"))
                return "It can't be purged.";

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
                case IPlayableCharacter playableCharacter:
                    PurgePlayableCharacter(playableCharacter);
                    break;
                case IItem item:
                    PurgeItem(item);
                    break;
            }
        }

        private void PurgeRoom(IRoom room)
        {
            Wiznet.Wiznet($"{Actor.DisplayName} purges room {room.Blueprint.Id}.", WiznetFlags.Punish);

            // Purge non playable characters (without NoPurge flag) (TODO: what if npc was wearing NoPurge items?)
            IReadOnlyCollection<INonPlayableCharacter> nonPlayableCharacters = new ReadOnlyCollection<INonPlayableCharacter>(room.NonPlayableCharacters.Where(x => !x.ActFlags.HasFlag(ActFlags.NoPurge)).ToList()); // clone
            foreach (INonPlayableCharacter nonPlayableCharacter in nonPlayableCharacters)
                CharacterManager.RemoveCharacter(nonPlayableCharacter);
            // Purge items (without NoPurge flag)
            IReadOnlyCollection<IItem> items = new ReadOnlyCollection<IItem>(room.Content.Where(x => !x.ItemFlags.IsSet("NoPurge")).ToList()); // clone
            foreach (IItem itemToPurge in items)
                ItemManager.RemoveItem(itemToPurge);
            Impersonating.Act(ActOptions.ToRoom, "{0} purge{0:v} the room!", Actor.Impersonating);
            Actor.Send("Ok.");
        }

        private void PurgeNonPlayableCharacter(INonPlayableCharacter nonPlayableCharacterVictim)
        {
            Wiznet.Wiznet($"{Actor.DisplayName} purges npc {nonPlayableCharacterVictim.DebugName}.", WiznetFlags.Punish);

            nonPlayableCharacterVictim.Act(ActOptions.ToRoom, "{0} purge{0:v} {1}.", Actor.Impersonating, nonPlayableCharacterVictim);
            CharacterManager.RemoveCharacter(nonPlayableCharacterVictim);
            Actor.Send("Ok.");
        }

        private void PurgePlayableCharacter(IPlayableCharacter playableCharacterVictim)
        {
            Wiznet.Wiznet($"{Actor.DisplayName} purges player {playableCharacterVictim.DebugName}.", WiznetFlags.Punish);

            playableCharacterVictim.Act(ActOptions.ToRoom, "{0} disintegrate{0:v} {1}.", Actor.Impersonating, playableCharacterVictim);
            playableCharacterVictim.StopFighting(true);
            if (playableCharacterVictim.ImpersonatedBy != null)
                playableCharacterVictim.StopImpersonation();
            CharacterManager.RemoveCharacter(playableCharacterVictim);
            Actor.Send("Ok.");
        }

        private void PurgeItem(IItem item)
        {
            Wiznet.Wiznet($"{Actor.DisplayName} purges item {item.DebugName}.", WiznetFlags.Punish);

            Impersonating.Act(ActOptions.ToAll, "{0:N} purge{0:v} {1}.", Actor.Impersonating, item);
            ItemManager.RemoveItem(item);
            Actor.Send("Ok.");
        }
    }
}
