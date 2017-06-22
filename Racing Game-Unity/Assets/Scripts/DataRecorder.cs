using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
public class DataRecorder : MonoBehaviour {

    [Header("DataRecorder")]
    [Tooltip("Set the car controller here")]
    public CarController carController;
    public int curFrame;
    
    public struct Data
    {
        public int frameNum; //1
        public float leftHitDis, leftfrontHitDis, frontHitDis, rightfrontHitDis, rightHitDis; //2-6
        public float steering; //7
        public float throttle; //8
        public float brake; //9
    };
    Data curData;
    public List<Data> data;

    private Vector3 leftDir, leftfrontDir, frontDir, rightfrontDir, rightDir;
    private RaycastHit leftHit, leftfrontHit, frontHit, rightfrontHit, rightHit;
    private const int RayLength = 200;

    // Use this for initialization
    void Start () {
        data = new List<Data>();
        curFrame = 1;
    }
	
	// Update is called once per frame
	void Update () {
        #region initializing
        curData = new Data();
        #endregion
        #region RayCast
        //left
        leftDir = this.transform.parent.right * -1;
        Ray leftRay = new Ray( this.transform.position, leftDir);
        if (Physics.Raycast(leftRay, out leftHit, RayLength))
        {
            if (leftHit.collider.tag != "Player") {
                //set data
                Debug.DrawLine(this.transform.position, this.transform.position + leftDir * RayLength, Color.blue, Time.deltaTime, true);
                curData.leftHitDis = Vector3.Distance(this.transform.position, leftHit.point) / RayLength;
            }
            else { Debug.DrawLine(this.transform.position, this.transform.position + leftDir * RayLength, Color.red, Time.deltaTime, true); }
        }
        else { Debug.DrawLine(this.transform.position, this.transform.position + leftDir * RayLength, Color.green, Time.deltaTime, true); }

        //left front
        leftfrontDir = Vector3.Lerp(this.transform.parent.right * -1, this.transform.parent.forward, 0.5f);;
        Ray leftfrontRay = new Ray(this.transform.position, leftfrontDir);
        if (Physics.Raycast(leftfrontRay, out leftfrontHit, RayLength))
        {
            if (leftfrontHit.collider.tag != "Player") {
                //set data
                Debug.DrawLine(this.transform.position, this.transform.position + leftfrontDir * RayLength, Color.blue, Time.deltaTime, true);
                curData.leftfrontHitDis = Vector3.Distance(this.transform.position, leftfrontHit.point) / RayLength;
            }
            else { Debug.DrawLine(this.transform.position, this.transform.position + leftfrontDir * RayLength, Color.red, Time.deltaTime, true); }
        }
        else { Debug.DrawLine(this.transform.position, this.transform.position + leftfrontDir * RayLength, Color.green, Time.deltaTime, true); }

        //front
        frontDir = this.transform.parent.forward;
        Ray frontRay = new Ray(this.transform.position, frontDir);
        if (Physics.Raycast(frontRay, out frontHit, RayLength))
        {
            if (frontHit.collider.tag != "Player") {
                //set data
                Debug.DrawLine(this.transform.position, this.transform.position + frontDir * RayLength, Color.blue, Time.deltaTime, true);
                curData.frontHitDis = Vector3.Distance(this.transform.position, frontHit.point) / RayLength;
            }
            else { Debug.DrawLine(this.transform.position, this.transform.position + frontDir * RayLength, Color.red, Time.deltaTime, true); }
        }
        else { Debug.DrawLine(this.transform.position, this.transform.position + frontDir * RayLength, Color.green, Time.deltaTime, true); }

        //right front
        rightfrontDir = Vector3.Lerp(this.transform.parent.forward, this.transform.parent.right, 0.5f);
        Ray rightfrontRay = new Ray(this.transform.position, rightfrontDir);
        if (Physics.Raycast(rightfrontRay, out rightfrontHit, RayLength))
        {
            if (rightfrontHit.collider.tag != "Player") {
                //set data
                Debug.DrawLine(this.transform.position, this.transform.position + rightfrontDir * RayLength, Color.blue, Time.deltaTime, true);
                curData.rightfrontHitDis = Vector3.Distance(this.transform.position, rightfrontHit.point) / RayLength;
            }
            else { Debug.DrawLine(this.transform.position, this.transform.position + rightfrontDir * RayLength, Color.red, Time.deltaTime, true); }
        }
        else { Debug.DrawLine(this.transform.position, this.transform.position + rightfrontDir * RayLength, Color.green, Time.deltaTime, true); }

        //right
        rightDir = this.transform.parent.right;
        Ray rightRay = new Ray(this.transform.position, rightDir);
        if (Physics.Raycast(rightRay, out rightHit, RayLength))
        {
            if (rightHit.collider.tag != "Player") {
                //set data
                Debug.DrawLine(this.transform.position, this.transform.position + rightDir * RayLength, Color.blue, Time.deltaTime, true);
                curData.rightHitDis = Vector3.Distance(this.transform.position, rightHit.point) / RayLength;
            }
            else { Debug.DrawLine(this.transform.position, this.transform.position + rightDir * RayLength, Color.red, Time.deltaTime, true); }
        }
        else { Debug.DrawLine(this.transform.position, this.transform.position + rightDir * RayLength, Color.green, Time.deltaTime, true); }

        #endregion
        #region Data
        // 轉換
        curData.leftHitDis = (curData.leftHitDis == 0) ? 1 : curData.leftHitDis;
        curData.leftfrontHitDis = (curData.leftfrontHitDis == 0) ? 1 : curData.leftfrontHitDis;
        curData.frontHitDis = (curData.frontHitDis == 0) ? 1 : curData.frontHitDis;
        curData.rightfrontHitDis = (curData.rightfrontHitDis == 0) ? 1 : curData.rightfrontHitDis;
        curData.rightHitDis = (curData.rightHitDis == 0) ? 1 : curData.rightHitDis;

        curData.frameNum = curFrame;
        curData.steering = carController.steering;
        curData.throttle = carController.throttle;
        curData.brake = carController.brake;

        if(carController.steering != 0 || carController.brake != 0 || carController.throttle != 0)
        {
            data.Add(curData);
            curFrame++;
        }
        #endregion
    }

    /// <summary>
    /// Write output record
    /// </summary>
    public void writeFile()
    {
        string filePath = "./Saved_data.csv";
        string delimiter = ",";
        string Header = "Number,leftHitDis,leftfrontHitDis,frontHitDis,rightfrontHitDis,rightHitDis,steering,throttle,brake\n";
        StringBuilder sBuilder = new StringBuilder();
        for(int i=0; i < data.Count; i++)
        {
            Data output = data[i];
            string[] line = new string[] { output.frameNum.ToString(),
                output.leftHitDis.ToString(),
                output.leftfrontHitDis.ToString(),
                output.frontHitDis.ToString(),
                output.rightfrontHitDis.ToString(),
                output.rightHitDis.ToString(),
                output.steering.ToString(),
                output.throttle.ToString(),
                output.brake.ToString()
            };
            sBuilder.AppendLine(string.Join(delimiter, line));
        }
        File.WriteAllText(filePath, Header + sBuilder.ToString());
    }
    /// <summary>
    /// Reset frame state and clear data
    /// </summary>
    public void reset()
    {
        curFrame = 1;
        data = new List<Data>();
    }


}
