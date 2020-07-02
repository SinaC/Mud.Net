using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using Mud.Settings.Interfaces;
using System;
using System.Linq;

namespace Mud.Server.Player.Avatar
{
    [PlayerCommand("impersonate", "Avatar", Priority = 100)]
    [Syntax(
        "[cmd]",
        "[cmd] <avatar name>")]
    public class Impersonate : PlayerGameAction
    {
        private IServerPlayerCommand ServerPlayerCommand { get; }
        private IRoomManager RoomManager { get; }
        private ICharacterManager CharacterManager { get; }
        private ISettings Settings { get; }
        private IWiznet Wiznet { get; }

        public PlayableCharacterData Whom { get; protected set; }

        public Impersonate(IServerPlayerCommand serverPlayerCommand, IRoomManager roomManager, ICharacterManager characterManager, ISettings settings, IWiznet wiznet)
        {
            ServerPlayerCommand = serverPlayerCommand;
            RoomManager = roomManager;
            CharacterManager = characterManager;
            Settings = settings;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Impersonating?.Fighting != null)
                return "Not while fighting!";

            if (actionInput.Parameters.Length == 0)
            {
                if (Impersonating == null)
                    return "Impersonate whom?";
                return null;
            }

            Whom = Actor.Avatars.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value));
            if (Whom == null)
                return "Avatar not found. Use 'listavatar' to display your avatar list.";

            if (Impersonating?.Name == Whom.Name)
                return $"You are already impersonation {Whom.Name}.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Whom == null)
            {
                Actor.Send("You stop impersonating {0}.", Impersonating.DisplayName);
                Actor.UpdateCharacterDataFromImpersonated();
                Actor.StopImpersonating();
                ServerPlayerCommand.Save(Actor);
                return;
            }

            if (Impersonating != null)
            {
                Actor.UpdateCharacterDataFromImpersonated();
                Actor.StopImpersonating();
                ServerPlayerCommand.Save(Actor);
            }

            IRoom location = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == Whom.RoomId);
            if (location == null)
            {
                location = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultRoomId);
                Wiznet.Wiznet($"Invalid roomId {Whom.RoomId} for character {Whom.Name}!!", WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            IPlayableCharacter avatar = CharacterManager.AddPlayableCharacter(Guid.NewGuid(), Whom, Actor, location);
            Actor.Send("%M%You start impersonating %C%{0}%x%.", avatar.DisplayName);
            Actor.StartImpersonating(avatar);
        }
    }
}
