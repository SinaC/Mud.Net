using System;
using System.Collections.Generic;

namespace Mud.Server
{
    public interface IEntity : IActor
    {
        Guid Id { get; }
        bool IsValid { get; } // always true unless entity has been removed from the game
        string Name { get; }
        IEnumerable<string> Keywords { get; }
        string DisplayName { get; }
        string Description { get; }
        string DebugName { get; }

        bool Incarnatable { get; }
        IAdmin IncarnatedBy { get; }

        // Auras
        IEnumerable<IPeriodicAura> PeriodicAuras { get; }
        IEnumerable<IAura> Auras { get; }

        // Recompute
        void Reset(); // Remove periodic auras
        void Recompute();

        // Auras
        IAura GetAura(string abilityName);
        IAura GetAura(IAbility ability);
        void AddPeriodicAura(IPeriodicAura aura);
        void RemovePeriodicAura(IPeriodicAura aura);
        void AddAura(IAura aura, bool recompute);
        void RemoveAura(IAura aura, bool recompute);
        void RemoveAuras(Func<IAura, bool> filterFunc, bool recompute);

        // Incarnation
        bool ChangeIncarnation(IAdmin admin);

        // Display
        string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false); // Use to get DisplayName relative to Beholder. If Beholder cannot see 'this', it will return Someone or Something. It 'this' is quest objective, (Quest) will be prefixed
        string RelativeDescription(ICharacter beholder); // Add (Quest) to description if beholder is on a quest with 'this' as objective
        void Act(IEnumerable<ICharacter> characters, string format, params object[] arguments); // to every entities in provided list

        //
        void OnRemoved(); // called before removing an entity from the game
        void OnCleaned(); // called when removing definitively an entity from the game
    }
}
