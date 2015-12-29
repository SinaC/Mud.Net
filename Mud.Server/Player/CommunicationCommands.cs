﻿using System;
using System.Linq;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("tell")]
        protected virtual bool DoTell(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
                Send("Tell whom what ?");
            else
            {
                IPlayer target = World.World.Instance.GetPlayer(parameters[0], true);
                if (target == null)
                    Send(StringHelpers.CharacterNotFound);
                else
                {
                    string what = CommandHelpers.JoinParameters(parameters.Skip(1));
                    Send("%g%You tell {0}: '%G%{1}%g%'%x%", target.DisplayName, what);
                    //target.Send("{0} tells you '" + StringConstants.Blue + "{1}" + StringConstants.Reset + "'", Name, what);
                    target.Send("%g%{0} tells you '%G%{1}%g%'%x%", DisplayName, what);
                }
            }

            return true;
        }

        [Command("gossip")]
        protected virtual bool DoGossip(string rawParameters, params CommandParameter[] parameters)
        {
            Send("%m%You gossip '%M%{0}%m%'%x%", rawParameters);
            string other = String.Format("%m%{0} gossips '%M%{1}%m%'%x%", DisplayName, rawParameters);
            foreach(IPlayer player in World.World.Instance.GetPlayers().Where(x => x != this))
                player.Send(other);

            return true;
        }
    }
}
