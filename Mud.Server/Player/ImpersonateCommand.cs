using System;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("impersonate")]
        protected virtual bool DoImpersonate(string rawParameters, params CommandParameter[] parameters)
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
                ICharacter target = World.World.Instance.GetCharacter(parameters[0]);
                if (target != null)
                {
                    Send("%M%You start impersonating %C%{0}%x%.", target.Name);
                    target.ChangeImpersonation(this);
                    Impersonating = target;
                    PlayerState = PlayerStates.Playing;
                    // TODO: if autolook
                    target.ExecuteCommand("look", String.Empty, CommandParameter.Empty);
                }
                else
                    Send(StringHelpers.CharacterNotFound);
            }

            return true;
        }
    }
}
