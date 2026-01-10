using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Common
{
    public static class SpellHelper
    {
        // TODO: maybe a table should be constructed for each spell to avoid computing at each cast

        private static readonly (string syllable, string transformed)[] SyllableTable =
        [
            ( " ",      " "         ),
            ( "ar",     "abra"      ),
            ( "au",     "kada"      ),
            ( "bless",  "fido"      ),
            ( "blind",  "nose"      ),
            ( "bur",    "mosa"      ),
            ( "cu",     "judi"      ),
            ( "de",     "oculo"     ),
            ( "en",     "unso"      ),
            ( "light",  "dies"      ),
            ( "lo",     "hi"        ),
            ( "mor",    "zak"       ),
            ( "move",   "sido"      ),
            ( "ness",   "lacri"     ),
            ( "ning",   "illa"      ),
            ( "per",    "duda"      ),
            ( "ra",     "gru"       ),
            ( "fresh",  "ima"       ),
            ( "re",     "candus"    ),
            ( "son",    "sabru"     ),
            ( "tect",   "infra"     ),
            ( "tri",    "cula"      ),
            ( "ven",    "nofo"      ),
            ( "a", "a" ), ( "b", "b" ), ( "c", "q" ), ( "d", "e" ),
            ( "e", "z" ), ( "f", "y" ), ( "g", "o" ), ( "h", "p" ),
            ( "i", "u" ), ( "j", "y" ), ( "k", "t" ), ( "l", "r" ),
            ( "m", "w" ), ( "n", "i" ), ( "o", "a" ), ( "p", "s" ),
            ( "q", "d" ), ( "r", "f" ), ( "s", "g" ), ( "t", "h" ),
            ( "u", "j" ), ( "v", "z" ), ( "w", "x" ), ( "x", "n" ),
            ( "y", "l" ), ( "z", "k" )
        ];

        public static void SaySpell(ICharacter caster, IAbilityDefinition abilityDefinition)
            => SaySpell(caster, abilityDefinition.Name);

        public static void SaySpell(ICharacter caster, string spellName)
        {
            var mysticalWords = Translate(spellName);

            // Say to people in room except source (if target is fluent with that spell, hear the spell clearly)
            foreach (var target in caster.Room.People.Where(x => x != caster && x.CanSee(caster)))
            {
                var (_, abilityLearned) = target.GetAbilityLearnedAndPercentage(spellName);
                if (abilityLearned != null && abilityLearned.Level < target.Level)
                    target.Act(ActOptions.ToCharacter, "%W%{0} casts the spell '{1}'%x%.", caster, spellName); // known the spell
                else
                    target.Act(ActOptions.ToCharacter, "%W%{0} utters the words, '{1}'%x%.", caster, mysticalWords); // doesn't known the spell
            }
        }

        private static string Translate(string spellName)
        {
            // Build mystical words for spell
            var translation = new StringBuilder();
            var toBeTranslated = spellName.ToLowerInvariant();
            var remaining = toBeTranslated;
            while (remaining.Length > 0)
            {
                bool found = false;
                foreach (var syllable in SyllableTable)
                {
                    if (remaining.StartsWith(syllable.syllable))
                    {
                        translation.Append(syllable.transformed);
                        remaining = remaining[syllable.syllable.Length..];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    translation.Append('?');
                    remaining = remaining[1..];
                    //Logger.LogWarning("Spell {abilityName} contains a character which is not found in syllable table", spellName);
                }
            }
            return translation.ToString();
        }
    }
}
