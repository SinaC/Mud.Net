using System;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("impersonate", Category = "Avatar")]
        protected virtual bool DoImpersonate(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: use impersonate list
            if (parameters.Length == 0)
            {
                if (Impersonating != null)
                {
                    Send("You stop impersonating {0}.", Impersonating.DisplayName);
                    StopImpersonating();
                }
                else
                    Send("Impersonate whom?");
            }
            else
            {
                ICharacter target = FindHelpers.FindByName(Repository.World.Characters, parameters[0]);
                if (target != null)
                {
                    if (target.Impersonable)
                    {
                        if (target.ImpersonatedBy != null)
                            Send("{0} is already impersonated by {1}.", target.DisplayName, target.ImpersonatedBy.DisplayName);
                        else
                        {
                            if (Impersonating != null)
                            {
                                Send("You stop impersonating {0}.", Impersonating.DisplayName);
                                Impersonating.ChangeImpersonation(null);
                            }
                            Send("%M%You start impersonating %C%{0}%x%.", target.DisplayName);
                            target.ChangeImpersonation(this);
                            Impersonating = target;
                            PlayerState = PlayerStates.Impersonating;
                            // TODO: if autolook
                            target.ExecuteCommand("look", String.Empty, CommandParameter.EmptyCommand); // TODO: target.DoLook(String.Empty, CommandParameter.Empty)
                        }
                    }
                    else
                        Send("{0} cannot be impersonated.", target.DisplayName);
                }
                else
                    Send(StringHelpers.CharacterNotFound);
            }

            return true;
        }

        [Command("listavatar", Category = "Avatar")]
        protected virtual bool DoList(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: display impersonation list
            Send("Not yet implemented");
            return true;
        }

        [Command("createavatar", Category = "Avatar")]
        protected virtual bool DoCreateAvatar(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating != null)
            {
                Send("You cannot create a new avatar while impersonating.");
                return true;
            }

            Send("Please choose an avatar name (type quit to stop and cancel creation).");
            CurrentStateMachine = new AvatarCreationStateMachine();
            return true;
        }
    }
}
