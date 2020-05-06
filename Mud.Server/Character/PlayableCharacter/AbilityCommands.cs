using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Abilities;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        // TODO: Practice/Gain
        [Command("Gain", "Ability")]
        [Syntax(
            "[cmd] list",
            "[cmd] skills|spells|passives",
            "[cmd] convert|revert",
            "[cmd] <ability>")]
        protected virtual CommandExecutionResults DoGain(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            if (StringCompareHelpers.StringStartsWith("list", parameters[0].Value))
            {

            }

            return CommandExecutionResults.Ok;
        }

        //TODO: move to another file
        //[Command("train")]
        //[Syntax(
        //    "[cmd]",
        //    "[cmd] <attribute>",
        //    "[cmd] hp|mana|move")]
        //protected virtual CommandExecutionResults DoTrain(string rawParameters, params CommandParameter[] parameters)
        //{
        //    Send(StringHelpers.NotYetImplemented);

        //    return CommandExecutionResults.Ok;
        //}

        private void DisplayAbilitiesAvailableToLearn(IEnumerable<KnownAbility> abilities)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (KnownAbility knownAbility in abilities)
            {

                //
                i++;
                if (i % 3 == 0)
                    sb.AppendLine();
            }
        }
    }
}
