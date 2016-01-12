Player are in-game once he/she has Impersonate a Character
Admin are in-game once he/she has Impersonate a Character or Incarnate an Entity (Character/Room/Item)

when char1 follows char2, char1.Leader = char2
when char2 groups char1, char2.GroupMembers.Add(char1) [can only group if char1.Leader == char2]
when char1 charms char2, char1.Slave = char2 + char2.Master = char1
