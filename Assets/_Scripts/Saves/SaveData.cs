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
    public int kickStrength;
}

[Serializable]
public class GameManagerSaveData
{
    public int baconCount;
    public int baconTotal;
    public int pigsMax;
    public int kickedPigs;
    public int angelKills;

    public float SSK;
    public float SPK;

    public int totalScore;
    public int newGamePlus;

}

[Serializable]
public class ShopSaveData
{
    public bool goldenPigBought;
    public bool alienBought;
    public bool cyborgBought;
    public bool hasTech;
    public bool suckoBought;

    public int permitCost;
    public int troughCost;
    public int patchCost;
    public int goldPatchCost;
    public int kickCost;

    public bool devilButtonActive;
    public string devilCostText;

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

}
