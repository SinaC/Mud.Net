using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mud.Logger;
using Mud.Server;
using Mud.Server.Player;

namespace Mud.POC
{
    public class Class1
    {
        // TO_ROOM: everyone in the room except origin
        // TO_NOTVICT: everyone on in the room except origin and target
        // TO_VICT: only to target
        // TO_CHAR: only to origin
        // TO_ALL: everyone in the room

        public enum ActOptions
        {
            ToRoom,
            ToNotVict,
            ToVict,
            ToChar,
            ToAll
        }

        public void Act(ActOptions options, string format, ICharacter character, object param1, object param2, params object[] parameters)
        {
            if (character == null || character.Room == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Act with null ICharacter or null room");
                return;
            }

            // Firstly, apply .net formatting
            string rawPhrase = String.Format(format, parameters);
            
            // Secondly, apply Mud formatting
            if (options == ActOptions.ToAll || options == ActOptions.ToRoom || options == ActOptions.ToNotVict)
                foreach (ICharacter to in character.Room.CharactersInRoom)
                {
                    if (options == ActOptions.ToAll
                        || (options == ActOptions.ToRoom && to != character)
                        || (options == ActOptions.ToNotVict && to != (ICharacter)param1))
                    {
                        string phrase = Format(rawPhrase, to, character, param1, param2);
                        to.Send(phrase);
                    }
                }
            else if (options == ActOptions.ToChar)
            {
                string phrase = Format(rawPhrase, character, character, param1, param2);
                character.Send(phrase);
            }
            else if (options == ActOptions.ToVict)
            {
                string phrase = Format(rawPhrase, (ICharacter)param1, character, param1, param2);
                ((ICharacter)param1).Send(phrase);
            }
        }

        // TODO: use regex or manual parsing
        // $n and $N must check if target can see character (someone if not)
        // $e, $E, $m, $M, $s and $S must use sex mapping table
        private string Format(string phrase, ICharacter target, ICharacter character, object param1, object param2)
        {
            phrase = phrase.Replace("$t", (string) param1);
            phrase = phrase.Replace("$T", (string)param2);
            phrase = phrase.Replace("$n", character.Name);
            phrase = phrase.Replace("$N", ((ICharacter)param1).Name);
            phrase = phrase.Replace("$e", "he");
            phrase = phrase.Replace("$E", "he");
            phrase = phrase.Replace("$m", "him");
            phrase = phrase.Replace("$M", "him");
            phrase = phrase.Replace("$s", "his");
            phrase = phrase.Replace("$S", "his");
            phrase = phrase.Replace("$p", ((IObject) param1).Name); //TODO: short description
            phrase = phrase.Replace("$P", ((IObject)param2).Name); //TODO: short description
            phrase = phrase.Replace("$d", param2 == null ? "door" : ((IExit)param2).Name);

            return phrase;
        }
    }
}
