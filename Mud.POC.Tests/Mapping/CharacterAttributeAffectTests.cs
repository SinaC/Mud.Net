using Mud.Domain;
using Mud.Server.Affects.Character;
using Riok.Mapperly.Abstractions;

namespace Mud.POC.Tests.Mapping
{
    [TestClass]
    public class CharacterAttributeAffectTests
    {
        [TestMethod]
        public void CharacterAttributeAffect_CharacterAttributeAffectData()
        {
            var characterAttributeAffect = new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.ArmorPierce, Modifier = 15, Operator = AffectOperators.Assign };

            var mapper = new CharacterAttributeAffectMapper();
            var characterAttributeAffectData = mapper.CharacterAttributeAffectToCharacterAttributeAffectData(characterAttributeAffect);

            Assert.IsNotNull(characterAttributeAffectData);
            Assert.AreEqual(CharacterAttributeAffectLocations.ArmorPierce, characterAttributeAffectData.Location);
            Assert.AreEqual(15, characterAttributeAffectData.Modifier);
            Assert.AreEqual(AffectOperators.Assign, characterAttributeAffectData.Operator);
        }

        [TestMethod]
        public void CharacterAttributeAffectData_CharacterAttributeAffect()
        {
            var characterAttributeAffectData = new CharacterAttributeAffectData { Location = CharacterAttributeAffectLocations.ArmorPierce, Modifier = 15, Operator = AffectOperators.Assign };

            var mapper = new CharacterAttributeAffectMapper();
            var characterAttributeAffect = mapper.CharacterAttributeAffectDataToCharacterAttributeAffect(characterAttributeAffectData);

            Assert.IsNotNull(characterAttributeAffect);
            Assert.AreEqual(CharacterAttributeAffectLocations.ArmorPierce, characterAttributeAffect.Location);
            Assert.AreEqual(15, characterAttributeAffect.Modifier);
            Assert.AreEqual(AffectOperators.Assign, characterAttributeAffect.Operator);
        }
    }

    [Mapper]
    public partial class CharacterAttributeAffectMapper
    {
        public partial CharacterAttributeAffectData CharacterAttributeAffectToCharacterAttributeAffectData(CharacterAttributeAffect characterAttributeAffect);
        public partial CharacterAttributeAffect CharacterAttributeAffectDataToCharacterAttributeAffect( CharacterAttributeAffectData characterAttributeAffectData);
    }
}
