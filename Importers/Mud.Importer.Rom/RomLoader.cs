using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Importer.Rom.Domain;
using System.ComponentModel;

namespace Mud.Importer.Rom;

[Export]
public class RomLoader : TextBasedLoader
{
    public const long A = 1 << 0;   //0x00000001;
    public const long B = 1 << 1;   //0x00000002;
    public const long C = 1 << 2;   //0x00000004;
    public const long D = 1 << 3;   //0x00000008;
    public const long E = 1 << 4;   //0x00000010;
    public const long F = 1 << 5;   //0x00000020;
    public const long G = 1 << 6;   //0x00000040;
    public const long H = 1 << 7;   //0x00000080;
    public const long I = 1 << 8;   //0x00000100;
    public const long J = 1 << 9;   //0x00000200;
    public const long K = 1 << 10;  //0x00000400;
    public const long L = 1 << 11;  //0x00000800;
    public const long M = 1 << 12;  //0x00001000;
    public const long N = 1 << 13;  //0x00002000;
    public const long O = 1 << 14;  //0x00004000;
    public const long P = 1 << 15;  //0x00008000;
    public const long Q = 1 << 16;  //0x00010000;
    public const long R = 1 << 17;  //0x00020000;
    public const long S = 1 << 18;  //0x00040000;
    public const long T = 1 << 19;  //0x00080000;
    public const long U = 1 << 20;  //0x00100000;
    public const long V = 1 << 21;  //0x00200000;
    public const long W = 1 << 22;  //0x00400000;
    public const long X = 1 << 23;  //0x00800000;
    public const long Y = 1 << 24;  //0x01000000;
    public const long Z = 1 << 25;  //0x02000000;
    public const long aa = 1 << 26; //0x04000000;
    public const long bb = 1 << 27; //0x8000000;
    public const long cc = 1 << 28; //0x10000000;
    public const long dd = 1 << 29; //0x20000000;
    public const long ee = 1 << 30; //0x40000000;

    private const string AreaDataHeader = "AREADATA";
    private const string OldAreaDataHeader = "AREA";
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


    private int _lastAreaVnum;

    internal IReadOnlyCollection<AreaData> Areas => _areas.AsReadOnly();

    internal IReadOnlyCollection<MobileData> Mobiles => _mobiles.AsReadOnly();

    internal IReadOnlyCollection<ObjectData> Objects => _objects.AsReadOnly();

    internal IReadOnlyCollection<RoomData> Rooms => _rooms.AsReadOnly();

    public RomLoader(ILogger<RomLoader> logger)
        :base(logger)
    {
        _areas = [];
        _mobiles = [];
        _objects = [];
        _rooms = [];
    }

    public override void Parse()
    {
        while (true)
        {
            var letter = ReadLetter();
            if (letter != '#')
                RaiseParseException("Parse: # not found");

            var word = ReadWord();

            if (word[0] == '$')
                break; // done
            if (word == AreaDataHeader)
                ParseArea();
            else if (word == OldAreaDataHeader)
                ParseOldArea();
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
            else if (word.ToUpper() == "STYLE")
            {
                Logger.LogWarning("Specific area file Style not yet available");
                ReadToNextSection();
            }
            else if (word.ToUpper() == "HELPS")
            {
                Logger.LogWarning("Specific area file Helps not yet available");
                ReadToNextSection();
            }
            else if (word.ToUpper() == "SOCIALS")
            {
                Logger.LogWarning("Specific area file Socials not yet available");
                break;
            }
            else if (word.ToUpper() == "OLIMITS" || word.ToUpper() == "OMPROGS")
            {
                Logger.LogWarning("Specific '{word}' not handled", word);
                ReadToNextSection();
            }
            else
                RaiseParseException("Bad section name: {0}", word);
        }
    }

    private void ParseArea()
    {
        Logger.LogTrace("Area section");

        var area = new AreaData();
        while (true)
        {
            if (IsEof())
                break;
            var word = ReadWord();
            if (word == "Builders")
                area.Builders = ReadString();
            else if (word == "Credits")
                area.Credits = ReadString();
            else if (word == "Flags")
                area.Flags = ReadFlags();
            else if (word == "Name")
                area.Name = ReadString();
            else if (word == "Security")
                area.Security = (int)ReadNumber();
            else if (word == "VNUMs")
            {
                area.MinVNum = (int)ReadNumber();
                area.MaxVNum = (int)ReadNumber();
            }
            else if (word == "End") // done
                break;
        }
        Logger.LogTrace("Area [{area}] parsed", area.Name);

        // Set unique number and filename
        area.VNum = 10+_areas.Count;
        area.FileName = CurrentFilename;
        // Save area
        _areas.Add(area);
        //
        _lastAreaVnum = area.VNum;
    }

    private void ParseOldArea()
    {
        Logger.LogTrace("Old area section");
        var area = new AreaData
        {
            FileName = ReadString(),
            Name = ReadString(),
            Credits = ReadString(),
            MinVNum = (int) ReadNumber(),
            MaxVNum = (int) ReadNumber()
        };
        Logger.LogTrace("Area [{area}] parsed", area.Name);

        area.VNum = 10 + _areas.Count;
        // Save area
        _areas.Add(area);
        //
        _lastAreaVnum = area.VNum;
    }

    private void ParseMobiles()
    {
        Logger.LogTrace("Mobiles section");

        while (true)
        {
            var letter = ReadLetter();
            if (letter != '#')
                RaiseParseException("ParseMobiles: # not found");

            var vnum = (int)ReadNumber();
            if (vnum == 0)
                break; // parsed

            if (_mobiles.Any(x => x.VNum == vnum))
                RaiseParseException("ParseMobiles: vnum {0} duplicated", vnum);

            var mobileData = new MobileData()
            {
                VNum = vnum,
                Name = TrimOldstylePrefix(ReadString()),
                ShortDescr = ReadString(),
                LongDescr = UpperCaseFirst(ReadString()),
                Description = UpperCaseFirst(ReadString()),
                Race = ReadString(),
                Act = ReadFlags(),
                AffectedBy = ReadFlags(),
                Alignment = (int)ReadNumber(),
                Group = (int)ReadNumber(),
                Level = (int)ReadNumber(),
                HitRoll = (int)ReadNumber()
            };
            ReadDice(mobileData.Hit);
            ReadDice(mobileData.Mana);
            ReadDice(mobileData.Damage);
            mobileData.DamageType = ReadWord();
            mobileData.Armor[0] = (int)ReadNumber() * 10;
            mobileData.Armor[1] = (int)ReadNumber() * 10;
            mobileData.Armor[2] = (int)ReadNumber() * 10;
            mobileData.Armor[3] = (int)ReadNumber() * 10;
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
                    var category = ReadWord();
                    var vector = ReadFlags();
                    if (category.StartsWith("act"))
                        mobileData.Act &= ~vector;
                    else if (category.StartsWith("aff"))
                        mobileData.AffectedBy &= ~vector;
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
                    var dummy = ReadFlags(); // not used
                }
                else if (letter == 'M')
                {
                    mobileData.ProgramTriggers.Add(ReadString());
                }
                else if (letter == 'Y')
                {
                    // TODO: affects (see db2.C:419)
                    var where = ReadWord();
                    var location = ReadWord();
                    var value1 = ReadNumber();
                    var value2 = ReadNumber();
                }
                else
                {
                    UngetChar(letter);
                    break;
                }
            }

            // TODO: convert act flags (see db.C:626)
            // TODO: fix parts (see db.C:520)

            Logger.LogTrace("Mobile [{vnum}] parsed", vnum);

            // Save mobile data
            _mobiles.Add(mobileData);
        }
    }

    private void ParseObjects()
    {
        Logger.LogTrace("Objects section");

        while (true)
        {
            char letter = ReadLetter();
            if (letter != '#')
                RaiseParseException("ParseObjects: # not found");

            int vnum = (int)ReadNumber();
            if (vnum == 0)
                break; // parsed

            if (_objects.Any(x => x.VNum == vnum))
                RaiseParseException("ParseObjects: vnum {0} duplicated", vnum);

            ObjectData objectData = new()
            {
                VNum = vnum,
                Name = ReadString(),
                ShortDescr = ReadString(),
                Description = ReadString(),
                Material = ReadString(),
                ItemType = ReadWord(),
                ExtraFlags = ReadFlags(),
                WearFlags = ReadFlags()
            };

            // Specific value depending on ItemType (see db2.C:564->681)
            switch (objectData.ItemType)
            {
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

            objectData.Level = (int)ReadNumber();
            objectData.Weight = (int)ReadNumber();
            objectData.Cost = ReadNumber();
            objectData.Condition = ReadLetter();

            while (true)
            {
                letter = ReadLetter();
                if (letter == 'A')
                {
                    ObjectAffect aff = new()
                    {
                        Where = ObjectAffect.WhereToAttributeOrResource,
                        Level = objectData.Level,
                        Location = (int)ReadNumber(),
                        Modifier = (int)ReadNumber(),
                        BitVector = 0,
                    };
                    objectData.Affects.Add(aff);
                }
                else if (letter == 'F')
                {
                    char where = ReadLetter();
                    ObjectAffect aff = new()
                    {
                        Level = objectData.Level,
                        Location = (int)ReadNumber(),
                        Modifier = (int)ReadNumber(),
                        BitVector = ReadFlags(),
                    };
                    switch (where)
                    {
                        case 'A': aff.Where = ObjectAffect.WhereToAffects; break;
                        case 'I': aff.Where = ObjectAffect.WhereToImmune; break;
                        case 'R': aff.Where = ObjectAffect.WhereToResist; break;
                        case 'V': aff.Where = ObjectAffect.WhereToVuln; break;
                        default:
                            Logger.LogError("ParseObjects: item [vnum:{vnum}] Invalid affect where '{where}'", vnum, where);
                            break;
                    }
                    objectData.Affects.Add(aff);
                }
                else if (letter == 'E')
                {
                    string keywords = ReadString();
                    string description = ReadString();
                    if (objectData.ExtraDescr.ContainsKey(keywords))
                        Logger.LogError("ParseObjects: item [vnum:{vnum}] Extra desc '{keywords}' already exists", vnum, keywords);
                    else
                        objectData.ExtraDescr.Add(keywords, description);
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

            Logger.LogTrace("Object [{vnum}] parsed", vnum);

            // Save object data
            _objects.Add(objectData);
        }
    }

    private void ParseResets()
    {
        Logger.LogTrace("Resets section");

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
            int arg1 = (int)ReadNumber();
            int arg2 = (int)ReadNumber();
            int arg3 = (letter == 'G' || letter == 'R') ? 0 : (int)ReadNumber();
            int arg4 = (letter == 'P' || letter == 'M') ? (int)ReadNumber() : 0;
            ReadToEol();

            ResetData resetData = new()
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
                var mobileData = _mobiles.FirstOrDefault(x => x.VNum == arg1);
                if (mobileData == null)
                    Warn("ParseResets: 'M' unknown mobile vnum {0}", arg1);
                var roomData = _rooms.FirstOrDefault(x => x.VNum == arg3);
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
                var objectData = _objects.FirstOrDefault(x => x.VNum == arg1);
                if (objectData == null)
                    Warn("ParseResets: 'O' unknown object vnum {0}", arg1);
                var roomData = _rooms.FirstOrDefault(x => x.VNum == arg3);
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
                var objectData = _objects.FirstOrDefault(x => x.VNum == arg1);
                if (objectData == null)
                    Warn("ParseResets: 'P' unknown object vnum {0}", arg1);
                var roomData = _rooms.FirstOrDefault(x => x.VNum == iLastObj);
                if (roomData == null)
                    Warn("ParseResets: 'P' unknown room vnum {0}", iLastObj);
                else
                    roomData.Resets.Add(resetData);
            }
            else if (letter == 'G' || letter == 'E')
            {
                var objectData = _objects.FirstOrDefault(x => x.VNum == arg1);
                if (objectData == null)
                    Warn("ParseResets: '{0}' unknown object vnum {1}", letter, arg1);
                var roomData = _rooms.FirstOrDefault(x => x.VNum == iLastRoom);
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
                var roomData = _rooms.FirstOrDefault(x => x.VNum == arg1);
                if (roomData == null)
                    Warn("ParseResets: 'D' unknown room vnum {0}", arg1);
                else
                {
                    if (arg2 < 0 || arg2 >= RoomData.MaxExits || roomData.Exits[arg2] == null)
                        RaiseParseException("ParseResets: 'D': exit {0} not door", arg2);
                    else
                    {
                        if (arg3 == 0)
#pragma warning disable CS0642 // Possible mistaken empty statement
                            ; // NOP
#pragma warning restore CS0642 // Possible mistaken empty statement
                        else if (arg3 == 1)
                            roomData.Exits[arg2].ExitInfo |= 0x2; // closed
                        else if (arg3 == 2)
                            roomData.Exits[arg2].ExitInfo |= 0x2 | 0x4; // closed + locked
                        else
                            Warn("ParseResets: 'D': bad 'locks': {0}", arg3);
                    }
                    roomData.Resets.Add(resetData);
                }
            }
            else if (letter == 'R')
            {
                if (arg2 < 0 || arg2 > RoomData.MaxExits)
                    RaiseParseException("ParseResets: 'R': exit {0} not door", arg2);
                var roomData = _rooms.FirstOrDefault(x => x.VNum == arg1);
                if (roomData == null)
                    Warn("ParseResets: 'R' unknown room vnum {0}", arg1);
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
        Logger.LogTrace("Rooms section");

        while (true)
        {
            char letter = ReadLetter();
            if (letter != '#')
                RaiseParseException("ParseRooms: # not found");

            int vnum = (int)ReadNumber();
            if (vnum == 0)
                break; // parsed

            if (_rooms.Any(x => x.VNum == vnum))
                RaiseParseException("ParseRooms: vnum {0} duplicated", vnum);

            RoomData roomData = new()
            {
                AreaVnum = _lastAreaVnum,
                VNum = vnum,
                Name = ReadString(),
                Description = ReadString()
            };
            ReadNumber(); // area number not used
            roomData.Flags = ReadFlags(); // convert_room_flags (see db.C:601)
            roomData.Sector = (int)ReadNumber();

            while (true)
            {
                letter = ReadLetter();

                if (letter == 'S')
                    break;
                else if (letter == 'H')
                    roomData.HealRate = (int)ReadNumber();
                else if (letter == 'M')
                    roomData.ManaRate = (int)ReadNumber();
                else if (letter == 'C')
                    roomData.Clan = ReadString();
                else if (letter == 'D')
                {
                    int door = (int)ReadNumber();
                    if (door < 0 || door >= RoomData.MaxExits)
                        RaiseParseException("ParseRooms: vnum {0} has bad door number", vnum);
                    var description = ReadString();
                    var keyword = ReadString();
                    var locks = ReadNumber();
                    var key = ReadNumber();
                    var destinationVnum = ReadNumber();
                    long exitInfo = 0;
                    switch (locks)
                    {
                        case 1: exitInfo = ExitData.ExDoor; break;
                        case 2: exitInfo = ExitData.ExDoor | ExitData.ExPickproof; break;
                        case 3: exitInfo = ExitData.ExDoor | ExitData.ExNoPass; break;
                        case 4: exitInfo = ExitData.ExDoor | ExitData.ExPickproof | ExitData.ExNoPass; break;
                    }
                    ExitData exitData = new()
                    {
                        Description = description,
                        Keyword = keyword,
                        ExitInfo = exitInfo,
                        Key = (int)key,
                        DestinationVNum = (int)destinationVnum
                    };
                    roomData.Exits[door] = exitData;
                }
                else if (letter == 'E')
                {
                    string keywords = ReadString();
                    string description = ReadString();
                    if (roomData.ExtraDescr.ContainsKey(keywords))
                        Logger.LogError("ParseRooms: room [vnum:{vnum}] Extra desc '{keywords}' already exists", vnum, keywords);
                    else
                        roomData.ExtraDescr.Add(keywords, description);
                }
                else if (letter == 'O')
                {
                    if (!string.IsNullOrWhiteSpace(roomData.Owner))
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
                    roomData.TimeBetweenRepop = (int)ReadNumber();
                    roomData.TimeBetweenRepopPeople = (int)ReadNumber();
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
            Logger.LogTrace("Room [{vnum}] parsed", vnum);

            // Save room data
            _rooms.Add(roomData);
        }
    }

    private void ParseShops()
    {
        Logger.LogTrace("Shops section");

        while (true)
        {
            int keeper = (int)ReadNumber();
            if (keeper == 0)
                break; // done
            var mobileData = _mobiles.FirstOrDefault(x => x.VNum == keeper);
            if (mobileData == null)
                RaiseParseException("ParseShops: unknown mobile vnum {0}", keeper);
            else
            {
                ShopData shopData = new()
                {
                    Keeper = keeper
                };
                for (int i = 0; i < ShopData.MaxTrades; i++)
                    shopData.BuyType[i] = (int)ReadNumber();
                shopData.ProfitBuy = (int)ReadNumber();
                shopData.ProfitSell = (int)ReadNumber();
                shopData.OpenHour = (int)ReadNumber();
                shopData.CloseHour = (int)ReadNumber();
                mobileData.Shop = shopData;

                Logger.LogTrace("Shop [{vnum}] parsed", keeper);
            }
            ReadToEol();
        }
    }

    private void ParseSpecials()
    {
        Logger.LogTrace("Specials section");

        while (true)
        {
            char letter = ReadLetter();

            if (letter == 'S') // done
                break;
            else if (letter == '*')
#pragma warning disable CS0642 // Possible mistaken empty statement
                ; // nop
#pragma warning restore CS0642 // Possible mistaken empty statement
            else if (letter == 'M')
            {
                int vnum = (int)ReadNumber();
                string special = ReadWord();
                var mobileData = _mobiles.FirstOrDefault(x => x.VNum == vnum);
                if (mobileData != null)
                    mobileData.Special = special;
                else
                    Warn("ParseSpecials: 'M' unknown mobile vnum {0}", vnum);
                Logger.LogTrace("Specials [{vnum}] parsed", vnum);
            }
            else
                RaiseParseException("ParseSpecials: letter {0} not *MS", letter);
            ReadToEol();
        }
    }

    private static string TrimOldstylePrefix(string s)
    {
        if (s.StartsWith("oldstyle"))
            return s["oldstyle".Length..].TrimStart();
        return s;
    }
}
