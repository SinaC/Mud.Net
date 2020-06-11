using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability
{
    public class KnownAbility : IKnownAbility
    {
        public IAbility Ability { get; set; }

        public ResourceKinds? ResourceKind { get; set; }

        public int CostAmount { get; set; }

        public CostAmountOperators CostAmountOperator { get; set; }

        public int Level { get; set; } // level at which ability can be learned
        
        public int Learned { get; set; } // practice percentage, 0 means not learned, 100 mean fully learned

        public int Rating { get; set; } // how difficult is it to improve/gain/practice

        public bool CanBeGained(IPlayableCharacter playableCharacter) => Level <= playableCharacter.Level && Learned == 0;
        public bool CanBePracticed(IPlayableCharacter playableCharacter) => Level <= playableCharacter.Level && Learned > 0;

        public KnownAbilityData MapKnownAbilityData()
        {
            return new KnownAbilityData
            {
                AbilityId = Ability?.Id ?? -1,
                ResourceKind = ResourceKind,
                CostAmount = CostAmount,
                CostAmountOperator = CostAmountOperator,
                Level = Level,
                Learned = Learned,
                Rating = Rating,
            };
        }
    }
}
