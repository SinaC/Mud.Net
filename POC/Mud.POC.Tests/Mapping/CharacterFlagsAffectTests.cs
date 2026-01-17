using Mud.DataStructures.Flags;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Riok.Mapperly.Abstractions;

namespace Mud.POC.Tests.Mapping;

[TestClass]
public class CharacterFlagsAffectTests
{
    [TestMethod]
    public void CharacterFlagsAffect_CharacterFlagsAffectData()
    {
        var characterFlagsAffect = new CharacterFlagsAffect { Modifier = new CharacterFlags("Calm,Invisible"), Operator = AffectOperators.Add };

        var mapper = new CharacterFlagsAffectMapper();
        var characterFlagsAffectData = mapper.CharacterFlagsAffectToCharacterFlagsAffectData(characterFlagsAffect);

        Assert.IsNotNull(characterFlagsAffectData);
        Assert.AreEqual("Calm,Invisible", characterFlagsAffectData.Modifier);
        Assert.AreEqual(AffectOperators.Add, characterFlagsAffectData.Operator);
    }

    [TestMethod]
    public void CharacterFlagsAffectData_CharacterFlagsAffect()
    {
        var characterFlagsAffectData = new CharacterFlagsAffectData { Modifier = "Calm,Invisible", Operator = AffectOperators.Add };

        var mapper = new CharacterFlagsAffectMapper();
        var characterFlagsAffect = mapper.CharacterFlagsAffectDataToCharacterFlagsAffect(characterFlagsAffectData);

        Assert.IsNotNull(characterFlagsAffect);
        CollectionAssert.AreEquivalent(new[] { "Calm", "Invisible" }, characterFlagsAffect.Modifier.Values.ToArray());
        Assert.AreEqual(AffectOperators.Add, characterFlagsAffect.Operator);
    }
}


[Mapper]
public partial class CharacterFlagsAffectMapper
{
    [MapProperty(nameof(CharacterFlagsAffect.Modifier), nameof(CharacterFlagsAffectData.Modifier), Use = nameof(MapModifierToString))]
    public partial CharacterFlagsAffectData CharacterFlagsAffectToCharacterFlagsAffectData(CharacterFlagsAffect CharacterFlagsAffect);
    private string MapModifierToString(ICharacterFlags modifier) => GenericMapModifierToString(modifier);

    [MapProperty(nameof(CharacterFlagsAffectData.Modifier), nameof(CharacterFlagsAffect.Modifier), Use = nameof(MapModifierFromString))]
    public partial CharacterFlagsAffect CharacterFlagsAffectDataToCharacterFlagsAffect(CharacterFlagsAffectData CharacterFlagsAffectData);
    private ICharacterFlags MapModifierFromString(string modifier) => new CharacterFlags(modifier);

    private string GenericMapModifierToString(IFlags<string> modifier) => string.Join(',', modifier.Values);
}