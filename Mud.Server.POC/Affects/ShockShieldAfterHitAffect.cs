using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.POC.Affects
{
    [Affect("ShockShield", typeof(ShockShieldAfterHitAffectData))]
    public class ShockShieldAfterHitAffect : ICharacterAfterHitAffect, ICustomAffect
    {
        private IRandomManager RandomManager { get; }

        public ShockShieldAfterHitAffect(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public void Initialize(AffectDataBase data)
        {
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%performs %Y%Shock%c% damage when being hit%x%");
        }


        public AffectDataBase MapAffectData()
        {
            return new ShockShieldAfterHitAffectData();
        }

        public void AfterHit(ICharacter hitter, ICharacter victim)
        {
            if (!hitter.ShieldFlags.IsSet("ShockShield"))
            {
                var damage = RandomManager.Range(15, 25);
                hitter.AbilityDamage(victim, damage, Domain.SchoolTypes.Lightning, "%Y%lightning bolt%x%", true);
            }
        }
    }
}
