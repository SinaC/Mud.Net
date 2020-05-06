using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        // TODO: Practice/Gain
        [Command("gain", "Ability")]
        [Syntax(
            "[cmd] list",
            "[cmd] skills|spells|passives",
            "[cmd] convert|revert",
            "[cmd] <ability>")]
        protected virtual CommandExecutionResults DoGain(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            if (StringCompareHelpers.StringStartsWith("list", parameters[0].Value))
            {
                StringBuilder sb = NotLearnedAbilityTableGenerator.Value.Generate("Abilities",  3, KnownAbilities.Where(x => x.Level <= Level && x.Learned == 0).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            if (StringCompareHelpers.StringStartsWith("skills", parameters[0].Value))
            {
                StringBuilder sb = NotLearnedAbilityTableGenerator.Value.Generate("Abilities", 3, KnownAbilities.Where(x => x.Ability.Kind == AbilityKinds.Skill && x.Level <= Level && x.Learned == 0).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            if (StringCompareHelpers.StringStartsWith("spells", parameters[0].Value))
            {
                StringBuilder sb = NotLearnedAbilityTableGenerator.Value.Generate("Abilities", 3, KnownAbilities.Where(x => x.Ability.Kind == AbilityKinds.Spell && x.Level <= Level && x.Learned == 0).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            if (StringCompareHelpers.StringStartsWith("skipassiveslls", parameters[0].Value))
            {
                StringBuilder sb = NotLearnedAbilityTableGenerator.Value.Generate("Abilities", 3, KnownAbilities.Where(x => x.Ability.Kind == AbilityKinds.Passive && x.Level <= Level && x.Learned == 0).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // TODO: convert/revert
            KnownAbility knownAbility = KnownAbilities.FirstOrDefault(x => x.Level <= Level && x.Learned == 0 && StringCompareHelpers.StringStartsWith(x.Ability.Name, parameters[0].Value));
            if (knownAbility)

            return CommandExecutionResults.Ok;
        }

        //TODO: move to another file
        //[Command("train")]
        //[Syntax(
        //    "[cmd]",
        //    "[cmd] <attribute>",
        //    "[cmd] hp|mana|move")]
        //protected virtual CommandExecutionResults DoTrain(string rawParameters, params CommandParameter[] parameters)
        //{
        //    Send(StringHelpers.NotYetImplemented);

        //    return CommandExecutionResults.Ok;
        //}

        private static readonly Lazy<TableGenerator<KnownAbility>> NotLearnedAbilityTableGenerator = new Lazy<TableGenerator<KnownAbility>>(() =>
        {
            TableGenerator<KnownAbility> generator = new TableGenerator<KnownAbility>();
            generator.AddColumn("Name", 18, x => x.Ability.Name);
            generator.AddColumn("Lvl", 5, x => x.Level.ToString());
            generator.AddColumn("Cost", 5, x => x.DifficulityMultiplier.ToString());
            return generator;
        });
    }
}
