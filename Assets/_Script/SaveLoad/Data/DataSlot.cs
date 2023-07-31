using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Transition;

namespace MFarm.Save
{
    public class DataSlot
    {
        /// <summary>
        /// 进度条，String是GUID
        /// </summary>
        public Dictionary<string, GameSaveData> dataDict = new Dictionary<string, GameSaveData>();

        #region
        public string DataTime
        {
            get
            {
                var key = TimeManager.Instance.GUID;

                if (dataDict.ContainsKey(key))
                {
                    var timeData = dataDict[key];
                    return timeData.timeDict["gameYear"] + "年/" + (Season)timeData.timeDict["gameSeason"] + "/" + timeData.timeDict["gameMonth"] + "月/" + timeData.timeDict["gameDay"] + "日/";
                }
                else
                    return string.Empty;
            }
        }
        public string DataScene
        {
            get
            {
                var key = TransitionManager.Instance.GUID;

                if (dataDict.ContainsKey(key))
                {
                    var transitionData = dataDict[key];
                    //TODO: 将所有场景改个好听的名称
                    return transitionData.dataSceneName switch
                    {
                        "01.Field" => "海岛",
                        "02.Home" => "温暖的家",
                        "03.SquareIsland" => "中央岛屿",
                        
                        _ => string.Empty,
                    };
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #endregion
    }
}

