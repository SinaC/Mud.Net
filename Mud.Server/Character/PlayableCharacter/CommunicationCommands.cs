using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("pray", "Communication", MinPosition = Positions.Dead)]
        [Syntax("[cmd] <msg>")]
        protected virtual CommandExecutionResults DoPray(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Pray what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            string what = $"%g%{DisplayName} has prayed '%x%{parameters[0].Value}%g%'%x%";
            foreach (IAdmin admin in AdminManager.Admins)
                admin.Send(what);
            return CommandExecutionResults.Ok;
        }
    }
}
