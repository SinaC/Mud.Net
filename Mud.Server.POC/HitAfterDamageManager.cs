using Mud.Common.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.POC
{
    [Export(typeof(IHitAfterDamageManager)), Shared]
    public class HitAfterDamageManager : IHitAfterDamageManager
    {
        private IRandomManager RandomManager { get; }

        public HitAfterDamageManager(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public void OnHit(ICharacter hitter, ICharacter victim)
        {
            if (victim.ShieldFlags.IsSet("FireShield") && !hitter.ShieldFlags.IsSet("FireShield"))
            {
                var damage = RandomManager.Range(5, 15);
                hitter.AbilityDamage(victim, damage, Domain.SchoolTypes.Fire, "fireball", true);
                if (hitter.Fighting != victim) // stop if not anymore fighting
                    return;
            }
            if (victim.ShieldFlags.IsSet("IceShield") && !hitter.ShieldFlags.IsSet("IceShield"))
            {
                var damage = RandomManager.Range(10, 20);
                hitter.AbilityDamage(victim, damage, Domain.SchoolTypes.Cold, "chilling touch", true);
                if (hitter.Fighting != victim) // stop if not anymore fighting
                    return;
            }
            if (victim.ShieldFlags.IsSet("ShockShield") && !hitter.ShieldFlags.IsSet("ShockShield"))
            {
                var damage = RandomManager.Range(15, 25);
                hitter.AbilityDamage(victim, damage, Domain.SchoolTypes.Lightning, "lightning bolt", true);
                if (hitter.Fighting != victim) // stop if not anymore fighting
                    return;
            }
        }
    }
}
