using Mud.Domain;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("pray", "Communication", MinPosition = Positions.Dead)]
        [Syntax("[cmd] <msg>")]
        protected virtual CommandExecutionResults DoPray(string rawParameters, params CommandParameter[] parameters)
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
