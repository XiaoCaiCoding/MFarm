using UnityEngine;
using System;

public class Settings
{
    public const float itemFadeDuration = 0.35f;
    public const float targetAlpha = 0.45f;

    //ʱ�����
    public const float secondThreshold = 0.1f;     //��ֵԽСʱ��Խ��
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 30;
    public const int seasonHold = 3;

    public const float fadeDuration = 2f;

    public const int reapCount = 2;
    //NPC�����ƶ�
    public const int gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    public const float pixelSize = 0.05f; //20*20 ռ 1Unit

    public const float animationBreakTime = 5f;

    public const int maxGridSize = 9999;

    //�ƹ�
    public const float lightChangeDuration = 25f;
    public static TimeSpan morningTime = new TimeSpan(5, 0, 0);
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);

    public static Vector3 playerStartPos = new Vector3(-58f, 12f, 0f);
    public const int playerStartMoney = 100;
}
