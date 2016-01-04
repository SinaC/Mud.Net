using System;
using System.Collections.Generic;
using Mud.Logger;
using Mud.Server;
using Mud.Server.Constants;

namespace Mud.POC
{
    public class TestAct
    {
        public enum ActOptions
        {
            ToRoom, // everyone in the room except origin
            ToNotVictim, // everyone on in the room except Character and Target
            ToVictim, // only to Target
            ToCharacter, // only to Character
            ToAll // everyone in the room
        }

        public static void Act(ActOptions options, string format, ICharacter character, object param1, object param2, params object[] parameters)
        {
            if (character == null || character.Room == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Act with null ICharacter or null room");
                return;
            }

            // Firstly, apply .net formatting
            string rawPhrase = String.Format(format, parameters);

            // Secondly, apply Mud formatting
            if (options == ActOptions.ToAll || options == ActOptions.ToRoom || options == ActOptions.ToNotVictim)
                foreach (ICharacter to in character.Room.People)
                {
                    if (options == ActOptions.ToAll
                        || (options == ActOptions.ToRoom && to != character)
                        || (options == ActOptions.ToNotVictim && to != (ICharacter) param1))
                    {
                        string phrase = Format(rawPhrase, to, character, param1, param2);
                        to.Send(phrase);
                    }
                }
            else if (options == ActOptions.ToCharacter)
            {
                string phrase = Format(rawPhrase, character, character, param1, param2);
                character.Send(phrase);
            }
            else if (options == ActOptions.ToVictim)
            {
                string phrase = Format(rawPhrase, (ICharacter) param1, character, param1, param2);
                ((ICharacter) param1).Send(phrase);
            }
        }

        // TODO: use regex or manual parsing
        // $n and $N must check if target can see character (someone if not)
        // $e, $E, $m, $M, $s and $S must use sex mapping table
        private static string Format(string phrase, ICharacter target, ICharacter character, object param1, object param2)
        {
            //phrase = phrase.Replace("$t", (string) param1); // useless
            //phrase = phrase.Replace("$T", (string) param2); // useless
            phrase = phrase.Replace("$n", target.CanSee(character) ? character.Name : "someone"); // PERS(target, character)
            phrase = phrase.Replace("$N", ((ICharacter) param1).Name); //PERS(target, param1)
            phrase = phrase.Replace("$e", "he"); // he or she (character.sex)
            phrase = phrase.Replace("$E", "he"); // he or she (target.sex)
            phrase = phrase.Replace("$m", "him"); // him or her (character.sex)
            phrase = phrase.Replace("$M", "him"); // him or her (target.sex)
            phrase = phrase.Replace("$s", "his"); // his or her (character.sex)
            phrase = phrase.Replace("$S", "his"); // his or her (target.sex)
            phrase = phrase.Replace("$p", ((IItem) param1).DisplayName);
            phrase = phrase.Replace("$P", ((IItem)param2).DisplayName);
            phrase = phrase.Replace("$d", param2 == null ? "door" : ((IExit) param2).Name);

            return phrase;
        }

        // Description macros.
//#define PERS(ch, looker)	( can_see( looker, (ch) ) ?		\
//                ( IS_NPC(ch) ? (ch)->short_descr	\
//                : (ch)->name ) : "someone" )

        //https://genderneutralpronoun.wordpress.com/tag/ze-and-zir/
        private static readonly IDictionary<Sex, string> _subjects = new Dictionary<Sex, string>
        {
            {Sex.Neutral, "it"},
            {Sex.Male, "he"},
            {Sex.Female, "she"},
        };

        private static readonly IDictionary<Sex, string> _objectives = new Dictionary<Sex, string>
        {
            {Sex.Neutral, "it"},
            {Sex.Male, "him"},
            {Sex.Female, "her"},
        };

        private static readonly IDictionary<Sex, string> _possessives = new Dictionary<Sex, string>
        {
            {Sex.Neutral, "its"},
            {Sex.Male, "his"},
            {Sex.Female, "her"},
        };

        public static void Act(string format, ICharacter character, ICharacter victim, ActOptions option)
        {
            GenericAct(option, character, victim, target => CreatePhrase(format, target, character, victim));
        }

        private static string CreatePhrase(string phrase, ICharacter target, ICharacter character, ICharacter victim)
        {
            phrase = phrase.Replace("$n", target.CanSee(character) ? character.Name : "someone"); // TODO: short description is NPC
            phrase = phrase.Replace("$N", target.CanSee(victim) ? victim.Name : "someone"); // TODO: short description is NPC
            phrase = phrase.Replace("$e", _subjects[character.Sex]);
            phrase = phrase.Replace("$E", _subjects[victim.Sex]);
            phrase = phrase.Replace("$m", _objectives[character.Sex]);
            phrase = phrase.Replace("$M", _objectives[victim.Sex]);
            phrase = phrase.Replace("$s", _possessives[character.Sex]);
            phrase = phrase.Replace("$S", _possessives[victim.Sex]);

            return phrase;
        }

        private static void GenericAct(ActOptions option, ICharacter character, ICharacter victim, Func<ICharacter, string> createPhraseFunc)
        {
            if (option == ActOptions.ToAll || option == ActOptions.ToRoom || option == ActOptions.ToNotVictim)
                foreach (ICharacter to in character.Room.People)
                {
                    if (option == ActOptions.ToAll
                        || (option == ActOptions.ToRoom && to != character)
                        || (option == ActOptions.ToNotVictim && to != victim))
                    {
                        string phrase = createPhraseFunc(character);
                        to.Send(phrase);
                    }
                }
            else if (option == ActOptions.ToCharacter)
            {
                string phrase = createPhraseFunc(character);
                character.Send(phrase);
            }
            else if (option == ActOptions.ToVictim)
            {
                string phrase = createPhraseFunc(character);
                victim.Send(phrase);
            }
        }
    }
}
