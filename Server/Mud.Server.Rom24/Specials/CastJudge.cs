using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Random;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_cast_judge")]
    public class CastJudge : ISpecialBehavior
    {
        private ILogger<CastAdept> Logger { get; }
        private IRandomManager RandomManager { get; }

        public CastJudge(ILogger<CastAdept> logger, IRandomManager randomManager)
        {
            Logger = logger;
            RandomManager = randomManager;
        }

        // use high explosive spells on victim
        public bool Execute(INonPlayableCharacter npc)
        {
            if (!npc.IsValid || npc.Room == null
                || npc.Position <= Positions.Sleeping || npc.Fighting == null)
                return false;

            // search a random 'victim'
            var victim = RandomManager.Random(npc.Room.People.Where(x => npc.Fighting == x && RandomManager.Chance(25)));
            if (victim == null)
                return false;

            return CastSpell(npc, "high explosive", victim);
        }

        private bool CastSpell(INonPlayableCharacter caster, string spellName, ICharacter victim)
        {
            var successfull = caster.CastSpell(spellName, victim);
            if (!successfull)
                Logger.LogError("CastJudge: error on {caster} while casting spell {spellName} on {victimName}", caster.DebugName, spellName, victim.DebugName);
            return successfull;
        }
    }
}
