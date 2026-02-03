using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Skill.Interfaces;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Rom24.Spells;
using System.Reflection;

namespace Mud.Server.Rom24.Tests.Abilities;

[TestClass]
public abstract class AbilityTestBase
{
    protected IServiceProvider _serviceProvider = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProvider = serviceProviderMock.Object;

        RegisterAdditionalDependencies(serviceProviderMock);
    }

    protected virtual void RegisterAdditionalDependencies(Mock<IServiceProvider> serviceProviderMock)
    {
    }

    protected static ICommandParameter[] BuildParameters(string parameters)
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);
        return parser.SplitParameters(parameters).Select(parser.ParseParameter).ToArray();
    }

    protected IActionInput BuildActionInput<TGameAction>(IActor actor, string commandLine)
        where TGameAction:IGameAction
    {
        Type type = typeof(TGameAction);
        CommandAttribute commandAttribute = type.GetCustomAttribute<CommandAttribute>()!;
        SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? GameActionInfo.DefaultSyntaxCommandAttribute;
        IEnumerable<AliasAttribute> aliasAttributes = type.GetCustomAttributes<AliasAttribute>();

        IGameActionInfo gameActionInfo;
        switch (commandAttribute)
        {
            case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
                {
                    gameActionInfo = new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                }
            case CharacterCommandAttribute characterCommandAttribute:
                {
                    if (type.IsAssignableTo(typeof(ISkill)))
                    {
                        var skillDefinition = new AbilityDefinition(type);
                        gameActionInfo = new SkillGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes, null, skillDefinition);
                    }
                    else
                        gameActionInfo = new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                }
            case AdminCommandAttribute adminCommandAttribute:
                {
                    gameActionInfo = new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                }
            case PlayerCommandAttribute playerCommandAttribute:
                {
                    gameActionInfo = new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                }
            case ItemCommandAttribute itemCommandAttribute:
                {
                    gameActionInfo = new ItemGameActionInfo(type, itemCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                }
            case RoomCommandAttribute roomCommandAttribute:
                {
                    gameActionInfo = new RoomGameActionInfo(type, roomCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                }
            case ActorCommandAttribute actorCommandAttribute:
                {
                    gameActionInfo = new ActorGameActionInfo(type, actorCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                }
            default:
                gameActionInfo = new GameActionInfo(type, commandAttribute, syntaxAttribute, aliasAttributes, null);
                break;
        }

        new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object).ExtractCommandAndParameters(commandLine, out var command, out var parameters);
        return new ActionInput(gameActionInfo, actor, commandLine, command, parameters);
    }

    protected static IAbilityLearned BuildAbilityLearned(string name)
    {
        var abilityDefinitionMock = new Mock<IAbilityDefinition>();
        abilityDefinitionMock.SetupGet(x => x.Name).Returns(name);
        var mock = new Mock<IAbilityLearned>();
        mock.SetupGet(x => x.AbilityUsage).Returns(new AbilityUsage(name, 1, [new AbilityResourceCost(ResourceKinds.Mana, 50, CostAmountOperators.Fixed)], 1, 100, abilityDefinitionMock.Object));
        mock.SetupGet(x => x.Name).Returns(name);
        mock.Setup(x => x.HasCost).Returns(true);
        return mock.Object;
    }

    public class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies => [typeof(Server.Server).Assembly, typeof(AcidBlast).Assembly];
    }
}
