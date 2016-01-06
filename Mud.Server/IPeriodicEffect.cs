using Mud.Server.Constants;

namespace Mud.Server
{
    public enum EffectTypes
    {
        Heal,
        Damage
    }

    public interface IPeriodicEffect
    {
        string Name { get; }

        // Heal/damage
        EffectTypes EffectType { get; }

        // Source of effect
        ICharacter Source { get; } // TODO: entity

        // Damage type (if EffectType is damage)
        DamageTypes DamageType { get; }

        // Amount + %/fixed
        int Amount { get; }
        AmountOperators AmountOperator { get; }

        // Is damage phrase visible
        bool Visible { get; }

        // Seconds left
        int SecondsLeft { get; }

        // Total ticks
        int TotalTicks { get; }

        // Delay between 2 ticks
        int TickDelay { get; }

        // Ticks left
        int TicksLeft { get; }

        // Reset source
        void ResetSource();

        // Process periodic effect (return true if effect is finished)
        bool Process(ICharacter victim);
    }
}
