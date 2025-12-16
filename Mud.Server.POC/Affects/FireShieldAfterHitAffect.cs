using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.POC.Affects;

[AffectNoData("FireShield")]
public class FireShieldAfterHitAffect : NoAffectDataAffectBase, ICharacterAfterHitAffect
{
    private IRandomManager RandomManager { get; }

    public FireShieldAfterHitAffect(IRandomManager randomManager)
        : base()
    {
        RandomManager = randomManager;
    }

    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%does %R%Fire%c% damage when being hit%x%");
    }


    public void AfterHit(ICharacter hitter, ICharacter victim)
    {
        if (!hitter.ShieldFlags.IsSet("FireShield"))
        {
            var damage = RandomManager.Range(5, 15);
            hitter.AbilityDamage(victim, damage, Domain.SchoolTypes.Fire, "%R%fireball%x%", true);
        }
    }
}
