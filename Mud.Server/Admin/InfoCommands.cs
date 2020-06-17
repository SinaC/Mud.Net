using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.DataStructures.HeapPriorityQueue;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;

// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("abilities", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoAbilities(string rawParameters, params ICommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(null, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("spells", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoSpells(string rawParameters, params ICommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(AbilityTypes.Spell, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("skills", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoSkills(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(AbilityTypes.Spell, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        //*********************** Helpers ***************************

        private bool DisplayAbilitiesList(AbilityTypes? filterOnAbilityType, params ICommandParameter[] parameters)
        {
            string title;
            if (filterOnAbilityType.HasValue)
            {
                switch (filterOnAbilityType.Value)
                {
                    case AbilityTypes.Passive: title = "Passives"; break;
                    case AbilityTypes.Spell: title = "Spells"; break;
                    case AbilityTypes.Skill: title = "Skills"; break;
                    default: title = "???"; break;
                }
            }
            else
                title = "Abilities";
            if (parameters.Length == 0)
            {
                // no filter
                StringBuilder sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate(title, AbilityManager.Abilities
                    .OrderBy(x => x.Name));
                Page(sb);
                return true;
            }

            // filter on class?
            IClass matchingClass = ClassManager.Classes.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingClass != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate($"{title} for {matchingClass.DisplayName}", matchingClass.Abilities
                    .OrderBy(x => x.Level)
                    .ThenBy(x => x.Name));
                Page(sb);
                return true;
            }

            // filter on race?
            IPlayableRace matchingRace = RaceManager.PlayableRaces.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingRace != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate($"{title} for {matchingRace.DisplayName}", matchingRace.Abilities
                    .OrderBy(x => x.Level)
                    .ThenBy(x => x.Name));
                Page(sb);
                return true;
            }
            return false;
        }

        private static string DisplayEntityAndContainer(IEntity entity)
        {
            if (entity == null)
                return "???";
            StringBuilder sb = new StringBuilder();
            sb.Append(entity.DebugName);
            // don't to anything if entity is IRoom
            if (entity is IItem item)
            {
                if (item.ContainedInto != null)
                {
                    sb.Append(" in ");
                    sb.Append("<");
                    sb.Append(DisplayEntityAndContainer(item.ContainedInto));
                    sb.Append(">");
                }
                else if (item.EquippedBy != null)
                {
                    sb.Append(" equipped by ");
                    sb.Append("<");
                    sb.Append(DisplayEntityAndContainer(item.EquippedBy));
                    sb.Append(">");
                }
                else
                    sb.Append("Seems to be nowhere!!!");
            }

            if (entity is ICharacter character)
            {
                sb.Append("<");
                sb.Append(DisplayEntityAndContainer(character.Room));
                sb.Append(">");
            }
            return sb.ToString();
        }

        private void AppendAuras(StringBuilder sb, IEnumerable<IAura> auras)
        {
            foreach (IAura aura in auras)
                aura.Append(sb);
        }
    }
}
