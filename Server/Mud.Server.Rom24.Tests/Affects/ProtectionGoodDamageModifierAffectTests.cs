using Moq;
using Mud.Domain;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Rom24.Affects;

namespace Mud.Server.Rom24.Tests.Affects;

[TestClass]
public class ProtectionGoodDamageModifierAffectTests
{
    [TestMethod]
    public void IsNotGood()
    {
        var chMock = new Mock<ICharacter>();
        chMock.SetupGet(x => x.IsGood).Returns(false);

        var affect = new ProtectionGoodDamageModifierAffect();
        var result = affect.ModifyDamage(chMock.Object, null!, SchoolTypes.Pierce, DamageSources.Ability, 100);

        Assert.AreEqual(DamageModifierAffectAction.Nop, result.Action);
        Assert.IsFalse(result.WornOff);
        Assert.AreEqual(100, result.ModifiedDamage);
    }

    [TestMethod]
    public void IsGood()
    {
        var chMock = new Mock<ICharacter>();
        chMock.SetupGet(x => x.IsGood).Returns(true);

        var affect = new ProtectionGoodDamageModifierAffect();
        var result = affect.ModifyDamage(chMock.Object, null!, SchoolTypes.Pierce, DamageSources.Ability, 100);

        Assert.AreEqual(DamageModifierAffectAction.DamageDecreased, result.Action);
        Assert.IsFalse(result.WornOff);
        Assert.AreEqual(75, result.ModifiedDamage);
    }
}
