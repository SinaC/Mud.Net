using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2.Rom24Effects
{
    public class RefreshEffect : IEffect<ICharacter>
    {
        public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
        {
            victim.UpdateMovePoints(level);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (victim.MovePoints == victim.MaxMovePoints)
                victim.Send("You feel fully refreshed!");
            else
                victim.Send("You feel less tired.");
            if (source != victim)
                victim.Send("Ok");
        }
    }
}
