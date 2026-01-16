using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Random;
using System.Text;

namespace Mud.Server.POC.Affects;

[AffectNoData("ShockShield")]
public class ShockShieldAfterHitAffect : NoAffectDataAffectBase, ICharacterAfterHitAffect
{
    private IRandomManager RandomManager { get; }

    public ShockShieldAfterHitAffect(IRandomManager randomManager)
        : base()
    {
        RandomManager = randomManager;
    }

    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%does %Y%Shock%c% damage when being hit%x%");
    }

    public void AfterHit(ICharacter hitter, ICharacter victim)
    {
        if (!hitter.ShieldFlags.IsSet("ShockShield"))
        {
            var damage = RandomManager.Range(15, 25);
            hitter.AbilityDamage(victim, damage, SchoolTypes.Lightning, "%Y%lightning bolt%x%", true);
        }
    }
}
