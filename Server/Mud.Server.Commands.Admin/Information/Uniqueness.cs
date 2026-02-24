using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.Admin.Information;


[AdminCommand("Uniqueness", "Information")]
[Syntax(
    "[cmd]")]
public class Uniqueness : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

    private IUniquenessManager UniquenessManager { get; }

    public Uniqueness(IUniquenessManager uniquenessManager)
    {
        UniquenessManager = uniquenessManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        var sb = UnavailableNamesTableGenerator.Value.Generate("Unavailable names", new TableGeneratorOptions { ColumnRepetionCount = 3 }, UniquenessManager.UnavailableNames);
        Actor.Page(sb);
    }


    private static readonly Lazy<TableGenerator<string>> UnavailableNamesTableGenerator = new(() =>
    {
        TableGenerator<string> generator = new();
        generator.AddColumn("Name", 20, x => x);
        return generator;
    });
}
