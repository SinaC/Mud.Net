﻿using Mud.Logger;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mud.Importer.Rot
{
    public class RotLoader : TextBasedLoader
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
        private const string MobilesHeader = "MOBILES";
        private const string ObjectsHeader = "OBJECTS";
        private const string ResetsHeader = "RESETS";
        private const string RoomsHeader = "ROOMS";
        private const string ShopsHeader = "SHOPS";
        private const string SpecialsHeader = "SPECIALS";
        private const string MobProgsHeader = "MOBPROGS";

        private readonly List<AreaData> _areas;
        private readonly List<MobileData> _mobiles;
        private readonly List<ObjectData> _objects;
        private readonly List<RoomData> _rooms;

        private int _lastAreaVnum;

        public IReadOnlyCollection<AreaData> Areas => _areas.AsReadOnly();

        public IReadOnlyCollection<MobileData> Mobiles => _mobiles.AsReadOnly();

        public IReadOnlyCollection<ObjectData> Objects => _objects.AsReadOnly();

        public IReadOnlyCollection<RoomData> Rooms => _rooms.AsReadOnly();

        public RotLoader()
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
                if (word == AreaDataHeader)
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
                else if (word == MobProgsHeader)
                    ParseMobProgs();
                else if (word.ToUpper() == "HELPS")
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Specific area file Helps not yet available");
                    ReadToNextSection();
                }
                else if (word.ToUpper() == "SOCIALS")
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Specific area file Socials not yet available");
                    break;
                }
                else if (word.ToUpper() == "DREAMS")
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Specific '{0}' not handled", word);
                    ReadToNextSection();
                }
                else
                    RaiseParseException("Bad section name: {0}", word);
            }
        }

        private void ParseArea()
        {
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
            Log.Default.WriteLine(LogLevels.Trace, "Area [{0}] parsed", area.Name);

            // Set unique number and filename
            area.VNum = 10 + _areas.Count;
            area.FileName = CurrentFilename;
            // Save area
            _areas.Add(area);
            //
            _lastAreaVnum = area.VNum;
        }

        private void ParseMobiles()
        {
            Log.Default.WriteLine(LogLevels.Trace, "Mobiles section");

            while (true)
            {
                char letter = ReadLetter();
                if (letter != '#')
                    RaiseParseException("ParseMobiles: # not found");

                int vnum = (int)ReadNumber();
                if (vnum == 0)
                    break; // parsed


                if (_mobiles.Any(x => x.VNum == vnum))
                    RaiseParseException("ParseMobiles: vnum {0} duplicated", vnum);

                MobileData mobileData = new MobileData
                {
                    VNum = vnum,
                    PlayerName = ReadString(),
                    ShortDescr = ReadString(),
                    LongDescr = UpperCaseFirst(ReadString()),
                    Description = UpperCaseFirst(ReadString()),
                    Race = ReadString(),
                    Act = ReadFlags(),
                    Act2 = ReadFlags(),
                    AffectedBy = ReadFlags(),
                    ShieldedBy = ReadFlags(),
                    Alignment = (int)ReadNumber(),
                    Group = (int)ReadNumber(),
                    Level = (int)ReadNumber(),
                    HitRoll = (int)ReadNumber(),
                };
                ReadDice(mobileData.Hit);
                ReadDice(mobileData.Mana);
                ReadDice(mobileData.Dam);
                mobileData.DamType = ReadWord();
                mobileData.Armor[0] = (int)ReadNumber();
                mobileData.Armor[1] = (int)ReadNumber();
                mobileData.Armor[2] = (int)ReadNumber();
                mobileData.Armor[3] = (int)ReadNumber();
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
                        mobileData.DieDescr = ReadString();
                    }
                    else if (letter == 'T')
                    {
                        mobileData.SayDescr = UpperCaseFirst(ReadString());
                    }
                    else if (letter == 'M')
                    {
                        MobProg mobProg = new MobProg
                        {
                            TrigType = ReadWord(),
                            VNum = (int)ReadNumber(),
                            TrigPhrase = ReadString(),
                        };
                        mobileData.MobProgs.Add(mobProg);
                    }
                    else if (letter == 'C')
                    {
                        mobileData.Clan = ReadString();
                    }
                    else
                    {
                        UngetChar(letter);
                        break;
                    }
                }

                Log.Default.WriteLine(LogLevels.Trace, "Mobile [{0}] parsed", vnum);
                Debug.WriteLine("Mobile [{0}] parsed", vnum);

                // Save mobile data
                _mobiles.Add(mobileData);
            }
        }

        private void ParseObjects()
        {
            Log.Default.WriteLine(LogLevels.Trace, "Objects section");

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

                ObjectData objectData = new ObjectData
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
                    case "pit":
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
                        objectData.Values[4] = ReadNumber();
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
                        ObjectAffect aff = new ObjectAffect
                        {
                            Where = ObjectAffect.WhereToObject,
                            Type = -1,
                            Level = objectData.Level,
                            Duration = -1,
                            Location = (int)ReadNumber(),
                            Modifier = (int)ReadNumber(),
                            BitVector = 0,
                        };
                    }
                    else if (letter == 'F')
                    {
                        char where = ReadLetter();
                        ObjectAffect aff = new ObjectAffect
                        {
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
                            case 'S': aff.Where = ObjectAffect.WhereToShields; break;
                            default:
                                Log.Default.WriteLine(LogLevels.Error, "ParseObjects: item [vnum:{0}] Invalid affect where '{1}'", vnum, where);
                                break;
                        }
                    }
                    else if (letter == 'E')
                    {
                        string keyword = ReadString();
                        string description = ReadString();
                        if (objectData.ExtraDescr.ContainsKey(keyword))
                            Log.Default.WriteLine(LogLevels.Error, "ParseObjects: item [vnum:{0}] Extra desc '{1}' already exists", vnum, keyword);
                        else
                            objectData.ExtraDescr.Add(keyword, description);
                    }
                    else if (letter == 'C')
                    {
                        objectData.Clan = ReadString();
                        if (objectData.ItemType == "armor")
                        {
                            objectData.Values[0] = 0;
                            objectData.Values[1] = 0;
                            objectData.Values[2] = 0;
                            objectData.Values[3] = 0;
                            objectData.Level = 1;
                            objectData.Cost = 0;
                        }
                        else if (objectData.ItemType == "weapon")
                        {
                            objectData.Values[1] = 0;
                            objectData.Values[2] = 0;
                            objectData.Level = 1;
                            objectData.Cost = 0;
                        }
                    }
                    else if (letter == 'G')
                    {
                        objectData.Guild = ReadString();
                    }
                    else
                    {
                        UngetChar(letter);
                        break;
                    }
                }

                Log.Default.WriteLine(LogLevels.Trace, "Object [{0}] parsed", vnum);
                Debug.WriteLine("Object [{0}] parsed", vnum);

                // Save object data
                _objects.Add(objectData);
            }
        }

        private void ParseResets()
        {
            Log.Default.WriteLine(LogLevels.Trace, "Resets section");

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
                        if (arg2 < 0 || arg2 >= RoomData.MaxDir || roomData.Exits[arg2] == null)
                            RaiseParseException("ParseResets: 'D': exit {0} not door", arg2);
                        else
                        {
                            if (arg3 == 0)
                                ; // NOP
                            else if (arg3 == 1)
                            {
                                roomData.Exits[arg2].ExitInfo |= ExitData.ExClosed;
                                roomData.Exits[arg2].RsFlags |= ExitData.ExClosed;
                            }
                            else if (arg3 == 2)
                            {
                                roomData.Exits[arg2].ExitInfo |= ExitData.ExClosed | ExitData.ExLocked;
                                roomData.Exits[arg2].RsFlags |= ExitData.ExClosed | ExitData.ExLocked;
                            }
                            else
                                Warn("ParseResets: 'D': bad 'locks': {0}", arg3);
                        }
                        // ResetData is not stored
                    }
                }
                else if (letter == 'R')
                {
                    if (arg2 < 0 || arg2 > RoomData.MaxDir)
                        RaiseParseException("ParseResets: 'R': exit {0} not door", arg2);
                    RoomData roomData = _rooms.FirstOrDefault(x => x.VNum == arg1);
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
            Log.Default.WriteLine(LogLevels.Trace, "Rooms section");

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

                RoomData roomData = new RoomData
                {
                    AreaVnum = _lastAreaVnum,
                    VNum = vnum,
                    Name = ReadString(),
                    Description = ReadString()
                };
                ReadNumber(); // area number not used
                roomData.RoomFlags = ReadFlags();
                roomData.SectorType = (int)ReadNumber();
                roomData.HealNeg = false;
                roomData.HealRate = 100;
                roomData.ManaRate = 100;

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
                    else if (letter == 'G')
                        roomData.Guild = ReadString();
                    else if (letter == 'R')
                        roomData.Race = ReadString();
                    else if (letter == 'D')
                    {
                        int door = (int)ReadNumber();
                        if (door < 0 || door >= RoomData.MaxDir)
                            RaiseParseException("ParseRooms: vnum {0} has bad door number", vnum);
                        ExitData exitData = new ExitData
                        {
                            Description = ReadString(),
                            Keyword = ReadString(),
                            ExitInfo = 0,
                            RsFlags = 0,
                        };
                        long locks = ReadFlags();
                        exitData.Key = (int)ReadNumber();
                        exitData.DestinationVNum = (int)ReadNumber();
                        switch (locks)
                        {
                            case 1:
                                exitData.ExitInfo = ExitData.ExDoor;
                                exitData.RsFlags = ExitData.ExDoor;
                                break;
                            case 2:
                                exitData.ExitInfo = ExitData.ExDoor | ExitData.ExPickproof;
                                exitData.RsFlags = ExitData.ExDoor | ExitData.ExPickproof;
                                break;
                            case 3:
                                exitData.ExitInfo = ExitData.ExDoor | ExitData.ExNoPass;
                                exitData.RsFlags = ExitData.ExDoor | ExitData.ExNoPass;
                                break;
                            case 4:
                                exitData.ExitInfo = ExitData.ExDoor | ExitData.ExNoPass | ExitData.ExPickproof;
                                exitData.RsFlags = ExitData.ExDoor | ExitData.ExNoPass | ExitData.ExPickproof;
                                break;
                        }
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
                        if (!string.IsNullOrWhiteSpace(roomData.Owner))
                            RaiseParseException("ParseRooms: vnum {0} has duplicate owner", vnum);
                        roomData.Owner = ReadString();
                    }
                    else if (letter == 'T')
                    {
                        ExitData exitData = new ExitData
                        {
                            Keyword = string.Empty,
                            Description = string.Empty,
                            Key = -1,
                            ExitInfo = ExitData.ExDoor,
                            DestinationVNum = (int)ReadNumber()
                        };
                        roomData.Transfer = exitData;
                    }
                    else if (letter == 'O')
                        roomData.Owner = ReadString();
                    else
                        RaiseParseException("ParseRooms: vnum {0} has unknown flag", vnum);
                }
                Log.Default.WriteLine(LogLevels.Trace, "Room [{0}] parsed", vnum);
                Debug.WriteLine("Room [{0}] parsed", vnum);

                // Save room data
                _rooms.Add(roomData);
            }
        }

        private void ParseShops()
        {
            Log.Default.WriteLine(LogLevels.Trace, "Shops section");

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
                    ShopData shopData = new ShopData
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

                    Log.Default.WriteLine(LogLevels.Trace, "Shop [{0}] parsed", keeper);
                }
                ReadToEol();
            }
        }

        private void ParseSpecials()
        {
            Log.Default.WriteLine(LogLevels.Trace, "Specials section");

            while (true)
            {
                char letter = ReadLetter();

                if (letter == 'S') // done
                    break;
                else if (letter == '*')
                    ; // nop
                else if (letter == 'M')
                {
                    int vnum = (int)ReadNumber();
                    string special = ReadWord();
                    MobileData mobileData = _mobiles.FirstOrDefault(x => x.VNum == vnum);
                    if (mobileData != null)
                        mobileData.Special = special;
                    else
                        Warn("ParseSpecials: 'M' unknown mobile vnum {0}", vnum);
                    Log.Default.WriteLine(LogLevels.Trace, "Specials [{0}] parsed", vnum);
                }
                else
                    RaiseParseException("ParseSpecials: letter {0} not *MS", letter);
                ReadToEol();
            }
        }

        private void ParseMobProgs()
        {
            while (true)
            {
                char letter = ReadLetter();
                if (letter != '#')
                    RaiseParseException("Parse: # not found");

                int vnum = (int)ReadNumber();
                if (vnum == 0)
                    break; // parsed

                //if (_mobProgs.Any(x => x.VNum == vnum)) // don't store mob programs than call FixMobProgrs, do it immediately
                //    RaiseParseException("ParseMobProgs: vnum {0} duplicated", vnum);

                bool found = false;
                string code = ReadString();
                // TODO
                //foreach (MobileData mobileData in _mobiles)
                //{
                //    foreach (MobProg mobProg in mobileData.MobProgs)
                //    {
                //        if (mobProg.VNum == vnum)
                //        {
                //            mobProg.Code = code;
                //            found = true;
                //        }
                //    }
                //}
                //if (!found)
                //    RaiseParseException("ParseMobProgs: vnum {0} was not found in mobile triggers", vnum);
            }
        }
    }
}
