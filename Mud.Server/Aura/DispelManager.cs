using Mud.Common;
using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Collections.ObjectModel;

namespace Mud.Server.Aura;

public class DispelManager : IDispelManager
{
    private IRandomManager RandomManager { get; }
    private IAbilityManager AbilityManager { get; }

    public DispelManager(IRandomManager randomManager, IAbilityManager abilityManager)
    {
        RandomManager = randomManager;
        AbilityManager = abilityManager;
    }

    public bool TryDispels(int dispelLevel, ICharacter victim) // dispels every spells
    {
        bool found = false;
        IReadOnlyCollection<IAura> clone = new ReadOnlyCollection<IAura>(victim.Auras.Where(x => x.IsValid).ToList());
        foreach (IAura aura in clone)
        {
            if (!SavesDispel(dispelLevel, aura))
            {
                bool isDispellable = true; // dispellable by default
                string? dispelRoomMessage = null;
                if (aura.AbilityName != null)
                {
                    var abilityInfo = AbilityManager[aura.AbilityName];
                    isDispellable = abilityInfo?.IsDispellable == true;
                    dispelRoomMessage = abilityInfo?.DispelRoomMessage;
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
        bool found = false;
        foreach (IAura aura in victim.Auras.Where(x => x.AbilityName == abilityName)) // no need to clone because at most one entry will be removed
        {
            if (!SavesDispel(dispelLevel, aura))
            {
                victim.RemoveAura(aura, true); // RemoveAura will display DispelMessage
                var abilityInfo = AbilityManager[aura.AbilityName];
                var dispelRoomMessage = abilityInfo?.DispelRoomMessage;
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
        save = save.Range(5, 95);
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
        save = save.Range(5, 95);
        return RandomManager.Chance(save);
    }
}
