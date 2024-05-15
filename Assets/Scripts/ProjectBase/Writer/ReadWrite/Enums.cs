public enum ItemType 
{
    material = 1,bullet = 2, prop = 4,
    ship = 8,ChopTool = 16,BreakTool = 32,ReapTool = 64,WaterTool = 128,CollectTool = 256,
    weapon = 512, equip = 1024, jewelry = 2048,
    ReapableScenery = 4096, collection = 8192,

}
public enum SlotType 
{
    Bag,Box,Shop
}

public enum InventoryLocation 
{
    Player,Box

}

public enum PartType 
{
    None,Carry,Hoe,Break
}

public enum Partname 
{
    Body,Hair,Arm,Tool
}

public enum Season 
{
春天,夏天,秋天,冬天

}

public enum DirectionEnum
{
    up = 1,
    down = 2,
    left = 4,
    right = 8,
}

public enum AtkType
{
    sharp = 1,
    weight = 2,
    fire = 4,
    water = 8,
    metals = 16,
    wood = 32,
    soil = 64,
    atk = 128,
    cure = 256,
}

public enum EntityType
{
    Player,Enemy
}

public enum E_EventTouchType
{
    Mine, Fight, Emergencies
}
public enum E_ResourceType
{
    n0, n1, n2, n3, n4, n5, n6, n7, n8, n9
}

public enum E_EventQuality
{
    ordinary, elite, epic, legend
}