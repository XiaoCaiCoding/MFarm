using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class LightControl : MonoBehaviour
{
    public LightPattenList_SO lightData;
    private Light2D currentLight;
    private LightDetails currentLightDetails;

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
    }

    //Êµ¼ÊÇÐ»»µÆ¹â
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        currentLightDetails = lightData.GetLightDetails(season, lightShift);

        if(timeDifference < Settings.lightChangeDuration)
        {
            var colorOffset = (currentLightDetails.lightColor - currentLight.color) / Settings.lightChangeDuration * timeDifference;
            currentLight.color += colorOffset;
            DOTween.To(() => currentLight.color, c => currentLight.color = c, currentLightDetails.lightColor, Settings.lightChangeDuration - timeDifference);
            DOTween.To(() => currentLight.intensity, i => currentLight.intensity = i, currentLightDetails.lightAmount, Settings.lightChangeDuration - timeDifference);
        }
        else
        {
            currentLight.color = currentLightDetails.lightColor;
            currentLight.intensity = currentLightDetails.lightAmount;
        }
    }
}
