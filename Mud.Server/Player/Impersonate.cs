using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("impersonate")]
        protected virtual bool DoImpersonate(string rawParameters, CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Impersonating != null)
                {
                    Send("You stop impersonating {0}.", Impersonating.Name);
                    Impersonating.ChangeImpersonation(null);
                    Impersonating = null;
                    PlayerState = PlayerStates.Connected;
                }
                else
                    Send("Impersonate before trying to un-impersonate.");
            }
            else
            {
                ICharacter target = World.Instance.GetCharacter(parameters[0]);
                if (target != null)
                {
                    Send("You start impersonating {0}.", target.Name);
                    target.ChangeImpersonation(this);
                    Impersonating = target;
                    PlayerState = PlayerStates.Playing;
                }
                else
                    Send(StringHelpers.CharacterNotFound);
            }

            return true;
        }
    }
}
