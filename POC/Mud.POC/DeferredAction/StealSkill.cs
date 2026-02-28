using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;


public class StealSkill : Skill
{
    public StealSkill() : base("steal", 0, 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (target == null || target == caster)
            return;

        if (caster.CurrentRoom.Flags.HasFlag(RoomFlags.Safe))
            return;

        caster.Wait = 2;

        // Skill roll (replace with real skill system later)
        bool success = Random.Shared.Next(100) < 60;

        if (!success)
        {
            HandleFailure(caster, target, world);
            return;
        }

        HandleSuccess(caster, target, world);
    }

    private void HandleFailure(Mob caster, Mob target, World world)
    {
        if (target.IsPlayer && !world.IsPkAllowedRoom(caster.CurrentRoom))
        {
            caster.PlayerFlags |= PlayerFlags.Thief;

            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{caster.Name} is now flagged as a THIEF!")));
        }

        // Victim may attack
        if (!target.InCombat)
        {
            target.CurrentTarget = caster;
            target.Position = Position.Fighting;
        }
    }

    private void HandleSuccess(Mob caster, Mob target, World world)
    {
        if (!target.Items.Any())
            return;

        var item = target.Items[Random.Shared.Next(target.Items.Count)];

        world.Enqueue(new TransferItemAction(item, caster));

        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{caster.Name} steals {item.Name} from {target.Name}!")));
    }
}
