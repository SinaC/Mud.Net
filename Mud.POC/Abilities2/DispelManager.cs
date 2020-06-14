using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Common;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;

namespace Mud.POC.Abilities2
{
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
                    found = true;
                    victim.RemoveAura(aura, false); // RemoveAura will display WearOff message
                    if (aura.AbilityName != null)
                    {
                        IAbilityInfo abilityInfo = AbilityManager[aura.AbilityName];
                        string dispelRoomMessage = abilityInfo?.DispelRoomMessage;
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
                    IAbilityInfo abilityInfo = AbilityManager[aura.AbilityName];
                    string dispelRoomMessage = abilityInfo?.DispelRoomMessage;
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

        public bool SavesDispel(int dispelLevel, IAura aura)
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
}
