A Player is in-game once he/she has Impersonate a Character
An Admin is in-game once he/she has Impersonate a Character or Incarnate an Entity (Character/Room/Item)

when char1 follows char2, char1.Leader = char2
when char2 groups char1, char2.GroupMembers.Add(char1) [can only group if char1.Leader == char2]
when char1 charms char2, char1.Slave = char2 + char2.Master = char1


Loot table: http://www.gammon.com.au/forum/bbshowpost.php?bbsubject_id=9715
Its a bit more complex, but here is how one MMORPG that I am rather familiar with did it:
 You have two types of data:
 1. Treasure Tables
 2. Monster Treasure Lists
 A Treasure Table is a pre-defined set that contains a list of items, each with a given weight and maximum drop quantity. Weights are relative to the total weight of all items in the table. 
 TreasureTable_Spider
 - Spider Venom, weight=25, max=1
 - Spider Webbing, weight=65, max=1
 - Severed Spider Leg, weight=10, max=1
 This would give the Spider Venom a 25% chance of dropping if this table were called (25 / 100).
 Now, you also have Monster Treasure Lists. These are defined on the monster itself and refer to Treasure Tables using relative weights and max quantities. The monster also has a min/max of items it can drop (# of pulls from the tables).
 Spider
 - Min=1, Max=1
 - TreasureTable_Spider, weight=45, max=1
 - TreasureTable_RareLoot, weight=5, max=1
 - EmptyTreasureTable, weight=50, max=1
 So basically 50% of the time you would get no loot, 45% of the time you'd get something from TreasureTable_Spider and 5% of the time from TreasureTable_RareLoot.
 What this does is allows you to manage what mobs drop without having to alter each and every mob. You want to add another item to the rare loot table, next load it will be there and available to players.
 The downside to this system is it can be complex and it is harder to get an idea of the overall drop-rate of individual items. For example, if you were looking for Spider Webbing using the data above your effective % chance would be 29.25% (45% to call the Spider table, 65% to get Webbing inside that table).