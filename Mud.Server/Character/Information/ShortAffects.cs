using System.Linq;
using System.Text;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Information
{
    [CharacterCommand("saffects", "Information")]
    [CharacterCommand("sauras", "Information")]
    public class ShortAffects : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            if (Actor.Auras.Any())
            {
                sb.AppendLine("%c%You are affected by the following auras:%x%");
                // Auras
                foreach (IAura aura in Actor.Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                    aura.Append(sb, true);
            }
            else
                sb.AppendLine("%c%You are not affected by any spells.%x%");
            Actor.Page(sb);
        }
    }
}
