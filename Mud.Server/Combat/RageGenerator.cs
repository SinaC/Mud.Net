using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;

namespace Mud.Server.Combat;

[Export(typeof(IRageGenerator)), Shared]
public class RageGenerator : IRageGenerator
{
    //https://vanilla-wow-archive.fandom.com/wiki/Rage#Rage_decay
    // rage generation
    //rage generated = 15d/4c + fs/2 <= 15d/c (damage done)
    //rage generated = 5d/2c (damage received)
    //d (damage output)
    //s (weapon speed)
    //f (hit factor)
    //Main Hand	Normal Hit	3.5
    //Main Hand	Critical Hit	7.0
    //Off Hand	Normal Hit	1.75
    //Off Hand	Critical Hit	3.5

    //c (Rage conversion value)
    //level   value
    //10      37.4
    //20      72.4
    //30      109.3
    //40      147.9
    //50      188.3
    //60      230.6
    //65      252.4
    //70      274.7
    //80      453.3
    public void GenerateRageFromIncomingDamage(ICharacter character, int damage, DamageSources damageSource)
    {
        if (!character.CurrentResourceKinds.Contains(ResourceKinds.Rage))
            return;
        var rageGenerated = 5 * damage / (2 * RageConversionValue(character));
        character.UpdateResource(ResourceKinds.Rage, rageGenerated);
    }

    public void GenerateRageFromOutgoingDamage(ICharacter character, int damage, DamageSources damageSource)
    {
        if (!character.CurrentResourceKinds.Contains(ResourceKinds.Rage) || damageSource != DamageSources.Hit)
            return;
        var c = RageConversionValue(character);
        var f = 2.2m; // TODO
        var s = 2; // TODO
        var rageGenerated = Math.Min(15 * damage / (4 * c) + f * s / 2, 15 * damage / c);
        character.UpdateResource(ResourceKinds.Rage, rageGenerated);
    }

    private static readonly (int level, decimal value)[] RageConversionValueTable =
    [
        (0,      1m),
        (10,     15.4m ),
        (20,     35.4m ),
        (30,     50.3m),
        (40,     70.9m),
        (50,     90.3m),
        (60,     115.6m),
    ];  

    private decimal RageConversionValue(ICharacter character)
    {
        // lerp
        var below = RageConversionValueTable.LastOrDefault(x => character.Level >= x.level);
        var above = RageConversionValueTable.FirstOrDefault(x => character.Level <= x.level);
        if (below == default || above.level == character.Level)
            return above.value;
        if (above == default || below.level == character.Level)
            return below.value;
        var result = character.Level * (above.value - below.value) / (above.level - below.level);
        return result;
    }
}
