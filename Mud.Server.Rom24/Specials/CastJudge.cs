using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Server.Random;
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
                || npc.Position <= Domain.Positions.Sleeping || npc.Fighting == null)
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
                Logger.LogError("CastJudge: error while casting spell {spellName} on {victimName}", spellName, victim);
            return successfull;
        }
    }
}
