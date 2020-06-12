using System;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Input;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Player
{
    public partial class Player
    {
        private IRoomManager RoomManager => DependencyContainer.Current.GetInstance<IRoomManager>();

        [Command("impersonate", "Avatar")]
        [Syntax(
            "[cmd]",
            "[cmd] <avatar name>")]
        protected virtual CommandExecutionResults DoImpersonate(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating?.Fighting != null)
            {
                Send("Not while fighting!");
                return CommandExecutionResults.NoExecution;
            }

            if (parameters.Length == 0)
            {
                if (Impersonating == null)
                {
                    Send("Impersonate whom?");
                    return CommandExecutionResults.SyntaxErrorNoDisplay;
                }
                Send("You stop impersonating {0}.", Impersonating.DisplayName);
                UpdateCharacterDataFromImpersonated();
                StopImpersonating();
                Save();
                return CommandExecutionResults.Ok;
            }
            PlayableCharacterData playableCharacterData = _avatarList.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (playableCharacterData == null)
            {
                Send("Avatar not found. Use 'listavatar' to display your avatar list.");
                return CommandExecutionResults.TargetNotFound;
            }

            // Re-impersonate same character
            if (Impersonating?.Name == playableCharacterData.Name)
            {
                Send("You are already impersonation {0}.", playableCharacterData.Name);
                return CommandExecutionResults.InvalidTarget;
            }

            // If already impersonating, stop first
            if (Impersonating != null)
            {
                StopImpersonating();
                Save();
            }

            // TODO: move room extraction in World.AddPlayableCharacter and remove Room parameter
            IRoom location = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == playableCharacterData.RoomId);
            if (location == null)
            {
                location = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultRoomId);
                Wiznet.Wiznet($"Invalid roomId {playableCharacterData.RoomId} for character {playableCharacterData.Name}!!", WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            IPlayableCharacter avatar = World.AddPlayableCharacter(Guid.NewGuid(), playableCharacterData, this, location);
            Send("%M%You start impersonating %C%{0}%x%.", avatar.DisplayName);
            Impersonating = avatar;
            PlayerState = PlayerStates.Impersonating;
            avatar.AutoLook();

            return CommandExecutionResults.Ok;
        }

        [Command("listavatar", "Avatar")]
        protected virtual CommandExecutionResults DoList(string rawParameters, params CommandParameter[] parameters)
        {
            if (!_avatarList.Any())
            {
                Send("You don't have any avatar available. Use createavatar to create one.");
                return CommandExecutionResults.NoExecution;
            }
            StringBuilder sb = AvatarTableGenerator.Value.Generate("Avatars", _avatarList);
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [PlayerCommand("createavatar", "Avatar", CannotBeImpersonated = true)]
        protected virtual CommandExecutionResults DoCreateAvatar(string rawParameters, params CommandParameter[] parameters)
        {
            if (_avatarList.Count >= Settings.MaxAvatarCount)
            {
                Send("Max. avatar count reached. Delete one before creating a new one.");
                return CommandExecutionResults.NoExecution;
            }

            Send("Please choose an avatar name (type quit to stop and cancel creation).");
            CurrentStateMachine = new AvatarCreationStateMachine();
            return CommandExecutionResults.Ok;
        }

        [PlayerCommand("deleteavatar", "Avatar", CannotBeImpersonated = true)]
        [Syntax("[cmd] <avatar name>")]
        protected virtual CommandExecutionResults DoDeleteAvatar(string rawParameters, params CommandParameter[] parameters)
        {
            //TODO UniquenessManager.RemoveAvatarName(avatarName)
            throw new NotImplementedException();
        }

        // Helpers
        // TODO: crappy workaround because ClassManager, RaceManager and World are needed
        private static readonly Lazy<TableGenerator<PlayableCharacterData>> AvatarTableGenerator = new Lazy<TableGenerator<PlayableCharacterData>>(() =>
        {
            IClassManager classManager = DependencyContainer.Current.GetInstance<IClassManager>();
            IRaceManager raceManager = DependencyContainer.Current.GetInstance<IRaceManager>();
            IRoomManager roomManager = DependencyContainer.Current.GetInstance<IRoomManager>();
            TableGenerator<PlayableCharacterData> generator = new TableGenerator<PlayableCharacterData>();
            generator.AddColumn("Name", 14, data => data.Name.UpperFirstLetter());
            generator.AddColumn("Level", 7, data => data.Level.ToString());
            generator.AddColumn("Class", 12, data => classManager[data.Class]?.DisplayName ?? "none");
            generator.AddColumn("Race", 12, data => raceManager[data.Race]?.DisplayName ?? "none");
            generator.AddColumn("Location", 40, data => roomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == data.RoomId)?.DisplayName ?? "In the void");
            return generator;
        });
    }
}
