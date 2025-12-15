using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.POC.Affects
{
    [Affect("FireShield", typeof(FireShieldAfterHitAffectData))]
    public class FireShieldAfterHitAffect : ICharacterAfterHitAffect, ICustomAffect
    {
        private IRandomManager RandomManager { get; }

        public FireShieldAfterHitAffect(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public void Initialize(AffectDataBase data)
        {
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%performs %R%Fire%c% damage when being hit%x%");
        }


        public AffectDataBase MapAffectData()
        {
            return new FireShieldAfterHitAffectData();
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
}
