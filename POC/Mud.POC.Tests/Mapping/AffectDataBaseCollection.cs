using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Affect;
using Riok.Mapperly.Abstractions;

namespace Mud.POC.Tests.Mapping;

[TestClass]
public class AffectDataBaseCollection
{
    [TestMethod]
    public void AffectToAffectData()
    {
        var affects = new List<IAffect>
        {
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Calm,Invisible"), Operator = AffectOperators.Add },
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.ArmorPierce, Modifier = 15, Operator = AffectOperators.Assign }
        };

        var mapper = new AffectMapper();
        var affectDatas = affects.Select(mapper.MapToAffectData).ToList();

        Assert.IsNotNull(affectDatas);
        var characterFlagsAffectData = (CharacterFlagsAffectData)affectDatas[0];
        Assert.AreEqual("Calm,Invisible", characterFlagsAffectData.Modifier);
        Assert.AreEqual(AffectOperators.Add, characterFlagsAffectData.Operator);
        var characterAttributeAffectData = (CharacterAttributeAffectData)affectDatas[1];
        Assert.AreEqual(CharacterAttributeAffectLocations.ArmorPierce, characterAttributeAffectData.Location);
        Assert.AreEqual(15, characterAttributeAffectData.Modifier);
        Assert.AreEqual(AffectOperators.Assign, characterAttributeAffectData.Operator);
    }

    [TestMethod]
    public void AffectDataToAffect()
    {
        var affectDatas = new List<AffectDataBase>
        {
            new CharacterFlagsAffectData { Modifier = "Calm,Invisible", Operator = AffectOperators.Add },
            new CharacterAttributeAffectData { Location = CharacterAttributeAffectLocations.ArmorPierce, Modifier = 15, Operator = AffectOperators.Assign }
        };

        var mapper = new AffectMapper();
        var affects = affectDatas.Select(mapper.MapToAffect).ToList();

        var characterFlagsAffect = (CharacterFlagsAffect)affects[0];
        Assert.IsNotNull(characterFlagsAffect);
        CollectionAssert.AreEquivalent(new[] { "Calm", "Invisible" }, characterFlagsAffect.Modifier.Values.ToArray());
        Assert.AreEqual(AffectOperators.Add, characterFlagsAffect.Operator);
        var characterAttributeAffect = (CharacterAttributeAffect)affects[1];
        Assert.AreEqual(CharacterAttributeAffectLocations.ArmorPierce, characterAttributeAffect.Location);
        Assert.AreEqual(15, characterAttributeAffect.Modifier);
        Assert.AreEqual(AffectOperators.Assign, characterAttributeAffect.Operator);
    }
}

[Mapper]
public partial class AffectMapper
{
    [MapDerivedType<CharacterAttributeAffect, CharacterAttributeAffectData>]
    [MapDerivedType<CharacterFlagsAffect, CharacterFlagsAffectData>]
    public partial AffectDataBase MapToAffectData(IAffect affect);

    [MapDerivedType<CharacterAttributeAffectData, CharacterAttributeAffect>]
    [MapDerivedType<CharacterFlagsAffectData, CharacterFlagsAffect>]
    public partial IAffect MapToAffect(AffectDataBase affect);
}

public partial class AffectMapper
{
    // character attribute
    public partial CharacterAttributeAffectData CharacterAttributeAffectToCharacterAttributeAffectData(CharacterAttributeAffect characterAttributeAffect);
    public partial CharacterAttributeAffect CharacterAttributeAffectDataToCharacterAttributeAffect(CharacterAttributeAffectData characterAttributeAffectData);
}

public partial class AffectMapper
{
    // character flags
    [MapProperty(nameof(CharacterFlagsAffect.Modifier), nameof(CharacterFlagsAffectData.Modifier), Use = nameof(MapModifierToString))]
    public partial CharacterFlagsAffectData CharacterFlagsAffectToCharacterFlagsAffectData(CharacterFlagsAffect CharacterFlagsAffect);
    private string MapModifierToString(ICharacterFlags modifier) => GenericMapModifierToString(modifier);

    [MapProperty(nameof(CharacterFlagsAffectData.Modifier), nameof(CharacterFlagsAffect.Modifier), Use = nameof(MapModifierFromString))]
    public partial CharacterFlagsAffect CharacterFlagsAffectDataToCharacterFlagsAffect(CharacterFlagsAffectData CharacterFlagsAffectData);
    private ICharacterFlags MapModifierFromString(string modifier) => new CharacterFlags(modifier);

    private string GenericMapModifierToString(IFlags<string> modifier) => string.Join(',', modifier.Values);
}

//[Mapper]
//public partial class AffectMapper
//{
//    [MapDerivedType<CharacterAttributeAffect, CharacterAttributeAffectData>] // for c# language level ≥ 11
//    [MapDerivedType<CharacterFlagsAffect, CharacterFlagsAffectData>] // for c# language level ≥ 11
//    public partial AffectDataBase MapAffect(IAffect affect);

//    // character attribute
//    public partial CharacterAttributeAffectData CharacterAttributeAffectToCharacterAttributeAffectData(CharacterAttributeAffect characterAttributeAffect);
//    public partial CharacterAttributeAffect CharacterAttributeAffectDataToCharacterAttributeAffect(CharacterAttributeAffectData characterAttributeAffectData);

//    // character flags
//    [MapProperty(nameof(CharacterFlagsAffect.Modifier), nameof(CharacterFlagsAffectData.Modifier), Use = nameof(MapModifierToString))]
//    public partial CharacterFlagsAffectData CharacterFlagsAffectToCharacterFlagsAffectData(CharacterFlagsAffect CharacterFlagsAffect);
//    private string MapModifierToString(ICharacterFlags modifier) => GenericMapModifierToString(modifier);

//    [MapProperty(nameof(CharacterFlagsAffectData.Modifier), nameof(CharacterFlagsAffect.Modifier), Use = nameof(MapModifierFromString))]
//    public partial CharacterFlagsAffect CharacterFlagsAffectDataToCharacterFlagsAffect(CharacterFlagsAffectData CharacterFlagsAffectData);
//    private ICharacterFlags MapModifierFromString(string modifier) => new CharacterFlags(modifier);

//    private string GenericMapModifierToString(IFlags<string> modifier) => string.Join(',', modifier.Values);
//}