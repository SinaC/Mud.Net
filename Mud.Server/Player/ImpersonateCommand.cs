using System;
using System.Linq;
using System.Text;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("impersonate", Category = "Avatar")]
        protected virtual bool DoImpersonate(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Impersonating == null)
                {
                    Send("Impersonate whom?");
                    return true;
                }
                Send("You stop impersonating {0}.", Impersonating.DisplayName);
                UpdateCharacterDataFromImpersonated();
                StopImpersonating();
                Save();
                return true;
            }
            CharacterData characterData = _avatarList.FirstOrDefault(x => FindHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (characterData == null)
            {
                Send("Avatar not found. Use 'listavatar' to display your avatar list.");
                return true;
            }
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
            avatar.ChangeImpersonation(this);
            Impersonating = avatar;
            PlayerState = PlayerStates.Impersonating;
            avatar.AutoLook();

            return true;
        }

        [Command("listavatar", Category = "Avatar")]
        protected virtual bool DoList(string rawParameters, params CommandParameter[] parameters)
        {
            if (!_avatarList.Any())
            {
                Send("You don't have any avatar available. Use createavatar to create one.");
                return true;
            }
            TableGenerator<CharacterData> generator = new TableGenerator<CharacterData>("Avatars");
            generator.AddColumn("Name", 14, data => StringHelpers.UpperFirstLetter(data.Name));
            generator.AddColumn("Level", 7, data => data.Level.ToString());
            generator.AddColumn("Class", 12, data => ClassManager[data.Class]?.DisplayName ?? "none");
            generator.AddColumn("Race", 12, data => RaceManager[data.Race]?.DisplayName ?? "none");
            generator.AddColumn("Location", 40, data => World.Rooms.FirstOrDefault(x => x.Blueprint.Id == data.RoomId)?.DisplayName ?? "In the void");
            StringBuilder sb = generator.Generate(_avatarList);
            Send(sb);
            return true;
        }

        [PlayerCommand("createavatar", Category = "Avatar", CannotBeImpersonated = true)]
        protected virtual bool DoCreateAvatar(string rawParameters, params CommandParameter[] parameters)
        {
            if (_avatarList.Count >= Settings.MaxAvatarCount)
            {
                Send("Max. avatar count reached. Delete one before creating a new one.");
                return true;
            }

            Send("Please choose an avatar name (type quit to stop and cancel creation).");
            CurrentStateMachine = new AvatarCreationStateMachine();
            return true;
        }

        [PlayerCommand("deleteavatar", Category = "Avatar", CannotBeImpersonated = true)]
        protected virtual bool DoDeleteAvatar(string rawParameters, params CommandParameter[] parameters)
        {
            //TODO UniquenessManager.RemoveAvatarName(avatarName)
            throw new NotImplementedException();
        }
    }
}
