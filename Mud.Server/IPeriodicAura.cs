using Mud.Server.Constants;

namespace Mud.Server
{
    public enum PeriodicAuraTypes
    {
        Heal,
        Damage
    }

    public interface IPeriodicAura
    {
        // Ability
        IAbility Ability { get; }

        // Heal/damage
        PeriodicAuraTypes AuraType { get; }

        // Source of Aura
        ICharacter Source { get; } // TODO: entity

        // Damage type (if AuraType is damage)
        SchoolTypes School { get; }

        // Amount + %/fixed
        int Amount { get; }
        AmountOperators AmountOperator { get; }

        // Is damage/heal phrase visible
        bool TickVisible { get; }

        // Seconds left
        int SecondsLeft { get; }

        // Total ticks
        int TotalTicks { get; }

        // Delay between 2 ticks (in seconds)
        int TickDelay { get; }

        // Ticks left
        int TicksLeft { get; }

        // Reset source
        void ResetSource();

        // Process periodic aura (return true if aura is elapsed)
        bool Process(ICharacter victim);

        // Refresh with a new aura
        void Refresh(IPeriodicAura aura);

        // Called when dispelled
        void OnDispelled(IEntity source);

        // Called when vanished
        void OnVanished(IEntity source);
    }
}
