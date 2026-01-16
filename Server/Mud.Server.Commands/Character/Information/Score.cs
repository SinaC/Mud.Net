using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("score", "Information", Priority = 2)]
[Help(
@"[cmd] shows much more detailed statistics to you only. Your ability scores 
are shown as true value(current value), so for example
Str: 23/15 means you have a 15 strength from training, but a 23 strength
from other factors (skills, spells or items).")]
public class Score : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        var pc = Actor as IPlayableCharacter;
        StringBuilder sb = new();
        sb.AppendLine(" + --------------------------------------------------------+"); // length 1 + 56 + 1
        sb.AppendLine("|" + Actor.DisplayName.CenterText(56) + "|");
        sb.AppendLine("+------------------------------+-------------------------+");
        sb.AppendLine("| %W%Attributes%x%                   |                         |");
        sb.AppendFormatLine("| %c%Strength     : %W%[{0,5}/{1,5}]%x% | %c%Race   : %W%{2,14}%x% |", Actor[CharacterAttributes.Strength], Actor.BaseAttribute(CharacterAttributes.Strength), Actor.Race?.DisplayName ?? "(none)");
        sb.AppendFormatLine("| %c%Intelligence : %W%[{0,5}/{1,5}]%x% | %c%Class  : %W%{2,14}%x% |", Actor[CharacterAttributes.Intelligence], Actor.BaseAttribute(CharacterAttributes.Intelligence), Actor.Class?.DisplayName ?? "(none)");
        sb.AppendFormatLine("| %c%Wisdom       : %W%[{0,5}/{1,5}]%x% | %c%Sex    : %W%{2,14}%x% |", Actor[CharacterAttributes.Wisdom], Actor.BaseAttribute(CharacterAttributes.Wisdom), Actor.Sex);
        sb.AppendFormatLine("| %c%Dexterity    : %W%[{0,5}/{1,5}]%x% | %c%Level  : %W%{2,14}%x% |", Actor[CharacterAttributes.Dexterity], Actor.BaseAttribute(CharacterAttributes.Dexterity), Actor.Level);
        if (pc != null)
            sb.AppendFormatLine("| %c%Constitution : %W%[{0,5}/{1,5}]%x% | %c%NxtLvl : %W%{2,14}%x% |", Actor[CharacterAttributes.Constitution], Actor.BaseAttribute(CharacterAttributes.Constitution), pc.ExperienceToLevel);
        else
            sb.AppendFormatLine("| %c%Constitution : %W%[{0,5}/{1,5}]%x% |                       |", Actor[CharacterAttributes.Constitution], Actor.BaseAttribute(CharacterAttributes.Constitution), Actor.Level);
        sb.AppendLine("+------------------------------+-------------------------+");
        sb.AppendLine("| %W%Resources%x%                    | %W%Defensive%x%              |");
        sb.AppendFormatLine("| %g%Hp     : %W%[{0,8}/{1,8}]%x% | %g%Bash         : %W%[{2,6}]%x% |", Actor[ResourceKinds.HitPoints], Actor.MaxResource(ResourceKinds.HitPoints), Actor[Armors.Bash]);
        sb.AppendFormatLine("| %g%Move   : %W%[{0,8}/{1,8}]%x% | %g%Pierce       : %W%[{2,6}]%x% |", Actor[ResourceKinds.MovePoints], Actor.MaxResource(ResourceKinds.MovePoints), Actor[Armors.Pierce]);
        List<string> resources = [];
        foreach (var resourceKind in Actor.CurrentResourceKinds.Where(x => !x.IsMandatoryResource()))
            resources.Add($"%g%{resourceKind,-7}: %W%[{Actor[resourceKind],8}/{Actor.MaxResource(resourceKind),8}]%x%");
        if (resources.Count < 3)
            resources.AddRange(Enumerable.Repeat("                            ", 3 - resources.Count));
        sb.AppendFormatLine("| {0} | %g%Slash        : %W%[{1,6}]%x% |", resources[0], Actor[Armors.Slash]);
        sb.AppendFormatLine("| {0} | %g%Exotic       : %W%[{1,6}]%x% |", resources[1], Actor[Armors.Exotic]);
        sb.AppendFormatLine("| {0} | %g%Saves        : %W%[{1,6}]%x% |", resources[2], Actor[CharacterAttributes.SavingThrow]);
        sb.AppendLine("+------------------------------+-------------------------+");
        if (pc != null)
            sb.AppendFormatLine("| %g%Hit:  %W%{0,6}%x%    %g%Dam:  %W%{1,6}%x% | %g%Train: %W%{2,3}%x%   %g%Pract: %W%{3,3}%x% |", Actor.HitRoll, Actor.DamRoll, pc.Trains, pc.Practices);
        else
            sb.AppendFormatLine("| %g%Hit:  %W%{0,6}%x%    %g%Dam:  %W%{1,6}%x% |                       |", Actor.HitRoll, Actor.DamRoll);
        sb.AppendLine("+------------------------------+-------------------------+");
        if (pc != null)
        {
            sb.AppendFormatLine("Wimpy set to {0} hit points.", pc.Wimpy);
            if (pc[Conditions.Drunk] > 10)
                sb.AppendLine("You are drunk.");
            if (pc[Conditions.Thirst] == 0)
                sb.AppendLine("You are thirsty.");
            if (pc[Conditions.Hunger] == 0)
                sb.AppendLine("You are hungry.");
            switch (pc.Position)
            {
                case Positions.Sleeping: sb.AppendLine("You are sleeping."); break;
                case Positions.Resting: sb.AppendLine("You are resting."); break;
                case Positions.Sitting: sb.AppendLine("You are sitting."); break;
                case Positions.Standing: sb.AppendLine("You are standing."); break;
            }
            if (pc.ImmortalMode != ImmortalModeFlags.None)
                sb.AppendFormatLine("You are %G%{0}%x%", pc.ImmortalMode);
            else
                sb.AppendLine("You are %R%MORTAL%x%");
            if (pc.AutoFlags.HasFlag(AutoFlags.Affect))
            {
                if (Actor.Auras.Any())
                {
                    sb.AppendLine("%c%You are affected by the following auras:%x%");
                    // Auras
                    foreach (var aura in Actor.Auras.Where(x => Actor.ImmortalMode.HasFlag(ImmortalModeFlags.Holylight) || !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.AuraFlags.HasFlag(AuraFlags.Permanent) ? int.MaxValue : x.PulseLeft))
                        aura.Append(sb, true); // short version
                }
            }
        }
        // TODO: resistances, gold, item, weight, conditions
        //if (!IS_NPC(ch) && ch->pcdata->condition[COND_DRUNK] > 10)
        //    send_to_char("You are drunk.\n\r", ch);
        //if (!IS_NPC(ch) && ch->pcdata->condition[COND_THIRST] == 0)
        //    send_to_char("You are thirsty.\n\r", ch);
        //if (!IS_NPC(ch) && ch->pcdata->condition[COND_HUNGER] == 0)
        //    send_to_char("You are hungry.\n\r", ch);
        // positions
        //switch (ch->position)
        //{
        //    case POS_DEAD:
        //        send_to_char("You are DEAD!!\n\r", ch);
        //        break;
        //    case POS_MORTAL:
        //        send_to_char("You are mortally wounded.\n\r", ch);
        //        break;
        //    case POS_INCAP:
        //        send_to_char("You are incapacitated.\n\r", ch);
        //        break;
        //    case POS_STUNNED:
        //        send_to_char("You are stunned.\n\r", ch);
        //        break;
        //    case POS_SLEEPING:
        //        send_to_char("You are sleeping.\n\r", ch);
        //        break;
        //    case POS_RESTING:
        //        send_to_char("You are resting.\n\r", ch);
        //        break;
        //    case POS_SITTING:
        //        send_to_char("You are sitting.\n\r", ch);
        //        break;
        //    case POS_STANDING:
        //        send_to_char("You are standing.\n\r", ch);
        //        break;
        //    case POS_FIGHTING:
        //        send_to_char("You are fighting.\n\r", ch);
        //        break;
        //}

        Actor.Send(sb);
    }
}
