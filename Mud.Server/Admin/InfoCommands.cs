using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.DataStructures.HeapPriorityQueue;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
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

        [AdminCommand("path", "Information", MustBeImpersonated = true)]
        [Syntax("[cmd] <location>")]
        protected virtual CommandExecutionResults DoPath(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            IRoom destination = FindHelpers.FindLocation(Impersonating, parameters[0]);
            if (destination == null)
            {
                Send("No such location.");
                return CommandExecutionResults.TargetNotFound;
            }

            string path = BuildPath(Impersonating.Room, destination);
            Send("Following path will lead to {0}:" + Environment.NewLine + "%c%" + path + "%x%", destination.DisplayName);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("info", "Information")]
        [Syntax(
            "[cmd] race",
            "[cmd] class",
            "[cmd] <race>",
            "[cmd] <class>")]
        protected virtual CommandExecutionResults DoInfo(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            // class
            if (parameters[0].Value == "class")
            {
                // Name, ShortName, DisplayName, ResourceKinds
                StringBuilder sb = TableGenerators.ClassTableGenerator.Value.Generate("Classes", ClassManager.Classes);
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // race
            if (parameters[0].Value == "race")
            {
                // Name, ShortName, DisplayName
                StringBuilder sb = TableGenerators.PlayableRaceTableGenerator.Value.Generate("Races", RaceManager.PlayableRaces);
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // class name
            IClass matchingClass = ClassManager.Classes.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingClass != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate(
                    new[]
                    {
                        matchingClass.DisplayName,
                        $"ShortName: {matchingClass.ShortName}",
                        $"Resource(s): {string.Join(",", matchingClass.ResourceKinds?.Select(x => $"{x.ResourceColor()}") ?? Enumerable.Empty<string>())}",
                        $"Prime attribute: %W%{matchingClass.PrimeAttribute}%x%",
                        $"Max practice percentage: %W%{matchingClass.MaxPracticePercentage}%x%",
                        $"Hp/level: min: %W%{matchingClass.MinHitPointGainPerLevel}%x% max: %W%{matchingClass.MaxHitPointGainPerLevel}%x%"
                    },
                    matchingClass.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Name));
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // race name
            IPlayableRace matchingRace = RaceManager.PlayableRaces.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingRace != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate(
                    new[]
                    {
                        matchingRace.DisplayName,
                        $"ShortName: {matchingRace.ShortName}",
                        $"Immunities: %y%{matchingRace.Immunities}%x%",
                        $"Resistances: %b%{matchingRace.Resistances}%x%",
                        $"Vulnerabilities: %r%{matchingRace.Vulnerabilities}%x%",
                        $"Size: %M%{matchingRace.Size}%x%",
                        $"Exp/Level:     %W%{string.Join(" ", ClassManager.Classes.Select(x => $"{x.ShortName,5}"))}%x%",
                        $"               %r%{string.Join(" ", ClassManager.Classes.Select(x => $"{matchingRace.ClassExperiencePercentageMultiplier(x)*10,5}"))}%x%", // *10 because base experience is 1000
                        $"Attributes:       %Y%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{x.ShortName(),3}"))}%x%",
                        $"Attributes start: %c%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{matchingRace.GetStartAttribute((CharacterAttributes)x),3}"))}%x%",
                        $"Attributes max:   %B%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{matchingRace.GetMaxAttribute((CharacterAttributes)x),3}"))}%x%",
                    },
                    matchingRace.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Name));
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            return CommandExecutionResults.SyntaxError;
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

        //

        private string BuildPath(IRoom origin, IRoom destination)
        {
            if (origin == destination)
                return destination.DisplayName + " is here.";

            Dictionary<IRoom, int> distance = new Dictionary<IRoom, int>(500);
            Dictionary<IRoom, Tuple<IRoom, ExitDirections>> previousRoom = new Dictionary<IRoom, Tuple<IRoom, ExitDirections>>(500);
            HeapPriorityQueue<IRoom> pQueue = new HeapPriorityQueue<IRoom>(500);

            // Search path
            distance[origin] = 0;
            pQueue.Enqueue(origin, 0);

            // Dijkstra
            while (!pQueue.IsEmpty())
            {
                IRoom nearest = pQueue.Dequeue();
                if (nearest == destination)
                    break;
                foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
                {
                    IRoom neighbour = nearest.GetRoom(direction);
                    if (neighbour != null && !distance.ContainsKey(neighbour))
                    {
                        int neighbourDist = distance[nearest] + 1;
                        int bestNeighbourDist;
                        if (!distance.TryGetValue(neighbour, out bestNeighbourDist))
                            bestNeighbourDist = int.MaxValue;
                        if (neighbourDist < bestNeighbourDist)
                        {
                            distance[neighbour] = neighbourDist;
                            pQueue.Enqueue(neighbour, neighbourDist);
                            previousRoom[neighbour] = new Tuple<IRoom, ExitDirections>(nearest, direction);
                        }
                    }
                }
            }

            // Build path
            Tuple<IRoom, ExitDirections> previous;
            if (previousRoom.TryGetValue(destination, out previous))
            {
                StringBuilder sb = new StringBuilder(500);
                while (true)
                {
                    sb.Insert(0, previous.Item2.ShortExitDirections());
                    if (previous.Item1 == origin)
                        break;
                    if (!previousRoom.TryGetValue(previous.Item1, out previous))
                    {
                        sb.Insert(0, "???");
                        break;
                    }
                }
                // compress path:  ssswwwwnn -> 3s4w2n
                return Compress(sb.ToString());
            }
            return "No path found.";
        }

        private static string Compress(string str) //http://codereview.stackexchange.com/questions/64929/string-compression-implementation-in-c
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 1, cnt = 1; i <= str.Length; i++, cnt++)
            {
                if (i == str.Length || str[i] != str[i - 1])
                {
                    if (cnt == 1)
                        builder.Append(str[i - 1]);
                    else
                        builder.Append(cnt).Append(str[i - 1]);
                    cnt = 0;
                }
            }
            return builder.ToString();
        }
    }
}
