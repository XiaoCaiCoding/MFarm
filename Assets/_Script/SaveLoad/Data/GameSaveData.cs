using System.Collections.Generic;
namespace MFarm.Save
{
    [System.Serializable]
    public class GameSaveData
    {
        public string dataSceneName;

        public Dictionary<string, SerializableVector3> characterPosDict;
        public Dictionary<string, List<SceneItem>> sceneItemDict;
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDict;
        public Dictionary<string, TileDetails> tileDetailsDict;
        public Dictionary<string, bool> firstLoadDict;
        public Dictionary<string, List<InventoryItem>> inventoryDict;

        public Dictionary<string, int> timeDict;

        public int playerMoney;

        //NPC
        public string targetScene;
        public bool interactable;
        public int animationInstanceID;
    }
}

