<Query Kind="Program" />

void Main()
{
    var path = @"D:\Projects\Muds\Rom2.4b6\src\const.c";
    
    var skills = new List<SkillDefinition>();
    
    var lines = File.ReadAllLines(path);
    var tableFound = false;
    var comment = false;
    for(var index = 0; index < lines.Length; index++)
    {
        var line = lines[index].Trim();
        if (!string.IsNullOrWhiteSpace(line))
        {
            if (tableFound)
            {
                if (line.Contains("/*", StringComparison.InvariantCultureIgnoreCase) && line.Contains("*/", StringComparison.InvariantCultureIgnoreCase))
                    ;
                else if (line.Contains("/*", StringComparison.InvariantCultureIgnoreCase))
                    comment = true;
                else if (line.Contains("*/", StringComparison.InvariantCultureIgnoreCase))
                    comment = false;
                else if (line.Contains("};", StringComparison.InvariantCultureIgnoreCase))
                    tableFound = false;
                else if (!comment)
                {
                    // spell/skill description found
                    /*
                    var (target, position) = line2.Split(',') switch { var a => (a[1], a[2]) };
                    var (cost, gcd) = line3.Split(',') switch { var a => (Convert.ToInt32(a[2]), Convert.ToInt32(a[3])) };
                    var (damMsg, wearOffSelf, wearOffOther) = line4.Split(',') switch { var a => (a[0], a[1], a[2]) };
                    var skillDefinition = new SkillDefinition
                    {
                        Name = line1,
                        //
                        Target = target,
                        Position = position,
                        Cost = cost,
                        GCD = gcd,
                        DamMsg = damMsg,
                        WearOffSelf = wearOffSelf,
                        WearOffOther = wearOffOther
                    };
                    skills.Add(skillDefinition);
                    */
                    StringBuilder sb = new();
                    int curlyCount = 0;
                    while(true)
                    {
                        //line.Dump("-->"+index.ToString());
                        bool inString = false;
                        foreach(var c in line)
                        {
                            if (c == '{')
                                curlyCount++;
                            else if (c == '}')
                                curlyCount--;
                            else if (c == ';')
                                tableFound = false;
                            else if (c == '"')
                                inString = !inString;
                            else if (inString || !char.IsWhiteSpace(c))
                                sb.Append(c);
                        }
                        if (curlyCount == 0)
                        {
                            //"spell found".Dump("woot");
                            //sb.Dump();
                            break;
                        }
                        line = lines[++index].Trim();
                        //line.Dump("<--"+index.ToString());
                    }
                    if (tableFound)
                    {
                        //sb.Dump();
                        //sb.ToString().Split(',').Dump();
                        var skillDefinition = new SkillDefinition(sb.ToString());
                        skills.Add(skillDefinition);
                    }
                }
            }
            if (line.Contains("skill_table", StringComparison.InvariantCultureIgnoreCase))
            {
                tableFound = true;
            }
        }
    }
    
    //skills.Dump();
	//"Mage".Dump();
    //skills.Where(x => x.MageLevel <= 50).OrderBy(x => x.MageLevel).ThenBy(x => x.Type).ThenBy(x => x.Name).Select(x => x.AddAbility(x.MageLevel, x.MageRating)).Dump();
    //"Cleric".Dump();
    //skills.Where(x => x.ClericLevel <= 50).OrderBy(x => x.ClericLevel).ThenBy(x => x.Type).ThenBy(x => x.Name).Select(x => x.AddAbility(x.ClericLevel, x.ClericRating)).Dump();
    //"Thief".Dump();
    //skills.Where(x => x.ThiefLevel <= 50).OrderBy(x => x.ThiefLevel).ThenBy(x => x.Type).ThenBy(x => x.Name).Select(x => x.AddAbility(x.ThiefLevel, x.ThiefRating)).Dump();
    //"Warrior".Dump();
    skills.Where(x => x.WarriorLevel <= 50).OrderBy(x => x.WarriorLevel).ThenBy(x => x.Type).ThenBy(x => x.Name).Select(x => x.AddAbility(x.WarriorLevel, x.WarriorRating)).Dump();
}

// You can define other methods, fields, classes and namespaces here
public class SkillDefinition
{
    public string Name {get;set;}
    public int MageLevel {get;set;}
    public int ClericLevel {get;set;}
    public int ThiefLevel {get;set;}
    public int WarriorLevel {get;set;}
    public int MageRating {get;set;}
    public int ClericRating {get;set;}
    public int ThiefRating {get;set;}
    public int WarriorRating {get;set;}
    public string Target {get;set;}
    public string Position {get;set;}
    public int Cost {get;set;}
    public int GCD {get;set;}
    public string DamMsg {get;set;}
    public string WearOffSelf {get;set;}
    public string WearOffOther {get;set;}
	public SkillType Type {get;set;}
    
    public SkillDefinition(string skill)
    {
        var tokens = skill.Split(',');
        Name = tokens[0];
        MageLevel = Convert.ToInt32(tokens[1]);
        ClericLevel = Convert.ToInt32(tokens[2]);
        ThiefLevel = Convert.ToInt32(tokens[3]);
        WarriorLevel = Convert.ToInt32(tokens[4]);
        MageRating = Convert.ToInt32(tokens[5]);
        ClericRating = Convert.ToInt32(tokens[6]);
        ThiefRating = Convert.ToInt32(tokens[7]);
        WarriorRating = Convert.ToInt32(tokens[8]);
        Target = tokens[10];
        Position = tokens[11];
        Cost = Convert.ToInt32(tokens[14]);
        GCD = Convert.ToInt32(tokens[15]);
        DamMsg = tokens[16];
        WearOffSelf = tokens[17];
        WearOffOther = tokens[18];
		if (tokens[9] != "spell_null")
			Type = SkillType.Spell;
		else if (GCD == 0)
			Type = SkillType.Passive;
		else
			Type = SkillType.Skill;
    }
	
	public enum SkillType
	{
		Passive = 1,
		Skill = 2,
		Spell = 3,
	}
	
	public string AddAbility(int level, int rating)
		=> Type switch
		{
			SkillType.Passive => $"AddPassive({level}, \"{Name}\", {rating});",
			SkillType.Spell => $"AddSpell({level}, \"{Name}\", Domain.ResourceKinds.Mana, {Cost}, CostAmountOperators.Fixed, {rating});",
			SkillType.Skill => $"AddSkill({level}, \"{Name}\", {rating});"
		};
} 