using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RayData
{
    public float leftHitDis;
    public float leftfrontHitDis;
    public float frontHitDis;
    public float rightfrontHitDis;
    public float rightHitDis;
};

public class NeuralNetworkCarController : MonoBehaviour
{
    void Update ()
    {
       
    }

    /// <summary>
    /// 打 Ray 看一下
    /// </summary>
    /// <returns></returns>
    private RayData RayCastData()
    {
        RayData data = new RayData();
        RaycastHit leftHit, leftfrontHit, frontHit, rightfrontHit, rightHit;

        #region 左
        Vector3 leftDir = this.transform.parent.right * -1;
        Ray leftRay = new Ray(this.transform.position, leftDir);
        if (Physics.Raycast(leftRay, out leftHit, 10f))
        {
            if (leftHit.collider.tag != "Player")
                data.leftHitDis = Vector3.Distance(this.transform.position, leftHit.point) / 10f;
        }
        else
            data.leftHitDis = 1;
        #endregion
        #region 左前
        Vector3 leftfrontDir = Vector3.Lerp(this.transform.parent.right * -1, this.transform.parent.forward, 0.5f);
        Ray leftfrontRay = new Ray(this.transform.position, leftfrontDir);
        if (Physics.Raycast(leftfrontRay, out leftfrontHit, 10f))
        { 
            if (leftfrontHit.collider.tag != "Player")
                data.leftfrontHitDis = Vector3.Distance(this.transform.position, leftfrontHit.point) / 10f;
        }
        else
            data.leftfrontHitDis = 1;
        #endregion
        #region 前
        Vector3 frontDir = this.transform.parent.forward;
        Ray frontRay = new Ray(this.transform.position, frontDir);
        if (Physics.Raycast(frontRay, out frontHit, 10f))
        {
            if (frontHit.collider.tag != "Player")
                data.frontHitDis = Vector3.Distance(this.transform.position, frontHit.point) / 10f;
        }
        else
            data.frontHitDis = 1;
        #endregion
        #region 右前
        Vector3 rightfrontDir = Vector3.Lerp(this.transform.parent.forward, this.transform.parent.right, 0.5f);
        Ray rightfrontRay = new Ray(this.transform.position, rightfrontDir);
        if (Physics.Raycast(rightfrontRay, out rightfrontHit, 10f))
        {
            if (rightfrontHit.collider.tag != "Player")
                data.rightfrontHitDis = Vector3.Distance(this.transform.position, rightfrontHit.point) / 10f;
        }
        else
            data.rightfrontHitDis = 1;
        #endregion
        #region 右
        Vector3 rightDir = this.transform.parent.right;
        Ray rightRay = new Ray(this.transform.position, rightDir);
        if (Physics.Raycast(rightRay, out rightHit, 10f))
        {
            if (rightHit.collider.tag != "Player")
                data.rightHitDis = Vector3.Distance(this.transform.position, rightHit.point) / 10f;
        }
        else
            data.rightHitDis = 1;
        #endregion
        return data; 
    }
}
