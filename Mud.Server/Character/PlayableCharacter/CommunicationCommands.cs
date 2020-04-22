using Mud.Server.Input;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("pray", Category = "Communication")]
        protected virtual bool DoPray(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Pray what?");
                return true;
            }

            string what = $"%g%{DisplayName} has prayed '%x%{parameters[0].Value}%g%'%x%";
            foreach (IAdmin admin in AdminManager.Admins)
                admin.Send(what);
            return true;
        }
    }
}
