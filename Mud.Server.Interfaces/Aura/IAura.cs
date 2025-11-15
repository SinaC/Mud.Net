using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using System.Text;

namespace Mud.Server.Interfaces.Aura;

public interface IAura
{
    bool IsValid { get; } // auras are not removed immediately but in cleanup step

    int Level { get; }

    int PulseLeft { get; } // irrelevant if AuraFlags.Permanent is set

    string AbilityName { get; }

    AuraFlags AuraFlags { get; }

    IEnumerable<IAffect> Affects { get; } // affects linked to this aura

    void Update(int level, TimeSpan duration);

    T? AddOrUpdateAffect<T>(Func<T, bool> filterFunc, Func<T> createFunc, Action<T> updateFunc)
        where T : IAffect;

    bool DecreasePulseLeft(int pulseCount); // return true if timed out

    void DecreaseLevel();

    void OnRemoved(); // set IsValid, Ability, Source

    // Display
    StringBuilder Append(StringBuilder sb, bool shortDisplay = false);

    // Serialization
    AuraData MapAuraData();
}
