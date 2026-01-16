using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Random;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_cast_mage")]
    public class CastMage : ISpecialBehavior
    {
        private ILogger<CastAdept> Logger { get; }
        private IRandomManager RandomManager { get; }

        public CastMage(ILogger<CastAdept> logger, IRandomManager randomManager)
        {
            Logger = logger;
            RandomManager = randomManager;
        }

        // use mage harmful spells on victim
        public bool Execute(INonPlayableCharacter npc)
        {
            if (!npc.IsValid || npc.Room == null
                || npc.Position <= Positions.Sleeping || npc.Fighting == null)
                return false;

            // search a random 'victim'
            var victim = RandomManager.Random(npc.Room.People.Where(x => npc.Fighting == x && RandomManager.Chance(25)));
            if (victim == null)
                return false;

            string? spellName = null;
            int minNpcLevel = 0;

            for (; ; )
            {
                var choice = RandomManager.Next(16);
                switch (choice)
                {
                    case 0:
                        spellName = "blindness";
                        minNpcLevel = 0;
                        break;
                    case 1:
                        spellName = "chill touch";
                        minNpcLevel = 3;
                        break;
                    case 2:
                        spellName = "weaken";
                        minNpcLevel = 7;
                        break;
                    case 3:
                        spellName = "teleport";
                        minNpcLevel = 8;
                        break;
                    case 4:
                        spellName = "colour spray";
                        minNpcLevel = 11;
                        break;
                    case 5:
                        spellName = "change sex";
                        minNpcLevel = 12;
                        break;
                    case 6:
                        spellName = "energy drain";
                        minNpcLevel = 13;
                        break;
                    case 7:
                    case 8:
                    case 9:
                        spellName = "fireball";
                        minNpcLevel = 15;
                        break;
                    case 10:
                        spellName = "plague";
                        minNpcLevel = 20;
                        break;
                    default:
                        spellName = "acid blast";
                        minNpcLevel = 20;
                        break;
                }
                if (npc.Level >= minNpcLevel)
                    break; // spell found
            }
            return CastSpell(npc, spellName, victim);
        }

        private bool CastSpell(INonPlayableCharacter caster, string spellName, ICharacter victim)
        {
            var successfull = caster.CastSpell(spellName, victim);
            if (!successfull)
                Logger.LogWarning("CastMage: {caster} unsuccessfully cast spell {spellName} on {victimName}", caster.DebugName, spellName, victim.DebugName);
            return successfull;
        }
    }
}
