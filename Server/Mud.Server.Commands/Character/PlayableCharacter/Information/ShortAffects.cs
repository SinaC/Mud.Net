using Mud.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("shortaffects", "Information", Priority = 100)]
[Alias("saffects")]
[Alias("sauras")]
[Help(
@"This command is used to show all the skills and spells affecting your
character. At low levels, only the spell name will be displayed, at
higher levels the effects and duration of the spell will also be shown.
Spell effects are no longer shown on score(this can be changed by using
'auto affect' command)")]
public class ShortAffects : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        // TODO: various information depending on level
        StringBuilder sb = new ();
        if (Actor.Auras.Any())
        {
            sb.AppendLine("%c%You are affected by following auras:%x%");
            // Auras
            foreach (var aura in Actor.Auras.Where(x => Actor.ImmortalMode.IsSet("Holylight") || !x.AuraFlags.IsSet("Hidden")).OrderBy(x => x.AuraFlags.IsSet("Permanent") ? int.MaxValue : x.PulseLeft))
                aura.Append(sb, true);
        }
        else
            sb.AppendLine("%c%You are not affected by any spells.%x%");

        if (Actor.Pets.Any())
            foreach (var pet in Actor.Pets.Where(x => x.Auras.Any()))
            {
                sb.AppendFormatLine("%c%{0} is affected by following auras:%x%", pet.DisplayName);
                foreach (IAura aura in pet.Auras.Where(x => !x.AuraFlags.IsSet("Hidden")).OrderBy(x => x.PulseLeft))
                    aura.Append(sb, true);
            }

        Actor.Page(sb);
    }
}
