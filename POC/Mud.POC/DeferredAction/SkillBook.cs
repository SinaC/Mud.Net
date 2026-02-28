using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public static class SkillBook
{
    private static readonly Dictionary<string, Skill> _skills = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fireball"] = new FireballSpell(),
        ["heal"] = new HealSpell()
    };

    public static Skill GetSkillByName(string name)
    {
        _skills.TryGetValue(name, out var skill);
        return skill;
    }
}
