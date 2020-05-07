using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Input;
using System;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [Command("train", "Attributes")]
        [Syntax(
            "[cmd]",
            "[cmd] <attribute>",
            "[cmd] <resource> (mana/psy)",
            "[cmd] hp")]
        protected virtual CommandExecutionResults DoTrain(string rawParameters, params CommandParameter[] parameters)
        {
            INonPlayableCharacter trainer = Room.NonPlayableCharacters.FirstOrDefault(x => CanSee(x) && x.ActFlags.HasFlag(ActFlags.Train));
            if (trainer == null)
            {
                Send("You can't do that here.");
                return CommandExecutionResults.TargetNotFound;
            }

            if (parameters.Length == 0)
            {
                Send("You have {0} training sessions left.", Trains);
                return CommandExecutionResults.Ok;
            }

            int cost = 0;
            Func<CommandExecutionResults> gainAction = null;
            // attribute
            var attributeFound = EnumHelpers.GetValues<CharacterAttributes>().Where(x => x >= CharacterAttributes.Strength && x <= CharacterAttributes.Constitution).Select(x => new { attribute = x, name = x.ToString() }).FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.name, parameters[0].Value));
            if (attributeFound != null)
            {
                cost = 1;
                gainAction = () =>
                {
                    if (BaseAttribute(attributeFound.attribute) >= Race?.GetMaxAttribute(attributeFound.attribute))
                    {
                        Send("Your {0} is already at maximum.", attributeFound.name);
                        return CommandExecutionResults.InvalidParameter;
                    }
                    SetBaseAttributes(attributeFound.attribute, BaseAttribute(attributeFound.attribute) + 1, false);
                    this[attributeFound.attribute]++;
                    Act(ActOptions.ToAll, "{0:p} {1} increases!", this, attributeFound.name);
                    return CommandExecutionResults.Ok;
                };
            }
            // resource
            var resourceFound = Class?.ResourceKinds.Select(x => new { resource = x, name = x.ToString() }).FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.name, parameters[0].Value));
            if (resourceFound != null)
            {
                cost = 1;
                gainAction = () =>
                {
                    SetMaxResource(resourceFound.resource, MaxResource(resourceFound.resource) + 10, false);
                    this[resourceFound.resource] += 10;
                    Act(ActOptions.ToAll, "{0:p} power increases!", this);
                    return CommandExecutionResults.Ok;
                };
            }
            // hp
            if (StringCompareHelpers.StringStartsWith("hp", parameters[0].Value))
            {
                cost = 1;
                gainAction = () =>
                {
                    SetBaseAttributes(CharacterAttributes.MaxHitPoints, BaseAttribute(CharacterAttributes.MaxHitPoints) + 10, false);
                    this[CharacterAttributes.MaxHitPoints] += 10;
                    HitPoints += 10;
                    Act(ActOptions.ToAll, "{0:p} durability increases!", this);
                    return CommandExecutionResults.Ok;
                };
            }

            if (gainAction == null || cost <= 0)
                return CommandExecutionResults.SyntaxError;

            // let's go
            if (cost > Trains)
            {
                Send("You don't have enough training sessions.");
                return CommandExecutionResults.NoExecution;
            }
            Trains -= cost;
            return gainAction();
        }
    }
}
