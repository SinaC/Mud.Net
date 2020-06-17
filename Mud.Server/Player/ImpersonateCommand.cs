using System;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Player
{
    public partial class Player
    {

        [PlayerCommand("deleteavatar", "Avatar", CannotBeImpersonated = true)]
        [Syntax("[cmd] <avatar name>")]
        protected virtual CommandExecutionResults DoDeleteAvatar(string rawParameters, params ICommandParameter[] parameters)
        {
            //TODO UniquenessManager.RemoveAvatarName(avatarName)
            throw new NotImplementedException();
        }

    }
}
