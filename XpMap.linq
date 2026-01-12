<Query Kind="Program" />

void Main()
{
	//var expGain = ComputeExperienceAndAlignment(5, 500, 5).Dump();
	//GainExperience(5, 1000, 5000, 8000);
	const int maxLevel = 50;
	long[,] xpGains = new long[maxLevel,maxLevel];
	for(var characterLevel = 1; characterLevel <= maxLevel; characterLevel++)
	{
		for(var victimLevel = 1; victimLevel <= maxLevel; victimLevel++)
		{
			var xpGain = ComputeExperienceAndAlignment(characterLevel, victimLevel, characterLevel);
			xpGains[characterLevel-1, victimLevel-1] = xpGain;
		}
	}
	xpGains.Dump();
	//ComputeExperienceAndAlignment(10, 10, 10).Dump();
}

// You can define other methods, fields, classes and namespaces here
private int ComputeExperienceAndAlignment(int level, int victimLevel, int totalLevel)
{
    var experience = 0;

    var levelDiff = victimLevel - level;
    // compute base experience
    var baseExp = levelDiff switch
    {
        -9 => 1,
        -8 => 2,
        -7 => 5,
        -6 => 9,
        -5 => 11,
        -4 => 22,
        -3 => 33,
        -2 => 50,
        -1 => 66,
        0 => 83,
        1 => 99,
        2 => 121,
        3 => 143,
        4 => 165,
        _ => 0,
    };
	if (levelDiff > 4)
    	baseExp = 160 + 20 * (levelDiff - 4);
    experience = baseExp;

    // more xp at low level
    if (level < 6)
        experience = 10 * experience / (level + 4);
    // less xp at high level
    if (level > 35)
        experience = 15 * experience / (level - 25);
    // TODO: depends on playing time since last level
    // adjust for grouping
    experience = experience * level / Math.Max(1, totalLevel - 1);

    return experience;
}

public void GainExperience(int level, long experienceByLevel, long currentExperience, long experience)
{
    currentExperience = Math.Max(experienceByLevel * (level-1), currentExperience + experience); // don't go below current level
	currentExperience.Dump();
    // Raise level
    if (experience > 0)
    {
        // In case multiple level are gain, check max level
        while (ExperienceToLevel(experienceByLevel, level, currentExperience) <= 0)
        {
			level.Dump("raise level");
			level++;
        }
    }
}

private static long ExperienceToLevel(long experienceByLevel, int level, long experience)
	=>(experienceByLevel * level) - experience;
	