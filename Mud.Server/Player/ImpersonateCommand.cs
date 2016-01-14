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
                    Send("You stop impersonating {0}." + Environment.NewLine, Impersonating.DisplayName);
                    Impersonating.ChangeImpersonation(null);
                    Impersonating = null;
                    PlayerState = PlayerStates.Playing;
                }
                else
                    Send("Impersonate whom?" + Environment.NewLine);
            }
            else
            {
                ICharacter target = Repository.World.GetCharacter(parameters[0]);
                if (target != null)
                {
                    if (target.Impersonable)
                    {
                        if (target.ImpersonatedBy != null)
                            Send("{0} is already impersonated by {1}."+Environment.NewLine, target.DisplayName, target.ImpersonatedBy.DisplayName);
                        else
                        {
                            if (Impersonating != null)
                            {
                                Send("You stop impersonating {0}." + Environment.NewLine, Impersonating.DisplayName);
                                Impersonating.ChangeImpersonation(null);
                            }
                            Send("%M%You start impersonating %C%{0}%x%." + Environment.NewLine, target.DisplayName);
                            target.ChangeImpersonation(this);
                            Impersonating = target;
                            PlayerState = PlayerStates.Impersonating;
                            // TODO: if autolook
                            target.ExecuteCommand("look", String.Empty, CommandParameter.Empty); // TODO: target.DoLook(String.Empty, CommandParameter.Empty)
                        }
                    }
                    else
                        Send("{0} cannot be impersonated." + Environment.NewLine, target.DisplayName);
                }
                else
                    Send(StringHelpers.CharacterNotFound);
            }

            return true;
        }
    }
}
