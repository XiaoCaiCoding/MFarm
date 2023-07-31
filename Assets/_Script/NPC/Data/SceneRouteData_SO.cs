using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SceneRouteData_SO",menuName = "Map/SceneRouteData")]
public class SceneRouteData_SO : ScriptableObject
{
    public List<SceneRoute> sceneRouteList;
}
