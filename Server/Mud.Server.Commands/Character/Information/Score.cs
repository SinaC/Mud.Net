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
        sb.AppendLine("+----------------------------------------------------------+"); // length 1 + 58 + 1
        sb.AppendLine("|" + Actor.DisplayName.CenterText(58) + "|");
        sb.AppendLine("+--------------------------------+-------------------------+");
        sb.AppendLine("| %W%Attributes%x%                     |                         |");
        sb.AppendFormatLine("| %c%Strength     :   %W%[{0,5}/{1,5}]%x% | %c%Race   : %W%{2,14}%x% |", Actor[CharacterAttributes.Strength], Actor.BaseAttribute(CharacterAttributes.Strength), Actor.Race?.DisplayName ?? "(none)");
        sb.AppendFormatLine("| %c%Intelligence :   %W%[{0,5}/{1,5}]%x% | %c%Class  : %W%{2,14}%x% |", Actor[CharacterAttributes.Intelligence], Actor.BaseAttribute(CharacterAttributes.Intelligence), Actor.Class?.DisplayName ?? "(none)");
        sb.AppendFormatLine("| %c%Wisdom       :   %W%[{0,5}/{1,5}]%x% | %c%Sex    : %W%{2,14}%x% |", Actor[CharacterAttributes.Wisdom], Actor.BaseAttribute(CharacterAttributes.Wisdom), Actor.Sex);
        sb.AppendFormatLine("| %c%Dexterity    :   %W%[{0,5}/{1,5}]%x% | %c%Level  : %W%{2,14}%x% |", Actor[CharacterAttributes.Dexterity], Actor.BaseAttribute(CharacterAttributes.Dexterity), Actor.Level);
        if (pc != null)
            sb.AppendFormatLine("| %c%Constitution :   %W%[{0,5}/{1,5}]%x% | %c%NxtLvl : %W%{2,14}%x% |", Actor[CharacterAttributes.Constitution], Actor.BaseAttribute(CharacterAttributes.Constitution), pc.ExperienceToLevel);
        else
            sb.AppendFormatLine("| %c%Constitution :   %W%[{0,5}/{1,5}]%x% |                         |", Actor[CharacterAttributes.Constitution], Actor.BaseAttribute(CharacterAttributes.Constitution), Actor.Level);
        sb.AppendLine("+--------------------------------+-------------------------+");
        sb.AppendLine("| %W%Resources%x%                      | %W%Defensive%x%               |");
        sb.AppendFormatLine("| %g%Hp       : %W%[{0,8}/{1,8}]%x% | %g%Bash   :         %W%{2,6}%x% |", Actor[ResourceKinds.HitPoints], Actor.MaxResource(ResourceKinds.HitPoints), Actor[Armors.Bash]);
        sb.AppendFormatLine("| %g%Move     : %W%[{0,8}/{1,8}]%x% | %g%Pierce :         %W%{2,6}%x% |", Actor[ResourceKinds.MovePoints], Actor.MaxResource(ResourceKinds.MovePoints), Actor[Armors.Pierce]);
        List<string> resources = [];
        foreach (var resourceKind in Actor.CurrentResourceKinds.Where(x => !x.IsMandatoryResource()))
            resources.Add($"%g%{resourceKind,-7}  : %W%[{Actor[resourceKind],8}/{Actor.MaxResource(resourceKind),8}]%x%");
        if (resources.Count < 3)
            resources.AddRange(Enumerable.Repeat("                              ", 3 - resources.Count));
        sb.AppendFormatLine("| {0} | %g%Slash  :         %W%{1,6}%x% |", resources[0], Actor[Armors.Slash]);
        sb.AppendFormatLine("| {0} | %g%Exotic :         %W%{1,6}%x% |", resources[1], Actor[Armors.Exotic]);
        sb.AppendFormatLine("| {0} | %g%Saves  :         %W%{1,6}%x% |", resources[2], Actor[CharacterAttributes.SavingThrow]);
        for(int i = 3; i < resources.Count; i++)
            sb.AppendFormatLine("| {0} |                         |", resources[i], Actor[Armors.Slash]);
        sb.AppendLine("+--------------------------------+-------------------------+");
            sb.AppendFormatLine("| %g%Hit:     %W%{0,6}%x%   %g%Dam:  %W%{1,6}%x% | %g%Alignment:       %W%{2,6}%x% |", Actor.HitRoll, Actor.DamRoll, Actor.Alignment);
        sb.AppendLine("+--------------------------------+-------------------------+");
        if (pc != null)
            sb.AppendFormatLine("| %y%Silver: %W%{0,7}%x%  %y%Gold: %W%{1,7}%x% | %y%Train: %W%{2,3}%x%   %y%Pract: %W%{3,3}%x% |", FormatCurrency(Actor.SilverCoins), FormatCurrency(Actor.GoldCoins), pc.Trains, pc.Practices);
        else
            sb.AppendFormatLine("| %y%Silver: %W%{0,7}%x%  %y%Gold: %W%{1,7}%x% |                         |", FormatCurrency(Actor.SilverCoins), FormatCurrency(Actor.GoldCoins));
        if (pc != null)
        {
            sb.AppendFormatLine("| %y%Carry  :       %W%[{0,6}/{1,6}]%x% | %y%Position:    %W%{2,10}%x% |", Actor.CarryNumber, Actor.MaxCarryNumber, pc.Position);
            sb.AppendFormatLine("| %y%Weight :       %W%[{0,6}/{1,6}]%x% | %y%Wimpy:         %W%{2,8}%x% |", Actor.CarryWeight, Actor.MaxCarryWeight, pc.Wimpy);
            foreach (var currency in Enum.GetValues<Currencies>())
                sb.AppendFormatLine("| %y%{0,10} :           %W%{1,6}%x% |                         |", currency, pc[currency]);
        }
        else
        {
            sb.AppendFormatLine("| %y%Carry  :       %W%[{0,6}/{1,6}]%x% |                         |", Actor.CarryNumber, Actor.MaxCarryNumber);
            sb.AppendFormatLine("| %y%Weight :       %W%[{0,6}/{1,6}]%x% |                         |", Actor.CarryWeight, Actor.MaxCarryWeight);
        }
        sb.AppendLine("+--------------------------------+-------------------------+");
        if (pc != null)
        {
            if (pc[Conditions.Drunk] > 10)
                sb.AppendLine("You are drunk.");
            if (pc[Conditions.Thirst] == 0)
                sb.AppendLine("You are thirsty.");
            if (pc[Conditions.Hunger] == 0)
                sb.AppendLine("You are hungry.");
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
        // TODO: resistances

        Actor.Send(sb);
    }

    private static string FormatCurrency(long currency)
    {
        if (currency < 1_000)
            return $"{currency,6}";
        else if (currency < 1_000_000)
        {
            var inThousand = currency / 1_000m;
            return $"{inThousand:N2}k";
        }
        else
        {
            var inMillion = currency / 1_000_000m;
            return $"{inMillion:N2}m";
        }
    }
}
