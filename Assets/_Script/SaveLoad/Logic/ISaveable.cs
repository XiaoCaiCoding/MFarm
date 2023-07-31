
namespace MFarm.Save
{
    public interface ISaveable
    {
        string GUID { get; }
        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }
        GameSaveData GenerateGameSaveData();
        void RestoreGameData(GameSaveData gameSaveData);
    }
}