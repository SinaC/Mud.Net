using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.MobProgram.Interfaces;
using Mud.Server.TableGenerator;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("mobprograms", "Information")]
[Alias("mprograms")]
[Syntax(
"[cmd]",
"[cmd] <id>")]
public class MobProgram : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

    private IMobProgramManager MobProgramManager { get; }

    public MobProgram(IMobProgramManager mobProgramManager)
    {
        MobProgramManager = mobProgramManager;
    }

    private bool DisplayList { get; set; }
    private IMobProgram What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // display list
        if (actionInput.Parameters.Length == 0)
        {
            DisplayList = true;
            return null;
        }

        // display specific mob program
        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        int id = actionInput.Parameters[0].AsNumber;
        var what = MobProgramManager.GetMobProgram(id);
        if (what == null)
            return "Not found.";
        What = what;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb;
        if (DisplayList)
            sb = MobProgramTableGenerator.Value.Generate("Mob Programs", new TableGeneratorOptions { ColumnRepetionCount = 5 }, MobProgramManager.MobPrograms);
        else
        {
            // TODO: syntax highlighting (use nodes ?)
            sb = new StringBuilder();
            sb.AppendFormatLine("MOB PROGRAM {0}", What.Blueprint.Id);
            var lines = What.Blueprint.Code.Split(["\r\n", "\n"], StringSplitOptions.None);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                var colorCode = ColorCodeByKeyword.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(trimmed, x.keyword)).colorCode;
                if (colorCode is not null)
                {
                    sb.Append(colorCode);
                    sb.Append(line);
                    sb.Append(StringHelpers.ColorTags.Reset);
                    sb.AppendLine();
                }
                else
                    sb.AppendLine(line);
            }
        }
        Actor.Page(sb);
    }

    private readonly List<(string keyword, string colorCode)> ColorCodeByKeyword =
    [
        ("if", StringHelpers.ColorTags.Yellow),
        ("else", StringHelpers.ColorTags.Yellow),
        ("endif", StringHelpers.ColorTags.Yellow),
        ("and", StringHelpers.ColorTags.LightGreen),
        ("or", StringHelpers.ColorTags.LightMagenta),
        ("*", StringHelpers.ColorTags.Gray),
        ("mob", StringHelpers.ColorTags.White)
    ];

    private static readonly Lazy<TableGenerator<IMobProgram>> MobProgramTableGenerator = new(() =>
    {
        TableGenerator<IMobProgram> generator = new();
        generator.AddColumn("Id", 9, x => x.Blueprint.Id.ToString());
        return generator;
    });
}
