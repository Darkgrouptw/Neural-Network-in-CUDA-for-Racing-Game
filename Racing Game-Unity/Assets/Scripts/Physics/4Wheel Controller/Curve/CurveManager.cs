using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 引擎 Torque 的曲線
public class CurveManager : MonoBehaviour
{
    public AnimationCurve[] gear = new AnimationCurve[5];
    public AnimationCurve LongForceCurve;
    public AnimationCurve LateralForceCurve;
    public AnimationCurve GyroScale;

    public GUIStyle style;
    // 畫 Canvas 上的點
    // 0,1 => FL Long Force, Slip Ratio
    // 2,3 => FR Long Force, Slip Ratio
    // 4,5 => RR Long Force, Slip Ratio
    // 6,7 => RL Long Force, Slip Ratio
    public CurveCanvas[] CurveArray;   
 
    void Awake()
    {
        CurveArray = new CurveCanvas[9];
        for (int i = 0; i < 8; i++)
            if(i % 2 == 0)
                CurveArray[i] = new CurveCanvas(25000, 0, 0, 1, 0.2f, 5000);
            else
                CurveArray[i] = new CurveCanvas(25000, 0, 0, 1, 0.2f, 5000);
        CurveArray[8] = new CurveCanvas(1, -1, 0, 10, 0.2f, 0.2f);
    }

    void Update()
    {
        // 如果按下 S ，可以把 Curve儲存起來
        if (Input.GetKeyDown(KeyCode.S))
            this.GetComponent<CurveManager>().SaveAll();
        if (Input.GetKeyDown(KeyCode.C))
            for (int i = 0; i < CurveArray.Length; i++)
                CurveArray[i].ClearAllPoint();
    }

    public void SaveAll()
    {
        for (int i = 0; i < CurveArray.Length; i++)
            CurveArray[i].Save("D:/Curve " + i + ".png");
        Debug.Log("Curve 的曲線以儲存!! (D:/)");
    }

    public float GetMaxSlip()
    {
        float result = -1;
        float MaxValue = -1;
        foreach (var item in this.LongForceCurve.keys)
        {
            if(MaxValue < 0 || MaxValue < item.value)
            {
                result = item.time;
            }
        }
        return result;
    }
    public float GetMaxAngle()
    {
        float result = -1;
        float MaxValue = -1;
        foreach (var item in this.LateralForceCurve.keys)
        {
            if (MaxValue < 0 || MaxValue < item.value)
            {
                result = item.time;
            }
        }
        return result;
    }
}
