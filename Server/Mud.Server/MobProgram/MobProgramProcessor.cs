using Microsoft.Extensions.Logging;
using Mud.Blueprints.MobProgram;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.MobProgram;
using Mud.Server.Parser.Interfaces;
using System.Text;

namespace Mud.Server.MobProgram;

[Export(typeof(IMobProgramProcessor))]
public class MobProgramProcessor : IMobProgramProcessor
{
    private ILogger<MobProgramProcessor> Logger { get; }
    private IParser Parser { get; }
    private IRandomManager RandomManager { get; }

    public MobProgramProcessor(ILogger<MobProgramProcessor> logger, IParser parser, IRandomManager randomManager)
    {
        Logger = logger;
        Parser = parser;
        RandomManager = randomManager;
    }

    public void Execute(INonPlayableCharacter npc, MobProgramBase mobProgram, params object?[] parameters)
    {
        Logger.LogDebug("MOBPROGRAM: {debugName}", npc.DebugName);
        // perform instructions
        foreach (var rawInstruction in mobProgram.Instructions)
        {
            Logger.LogDebug("MOBPROGRAM: {debugName} execute instruction {instruction}", npc.DebugName, rawInstruction);
            if (rawInstruction.StartsWith('*')) // comments
                continue;
            var instruction = SubstituteArguments(npc, rawInstruction, parameters);
            Logger.LogDebug("MOBPROGRAM: {debugName} after substitute argments instruction {instruction}", npc.DebugName, instruction);
            //// TODO: handle other keywords
            //if (StringCompareHelpers.StringStartsWith(instruction, "MOB"))
            //{
            //    var mobInstruction = "mp" + instruction[3..].Trim(); // prefix command with 'mp' to avoid issue with command existing for character and mob program such as kill
            //    var mobInstructionParseResult = Parser.Parse(mobInstruction);
            //    if (mobInstructionParseResult is null)
            //        Logger.LogError("MOBPROGRAM: {debugName} cannot parse mob instruction {mobInstruction}", npc.DebugName, mobInstruction);
            //    else
            //    {
            //        var mobInstructionExecuted = npc.ExecuteCommand(mobInstruction, mobInstructionParseResult);
            //        if (!mobInstructionExecuted)
            //            Logger.LogError("MOBPROGRAM: {debugName} cannot execute mob instruction: CMD={command} RAWPARAM={rawParameters}", npc.DebugName, mobInstructionParseResult.Command, mobInstructionParseResult.RawParameters);
            //    }
            //}
            //else
            //{
                var processed = npc.ProcessInput(instruction);
                if (!processed)
                    Logger.LogError("MOBPROGRAM: {debugName} execution failed for instruction {instruction}", npc.DebugName, instruction);
            //}
        }
    }

    // (mob, ch, arg1, arg2, rch)
    // obj1 = arg1
    // vch = arg2
    // obj2 = arg2
    // mob.target set with MPREMEMBER and unset with MPFORGET

    // Name/ShortDescr
    //  $i: mob.name
    //  $I: mob.shortDescr
    //  $n: ch.name.capitalize or someone (if mob cannot see ch or null)
    //  $N: ch.shortDesc (npc) or ch.name (pc) or someone (if mob cannot see ch or null)
    //  $t: vch.name.capitalize or someone (if mob cannot see vch or null)
    //  $T: vch.shortDesc (npc) or vch.name (pc) or someone (if mob cannot see vch or null)
    //  $r: rch ??= random char in mob.room, then rch.name.capitalize or someone (if mob cannot see rch or null)
    //  $R: rch ??= random char in mob.room, then rch.shortDesc(npc) or rch.name(pc) or someone(if mob cannot see rch or null)
    //  $q: mob.target.name.capitalize or someone (if mob cannot see mob.target or null)
    //  $Q: mob.target.shortDesc (npc) or mob.target.name (pc) or someone (if mob cannot see mob.target or null)

    // Subjects
    //  $j: he_she_it[mob]
    //  $e: he_she_it[ch] or someone (if mob cannot see ch or null)
    //  $E: he_she_it[vch] or someone (if mob cannot see vch or null)
    //  $J: rch ??= random char in mob.room, then he_she_it[rch] or someone (if mob cannot see rch or null)
    //  $X: he_she_it[mob.target] or someone (if mob cannot see mob.target or null)

    // Objectives
    //  $k: him_her_it[mob]
    //  $m: him_her_it[ch] or someone (if mob cannot see ch or null)
    //  $M: him_her_it[vch] or someone (if mob cannot see vch or null)
    //  $K: rch ??= random char in mob.room, then him_her_it[rch] or someone (if mob cannot see rch or null)
    //  $Y: him_her_it[mob.target] or someone (if mob cannot see mob.target or null)

    // Possessives
    //  $l: his_her_its[mob]
    //  $s: his_her_its[ch] or someone (if mob cannot see ch or null)
    //  $S: his_her_its[vch] or someone (if mob cannot see vch or null)
    //  $L: rch ??= random char in mob.room, then his_her_its[rch] or someone (if mob cannot see rch or null)
    //  $Z: his_her_its[mob.target] or someone (if mob cannot see mob.target or null)

    // Item Name/ShortDescr
    //  $o: obj1.name or something (if mob cannot see obj1 or null)
    //  $O: obj1.shortDescr or something (if mob cannot see obj1 or null)
    //  $p: obj2.name or something (if mob cannot see obj2 or null)
    //  $P: obj2.shortDescr or something (if mob cannot see obj2 or null)

    private string SubstituteArguments(INonPlayableCharacter npc, string instruction, params object?[] parameters)
    {
        var sb = new StringBuilder(instruction);

        // GetRandomPlayableCharacterInRoom is unique by program
        var randomPC = GetRandomPlayableCharacterInRoom(npc);

        // name/display name
        sb.Replace("$i", npc.Keywords.First());
        sb.Replace("$I", npc.DisplayName);
        sb.Replace("$n", GetName(npc, GetParameter<ICharacter>(0, parameters)));
        sb.Replace("$N", GetDisplayName(npc, GetParameter<ICharacter>(0, parameters)));
        sb.Replace("$t", GetName(npc, GetParameter<ICharacter>(1, parameters)));
        sb.Replace("$T", GetDisplayName(npc, GetParameter<ICharacter>(1, parameters)));
        sb.Replace("$r", GetName(npc, randomPC));
        sb.Replace("$R", GetDisplayName(npc, randomPC));
        sb.Replace("$q", GetName(npc, npc.MobProgramTarget));
        sb.Replace("$Q", GetDisplayName(npc, npc.MobProgramTarget));

        // subjects
        sb.Replace("$j", StringHelpers.Subjects[npc.Sex]);
        sb.Replace("$e", GetValueFromSex(npc, GetParameter<ICharacter>(0, parameters), StringHelpers.Subjects));
        sb.Replace("$E", GetValueFromSex(npc, GetParameter<ICharacter>(1, parameters), StringHelpers.Subjects));
        sb.Replace("$J", GetValueFromSex(npc, randomPC, StringHelpers.Subjects));
        sb.Replace("$X", GetValueFromSex(npc, npc.MobProgramTarget, StringHelpers.Subjects));

        // objectives
        sb.Replace("$k", StringHelpers.Objectives[npc.Sex]);
        sb.Replace("$m", GetValueFromSex(npc, GetParameter<ICharacter>(0, parameters), StringHelpers.Objectives));
        sb.Replace("$M", GetValueFromSex(npc, GetParameter<ICharacter>(1, parameters), StringHelpers.Objectives));
        sb.Replace("$K", GetValueFromSex(npc, randomPC, StringHelpers.Objectives));
        sb.Replace("$Y", GetValueFromSex(npc, npc.MobProgramTarget, StringHelpers.Objectives));

        // possessives
        sb.Replace("$l", StringHelpers.Possessives[npc.Sex]);
        sb.Replace("$s", GetValueFromSex(npc, GetParameter<ICharacter>(0, parameters), StringHelpers.Possessives));
        sb.Replace("$S", GetValueFromSex(npc, GetParameter<ICharacter>(1, parameters), StringHelpers.Possessives));
        sb.Replace("$L", GetValueFromSex(npc, randomPC, StringHelpers.Possessives));
        sb.Replace("$Z", GetValueFromSex(npc, npc.MobProgramTarget, StringHelpers.Possessives));

        // item name/display name
        sb.Replace("$o", GetName(npc, GetParameter<IItem>(0, parameters)));
        sb.Replace("$O", GetDisplayName(npc, GetParameter<IItem>(0, parameters)));
        sb.Replace("$p", GetName(npc, GetParameter<IItem>(1, parameters)));
        sb.Replace("$P", GetDisplayName(npc, GetParameter<IItem>(1, parameters)));

        return sb.ToString();
    }
    private ICharacter? GetRandomPlayableCharacterInRoom(INonPlayableCharacter npc)
        => RandomManager.Random(npc.Room.PlayableCharacters.Where(x => x != this));

    private static string GetName(INonPlayableCharacter npc, ICharacter? character)
    {
        if (character == null || !npc.CanSee(character))
            return "someone";
        return character.Keywords.First().UpperFirstLetter();
    }

    private static string GetDisplayName(INonPlayableCharacter npc, ICharacter? character)
    {
        if (character == null || !npc.CanSee(character))
            return "someone";
        return character.DisplayName;
    }

    private static string GetName(INonPlayableCharacter npc, IItem? item)
    {
        if (item == null || !npc.CanSee(item))
            return "something";
        return item.Keywords.First().UpperFirstLetter();
    }

    private static string GetDisplayName(INonPlayableCharacter npc, IItem? item)
    {
        if (item == null || !npc.CanSee(item))
            return "something";
        return item.DisplayName;
    }

    private static string GetValueFromSex(INonPlayableCharacter npc, ICharacter? character, IDictionary<Sex, string> valueBySex)
    {
        if (character == null || !npc.CanSee(character))
            return "someone";
        return valueBySex[character.Sex];
    }

    private static TEntity? GetParameter<TEntity>(int index, params object?[] parameters)
        where TEntity : IEntity
    {
        if (index >= parameters.Length || parameters[index] is not TEntity entity)
            return default!;
        return entity;
    }
}
