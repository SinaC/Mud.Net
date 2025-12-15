using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.POC.Affects
{
    [Affect(AffectName, typeof(NoAffectData))]
    public class FireShieldAfterHitAffect : ICharacterAfterHitAffect
    {
        private const string AffectName = "FireShield";

        private IRandomManager RandomManager { get; }

        public FireShieldAfterHitAffect(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%does %R%Fire%c% damage when being hit%x%");
        }


        public AffectDataBase MapAffectData()
        {
            return new NoAffectData { AffectName = AffectName };
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
