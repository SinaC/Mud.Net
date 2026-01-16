using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Random;
using System.Text;

namespace Mud.Server.POC.Affects;

[AffectNoData("IceShield")]
public class IceShieldAfterHitAffect : NoAffectDataAffectBase, ICharacterAfterHitAffect
{
    private IRandomManager RandomManager { get; }

    public IceShieldAfterHitAffect(IRandomManager randomManager)
        : base()
    {
        RandomManager = randomManager;
    }

    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%does %C%Cold%c% damage when being hit%x%");
    }

    public void AfterHit(ICharacter hitter, ICharacter victim)
    {
        if (!hitter.ShieldFlags.IsSet("IceShield"))
        {
            var damage = RandomManager.Range(10, 20);
            hitter.AbilityDamage(victim, damage, SchoolTypes.Cold, "%C%chilling touch%x%", true);
        }
    }
}
