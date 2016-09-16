using System;
using Mud.Server.Constants;

namespace Mud.Server
{
    public interface IAura
    {
        // Ability
        IAbility Ability { get; }

        // Source of Aura
        ICharacter Source { get; } // TODO: entity

        // Modifier
        AuraModifiers Modifier { get; }

        // Amount + %/fixed
        int Amount { get; }
        AmountOperators AmountOperator { get; }

        // Start time
        DateTime StartTime { get; }

        // Total left
        int TotalSeconds { get; }

        // Seconds left
        int SecondsLeft { get; }

        // Reset source
        void ResetSource();

        // Absorb, returns remaining damage (only for absorb Aura)
        int Absorb(int amount);

        // Refresh with a new aura
        void Refresh(IAura aura);

        // Called when dispelled
        void OnDispelled(IEntity dispelSource);

        // Called when vanished
        void OnVanished();
    }

    // TODO ???
    //public interface IAbsorbAura : IAura
    //{
    //    // Absorb, returns remaining damage (only for absorb Aura)
    //    int Absorb(int damage);
    //}
}
