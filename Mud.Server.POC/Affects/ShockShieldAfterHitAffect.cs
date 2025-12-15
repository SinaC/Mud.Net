using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.POC.Affects
{
    [Affect(AffectName, typeof(NoAffectData))]
    public class ShockShieldAfterHitAffect : ICharacterAfterHitAffect
    {
        private const string AffectName = "ShockShield";

        private IRandomManager RandomManager { get; }

        public ShockShieldAfterHitAffect(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%does %Y%Shock%c% damage when being hit%x%");
        }


        public AffectDataBase MapAffectData()
        {
            return new NoAffectData { AffectName = AffectName };
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
