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

[RequireComponent(typeof(NeuralNetworkManager))]
public class NeuralNetworkCarController : MonoBehaviour
{
    // 把所有的輪子都放在這
    [Header("----- Basic Params -----")]
    public Wheel[] wheels;
    public int ForwardMaxSpeed = 300;
    public int BackwardMaxSpeed = 110;
    public int NitrousUpSpeed = 50;
    public static bool UseNitrous = false;

    public Transform centerOfMass;          // 車子的質心
    public float inertiaFactor = 1.5f;      // 車子的慣性


    public float brake;                            // 煞車
    public float throttle;                         // 油門
    float throttleInput;                    // 油門Input，只有按煞車會減
    float throttleForDrifting;
    public float steering;
    float steerInput;
    float unmodifedSteering;
    float lastShiftTime = -1;
    float handbrake;
    private Drivetrain drivetrain;

    [Header("----- Throttle Params -----")]
    public float shiftSpeed = 0.4f;
    public float throttleTime = 1.0f;                               // 油門準到底，所需要的時間
    public float throttleTimeTraction = 10.0f;                      // 油門係數
    public float throttleReleaseTime = 0.5f;                        // 放掉的時候，慢慢放掉，所需要的時間
    public float throttleReleaseTimeTraction = 0.1f;                // 係數

    // These values determine how fast steering value is changed when the steering keys are pressed or released.
    // Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.

    // How long it takes to fully turn the steering wheel from center to full lock
    public float steerTime = 0.6f;
    // This is added to steerTime per m/s of velocity, so steering is slower when the car is moving faster.
    // 為了讓車子穩定，所以車子越快的時候 steerSpeed 要慢慢 steer，所以要呈上一個係數
    public float veloSteerTime = 0.03f;

    // How long it takes to fully turn the steering wheel from full lock to center
    public float steerReleaseTime = 0.3f;
    // When detecting a situation where the player tries to counter steer to correct an oversteer situation,
    // steering speed will be multiplied by the difference between optimal and current steering times this 
    // factor, to make the correction easier.
    public float steerCorrectionFactor = 4.0f;

    // current input state
    [System.NonSerialized]
    public bool bShiftUp = false;
    [System.NonSerialized]
    public bool bShiftDown = false;

    // state
    [System.NonSerialized]
    public bool isAnyWheelOnGround = false;
    [System.NonSerialized]
    public bool isAllWheelOnGround = false;

    // TCS
    // Traction Control System 
    // 中文：循跡控制系統
    // 說明：假設輪胎速度大於原廠設定的速度，為了怕打滑，而故意做的一個降低輪胎速度的系統
    [Header("----- TCS Params -----")]
    public bool bTCS = true;
    public float tcsSlip = 0.1f;
    public float tcsMaxSlip = 0.2f;

    // ABS
    // Anti-lock Braking System
    // 中文：防鎖死煞車系統
    // 說明：假設車子在煞車的時候，不會把輪胎馬上鎖死，不然會失去側巷的抓地力，導致打滑
    [Header("----- ABS Params -----")]
    public bool bABS = true;
    public float absSpeed = 100.0f; // car speed (in km/hr) threshold to release brake
    public float absSlipMaxThresh = 0.07f; // if slip value more than this threshold, brake = 0
    public float absSlipMinThresh = 0.0f; // if slip value less than this threshold, brake = 1
    public float brakeMinThresh = 0.0f;

    [System.NonSerialized]
    float modifiedThrottle = 0f;
    float modifiedBrake = 0f;
    float modifiedSteering = 0f;

    Vector3 avgGroundNormal = Vector3.zero;

    [Header("----- Update System -----")]
    public int MaxLevel = 5;
    public int SpeedLevel = 5;
    public int AccelLevel = 5;
    public int SteeringLevel = 5;

    private CurveManager curveM;


    private NeuralNetworkManager NNManger;
    private const int RayLength = 200;

    private void Start()
    {
        // 將輪胎裡面的 Curve 給進去
        curveM = this.GetComponent<CurveManager>();
        for (int i = 0; i < wheels.Length; i++)
            wheels[i].CurveM = curveM;

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;

        // 拿東西
        if (centerOfMass != null)
            this.GetComponent<Rigidbody>().centerOfMass = centerOfMass.localPosition;
        this.GetComponent<Rigidbody>().inertiaTensor *= inertiaFactor;
        drivetrain = this.GetComponent<Drivetrain>();

        // 拿 NN Manager
        NNManger = this.GetComponent<NeuralNetworkManager>();
    }

    void Update ()
    {
        // 0 => Steering
        // 1 => Throttle
        // 2 => Brake
        float[] Data = NNManger.Compute(RayCastData());

        #region Input 更改
        if (Data[0] > 0)
            steerInput = 1;
        else if (Data[0] < 0)
            steerInput = -1;

        if (Data[1] > 0)
            throttleInput = 1;
        #endregion
        #region 給值
        float shiftThrottleFactor = Mathf.Clamp01((Time.time - lastShiftTime) / shiftSpeed);
        drivetrain.throttle = throttle * shiftThrottleFactor;
        drivetrain.throttleInput = throttleInput;

        steering = Data[0];
        throttle = Data[1];
        brake = Data[2];

        // Apply inputs
        foreach (Wheel w in wheels)
        {
            w.brake = brake;
            w.handbrake = handbrake;
            w.steering = steering;
        }
        #endregion
    }
    void FixedUpdate()
    {
        float velMag = GetComponent<Rigidbody>().velocity.magnitude;
        float speedMag = velMag * 3.6f;

        throttle = Mathf.Clamp(throttle, 0, 1f);
        brake = Mathf.Clamp01(brake);
        steering = Mathf.Clamp(steering, -1f, 1f);

        modifiedThrottle = throttle;
        modifiedBrake = brake;

        modifiedSteering = steering;

        // TCS
        if (bTCS && drivetrain.slipRatio > tcsSlip && speedMag > 5.0f)
        {
            float slip = Mathf.Clamp(drivetrain.slipRatio, 0f, tcsMaxSlip);
            float throttleRatio = 1f - ((slip - tcsSlip) / (tcsMaxSlip - tcsSlip));
            throttleRatio = Mathf.Max(throttleRatio, 0.6f);
            modifiedThrottle *= throttleRatio;
        }

        // ABS
        if (bABS && speedMag > absSpeed)
        {
            float slip = Mathf.Abs(drivetrain.slipRatio);

            float brakeRatio = Mathf.Lerp(1f, 0f, (slip - absSlipMinThresh) / (absSlipMaxThresh - absSlipMinThresh));
            brakeRatio = Mathf.Clamp01(Mathf.Max(brakeMinThresh, brakeRatio));
            modifiedBrake *= brakeRatio;
        }

        // Apply inputs
        // 1. drivetrain
        drivetrain.bShiftUp = bShiftUp;
        drivetrain.bShiftDown = bShiftDown;
        drivetrain.throttle = modifiedThrottle;
        drivetrain.brake = modifiedBrake;

        // detect ground
        isAnyWheelOnGround = false;
        isAllWheelOnGround = true;
        avgGroundNormal = Vector3.zero;
        int onAirWheelNum = 0;

        for (int i = 0; i < wheels.Length; ++i)
        {
            wheels[i].brake = modifiedBrake;
            wheels[i].throttle = modifiedThrottle;
            wheels[i].steering = steering;
            wheels[i].modifiedSteering = modifiedSteering;
            wheels[i].CustomFixedUpdateGround();

            if (wheels[i].onGround)
            {
                isAnyWheelOnGround = true;
                avgGroundNormal += wheels[i].groundNormal;
            }
            else
            {
                isAllWheelOnGround = false;
                ++onAirWheelNum;
            }
        }
        avgGroundNormal.Normalize();

        // update wheel
        for (int i = 0; i < wheels.Length; ++i)
            wheels[i].CustomFixedUpdate(avgGroundNormal);

        // apply wheel force
        Vector3 totalWheelForce = Vector3.zero;
        for (int i = 0; i < wheels.Length; ++i)
        {
            //Vector3 f = wheels[i].suspensionForce_ + wheels[i].roadForce_;
            Vector3 f = wheels[i].roadForce_;
            totalWheelForce += f;
        }

        Vector3 carDir = transform.forward;
        float fVelo = GetComponent<Rigidbody>().velocity.magnitude;
        Vector3 veloDir = GetComponent<Rigidbody>().velocity * (1 / fVelo);

        float slipAngle = 0.0f;
        slipAngle = Vector3.Angle(veloDir, carDir) * Mathf.Sign(Vector3.Cross(veloDir, carDir)[1]);
    }

    /// <summary>
    /// 打 Ray 看一下
    /// </summary>
    /// <returns></returns>
    private float[] RayCastData()
    {
        RayData data = new RayData();
        float[] OutputData = new float[5];
        RaycastHit leftHit, leftfrontHit, frontHit, rightfrontHit, rightHit;
        #region 左
        Vector3 leftDir = this.transform.right * -1;
        Ray leftRay = new Ray(this.transform.position, leftDir);
        if (Physics.Raycast(leftRay, out leftHit, RayLength))
        {
            if (leftHit.collider.tag != "Player")
                data.leftHitDis = Vector3.Distance(this.transform.position, leftHit.point) / RayLength;
        }
        else
            data.leftHitDis = 1;
        #endregion
        #region 左前
        Vector3 leftfrontDir = Vector3.Lerp(this.transform.right * -1, this.transform.forward, 0.5f);
        Ray leftfrontRay = new Ray(this.transform.position, leftfrontDir);
        if (Physics.Raycast(leftfrontRay, out leftfrontHit, RayLength))
        { 
            if (leftfrontHit.collider.tag != "Player")
                data.leftfrontHitDis = Vector3.Distance(this.transform.position, leftfrontHit.point) / RayLength;
        }
        else
            data.leftfrontHitDis = 1;
        #endregion
        #region 前
        Vector3 frontDir = this.transform.forward;
        Ray frontRay = new Ray(this.transform.position, frontDir);
        if (Physics.Raycast(frontRay, out frontHit, RayLength))
        {
            if (frontHit.collider.tag != "Player")
                data.frontHitDis = Vector3.Distance(this.transform.position, frontHit.point) / RayLength;
        }
        else
            data.frontHitDis = 1;
        #endregion
        #region 右前
        Vector3 rightfrontDir = Vector3.Lerp(this.transform.forward, this.transform.right, 0.5f);
        Ray rightfrontRay = new Ray(this.transform.position, rightfrontDir);
        if (Physics.Raycast(rightfrontRay, out rightfrontHit, RayLength))
        {
            if (rightfrontHit.collider.tag != "Player")
                data.rightfrontHitDis = Vector3.Distance(this.transform.position, rightfrontHit.point) / RayLength;
        }
        else
            data.rightfrontHitDis = 1;
        #endregion
        #region 右
        Vector3 rightDir = this.transform.right;
        Ray rightRay = new Ray(this.transform.position, rightDir);
        if (Physics.Raycast(rightRay, out rightHit, RayLength))
        {
            if (rightHit.collider.tag != "Player")
                data.rightHitDis = Vector3.Distance(this.transform.position, rightHit.point) / RayLength;
        }
        else
            data.rightHitDis = 1;
        #endregion
        #region 存到輸出裡面
        OutputData[0] = data.leftHitDis;
        OutputData[1] = data.leftfrontHitDis;
        OutputData[2] = data.frontHitDis;
        OutputData[3] = data.rightfrontHitDis;
        OutputData[4] = data.rightHitDis;
        #endregion
        return OutputData; 
    }
}