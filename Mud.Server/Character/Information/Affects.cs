using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Character.Information;

[CharacterCommand("affects", "Information")]
[Alias("auras")]
public class Affects : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
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
