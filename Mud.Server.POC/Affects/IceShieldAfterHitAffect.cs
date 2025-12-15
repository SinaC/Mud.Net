using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.POC.Affects
{
    [Affect("IceShield", typeof(IceShieldAfterHitAffectData))]
    public class IceShieldAfterHitAffect : ICharacterAfterHitAffect, ICustomAffect
    {
        private IRandomManager RandomManager { get; }

        public IceShieldAfterHitAffect(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public void Initialize(AffectDataBase data)
        {
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%performs %C%Cold%c% damage when being hit%x%");
        }


        public AffectDataBase MapAffectData()
        {
            return new IceShieldAfterHitAffectData();
        }

        public void AfterHit(ICharacter hitter, ICharacter victim)
        {
            if (!hitter.ShieldFlags.IsSet("IceShield"))
            {
                var damage = RandomManager.Range(10, 20);
                hitter.AbilityDamage(victim, damage, Domain.SchoolTypes.Cold, "%C%chilling touch%x%", true);
            }
        }
    }
}
