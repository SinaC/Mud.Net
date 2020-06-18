using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Ability
{
    [CharacterCommand("cd", "Ability")]
    [CharacterCommand("cooldowns", "Ability")]
    [Syntax(
        "[cmd]",
        "[cmd] <ability>")]
    public class Cooldowns : CharacterGameAction
    {
        public IAbilityLearned AbilityLearned { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return null;
            AbilityLearned = Actor.LearnedAbilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value));
            if (AbilityLearned == null)
                return "You don't know any abilities of that name.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (AbilityLearned == null)
            {
                if (Actor.AbilitiesInCooldown.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("%c%Following abilities are in cooldown:%x%");
                    foreach (var cooldown in Actor.AbilitiesInCooldown
                        .Select(x => new { AbilityName = x.Key, SecondsLeft = x.Value / Pulse.PulsePerSeconds })
                        .OrderBy(x => x.SecondsLeft))
                    {
                        sb.AppendFormatLine("%B%{0}%x% is in cooldown for %W%{1}%x%.", cooldown.AbilityName, cooldown.SecondsLeft.FormatDelay());
                    }
                    Actor.Send(sb);
                }
                Actor.Send("%c%No abilities in cooldown.%x%");
                return;
            }
            int pulseLeft = Actor.CooldownPulseLeft(AbilityLearned.Name);
            if (pulseLeft <= 0)
                Actor.Send("{0} is not in cooldown.", AbilityLearned.Name);
            else
                Actor.Send("{0} is in cooldown for {1}.", AbilityLearned.Name, (pulseLeft / Pulse.PulsePerSeconds).FormatDelay());
        }
    }
}
