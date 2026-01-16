using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("affects", "Information")]
[Alias("auras")]
[Help(
@"This command is used to show all the skills and spells affecting your
character. At low levels, only the spell name will be displayed, at
higher levels the effects and duration of the spell will also be shown.
Spell effects are no longer shown on score(this can be changed by using
'auto affect' command)")]
public class Affects : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        // TODO: various information depending on level
        StringBuilder sb = new();
        if (Actor.Auras.Any())
        {
            sb.AppendLine("%c%You are affected by the following auras:%x%");
            // Auras
            foreach (var aura in Actor.Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                aura.Append(sb);
        }
        else
            sb.AppendLine("%c%You are not affected by any spells.%x%");
        Actor.Page(sb);
    }
}
