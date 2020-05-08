using System;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Player
{
    public partial class Player
    {
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
            CharacterData characterData = _avatarList.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (characterData == null)
            {
                Send("Avatar not found. Use 'listavatar' to display your avatar list.");
                return CommandExecutionResults.TargetNotFound;
            }

            // Re-impersonate same character
            if (Impersonating?.Name == characterData.Name)
            {
                Send("You are already impersonation {0}.", characterData.Name);
                return CommandExecutionResults.InvalidTarget;
            }

            // If already impersonating, stop first
            if (Impersonating != null)
            {
                StopImpersonating();
                Save();
            }

            // TODO: move room extraction in World.AddPlayableCharacter and remove Room parameter
            IRoom location = World.Rooms.FirstOrDefault(x => x.Blueprint.Id == characterData.RoomId);
            if (location == null)
            {
                location = World.Rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultRoomId);
                string msg = $"Invalid roomId {characterData.RoomId} for character {characterData.Name}!!";
                Log.Default.WriteLine(LogLevels.Error, msg);
                Wiznet.Wiznet(msg, WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            IPlayableCharacter avatar = World.AddPlayableCharacter(Guid.NewGuid(), characterData, this, location);
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
        private readonly static Lazy<TableGenerator<CharacterData>> AvatarTableGenerator = new Lazy<TableGenerator<CharacterData>>(() =>
        {
            IClassManager classManager = DependencyContainer.Current.GetInstance<IClassManager>();
            IRaceManager raceManager = DependencyContainer.Current.GetInstance<IRaceManager>();
            IWorld world = DependencyContainer.Current.GetInstance<IWorld>();
            TableGenerator<CharacterData> generator = new TableGenerator<CharacterData>();
            generator.AddColumn("Name", 14, data => data.Name.UpperFirstLetter());
            generator.AddColumn("Level", 7, data => data.Level.ToString());
            generator.AddColumn("Class", 12, data => classManager[data.Class]?.DisplayName ?? "none");
            generator.AddColumn("Race", 12, data => raceManager[data.Race]?.DisplayName ?? "none");
            generator.AddColumn("Location", 40, data => world.Rooms.FirstOrDefault(x => x.Blueprint.Id == data.RoomId)?.DisplayName ?? "In the void");
            return generator;
        });
    }
}
