using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class simulates a single car's wheel with tire, brake and simple
// suspension (basically just a single, independant spring and damper).
public class Wheel : MonoBehaviour
{
    //wheel's id
    public int wheelNumber = 0;
    //unity circle
    public Vector2 UnityCircle = new Vector2();
    // Wheel radius in meters
    public float radius = 0.34f;
    // Wheel suspension travel in meters
    public float suspensionTravel = 0.12f;
    // apply extra damping force when wheel is underground
    public float extraSuspensionRate = 30.0f;
    // Damper strength in kg/s
    public float damping = 5000;
    // Wheel angular inertia in kg * m^2
    public float inertia = 2.2f;
    //public float inertia = 10.f;
    // Coeefficient of grip - this is simly multiplied to the resulting forces, 
    // so it is not quite realitic, but an easy way to quickly change handling characteritics
    public float latGrip = 1.0f;
    public float longGrip = 1.0f;
    public float grip = 1.0f;
    // Maximal braking torque (in Nm)
    public float brakeFrictionTorque = 4000;
    // Maximal handbrake torque (in Nm)
    public float handbrakeFrictionTorque = 0;
    // Base friction torque (in Nm)
    public float frictionTorque = 10;
    // Maximal steering angle (in degrees)
    public float maxSteeringAngle = 28f;
    // Graphical wheel representation (to be rotated accordingly)
    public GameObject model;
    // Fraction of the car's mass carried by this wheel
    public float massFraction = 0.25f;
    // Magic Formula 參數(Pacejka coefficients)
    public float[] a = { 1.0f, -60f, 1688f, 4140f, 6.026f, 0f, -0.3589f, 1f, 0f, -6.111f / 1000f, -3.244f / 100f, 0f, 0f, 0f, 0f };
    public float[] b = { 1.0f, -60f, 1588f, 0f, 229f, 0f, 0f, 0f, -10f, 0f, 0f };
    public float driftSteeringFactor = 1.0f;

    // inputs
    // engine torque applied to this wheel
    public float driveTorque = 0;
    // engine braking and other drivetrain friction torques applied to this wheel
    public float driveFrictionTorque = 0;
    // brake input
    public float brake = 0;
    // handbrake input
    public float handbrake = 0;
    // steering input
    public float steering = 0;
    // drivetrain inertia as currently connected to this wheel
    public float drivetrainInertia = 0;
    // suspension force externally applied (by anti-roll bars)
    public float suspensionForceInput = 0;

    // output
    public float angularVelocity;
    public float slipRatio;
    public float compression;

    // state
    float fullCompressionSpringForce;
    public Vector3 wheelVelo;
    Vector3 localVelo;
    public Vector3 groundNormal;
    float rotation;
    float normalForce;
    Vector3 suspensionForce;
    public Vector3 roadForce;
    Vector3 up, forward;
    Quaternion localRotation = Quaternion.identity;
    Quaternion inverseLocalRotation = Quaternion.identity;
    public float slipAngle;
    //int lastSkid = -1;

    public bool onGround = false;

    // cached values
    Rigidbody body;
    float maxSlip = 0.7339941f;
    float maxAngle = 1.448012f;
    float oldAngle;

    // SDM ---------------------------------------------------------------------
    public int idx_ = 0;

    public float modifiedSteering = 0f;

    // inputs
    [System.NonSerialized]
    public float throttle = 0;

    // Raycast
    [System.NonSerialized]
    public RaycastHit hit_;

    static LayerMask collisionMask_ = 1 << 8 | 1 << 19;

    // state
    [System.NonSerialized]
    public Vector3 groundNormal_;
    [System.NonSerialized]
    public Vector3 groundPos_;
    [System.NonSerialized]
    public Vector3 wheelVelo_;
    [System.NonSerialized]
    public Vector3 suspensionForce_;
    [System.NonSerialized]
    public Vector3 roadForce_;

    // output
    [System.NonSerialized]
    public float compression_;

    // 曲線的 Curve
    [HideInInspector]
    public CurveManager CurveM;

    // 畫線的 render (彈簧力， 縱向力， 側向力)
    //public GameObject lineGameObject;
    private LineRenderer[] lineRender = new LineRenderer[3];
    public float fGroundDist;                                           // Raycast 打在地上的距離
    public bool StableOnGround = false;                                 // 輪子是否在地面上(給 Stablizer 用)

    private int SteeringLevel;
    private int MaxLevel;

    public float tmp;

    public bool showForce = true;

    Vector3 CombinedForce(float Fz, float slip, float slipAngle)
    {
        // 轉彎的時候，不要讓他一下子就轉倒到底，所以會去慢慢減一個 steering speed
        float SteeringFactor = (float)Mathf.Clamp(1, SteeringLevel, MaxLevel) / MaxLevel;

        Vector3 forward = new Vector3(0, -groundNormal.z, groundNormal.y);

        float wheelRoadVelo = Vector3.Dot(wheelVelo, forward);

        return latGrip * Vector3.right * Mathf.Sign(slipAngle) * CurveM.LateralForceCurve.Evaluate(Mathf.Abs(slipAngle)) +
            longGrip * forward * Mathf.Sign(slip) * CurveM.LongForceCurve.Evaluate(Mathf.Abs(slip));
    }

    void Start()
    {
        Transform trs = transform;
        while (trs != null && trs.GetComponent<Rigidbody>() == null)
            trs = trs.parent;
        if (trs != null)
            body = trs.GetComponent<Rigidbody>();

        fullCompressionSpringForce = body.mass * massFraction * 2.0f * -Physics.gravity.y;

        // 轉彎的 Level 設定
        CarController carinfo = this.transform.parent.GetComponent<CarController>();
        SteeringLevel = carinfo.SteeringLevel;
        MaxLevel = carinfo.MaxLevel;
    }

    Vector3 SuspensionForce()
    {
        // compression 是 0~1 的值(目前懸吊伸長量 / 懸吊最大伸長量) => 壓縮比(0是沒壓縮，1是壓縮到底)
        // springForce 是壓縮
        float springForce = compression * fullCompressionSpringForce;
        normalForce = springForce;

        float damperForce = Vector3.Dot(localVelo, groundNormal) * damping;

        return (springForce - damperForce + suspensionForceInput) * up;
    }

    float SlipRatio()
    {
        const float fullSlipVelo = 4.0f;

        float wheelRoadVelo = Vector3.Dot(wheelVelo, forward);
        if (Mathf.Abs(wheelRoadVelo) < 0.1f)
            if(Mathf.Abs(throttle) == 0)
                return 0;

        float absRoadVelo = Mathf.Abs(wheelRoadVelo);
        float damping = Mathf.Clamp01(absRoadVelo / fullSlipVelo);

        float wheelTireVelo = angularVelocity * radius;

        return (wheelTireVelo - wheelRoadVelo) / absRoadVelo * damping;
    }
    float SlipAngle()
    {
        const float fullAngleVelo = 4.0f;

        Vector3 wheelMotionDirection = localVelo;
        wheelMotionDirection.y = 0;

        if (wheelMotionDirection.sqrMagnitude < Mathf.Epsilon)
            return 0;

        float sinSlipAngle = wheelMotionDirection.normalized.x;

        Mathf.Clamp(sinSlipAngle, -1, 1); // To avoid precision errors.

        // 這裡加入damping是為了處理低速時輪胎速度不穩定的現象
        // 如果 fullAngleVelo 設為 2.0f 則當輪胎速度在 0.0f ~ 2.0f 之間會產生 0~1 之間的一個damping值
        float damping = Mathf.Clamp01(localVelo.magnitude / fullAngleVelo);

        return -Mathf.Asin(sinSlipAngle) * damping * damping;
    }
    Vector3 RoadForce()
    {
        int slipRes = (int)((300 - Mathf.Abs(angularVelocity)) / (10.0f));
        if (slipRes < 1)
            slipRes = 1;
        float invSlipRes = (1.0f / (float)slipRes);

        float totalInertia = inertia + drivetrainInertia;
        float driveAngularDelta = driveTorque * Time.deltaTime * invSlipRes / totalInertia;
        float totalFrictionTorque = brakeFrictionTorque * brake + handbrakeFrictionTorque * handbrake + frictionTorque + driveFrictionTorque;
        float frictionAngularDelta = totalFrictionTorque * Time.deltaTime * invSlipRes / totalInertia;

        Vector3 totalForce = Vector3.zero;
        float newAngle = maxSteeringAngle * steering * driftSteeringFactor;

        for (int i = 0; i < slipRes; i++)
        {
            float f = i * 1.0f / (float)slipRes;

            // 根據steering的大小決定 輪胎的轉向角度 newAngle
            // 以尤拉角的方式定義輪胎local座標的轉向Quaternion
            localRotation = Quaternion.Euler(0, oldAngle + (newAngle - oldAngle) * f, 0);
            inverseLocalRotation = Quaternion.Inverse(localRotation);

            // 在輪胎的local座標根據轉向Quaternion將輪胎local座標的forward方向和right方向 進行旋轉
            // 將local座標的foward和right轉換到global座標
            forward = transform.TransformDirection(localRotation * Vector3.forward);

            slipRatio = SlipRatio();
            slipAngle = SlipAngle();

            Vector3 force = invSlipRes * CombinedForce(normalForce, slipRatio, slipAngle);
            Vector3 worldForce = transform.TransformDirection(localRotation * force);
            angularVelocity -= (force.z * radius * Time.deltaTime) / totalInertia;
            angularVelocity += driveAngularDelta;
            if (Mathf.Abs(angularVelocity) > frictionAngularDelta)
                angularVelocity -= frictionAngularDelta * Mathf.Sign(angularVelocity);
            else
                angularVelocity = 0;

            wheelVelo += worldForce * (1 / body.mass) * Time.deltaTime * invSlipRes;
            totalForce += worldForce;
        }

        oldAngle = newAngle;
        return totalForce;
    }

    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        up = transform.up;
        // hit 是判斷車子有沒有離地 
        // hitGround 是給算出距離
        RaycastHit hit, hitGround;
        StableOnGround = Physics.Raycast(pos, -up, out hit, suspensionTravel + radius);
        Physics.Raycast(pos, -up, out hitGround);
        fGroundDist = hitGround.distance;

        // 強制貼地
        groundNormal = transform.InverseTransformDirection(inverseLocalRotation * hit.normal);
        compression = 1.0f - ((hitGround.distance - radius) / suspensionTravel);
        wheelVelo = body.GetPointVelocity(pos);
        localVelo = transform.InverseTransformDirection(inverseLocalRotation * wheelVelo);
        suspensionForce = SuspensionForce();
        suspensionForce_ = suspensionForce;
        roadForce = RoadForce();
        roadForce_ = roadForce;
        body.AddForceAtPosition(suspensionForce + roadForce, pos);

        compression = Mathf.Clamp01(compression);
        rotation += angularVelocity * Time.deltaTime;
        if (model != null)
            model.transform.localRotation = Quaternion.Euler(Mathf.Rad2Deg * rotation, maxSteeringAngle * steering * driftSteeringFactor, 0);
    }

    void ExtraSuspensionForce()
    {
        if (hit_.collider.attachedRigidbody)
            return;

        // 避免輪胎穿地
        if (hit_.distance < radius)
        {
            float dist = radius - hit_.distance;
            body.transform.position += groundNormal_ * dist;
            float force = normalForce * dist * extraSuspensionRate;
            body.AddForceAtPosition(force * transform.up, transform.position);
            body.AddForce(force * -transform.up);
        }
    }
    public void CustomFixedUpdate(Vector3 n)
    {
        //avgGroundNormal = n;

        if (onGround)
        {
            ExtraSuspensionForce();

            //## suspensionTravel是懸吊最大伸長量
            //## hit_.distance - radius 是目前懸吊的長度
            compression_ = 1.0f - ((hit_.distance - radius) / suspensionTravel);

            compression_ = Mathf.Clamp01(compression_);

            //## 算出輪胎local座標的速度
            wheelVelo_ = body.GetPointVelocity(transform.position);

            suspensionForce_ = SuspensionForce();

            roadForce_ = RoadForce();
        }
    }
    public void CustomFixedUpdateGround()
    {
        //onGround = Physics.Raycast(transform.position, -transform.up, out hit_, suspensionTravel + radius, collisionMask_);
        onGround = Physics.Raycast(transform.position, -transform.up, out hit_, suspensionTravel + radius);
        
        if (onGround && hit_.collider.isTrigger)
        {
            onGround = false;
            float dist = suspensionTravel + radius;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, -transform.up, dist, collisionMask_);

            foreach (RaycastHit test in hits)
            {
                if (!test.collider.isTrigger &&
                    test.distance <= dist)
                {
                    onGround = true;
                    hit_ = test;
                    dist = test.distance;
                }
            }
        }

        groundPos_ = hit_.point;
        groundNormal_ = hit_.normal;
    }

    public void ReturnToOrigin()
    {
        angularVelocity = 0;
        wheelVelo = new Vector3(0, 0, 0);
    }
}