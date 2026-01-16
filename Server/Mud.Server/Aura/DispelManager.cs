using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Aura;

[Export(typeof(IDispelManager)), Shared]
public class DispelManager : IDispelManager
{
    private IRandomManager RandomManager { get; }

    public DispelManager(IRandomManager randomManager)
    {
        RandomManager = randomManager;
    }

    public bool TryDispels(int dispelLevel, ICharacter victim) // dispels every spells
    {
        bool found = false;
        var validAuras = victim.Auras.Where(x => x.IsValid).ToArray(); // needed to clone because we may remove entries
        foreach (var aura in validAuras)
        {
            if (!SavesDispel(dispelLevel, aura))
            {
                bool isDispellable = true; // dispellable by default
                string? dispelRoomMessage = null;
                if (aura.AbilityDefinition != null)
                {
                    isDispellable = aura.AbilityDefinition.IsDispellable == true;
                    dispelRoomMessage = aura.AbilityDefinition.DispelRoomMessage;
                }

                if (isDispellable)
                {
                    found = true;
                    victim.RemoveAura(aura, false); // RemoveAura will display WearOff message
                    if (!string.IsNullOrWhiteSpace(dispelRoomMessage))
                        victim.Act(ActOptions.ToRoom, dispelRoomMessage, victim);
                }
            }
            else
                aura.DecreaseLevel();
        }
        if (found)
            victim.Recompute();
        return found;
    }

    public TryDispelReturnValues TryDispel(int dispelLevel, ICharacter victim, string abilityName) // was called check_dispel in Rom24
    {
        var found = false;
        var validMatchingAuras = victim.Auras.Where(x => x.IsValid && StringCompareHelpers.StringEquals(x.AbilityName, abilityName)).ToArray(); // needed to clone because we may remove entries
        foreach (var aura in validMatchingAuras)
        {
            if (!SavesDispel(dispelLevel, aura))
            {
                victim.RemoveAura(aura, true); // RemoveAura will display DispelMessage
                var dispelRoomMessage = aura.AbilityDefinition?.DispelRoomMessage;
                if (!string.IsNullOrWhiteSpace(dispelRoomMessage))
                    victim.Act(ActOptions.ToRoom, dispelRoomMessage, victim);
                return TryDispelReturnValues.Dispelled; // stop at first aura dispelled
            }
            else
                aura.DecreaseLevel();
            found = true;
        }
        return found
            ? TryDispelReturnValues.FoundAndNotDispelled
            : TryDispelReturnValues.NotFound;
    }

    public bool SavesDispel(int dispelLevel, int spellLevel, int pulseLeft)
    {
        if (pulseLeft < 0) // very hard to dispel permanent effects
            spellLevel += 5;

        int save = 50 + (spellLevel - dispelLevel) * 5;
        save = Math.Clamp(save, 5, 95);
        return RandomManager.Chance(save);
    }

    private bool SavesDispel(int dispelLevel, IAura aura)
    {
        if (aura.AuraFlags.HasFlag(AuraFlags.NoDispel))
            return true;
        int auraLevel = aura.Level;
        if (aura.AuraFlags.HasFlag(AuraFlags.Permanent)
            || aura.PulseLeft < 0) // very hard to dispel permanent effects
            auraLevel += 5;

        int save = 50 + (auraLevel - dispelLevel) * 5;
        save = Math.Clamp(save, 5, 95);
        return RandomManager.Chance(save);
    }
}
