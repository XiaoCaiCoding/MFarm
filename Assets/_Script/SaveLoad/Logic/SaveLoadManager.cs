using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Newtonsoft.Json;
using System.IO;

namespace MFarm.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        public List<ISaveable> saveableList = new List<ISaveable>();
        public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);

        private string jsonFolder;
        private int currentDataIndex;
        protected override void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/SAVE DATA/";
            ReadSaveData();
        }
        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }
        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
                Save(currentDataIndex);
            if (Input.GetKeyDown(KeyCode.O))
                Load(currentDataIndex);
        }
        private void OnStartNewGameEvent(int index)
        {
            currentDataIndex = index;
        }
        private void OnEndGameEvent()
        {
            Save(currentDataIndex);
        }

        public void RegisterSaveable(ISaveable saveable)
        {
            if (!saveableList.Contains(saveable))
                saveableList.Add(saveable);
        }
        private void ReadSaveData()
        {
            if (Directory.Exists(jsonFolder))
            {
                for (int i = 0; i < dataSlots.Count; i++)
                {
                    var resultPath = jsonFolder + "data" + i.ToString("00") + ".json";
                    if (File.Exists(resultPath))
                    {
                        var stringData = File.ReadAllText(resultPath);
                        var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                        dataSlots[i] = jsonData;
                    }
                }
            }
        }
        public void Save(int index)
        {
            DataSlot data = new DataSlot();
            foreach (var saveable in saveableList)
            {
                data.dataDict.Add(saveable.GUID, saveable.GenerateGameSaveData());
            }
            dataSlots[index] = data;

            var resultPath = jsonFolder + "data" + index.ToString("00") + ".json";
            //var jsonData = JsonMapper.ToJson(dataSlots[index]);

            var jsonData = JsonConvert.SerializeObject(dataSlots[index], Formatting.Indented);
            if (!File.Exists(resultPath))
            {
                Directory.CreateDirectory(jsonFolder);
            }
            Debug.Log("Data" + index + "Saved!!");
            File.WriteAllText(resultPath, jsonData);
        }
        public void Load(int index)
        {
            currentDataIndex = index;

            var resultPath = jsonFolder + "data" + index.ToString("00") + ".json";

            var stringData = File.ReadAllText(resultPath);

            //TODO: 储存没问题，但解析失败，SerializeableVector3不知道类型
            //var jsonData = JsonMapper.ToObject<DataSlot>(stringData);

            //只能用newtonSoft.Json
            var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
            foreach (var saveable in saveableList)
            {
                saveable.RestoreGameData(jsonData.dataDict[saveable.GUID]);
            }
        }
    }
}

