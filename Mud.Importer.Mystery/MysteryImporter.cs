using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Logger;

namespace Mud.Importer.Mystery
{
    public class MysteryImporter : TextBasedImporter
    {
        private const string AreaDataHeader = "AREADATA";
        private const string AreaDataHeader2 = "AREA";
        private const string MobilesHeader = "MOBILES";
        private const string ObjectsHeader = "OBJECTS";
        private const string ResetsHeader = "RESETS";
        private const string RoomsHeader = "ROOMS";
        private const string ShopsHeader = "SHOPS";
        private const string SpecialsHeader = "SPECIALS";

        private readonly List<AreaData> _areas;
        private readonly List<MobileData> _mobiles;
        private readonly List<ObjectData> _objects;
        private readonly List<RoomData> _rooms;

        public IReadOnlyCollection<AreaData> Areas
        {
            get { return _areas.AsReadOnly(); }
        }

        public IReadOnlyCollection<MobileData> Mobiles
        {
            get { return _mobiles.AsReadOnly(); }
        }

        public IReadOnlyCollection<ObjectData> Objects
        {
            get { return _objects.AsReadOnly(); }
        }

        public IReadOnlyCollection<RoomData> Rooms
        {
            get { return _rooms.AsReadOnly(); }
        }

        public MysteryImporter()
        {
            _areas = new List<AreaData>();
            _mobiles = new List<MobileData>();
            _objects = new List<ObjectData>();
            _rooms = new List<RoomData>();
        }

        public override void Parse()
        {
            while (true)
            {
                char letter = ReadLetter();
                if (letter != '#')
                    RaiseParseException("Parse: # not found");

                string word = ReadWord();

                if (word[0] == '$')
                    break; // done
                if (word == AreaDataHeader || word == AreaDataHeader2)
                    ParseArea();
                else if (word == MobilesHeader)
                    ParseMobiles();
                else if (word == ObjectsHeader)
                    ParseObjects();
                else if (word == ResetsHeader)
                    ParseResets();
                else if (word == RoomsHeader)
                    ParseRooms();
                else if (word == ShopsHeader)
                    ParseShops();
                else if (word == SpecialsHeader)
                    ParseSpecials();
                else
                    RaiseParseException("Bad section name: {0}", word);
            }
        }

        private void ParseArea()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Area section");

            AreaData area = new AreaData();
            while (true)
            {
                if (IsEof())
                    break;
                string word = ReadWord();
                if (word == "Builders")
                    area.Builders = ReadString();
                else if (word == "Credits")
                    area.Credits = ReadString();
                else if (word == "Flags")
                    area.Flags = ReadFlags();
                else if (word == "Name")
                    area.Name = ReadString();
                else if (word == "Security")
                    area.Security = (int) ReadNumber();
                else if (word == "VNUMs")
                {
                    area.MinVNum = (int) ReadNumber();
                    area.MaxVNum = (int) ReadNumber();
                }
                else if (word == "End") // done
                    break;
            }
            Log.Default.WriteLine(LogLevels.Debug, "Area [{0}] parsed", area.Name);

            // Set unique number and filename
            area.VNum = _areas.Count;
            area.FileName = CurrentFilename;
            // Save area
            _areas.Add(area);
        }

        private void ParseMobiles()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Mobiles section");

            while (true)
            {
                char letter = ReadLetter();
                if (letter != '#')
                    RaiseParseException("ParseMobiles: # not found");

                int vnum = (int) ReadNumber();
                if (vnum == 0)
                    break; // parsed

                if (_mobiles.Any(x => x.VNum == vnum))
                    RaiseParseException("ParseMobiles: vnum {0} duplicated", vnum);

                MobileData mobileData = new MobileData();
                mobileData.VNum = vnum;
                mobileData.Name = ReadString();
                mobileData.ShortDescr = ReadString();
                mobileData.LongDescr = UpperCaseFirst(ReadString());
                mobileData.Description = UpperCaseFirst(ReadString());
                mobileData.Race = ReadString();
                mobileData.Classes = ReadFlags();
                mobileData.Act = ReadFlags();
                mobileData.AffectedBy = ReadFlags();
                mobileData.AffectedBy2 = ReadFlags();
                mobileData.Etho = (int) ReadNumber();
                mobileData.Alignment = (int) ReadNumber();
                mobileData.Group = (int) ReadNumber();
                mobileData.Level = (int) ReadNumber();
                mobileData.HitRoll = (int) ReadNumber();
                ReadDice(mobileData.Hit);
                ReadDice(mobileData.Mana);
                ReadDice(mobileData.Psp);
                ReadDice(mobileData.Damage);
                mobileData.DamageType = ReadWord();
                mobileData.Armor[0] = (int) ReadNumber()*10;
                mobileData.Armor[1] = (int) ReadNumber()*10;
                mobileData.Armor[2] = (int) ReadNumber()*10;
                mobileData.Armor[3] = (int) ReadNumber()*10;
                mobileData.OffFlags = ReadFlags();
                mobileData.ImmFlags = ReadFlags();
                mobileData.ResFlags = ReadFlags();
                mobileData.VulnFlags = ReadFlags();
                mobileData.StartPos = ReadWord();
                mobileData.DefaultPos = ReadWord();
                mobileData.Sex = ReadWord();
                mobileData.Wealth = ReadNumber();
                mobileData.Form = ReadFlags();
                mobileData.Parts = ReadFlags();
                mobileData.Size = ReadWord();
                mobileData.Material = ReadWord();

                while (true)
                {
                    letter = ReadLetter();

                    if (letter == 'F')
                    {
                        string category = ReadWord();
                        long vector = ReadFlags();
                        if (category.StartsWith("act"))
                            mobileData.Act &= ~vector;
                        else if (category.StartsWith("aff"))
                            mobileData.AffectedBy &= ~vector;
                        else if (category.StartsWith("aff2"))
                            mobileData.AffectedBy2 &= ~vector;
                        else if (category.StartsWith("off"))
                            mobileData.OffFlags &= ~vector;
                        else if (category.StartsWith("imm"))
                            mobileData.ImmFlags &= ~vector;
                        else if (category.StartsWith("res"))
                            mobileData.ResFlags &= ~vector;
                        else if (category.StartsWith("vul"))
                            mobileData.VulnFlags &= ~vector;
                        else if (category.StartsWith("for"))
                            mobileData.Form &= ~vector;
                        else if (category.StartsWith("par"))
                            mobileData.Parts &= ~vector;
                    }
                    else if (letter == 'D')
                    {
                        long dummy = ReadFlags(); // not used
                    }
                    else if (letter == 'M')
                    {
                        mobileData.Program = ReadWord();
                    }
                    else if (letter == 'Y')
                    {
                        // TODO: affects (see db2.C:419)
                        string where = ReadWord();
                        string location = ReadWord();
                        long value1 = ReadNumber();
                        long value2 = ReadNumber();
                    }
                    else
                    {
                        UngetChar(letter);
                        break;
                    }
                }

                // TODO: convert act flags (see db.C:626)
                // TODO: fix parts (see db.C:520)

                Log.Default.WriteLine(LogLevels.Debug, "Mobile [{0}] parsed", vnum);

                // Save mobile data
                _mobiles.Add(mobileData);
            }
        }

        private void ParseObjects()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Objects section");

            while (true)
            {
                char letter = ReadLetter();
                if (letter != '#')
                    RaiseParseException("ParseObjects: # not found");

                int vnum = (int) ReadNumber();
                if (vnum == 0)
                    break; // parsed

                if (_objects.Any(x => x.VNum == vnum))
                    RaiseParseException("ParseObjects: vnum {0} duplicated", vnum);

                ObjectData objectData = new ObjectData();
                objectData.VNum = vnum;
                objectData.Name = ReadString();
                objectData.ShortDescr = ReadString();
                objectData.Description = ReadString();
                objectData.Material = ReadString();
                objectData.ItemType = ReadWord();
                objectData.ExtraFlags = ReadFlags();
                objectData.WearFlags = ReadFlags();

                // Specific value depending on ItemType (see db2.C:564->681)
                switch (objectData.ItemType)
                {
                    case "component":
                        objectData.Values[0] = ReadWord();
                        objectData.Values[1] = ReadWord();
                        objectData.Values[2] = ReadWord();
                        objectData.Values[3] = ReadWord();
                        objectData.Values[4] = ReadWord();
                        break;
                    case "weapon":
                        objectData.Values[0] = ReadWord();
                        objectData.Values[1] = ReadNumber();
                        objectData.Values[2] = ReadNumber();
                        objectData.Values[3] = ReadWord();
                        objectData.Values[4] = ReadFlags();
                        break;
                    case "container":
                        objectData.Values[0] = ReadNumber();
                        objectData.Values[1] = ReadFlags();
                        objectData.Values[2] = ReadNumber();
                        objectData.Values[3] = ReadNumber();
                        objectData.Values[4] = ReadNumber();
                        break;
                    case "drink":
                    case "fountain":
                        objectData.Values[0] = ReadNumber();
                        objectData.Values[1] = ReadNumber();
                        objectData.Values[2] = ReadWord();
                        objectData.Values[3] = ReadNumber();
                        objectData.Values[4] = ReadFlags();
                        break;
                    case "wand":
                    case "staff":
                        objectData.Values[0] = ReadNumber();
                        objectData.Values[1] = ReadNumber();
                        objectData.Values[2] = ReadNumber();
                        objectData.Values[3] = ReadWord();
                        objectData.Values[4] = ReadNumber();
                        break;
                    case "potion":
                    case "pill":
                    case "scroll":
                    case "template":
                        objectData.Values[0] = ReadNumber();
                        objectData.Values[1] = ReadWord();
                        objectData.Values[2] = ReadWord();
                        objectData.Values[3] = ReadWord();
                        objectData.Values[4] = ReadWord();
                        break;
                    default:
                        objectData.Values[0] = ReadFlags();
                        objectData.Values[1] = ReadFlags();
                        objectData.Values[2] = ReadFlags();
                        objectData.Values[3] = ReadFlags();
                        objectData.Values[4] = ReadFlags();
                        break;
                }

                objectData.Level = (int) ReadNumber();
                objectData.Weight = (int) ReadNumber();
                objectData.Cost = ReadNumber();
                objectData.Condition = ReadLetter();

                while (true)
                {
                    letter = ReadLetter();
                    if (letter == 'S') // code moved into while loop
                    {
                        objectData.Size = ReadWord();
                    }
                    else if (letter == 'R')
                    {
                        // TODO: restriction (see db2.C:746)
                        string type = ReadWord();
                        long value = ReadNumber();
                        long notR = ReadNumber();
                    }
                    else if (letter == 'W')
                    {
                        // TODO: restriction (see db2.C:771)
                        string skill = ReadWord();
                        long value = ReadNumber();
                        long notR = ReadNumber();
                    }
                    else if (letter == 'Z')
                    {
                        // TODO: ability upgrade (see db2.C:811)
                        string skill = ReadWord();
                        long value = ReadNumber();
                    }
                    else if (letter == 'A')
                    {
                        // TODO: oldstyle affects (see db2.C:841)
                        int location = (int) ReadNumber();
                        int modifier = (int) ReadNumber();
                    }
                    else if (letter == 'F')
                    {
                        // TODO: affects (see db2.C:863)
                        letter = ReadLetter();
                        int location = (int) ReadNumber();
                        int modifier = (int) ReadNumber();
                        long vector = ReadFlags();
                    }
                    else if (letter == 'E')
                    {
                        string keyword = ReadString();
                        string description = ReadString();
                        objectData.ExtraDescr.Add(keyword, description);
                    }
                    else if (letter == 'Y')
                    {
                        // TODO: affects (see db2.C:948)
                        string where = ReadWord();
                        string location = ReadWord();
                        long value1 = ReadNumber();
                        long value2 = ReadNumber();
                    }
                    else if (letter == 'M')
                    {
                        objectData.Program = ReadWord();
                    }
                    else
                    {
                        UngetChar(letter);
                        break;
                    }
                }

                Log.Default.WriteLine(LogLevels.Debug, "Object [{0}] parsed", vnum);

                // Save object data
                _objects.Add(objectData);
            }
        }

        private void ParseResets()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Resets section");

            int iLastObj = 0; // TODO: replace with RoomData
            int iLastRoom = 0; // TODO: replace with RoomData
            while (true)
            {
                char letter = ReadLetter();

                if (letter == 'S') // done
                    break;
                else if (letter == '*')
                {
                    ReadToEol();
                    continue;
                }

                ReadNumber(); // unused
                int arg1 = (int) ReadNumber();
                int arg2 = (int) ReadNumber();
                int arg3 = (letter == 'G' || letter == 'R') ? 0 : (int) ReadNumber();
                int arg4 = (letter == 'P' || letter == 'M' || letter == 'Z') ? (int) ReadNumber() : 0;
                ReadToEol();

                ResetData resetData = new ResetData
                {
                    Command = letter,
                    Arg1 = arg1,
                    Arg2 = arg2,
                    Arg3 = arg3,
                    Arg4 = arg4
                };

                if (letter == 'M')
                {
                    if (arg2 == 0 || arg4 == 0)
                        Warn("ParseResets: 'M' has arg2 or arg4 equal to 0 (room: {0})", arg3);
                    MobileData mobileData = _mobiles.FirstOrDefault(x => x.VNum == arg1);
                    if (mobileData == null)
                        Warn("ParseResets: 'M' unknown mobile vnum {0}", arg1);
                    RoomData roomData = _rooms.FirstOrDefault(x => x.VNum == arg3);
                    if (roomData == null)
                        Warn("ParseResets: 'M' unknown room vnum {0}", arg3);
                    else
                    {
                        roomData.Resets.Add(resetData);
                        iLastRoom = arg3;
                    }
                }
                else if (letter == 'O')
                {
                    ObjectData objectData = _objects.FirstOrDefault(x => x.VNum == arg1);
                    if (objectData == null)
                        Warn("ParseResets: 'O' unknown object vnum {0}", arg1);
                    RoomData roomData = _rooms.FirstOrDefault(x => x.VNum == arg3);
                    if (roomData == null)
                        Warn("ParseResets: 'O' unknown room vnum {0}", arg3);
                    else
                    {
                        roomData.Resets.Add(resetData);
                        iLastObj = arg3;
                    }
                }
                else if (letter == 'P')
                {
                    if (arg2 == 0 || arg4 == 0)
                        Warn("ParseResets: 'P' has arg2 or arg4 equal to 0 (room: {0})", iLastObj);
                    ObjectData objectData = _objects.FirstOrDefault(x => x.VNum == arg1);
                    if (objectData == null)
                        Warn("ParseResets: 'P' unknown object vnum {0}", arg1);
                    RoomData roomData = _rooms.FirstOrDefault(x => x.VNum == iLastObj);
                    if (roomData == null)
                        Warn("ParseResets: 'P' unknown room vnum {0}", iLastObj);
                    else
                        roomData.Resets.Add(resetData);
                }
                else if (letter == 'G' || letter == 'E')
                {
                    ObjectData objectData = _objects.FirstOrDefault(x => x.VNum == arg1);
                    if (objectData == null)
                        Warn("ParseResets: '{0}' unknown object vnum {1}", letter, arg1);
                    RoomData roomData = _rooms.FirstOrDefault(x => x.VNum == iLastRoom);
                    if (roomData == null)
                        Warn("ParseResets: '{0}' unknown room vnum {1}", letter, iLastRoom);
                    else
                    {
                        roomData.Resets.Add(resetData);
                        iLastObj = iLastRoom;
                    }
                }
                else if (letter == 'D')
                {
                    RoomData roomData = _rooms.FirstOrDefault(x => x.VNum == arg1);
                    if (roomData == null)
                        Warn("ParseResets: 'D' unknown room vnum {0}", arg1);
                    else
                    {
                        if (arg2 < 0 || arg2 >= RoomData.MaxExits || roomData.Exits[arg2] == null)
                            RaiseParseException("ParseResets: 'D': exit {0} not door", arg2);
                        else
                        {
                            if (arg3 == 0)
                                ; // NOP
                            else if (arg3 == 1)
                                roomData.Exits[arg2].ExitInfo |= 0x2; // closed
                            else if (arg3 == 2)
                                roomData.Exits[arg2].ExitInfo |= 0x2 | 0x4; // closed + locked
                            else
                                Warn("ParseResets: 'D': bad 'locks': {0}", arg3);
                        }
                        // ResetData is not stored
                    }
                }
                else if (letter == 'R')
                {
                    if (arg2 < 0 || arg2 >= RoomData.MaxExits)
                        RaiseParseException("ParseResets: 'R': exit {0} not door", arg2);
                    RoomData roomData = _rooms.FirstOrDefault(x => x.VNum == arg1);
                    if (roomData == null)
                        Warn("ParseResets: 'D' unknown room vnum {0}", arg1);
                    else
                    {
                        roomData.Resets.Add(resetData);
                    }
                }
                else if (letter == 'Z')
                {
                    if (arg1 < 2 || arg2 < 2 || arg1*arg2 > 100)
                        RaiseParseException("ParseResets: 'Z': bad width, height (room {0})", arg3);
                    if (arg4 > 0)
                    {
                        ObjectData map = _objects.FirstOrDefault(x => x.VNum == arg4);
                        if (map == null)
                            RaiseParseException("ParseResets: 'Z': bad map vnum: {0}", arg4);
                    }
                    RoomData roomData = _rooms.FirstOrDefault(x => x.VNum == arg1);
                    if (roomData == null)
                        Warn("ParseResets: 'Z' unknown room vnum {0}", arg1);
                    else
                    {
                        roomData.Resets.Add(resetData);
                    }
                }
                else
                    RaiseParseException("ParseResets: bad command '{0}'", letter);
            }
        }

        private void ParseRooms()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Rooms section");

            while (true)
            {
                char letter = ReadLetter();
                if (letter != '#')
                    RaiseParseException("ParseRooms: # not found");

                int vnum = (int) ReadNumber();
                if (vnum == 0)
                    break; // parsed

                if (_rooms.Any(x => x.VNum == vnum))
                    RaiseParseException("ParseRooms: vnum {0} duplicated", vnum);

                RoomData roomData = new RoomData();
                roomData.VNum = vnum;
                roomData.Name = ReadString();
                roomData.Description = ReadString();
                ReadNumber(); // area number not used
                roomData.Flags = ReadFlags(); // convert_room_flags (see db.C:601)
                roomData.Sector = (int) ReadNumber();
                roomData.MaxSize = (int) ReadNumber();

                while (true)
                {
                    letter = ReadLetter();

                    if (letter == 'S')
                        break;
                    else if (letter == 'H')
                        roomData.HealRate = (int) ReadNumber();
                    else if (letter == 'M')
                        roomData.ManaRate = (int) ReadNumber();
                    else if (letter == 'P')
                        roomData.PspRate = (int) ReadNumber();
                    else if (letter == 'C')
                        roomData.Clan = ReadString();
                    else if (letter == 'D')
                    {
                        int door = (int) ReadNumber();
                        if (door < 0 || door >= RoomData.MaxExits)
                            RaiseParseException("ParseRooms: vnum {0} has bad door number", vnum);
                        ExitData exitData = new ExitData
                        {
                            Description = ReadString(),
                            Keyword = ReadString(),
                            ExitInfo = ReadFlags(),
                            Key = (int) ReadNumber(),
                            Destination = (int) ReadNumber()
                        };
                        roomData.Exits[door] = exitData;
                    }
                    else if (letter == 'E')
                    {
                        string keyword = ReadString();
                        string description = ReadString();
                        if (roomData.ExtraDescr.ContainsKey(keyword))
                            Warn("ParseRooms: duplicate description in vnum {0}", vnum);
                        else
                            roomData.ExtraDescr.Add(keyword, description);
                    }
                    else if (letter == 'O')
                    {
                        if (!String.IsNullOrWhiteSpace(roomData.Owner))
                            RaiseParseException("ParseRooms: vnum {0} has duplicate owner", vnum);
                        roomData.Owner = ReadString();
                    }
                    else if (letter == 'G')
                    {
                        roomData.Guilds = ReadFlags();
                    }
                    else if (letter == 'Z')
                    {
                        roomData.Program = ReadWord();
                    }
                    else if (letter == 'R')
                    {
                        roomData.TimeBetweenRepop = (int) ReadNumber();
                        roomData.TimeBetweenRepopPeople = (int) ReadNumber();
                    }
                    else if (letter == 'Y')
                    {
                        // TODO: affects (see db.C:3502)
                        string where = ReadWord();
                        string location = ReadWord();
                        long value1 = ReadNumber();
                        long value2 = ReadNumber();
                    }
                    else
                        RaiseParseException("ParseRooms: vnum {0} has unknown flag", vnum);
                }
                Log.Default.WriteLine(LogLevels.Debug, "Room [{0}] parsed", vnum);

                // Save room data
                _rooms.Add(roomData);
            }
        }

        private void ParseShops()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Shops section");

            while (true)
            {
                int keeper = (int)ReadNumber();
                if (keeper == 0)
                    break; // done
                MobileData mobileData = _mobiles.FirstOrDefault(x => x.VNum == keeper);
                if (mobileData == null)
                    RaiseParseException("ParseShops: unknown mobile vnum {0}", keeper);
                else
                {
                    ShopData shopData = new ShopData();
                    shopData.Keeper = keeper;
                    for (int i = 0; i < ShopData.MaxTrades; i++)
                        shopData.BuyType[i] = (int) ReadNumber();
                    shopData.ProfitBuy = (int) ReadNumber();
                    shopData.ProfitSell = (int) ReadNumber();
                    shopData.OpenHour = (int) ReadNumber();
                    shopData.CloseHour = (int) ReadNumber();
                    mobileData.Shop = shopData;
                }
                ReadToEol();
            }
        }

        private void ParseSpecials()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Specials section");

            while (true)
            {
                char letter = ReadLetter();

                if (letter == 'S') // done
                    break;
                else if (letter == '*')
                    ; // nop
                else if (letter == 'M')
                {
                    int vnum = (int) ReadNumber();
                    string special = ReadWord();
                    MobileData mobileData = _mobiles.FirstOrDefault(x => x.VNum == vnum);
                    if (mobileData != null)
                        mobileData.Special = special;
                    else
                        Warn("ParseSpecials: 'M' unknown mobile vnum {0}", vnum);
                }
                else
                    RaiseParseException("ParseSpecials: letter {0} not *MS", letter);
                ReadToEol();
            }
        }
    }
}
