using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using InControl;
public class CarController : MonoBehaviour
{
    // ��Ҧ������l����b�o
    [Header("----- Basic Params -----")]
    public Wheel[] wheels;
    public int ForwardMaxSpeed = 500;
    public int BackwardMaxSpeed = 110;
    public int NitrousUpSpeed = 50;
    public static bool UseNitrous = false;

    //public DriftController driftController_;

    public Transform centerOfMass;          // ���l�����
    public float inertiaFactor = 1.5f;      // ���l���D��

    // �q��L���X State
    //[HideInInspector]
    public float brake;                            // �٨�
    public float throttle;                         // �o��
    float throttleInput;                    // �o��Input�A�u�����٨��|��
    float throttleForDrifting;
    public float steering;
    float steerInput;
    float lastShiftTime = -1;
    float handbrake;
		
	// cached Drivetrain reference
	private Drivetrain drivetrain;

    [Header("----- Throttle Params -----")]
    public float shiftSpeed = 0.4f;
    public float throttleTime = 1.0f;                               // �o���Ǩ쩳�A�һݭn���ɶ�
    public float throttleTimeTraction = 10.0f;                      // �o���Y��
    public float throttleReleaseTime = 0.5f;                        // �񱼪��ɭԡA�C�C�񱼡A�һݭn���ɶ�
    public float throttleReleaseTimeTraction = 0.1f;                // �Y��
	
	
	// These values determine how fast steering value is changed when the steering keys are pressed or released.
	// Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.
	
	// How long it takes to fully turn the steering wheel from center to full lock
	public float steerTime = 0.6f;
	// This is added to steerTime per m/s of velocity, so steering is slower when the car is moving faster.
    // ���F�����lí�w�A�ҥH���l�V�֪��ɭ� steerSpeed �n�C�C steer�A�ҥH�n�e�W�@�ӫY��
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
    // ����G�`�񱱨�t��
    // �����G���]���L�t�פj���t�]�w���t�סA���F�ȥ��ơA�ӬG�N�����@�ӭ��C���L�t�ת��t��
    [Header("----- TCS Params -----")]
    public bool bTCS = true;
    public float tcsSlip = 0.1f;
    public float tcsMaxSlip = 0.2f;

    // ABS
    // Anti-lock Braking System
    // ����G���ꦺ�٨��t��
    // �����G���]���l�b�٨����ɭԡA���|����L���W�ꦺ�A���M�|���h���Ѫ���a�O�A�ɭP����
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

    [Header("----- For Moblie Device -----")]
    public GameObject MoblieController;
    private bool MoblieLeft = false;
    private bool MoblieRight = false;
    private bool MoblieBrake = false;
    [HideInInspector]
    public bool MoblieDrift = false;
    
    //For camera test scene
    [HideInInspector]
    public bool autoAccel = false;
    [HideInInspector]
    public bool autoBrake = false;
    [HideInInspector]
    public bool autoLeft = false;
    [HideInInspector]
    public bool autoRight = false;

    private static bool bAutoControlled = false;

    public bool accelFlag = false;
    public bool rightFlag = false;
    public bool leftFlag = false;
    public CurveManager curveM;

    void Awake()
    {
        // �N���L�̭��� Curve ���i�h
        curveM = this.GetComponent<CurveManager>();
        for (int i = 0; i < wheels.Length; i++)
            wheels[i].CurveM = curveM;

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        if (centerOfMass != null)
            this.GetComponent<Rigidbody>().centerOfMass = centerOfMass.localPosition;
        this.GetComponent<Rigidbody>().inertiaTensor *= inertiaFactor;
        drivetrain = this.GetComponent<Drivetrain>();
        //driftController_ = GetComponent<DriftController>();

        switch(Application.platform)
        {
            case RuntimePlatform.Android:
                MoblieController.SetActive(true);

                // �]�w����i�H�Ϊ�����
                Input.gyro.enabled = true;
                break;
        }

        //For camera test scene
        //if(GetComponent<AutoCarController>() != null)
        //{
        //    bAutoControlled = true;
        //}
    }
	
	void Update () 
	{
		// �����l����V�A�t�� & �t�פ�V
		Vector3 carDir = transform.forward;
		float fVelo = GetComponent<Rigidbody>().velocity.magnitude;
		Vector3 veloDir = GetComponent<Rigidbody>().velocity.normalized;

        // ��X���l slipAngle ������
		float angle = -Mathf.Asin(Mathf.Clamp( Vector3.Cross(veloDir, carDir).y, -1, 1));
		float optimalSteering = angle / (wheels[0].maxSteeringAngle * Mathf.Deg2Rad);
		if (fVelo < 1)
			optimalSteering = 0;

        #region Steering �� ���k
        steerInput = 0;
        switch(Application.platform)
        {
            case RuntimePlatform.Android:
                /*if (MoblieLeft)
                    steerInput = -1;
                if (MoblieRight)
                    steerInput = 1;*/
                steerInput = GyroFunction(Input.gyro.gravity.x);
                //Mathf.Clamp(Input.gyro.gravity.x * 2, -1, 1);
                break;
            case RuntimePlatform.WindowsEditor:
                if (Input.GetKey (KeyCode.LeftArrow))
			        steerInput = -1;
		        if (Input.GetKey (KeyCode.RightArrow))
			        steerInput = 1;
                if (Input.GetKey(KeyCode.Space))
                    MoblieDrift = true;
                break;
            case RuntimePlatform.PS4:
                InputDevice input = InputManager.ActiveDevice;
                steerInput = input.LeftStickX.Value;
                break;
        }
        #endregion
#if UNITY_EDITOR
        if (bAutoControlled)
        {
            //For camera test scene
            if (autoLeft)
            {
                steerInput = -1;
            }
            if (autoRight)
            {
                steerInput = 1;
            }
        }
        if(leftFlag) {
            steerInput = -1;
        }
        if(rightFlag) {
            steerInput = 1;
        }
#endif
        if (steerInput < steering)                                          // ����
        {
            // Steering > 0 �N��q�k��ϥ��^��
            float steerSpeed = (steering > 0) ? (1 / steerReleaseTime) : (1 / (steerTime + veloSteerTime * fVelo));
            if (steering > optimalSteering)
                steerSpeed *= 1 + (steering - optimalSteering) * steerCorrectionFactor;
            steering -= steerSpeed * Time.deltaTime;

            // �n���L�d�b -1
            if (steerInput > steering)
                steering = steerInput;
        }
        else if (steerInput > steering)                                     // �k��
        {
            float steerSpeed = (steering < 0) ? (1 / steerReleaseTime) : (1 / (steerTime + veloSteerTime * fVelo));
            if (steering < optimalSteering)
                steerSpeed *= 1 + (optimalSteering - steering) * steerCorrectionFactor;
            steering += steerSpeed * Time.deltaTime;

            // �n���L�d�b 1
            if (steerInput < steering)
                steering = steerInput;
        }


        #region �o������ & �٨���
        bool accelKey = false;
		bool brakeKey = false;
        switch(Application.platform)
        {
            case RuntimePlatform.Android:
                accelKey = !(MoblieBrake);
                brakeKey = MoblieBrake;
                break;
            case RuntimePlatform.WindowsEditor:
                accelKey = Input.GetKey (KeyCode.UpArrow);
                brakeKey = Input.GetKey(KeyCode.DownArrow);

                if (Input.GetKeyDown(KeyCode.Z))
                    UseNitrous = true;
                else if (Input.GetKeyUp(KeyCode.Z))
                    UseNitrous = false;
                break;
            case RuntimePlatform.PS4:
                InputDevice input = InputManager.ActiveDevice;
                accelKey = input.RightTrigger.IsPressed;
                brakeKey = input.LeftTrigger.IsPressed;
                UseNitrous = input.Action1.IsPressed;
                break;
        }
        #endregion

        // �˨����ɭԡA�[�t�M�٨���Ų�ѬۤϪ�
        if (drivetrain.automatic && drivetrain.gear == 0)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    accelKey = MoblieBrake;
                    brakeKey = !(MoblieBrake);
                    break;
                case RuntimePlatform.WindowsEditor:
                    accelKey = Input.GetKey(KeyCode.DownArrow);
                    brakeKey = Input.GetKey(KeyCode.UpArrow);
                    break;
                case RuntimePlatform.PS4:
                    InputDevice input = InputManager.ActiveDevice;
                    accelKey = input.LeftTrigger.IsPressed;
                    brakeKey = input.RightTrigger.IsPressed;
                    break;

            }
        }
#if UNITY_EDITOR
        if (bAutoControlled)
        {
            //For camera test scene
            accelKey = autoAccel || accelKey;
            brakeKey = autoBrake || brakeKey;
        }
        if(accelFlag) {
            accelKey = true;
        }
#endif

        if (accelKey)
		{
            // �����]�� slipRatio? ��M�C
			if (drivetrain.slipRatio < 0.10f)
				throttle      += Time.deltaTime / throttleTime;
			else
				throttle -= Time.deltaTime / throttleReleaseTime;

			if (throttleInput < 0)
				throttleInput = 0;
            if(throttleForDrifting < 0)
                throttleForDrifting = 0;

			throttleInput += Time.deltaTime / throttleTime;
            throttleForDrifting += Time.deltaTime / throttleTime;
			brake = 0;
		}
		else 
		{
			if (drivetrain.slipRatio < 0.2f)
				throttle -= Time.deltaTime / throttleReleaseTime;
			else
				throttle -= Time.deltaTime / throttleReleaseTimeTraction;

            throttleForDrifting -= Time.deltaTime / throttleReleaseTime;
		}
		throttle = Mathf.Clamp01 (throttle);
        throttleForDrifting = Mathf.Clamp01 (throttleForDrifting);

		if (brakeKey)
		{
			if (drivetrain.slipRatio < 0.2f)
				brake += Time.deltaTime / throttleTime;
			else
				brake += Time.deltaTime / throttleTimeTraction;
			throttle = 0;
			throttleInput -= Time.deltaTime / throttleTime;
		}
		else 
		{
			if (drivetrain.slipRatio < 0.2f)
				brake -= Time.deltaTime / throttleReleaseTime;
			else
				brake -= Time.deltaTime / throttleReleaseTimeTraction;
		}
        brake = Mathf.Clamp01(brake);
        throttleInput = Mathf.Clamp(throttleInput, -1, 1);
				
		// Handbrake
        handbrake = Mathf.Clamp01(handbrake + (Input.GetKey(KeyCode.Space) ? Time.deltaTime : -Time.deltaTime));
		
		// Gear shifting
		float shiftThrottleFactor = Mathf.Clamp01((Time.time - lastShiftTime)/shiftSpeed);
		drivetrain.throttle = throttle * shiftThrottleFactor;
		drivetrain.throttleInput = throttleInput;
		
        if(!drivetrain.automatic)
            if (Input.GetKeyDown(KeyCode.A))
            {
                lastShiftTime = Time.time;
                drivetrain.ShiftUp();
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                lastShiftTime = Time.time;
                drivetrain.ShiftDown();
            }

		// Apply inputs
		foreach(Wheel w in wheels)
		{
			w.brake = brake;
			w.handbrake = handbrake;
			w.steering = steering;
		}
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

            float brakeRatio = Mathf.Lerp(1f, 0f, (slip - absSlipMinThresh)/(absSlipMaxThresh - absSlipMinThresh));
            brakeRatio = Mathf.Clamp01(Mathf.Max(brakeMinThresh, brakeRatio));
            modifiedBrake *= brakeRatio;
        }

        // Apply inputs
        // 1. drivetrain
        drivetrain.bShiftUp = bShiftUp;
        drivetrain.bShiftDown = bShiftDown;
        drivetrain.throttle = modifiedThrottle;
        drivetrain.brake = modifiedBrake;
        //if(driftController_ != null)
        //{
        //    drivetrain.torqueFactor2 = driftController_.torqueFactor_;

        //    // 2. driftController_
        //    driftController_.steering = steering;
        //    driftController_.steeringInput = (int)Mathf.Sign(steerInput);
        //    driftController_.throttle = throttle;
        //    driftController_.throttleInput = throttleForDrifting;
        //    driftController_.brake = brake;

        //}

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

        // �p�G�S�� driftController ���ܡA�o�̴N����
        //if (driftController_ != null)
        //{
        //    driftController_.slipAng = slipAngle;
        //    driftController_.totalWheelForce_ = totalWheelForce;
        //    driftController_.CustomFixedUpdate();
        //}
    }


    public void ReturnToOrigin()
    {
        steering = 0;
        steerInput = 0;
        throttle = 0;
        throttleInput = 0;
        brake = 0;
        for (int i = 0; i < wheels.Length; i++)
            wheels[i].ReturnToOrigin();

    }

    public float GetSteering()
    {
        return steering;
    }

    #region �����J
    public void MoblieLeftDown()
    {
        MoblieLeft = true;
    }
    public void MoblieRightDown()
    {
        MoblieRight = true;
    }
    public void MoblieLeftUp()
    {
        MoblieLeft = false;
    }
    public void MoblieRightUp()
    {
        MoblieRight = false;
    }
    public void MoblieDriftPress()
    {
        MoblieDrift = true;
    }
    public void MoblieDiftUp()
    {
        MoblieDrift = false;
    }
    public void MoblieBrakeDown()
    {
        MoblieBrake = true;
    }

    public void MoblieBrakeUp()
    {
        MoblieBrake = false;
    }
    #endregion


    private float GyroFunction(float x)
    {
        return curveM.GyroScale.Evaluate(Mathf.Abs(x)) * Mathf.Sign(x);
    }
}