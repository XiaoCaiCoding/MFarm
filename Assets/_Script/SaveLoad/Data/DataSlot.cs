using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Transition;

namespace MFarm.Save
{
    public class DataSlot
    {
        /// <summary>
        /// ��������String��GUID
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
                    return timeData.timeDict["gameYear"] + "��/" + (Season)timeData.timeDict["gameSeason"] + "/" + timeData.timeDict["gameMonth"] + "��/" + timeData.timeDict["gameDay"] + "��/";
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
                    //TODO: �����г����ĸ�����������
                    return transitionData.dataSceneName switch
                    {
                        "01.Field" => "����",
                        "02.Home" => "��ů�ļ�",
                        "03.SquareIsland" => "���뵺��",
                        
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

