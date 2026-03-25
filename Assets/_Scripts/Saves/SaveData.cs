using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public PlayerSaveData player;
    public GameManagerSaveData gameManager;
    public ShopSaveData shop;

    public float carrotPatchInterval;
    public float goldCarrotPatchInterval;
    public float passiveBaconInterval;

    public List<CrateSaveData> crates = new();
    public List<PigSaveData> pigs = new();
}

[Serializable]
public class PlayerSaveData
{
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
}

[Serializable]
public class GameManagerSaveData
{
    public int baconCount;
    public int baconTotal;
    public int pigsMax;
    public int kickedPigs;
    public int angelKills;
}

[Serializable]
public class ShopSaveData
{
    public bool goldenPigBought;
    public bool alienBought;
    public bool hasTech;

    public int permitCost;
    public int troughCost;
    public int patchCost;

    public bool dishActive;
    public bool ufoActive;
    public bool alienButtonActive;
    public bool cyborgButtonActive;
    public bool cyboargButtonActive;
    public bool angelButtonActive;
    
    public bool devilButtonActive;
    public string devilCostText;

    public bool carrotButtonActive;
    public bool patchButtonActive;
    public bool goldPatchButtonActive;
    public bool techButtonActive;

    public bool dishButtonInteractable;
    public bool permitButtonInteractable;
    public bool troughButtonInteractable;
    public bool patchButtonInteractable;
    public bool goldPatchButtonInteractable;
}

[Serializable]
public class CrateSaveData
{
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public int pigType;
}

[Serializable]
public class PigSaveData
{
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;

    public int hp;
    public int mhp;

    public int pigType;
    public bool beenKicked;

    // Optional if you need it later
    public int baconPerKick;
}
