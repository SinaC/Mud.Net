using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Flags;

namespace Mud.Server.Tests.Affects
{
    // TODO: CharacterFlagsAffect, CharacterIRVAffect, CharacterSexAffect
    [TestClass]
    public class CharacterAffectTests : TestBase
    {
        [TestMethod]
        public void Character_OneCharacerAddAffect_DifferentBaseValue_Test()
        {
            var room = GenerateRoom("");
            var npc = GenerateNPC("Charm", room);
            var characterAura = new Aura.Aura(null!, null!, AuraFlags.Permanent, 10, TimeSpan.FromMinutes(10), new CharacterFlagsAffect { Operator = AffectOperators.Add, Modifier = new CharacterFlags("Haste") });
            npc.AddAura(characterAura, false);
            npc.Recompute();

            Assert.IsTrue(npc.BaseCharacterFlags.IsSet("Charm"));
            Assert.IsTrue(npc.CharacterFlags.HasAll("Charm", "Haste"));
        }

        [TestMethod]
        public void OneNormalAttribute_Add_Test()
        {
            var room = GenerateRoom("");
            var npc = GenerateNPC("", room);
            var strAffect = new CharacterAttributeAffect
            {
                Location = CharacterAttributeAffectLocations.Strength,
                Operator = AffectOperators.Add,
                Modifier = 6
            };

            int originalBaseStr = npc.BaseAttribute(CharacterAttributes.Strength);
            int originalCurrentStr = npc[CharacterAttributes.Strength];
            npc.ApplyAffect(strAffect);

            Assert.AreEqual(originalBaseStr, npc.BaseAttribute(CharacterAttributes.Strength));
            Assert.AreEqual(originalCurrentStr + 6, npc[CharacterAttributes.Strength]);
        }

        [TestMethod]
        public void OneNormalAttribute_Assign_Test()
        {
            var room = GenerateRoom("");
            var npc = GenerateNPC("", room);
            var intAffect = new CharacterAttributeAffect
            {
                Location = CharacterAttributeAffectLocations.Intelligence,
                Operator = AffectOperators.Assign,
                Modifier = 6
            };

            int originalBaseInt = npc.BaseAttribute(CharacterAttributes.Intelligence);
            npc.ApplyAffect(intAffect);

            Assert.AreEqual(originalBaseInt, npc.BaseAttribute(CharacterAttributes.Intelligence));
            Assert.AreEqual(6, npc[CharacterAttributes.Intelligence]);
        }

        [TestMethod]
        public void OneCombinedAttribute_Add_Test()
        {
            var room = GenerateRoom("");
            var npc = GenerateNPC("", room);
            var caracAffect = new CharacterAttributeAffect
            {
                Location = CharacterAttributeAffectLocations.Characteristics,
                Operator = AffectOperators.Add,
                Modifier = 3
            };

            int originalBaseStr = npc.BaseAttribute(CharacterAttributes.Strength);
            int originalCurrentStr = npc[CharacterAttributes.Strength];
            int originalBaseInt = npc.BaseAttribute(CharacterAttributes.Intelligence);
            int originalCurrentInt = npc[CharacterAttributes.Intelligence];
            int originalBaseWis = npc.BaseAttribute(CharacterAttributes.Wisdom);
            int originalCurrentWis = npc[CharacterAttributes.Wisdom];
            int originalBaseDex = npc.BaseAttribute(CharacterAttributes.Dexterity);
            int originalCurrentDex = npc[CharacterAttributes.Dexterity];
            int originalBaseCon = npc.BaseAttribute(CharacterAttributes.Constitution);
            int originalCurrentCon = npc[CharacterAttributes.Constitution];
            npc.ApplyAffect(caracAffect);

            Assert.AreEqual(originalBaseStr, npc.BaseAttribute(CharacterAttributes.Strength));
            Assert.AreEqual(originalCurrentStr + 3, npc[CharacterAttributes.Strength]);
            Assert.AreEqual(originalBaseInt, npc.BaseAttribute(CharacterAttributes.Intelligence));
            Assert.AreEqual(originalCurrentInt + 3, npc[CharacterAttributes.Intelligence]);
            Assert.AreEqual(originalBaseWis, npc.BaseAttribute(CharacterAttributes.Wisdom));
            Assert.AreEqual(originalCurrentWis + 3, npc[CharacterAttributes.Wisdom]);
            Assert.AreEqual(originalBaseDex, npc.BaseAttribute(CharacterAttributes.Dexterity));
            Assert.AreEqual(originalCurrentDex + 3, npc[CharacterAttributes.Dexterity]);
            Assert.AreEqual(originalBaseCon, npc.BaseAttribute(CharacterAttributes.Constitution));
            Assert.AreEqual(originalCurrentCon + 3, npc[CharacterAttributes.Constitution]);
        }

        [TestMethod]
        public void OneCombinedAttribute_Assign_Test()
        {
            var room = GenerateRoom("");
            var npc = GenerateNPC("", room);
            var caracAffect = new CharacterAttributeAffect
            {
                Location = CharacterAttributeAffectLocations.AllArmor,
                Operator = AffectOperators.Assign,
                Modifier = 3
            };

            int originalBaseBash = npc.BaseAttribute(CharacterAttributes.ArmorBash);
            int originalBasePierce = npc.BaseAttribute(CharacterAttributes.ArmorPierce);
            int originalBaseSlash = npc.BaseAttribute(CharacterAttributes.ArmorSlash);
            int originalBaseMagic = npc.BaseAttribute(CharacterAttributes.ArmorExotic);
            npc.ApplyAffect(caracAffect);

            Assert.AreEqual(originalBaseBash, npc.BaseAttribute(CharacterAttributes.ArmorBash));
            Assert.AreEqual(3, npc[CharacterAttributes.ArmorBash]);
            Assert.AreEqual(originalBasePierce, npc.BaseAttribute(CharacterAttributes.ArmorPierce));
            Assert.AreEqual(3, npc[CharacterAttributes.ArmorPierce]);
            Assert.AreEqual(originalBaseSlash, npc.BaseAttribute(CharacterAttributes.ArmorSlash));
            Assert.AreEqual(3, npc[CharacterAttributes.ArmorSlash]);
            Assert.AreEqual(originalBaseMagic, npc.BaseAttribute(CharacterAttributes.ArmorExotic));
            Assert.AreEqual(3, npc[CharacterAttributes.ArmorExotic]);
        }
    }
}
