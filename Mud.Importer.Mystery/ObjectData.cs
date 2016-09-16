using System.Collections.Generic;

namespace Mud.Importer.Mystery
{
    public class ObjectData
    {
        public int VNum { get; set; }
        public string Name { get; set; }
        public string ShortDescr { get; set; }
        public string Description { get; set; }
        public string Material { get; set; } // lookup table
        public string ItemType { get; set; } // lookup table
        public long ExtraFlags { get; set; } // flags
        public long WearFlags { get; set; } // flags
        public object[] Values { get; set; } // should be long
        public int Level { get; set; }
        public int Weight { get; set; }
        public long Cost { get; set; }
        public char Condition { get; set; } // lookup table
        public string Size { get; set; } // lookup table
        public Dictionary<string, string> ExtraDescr { get; set; } // keyword -> description
        public string Program { get; set; } // Obj program name

        public ObjectData()
        {
            Values = new object[5];
            ExtraDescr = new Dictionary<string, string>();
        }
    }

    // Condition table
    //case ('P') :		pObjIndex->condition = 100; break;
    //case ('G') :		pObjIndex->condition =  90; break;
    //case ('A') :		pObjIndex->condition =  75; break;
    //case ('W') :		pObjIndex->condition =  50; break;
    //case ('D') :		pObjIndex->condition =  25; break;
    //case ('B') :		pObjIndex->condition =  10; break;
    //case ('R') :		pObjIndex->condition =   0; break;
    //default:			pObjIndex->condition = 100; break;

    // ItemType table
    //#define ITEM_LIGHT		      1
    //#define ITEM_SCROLL		      2
    //#define ITEM_WAND		      3
    //#define ITEM_STAFF		      4
    //#define ITEM_WEAPON		      5
    //#define ITEM_TREASURE		      8
    //#define ITEM_ARMOR		      9
    //#define ITEM_POTION		     10
    //#define ITEM_CLOTHING		     11
    //#define ITEM_FURNITURE		     12
    //#define ITEM_TRASH		     13
    //#define ITEM_CONTAINER		     15
    //#define ITEM_DRINK_CON		     17
    //#define ITEM_KEY		     18
    //#define ITEM_FOOD		     19
    //#define ITEM_MONEY		     20
    //#define ITEM_BOAT		     22
    //#define ITEM_CORPSE_NPC		     23
    //#define ITEM_CORPSE_PC		     24
    //#define ITEM_FOUNTAIN		     25
    //#define ITEM_PILL		     26
    //#define ITEM_MAP		     28
    //#define ITEM_PORTAL		     29
    //#define ITEM_WARP_STONE		     30
    //#define ITEM_COMPONENT               31
    //#define ITEM_GEM		     32
    //#define ITEM_JEWELRY		     33
    //#define ITEM_INSTRUMENT              35
    //#define ITEM_WINDOW                  37
    //#define ITEM_TEMPLATE                40
    //#define ITEM_SADDLE                  41
    //#define ITEM_ROPE                    42
  //{	ITEM_LIGHT,	"light"		},
  //{	ITEM_SCROLL,	"scroll"	},
  //{	ITEM_WAND,	"wand"		},
  //{     ITEM_STAFF,	"staff"		},
  //{     ITEM_WEAPON,	"weapon"	},
  //{     ITEM_TREASURE,	"treasure"	},
  //{     ITEM_ARMOR,	"armor"		},
  //{	ITEM_POTION,	"potion"	},
  //{	ITEM_CLOTHING,	"clothing"	},
  //{     ITEM_FURNITURE,	"furniture"	},
  //{	ITEM_TRASH,	"trash"		},
  //{	ITEM_CONTAINER,	"container"	},
  //{	ITEM_DRINK_CON, "drink"		},
  //{	ITEM_KEY,	"key"		},
  //{	ITEM_FOOD,	"food"		},
  //{	ITEM_MONEY,	"money"		},
  //{	ITEM_BOAT,	"boat"		},
  //{	ITEM_CORPSE_NPC,"npc_corpse"	},
  //{	ITEM_CORPSE_PC,	"pc_corpse"	},
  //{     ITEM_FOUNTAIN,	"fountain"	},
  //{	ITEM_PILL,	"pill"		},
  //{	ITEM_MAP,	"map"		},
  //{	ITEM_PORTAL,	"portal"	},
  //{	ITEM_WARP_STONE,"warp_stone"	},
  //{     ITEM_COMPONENT, "component"     },
  //{	ITEM_GEM,	"gem"		},
  //{	ITEM_JEWELRY,	"jewelry"	},
  //{     ITEM_INSTRUMENT, "instrument"   },
  //{     ITEM_WINDOW,     "window"       },
  //{     ITEM_TEMPLATE,   "template"     },
  //{     ITEM_SADDLE,     "saddle"       },
  //{     ITEM_ROPE,       "rope"         },
}
