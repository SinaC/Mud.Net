using System;
using Mud.Domain;

namespace Mud.Server
{
    public interface IAura
    {
        // Ability
        IAbility Ability { get; }

        // Source of Aura
        IEntity Source { get; }

        // Modifier
        AuraModifiers Modifier { get; }

        // Amount + %/fixed
        int Amount { get; }
        AmountOperators AmountOperator { get; }

        // Level
        int Level { get; }

        // Pulse left
        int PulseLeft { get; } // -1: infinite

        // Reset source
        void ResetSource();

        // Absorb, returns remaining damage (only for absorb Aura)
        int Absorb(int amount);

        // Refresh with a new aura
        void Refresh(IAura aura);

        // Change level, amount and pulse
        void Modify(int? level, int? amount, TimeSpan? ts);

        // Change level, amount, pulse and ability
        void Modify(int? level, int? amount, TimeSpan? ts, IAbility ability);

        // Called when dispelled
        void OnDispelled(IEntity dispelSource);

        // Called when vanished
        void OnVanished();

        // Decrease pulse left
        bool DecreasePulseLeft(int pulseCount); // true if timed out

        // Serialization
        AuraData MapAuraData();
    }

    // TODO ???
    //public interface IAbsorbAura : IAura
    //{
    //    // Absorb, returns remaining damage (only for absorb Aura)
    //    int Absorb(int damage);
    //}
}
