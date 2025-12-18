using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Server.Random;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_cast_adept")]
    public class CastAdept : ISpecialBehavior
    {
        private ILogger<CastAdept> Logger { get; }
        private IRandomManager RandomManager { get; }

        public CastAdept(ILogger<CastAdept> logger, IRandomManager randomManager)
        {
            Logger = logger;
            RandomManager = randomManager;
        }

        // use healing spells on random people below level 11 in the room
        public bool Execute(INonPlayableCharacter npc)
        {
            if (!npc.IsValid || npc.Room == null
                || npc.Position <= Positions.Sleeping )
                return false;

            // search a random pc 'victim' below level 11
            var pcVictim = RandomManager.Random(npc.Room.People.OfType<IPlayableCharacter>().Where(x => npc.CanSee(x) && RandomManager.Chance(50) && x.Level < 11));
            if (pcVictim == null)
                return false;

            var choice = RandomManager.Next(16); // 7 out of 15 will lead to spell

            switch (choice)
            {
                case 0: // armor
                    return CastSpell(npc, "armor", pcVictim);
                case 1: // bless
                    return CastSpell(npc, "bless", pcVictim);
                case 2: // cure blindness
                    return CastSpell(npc, "cure blindness", pcVictim);
                case 3: // cure light
                    return CastSpell(npc, "cure light", pcVictim);
                case 4: // cure poison
                    return CastSpell(npc, "cure poison", pcVictim);
                case 5: // refresh
                    return CastSpell(npc, "cast refresh", pcVictim);
                case 6: // cure disease
                    return CastSpell(npc, "cure disease", pcVictim);
                default:
                    return false;
            }
        }

        private bool CastSpell(INonPlayableCharacter caster, string spellName, ICharacter victim)
        {
            var successfull = caster.CastSpell(spellName, victim);
            if (!successfull)
                Logger.LogError("CastAdept: error on {caster} while casting spell {spellName} on {victimName}", caster.DebugName, spellName, victim.DebugName);
            return successfull;
        }
    }
}
