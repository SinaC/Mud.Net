using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Specials
{
    public abstract class BreathBase : ISpecialBehavior
    {
        protected ILogger<BreathBase> Logger { get; }
        protected IRandomManager RandomManager { get; }

        protected BreathBase(ILogger<BreathBase> logger, IRandomManager randomManager)
        {
            Logger = logger;
            RandomManager = randomManager;
        }

        public bool Execute(INonPlayableCharacter npc)
        {
            if (!npc.IsValid || npc.Room == null
                || npc.Position <= Domain.Positions.Sleeping || npc.Fighting == null)
                return false;

            // search a random 'victim'
            var victim = RandomManager.Random(npc.Room.People.Where(x => npc.Fighting == x/* && RandomManager.Chance(12)*/));
            if (victim == null)
                return false;

            var spellName = GetSpellName();
            if (string.IsNullOrWhiteSpace(spellName))
                return false;

            return CastSpell(npc, spellName, victim);
        }

        protected abstract string? GetSpellName();

        private bool CastSpell(INonPlayableCharacter caster, string spellName, ICharacter victim)
        {
            var successfull = caster.CastSpell(spellName, victim);
            if (!successfull)
                Logger.LogError("CastMage: error while casting spell {spellName} on {victimName}", spellName, victim);
            return successfull;
        }
    }
}
