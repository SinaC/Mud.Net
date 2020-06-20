using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Linq;
using System.Text;

namespace Mud.Server.Character.PlayableCharacter.Information
{
    [PlayableCharacterCommand("affects", "Information")]
    [Alias("auras")]
    public class Affects : PlayableCharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            if (Actor.Auras.Any())
            {
                sb.AppendLine("%c%You are affected by following auras:%x%");
                // Auras
                foreach (IAura aura in Actor.Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                    aura.Append(sb);
            }
            else
                sb.AppendLine("%c%You are not affected by any spells.%x%");

            if (Actor.Pets.Any())
                foreach (INonPlayableCharacter pet in Actor.Pets.Where(x => x.Auras.Any()))
                {
                    sb.AppendFormatLine("%c%{0} is affected by following auras:%x%", pet.DisplayName);
                    foreach (IAura aura in pet.Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                        aura.Append(sb);
                }

            Actor.Page(sb);
        }
    }
}
