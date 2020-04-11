using System;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.Datas.DataContracts;
using Mud.Logger;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Server;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("impersonate", Category = "Avatar")]
        protected virtual bool DoImpersonate(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Impersonating != null)
                {
                    Send("You stop impersonating {0}.", Impersonating.DisplayName);
                    UpdateCharacterDataFromImpersonated();
                    StopImpersonating();
                }
                else
                    Send("Impersonate whom?");
                return true;
            }
            CharacterData characterData = _avatarList.FirstOrDefault(x => FindHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (characterData == null)
            {
                Send("Avatar not found. Use listavatar to display your avatar list.");
                return true;
            }
            IRoom location = DependencyContainer.Instance.GetInstance<IWorld>().Rooms.FirstOrDefault(x => x.Blueprint.Id == characterData.RoomId);
            if (location == null)
            {
                string msg = $"Invalid roomId {characterData.RoomId} for character {characterData.Name}!!";
                location = DependencyContainer.Instance.GetInstance<IWorld>().Rooms.FirstOrDefault(x => x.Blueprint.Id == 3001); // TODO: default room in IWorld
                Log.Default.WriteLine(LogLevels.Error, msg);
                DependencyContainer.Instance.GetInstance<IServer>().Wiznet(msg, WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            ICharacter avatar = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), characterData, location);
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
            generator.AddColumn("Class", 12, data => DependencyContainer.Instance.GetInstance<IClassManager>()[data.Class]?.DisplayName ?? "none");
            generator.AddColumn("Race", 12, data => DependencyContainer.Instance.GetInstance<IRaceManager>()[data.Race]?.DisplayName ?? "none");
            generator.AddColumn("Location", 40, data => DependencyContainer.Instance.GetInstance<IWorld>().Rooms.FirstOrDefault(x => x.Blueprint.Id == data.RoomId)?.DisplayName ?? "In the void");
            StringBuilder sb = generator.Generate(_avatarList);
            Send(sb);
            return true;
        }

        [PlayerCommand("createavatar", Category = "Avatar", CannotBeImpersonated = true)]
        protected virtual bool DoCreateAvatar(string rawParameters, params CommandParameter[] parameters)
        {
            if (_avatarList.Count >= ServerOptions.MaxAvatarCount)
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
            throw new NotImplementedException();
        }
    }
}
