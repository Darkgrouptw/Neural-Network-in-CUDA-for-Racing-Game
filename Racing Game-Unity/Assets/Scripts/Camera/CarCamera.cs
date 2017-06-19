using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
[System.Serializable]
public class CarCamera : MonoBehaviour
{
    public bool multiCameraTest = false;
    public enum CameraType { FollowCamera, FixPositionCamera, MovingCamera };
    public CameraType cameraType;

    public bool ShowStatus = false;

    public enum FollowRotation
    {
        Model,
        Velocity,
        Mix
    }

    public FollowRotation FollowRotationMode = FollowRotation.Model;

    public float FollowRotationMixRatioX = 0.5f;
    public float FollowRotationMixRatioY = 0.5f;
    public float FollowRotationMixRatioZ = 0.5f;

    float[] FollowRotationMixRatio;

    public bool kalmanFilter = true;
    
    public int kalmanFilterSampleSize = 30;

    public bool PitchEffect = true;
    [SerializeField]
    public AnimationCurve PitchCurve = null;
    float PitchEffectValue = 0f;

    public Transform target = null;

    public bool bIsManualFOV = false;
    [Range(1, 179)]
    public float manualFOV = 49.5f;

    [HideInInspector]
    public Vector3 debug1 = Vector3.zero;
    [HideInInspector]
    public float debug2;

    [Range(0.01f, 10)]
    public float nearClipPlane = 0.3f;
    [Range(100, 100000)]
    public float farClipPlane = 1000;

    public AnimationCurve curveFOV = null;
    public AnimationCurve FOVAffectCamDistance = null; //The distance change with FOV

    [HideInInspector]
    public float fPositionDamping = 3f;
    [HideInInspector]
    public float fVelocityDamping = 3f;
    [HideInInspector]
    public float fRotationDamping = 3.0f;

    public float fMaxTargetDistance = 1.0f;
    public Vector3 targetSpringConstant = new Vector3(0.0f, 5000.0f, 1000.0f);
    public float fOriginalDistance = 6.0f;            //The original distance between target and camera
    public float fMinAccelDistance = -0.5f;           //The minimum delta distance of camera affected by the accel key
    public float fMaxAccelDistance = 1.0f;           //The maximum delta distance of camera affected by the accel key
    public float fAccelKeyForce = 500.0f;             //How the accel key pressed affects the camera distance
    public float fBrakeKeyForce = 800.0f;             //How the brake key pressed affects the camera distance\
    public float fDistanceSpringConstant = 400.0f;
    [SerializeField]
    public Vector3 rotateSpringConstant = new Vector3(25.0f, 15.0f, 30.0f);
    public mSpringSystem rotateSpring = new mSpringSystem(250.0f, 150.0f, 1000.0f);
    [SerializeField]
    public Vector3 rotateDampingConstant = new Vector3(10.0f, 7.75f, 11.0f);
    //public bool rotateDampingConstant = new Vector3(200.0f, 200.0f, 200.0f);
    public float fMaxSpeed = 83.0f;
    public Vector3 targetOriginalRotation = new Vector3(0.0f, 0.0f, 0.0f);
    public float fMaxTargetAngle_y = 75.0f;
    public float fMinTargetAngle_y = 0f;
    public bool bIsNoise = false;
    public Vector3 targetMaxNoise = new Vector3(0.0f, 0.05f, 0.05f);

    public float fTargetHeight = 2.0f;
    public float fMass = 1.0f;

    public LayerMask ignoreLayers = -1;


    private MyCamera mainCamera;

    private RaycastHit hit = new RaycastHit();

    private LayerMask raycastLayers = -1;
    private Vector3 currentVelocity_carAxis = Vector3.zero;

    //Switch to the next camera instance
    public bool bIsSwitchCam = false;
    public enum SwitchCondition { TargetDistance, Duration, FinishTravel, None };
    public SwitchCondition switchCriteria = SwitchCondition.TargetDistance;
    public float switchThreshold = 9999999f;
    private float switchCounter = 0.0f;

    //The main camera gameObject in scene
    public GameObject SceneMainCamera;

    //Return camera properties;not for access
    private Vector3 newCamPos, newTargetPos;

    //Get Drivetrain.cs
    private Drivetrain drivetrainScript = null;

    private Vector3 currentVelocityDir = Vector3.zero;
    private Vector3 prevVelocity = Vector3.zero;

    //Properties for the moving camera
    public AnimationCurve fTravelTimeCurve;

    //Get keyboard input
    private bool bIsAccelKey, bIsBrakeKey;
    private float fAccelKeyTime, fBrakeKeyTime, fReleaseKeyTime;

    public delegate bool InputGetter();
    public delegate void SwitchCounterUpdater();

    private InputGetter accelKeyGetter, brakeKeyGetter;
    private SwitchCounterUpdater switchCounterUpdater;

    public MyCamera CreateCamera()
    {
        return new OrbitCamera(this);
    }

    void Awake()
    {
        drivetrainScript = target.GetComponent<Drivetrain>();
        raycastLayers = ~ignoreLayers;

        //If curveFOV hasn't assigned in inspector
        //if (curveFOV == null || curveFOV.length == 0)
        {
            curveFOV = new AnimationCurve();
            curveFOV.AddKey(0.0f, 55.0f);
            curveFOV.AddKey(1.0f, 72.0f);
        }

        //if (FOVAffectCamDistance == null || FOVAffectCamDistance.length == 0)
        {
            FOVAffectCamDistance = new AnimationCurve();
            FOVAffectCamDistance.AddKey(0.0f, 0.0f);
            FOVAffectCamDistance.AddKey(1.0f, -2.0f);
        }

        //if (PitchCurve == null || PitchCurve.length == 0)
        {
            PitchCurve = new AnimationCurve();
            PitchCurve.AddKey(0.0f, 0.0f);
            PitchCurve.AddKey(0.1f, 8.0f);
            PitchCurve.AddKey(1.0f, 1.0f);
            Keyframe key = new Keyframe(0.1f, 8.0f);
            key.tangentMode = 21;
            key.inTangent = 0;
            key.outTangent = 0;
            PitchCurve.MoveKey(1, key);
        }

        mainCamera = CreateCamera();
        
        accelKeyGetter = () => Input.GetKey(KeyCode.UpArrow);
        brakeKeyGetter = () => Input.GetKey(KeyCode.DownArrow);

        if (fMinTargetAngle_y < 0.0f)
        {
            fMinTargetAngle_y += 360f;
        }

        //Find MainCamera
        if (!multiCameraTest)
        {

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MainCamera"))
            {
                if (obj.activeInHierarchy == true && obj.GetComponent<Camera>())
                {
                    SceneMainCamera = obj;
                }
            }
        }
        else
        {
            /*
            SceneMainCamera = new GameObject("MultiCameraTest_" + name);
            SceneMainCamera.AddComponent<Camera>();
            if (CameraManager.MultiTestCameraInstances == null)
                CameraManager.MultiTestCameraInstances = new List<CarCamera>();
            CameraManager.MultiTestCameraInstances.Add(this);
            */
        }
        MethodInfo minfo = typeof(CarCamera).GetMethod("switchCounterUpdate_" + switchCriteria.ToString("F"), BindingFlags.NonPublic | BindingFlags.Instance);
        switchCounterUpdater = (SwitchCounterUpdater)Delegate.CreateDelegate(typeof(SwitchCounterUpdater), this, minfo, false);
    }

    public void Restart()
    {

        switchCounter = 0.0f;
        bIsSwitchCam = false;
        mainCamera = CreateCamera();
    }

    void FixedUpdate()
    {
        currentVelocityDir = Vector3.Lerp(prevVelocity, target.root.GetComponent<Rigidbody>().velocity, fVelocityDamping * Time.deltaTime);
        currentVelocity_carAxis.x = Vector3.Dot(currentVelocityDir, target.forward);
        currentVelocity_carAxis.y = Vector3.Dot(currentVelocityDir, target.up);
        currentVelocity_carAxis.z = Vector3.Dot(currentVelocityDir, target.right);

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Do something start from here

        mainCamera.Update();

        /// End
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        prevVelocity = currentVelocityDir;
    }

    void LateUpdate()
    {
        newCamPos = mainCamera.getCamLocation();
        newTargetPos = mainCamera.getTargetLocation();

        Vector3 targetDirection = newCamPos - newTargetPos;

        //Is there a wall between target point and camera??
        if (Physics.Raycast(newTargetPos, targetDirection, out hit, targetDirection.magnitude, raycastLayers))
            newCamPos = hit.point;

        cameraUpdate(SceneMainCamera);
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Custom Functions

    //Reading Key inputs and calculating the relative data
    void readInput()
    {
        bIsAccelKey = accelKeyGetter();
        bIsBrakeKey = brakeKeyGetter();

        fAccelKeyTime *= System.Convert.ToSingle(bIsAccelKey);
        fAccelKeyTime += System.Convert.ToSingle(bIsAccelKey) * Time.deltaTime;

        fBrakeKeyTime *= System.Convert.ToSingle(bIsBrakeKey);
        fBrakeKeyTime += System.Convert.ToSingle(bIsBrakeKey) * Time.deltaTime;

        fReleaseKeyTime *= (System.Convert.ToSingle(!bIsAccelKey)) * (System.Convert.ToSingle(!bIsBrakeKey));
        fReleaseKeyTime += (System.Convert.ToSingle(!bIsAccelKey)) * (System.Convert.ToSingle(!bIsBrakeKey)) * Time.deltaTime;
    }

    //Update camera properties;e.g. transform, lookAt
    void cameraUpdate(GameObject camera)
    {
        camera.GetComponent<Camera>().nearClipPlane = nearClipPlane;
        camera.GetComponent<Camera>().farClipPlane = farClipPlane;

        camera.GetComponent<Camera>().fieldOfView = mainCamera.getFOV();

        camera.transform.position = newCamPos;
        camera.transform.LookAt(newTargetPos, (mainCamera.getCamRotation(transform.forward)) * Vector3.up);

        //Switch counter update
        switchCounterUpdater();

        //Check switch threshold
        if (switchCounter > switchThreshold)
        {
            bIsSwitchCam = true;
        }
    }

    //Switch counter Update and check
    void switchCounterUpdate_TargetDistance()
    {
        switchCounter = Vector3.Distance(mainCamera.getCamLocation(), mainCamera.getTargetLocation());
    }

    void switchCounterUpdate_Duration()
    {
        switchCounter += Time.deltaTime;
    }

    void switchCounterUpdate_FinishTravel()
    {
        if (mainCamera.getMovingCameraTravel() >= 1.0f)
        {
            bIsSwitchCam = true;
        }
    }

    void switchCounterUpdate_None()
    {

    }

    public static void SetRelatedRotation(string targetName)
    {
        CarCamera[] allCarCamera = FindObjectsOfType<CarCamera>();
        foreach (CarCamera carCamera in allCarCamera)
        {
            if (carCamera.target.name == targetName && carCamera.cameraType == CameraType.FollowCamera)
                ((OrbitCamera)carCamera.mainCamera).setCamRelatedRotation();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class mSpringSystem
    {
        public List<mSpring> SpringList;
        public bool isAllCriticallyDamped
        {
            get
            {
                return isAllCriticallyDamped;
            }
            set
            {
                foreach (mSpring s in SpringList)
                {
                    s.isCriticallyDamped = isAllCriticallyDamped;
                }
            }
        }

        public mSpringSystem(int number, bool isAllCD = false)
        {
            SpringList = new List<mSpring>();
            for (; number > 0; number--)
            {
                SpringList.Add(new mSpring());
            }
            isAllCriticallyDamped = isAllCD;
        }

        public mSpringSystem(float k1, float k2, float k3, float mass = 1, bool isAllCD = false)
        {
            SpringList = new List<mSpring>();
            SpringList.Add(new mSpring(k1, mass, isAllCD));
            SpringList.Add(new mSpring(k2, mass, isAllCD));
            SpringList.Add(new mSpring(k3, mass, isAllCD));
        }
    }

    public class mSpring
    {
        public float springForce, dampingForce;
        public float k;
        public float c
        {
            get
            {
                return (isCriticallyDamped) ? 2.0f * mass * Mathf.Sqrt(k / 2.0f) : _c;
            }
            set
            {
                _c = value;
            }
        }
        private float _c;
        public float mass;
        public bool isCriticallyDamped;

        public mSpring(float k, float c, float mass, bool isCD = false)
        {
            Initialize(k, c, mass, isCD);
        }

        public mSpring(float k, float mass, bool isCD = false)
        {
            Initialize(k, 0, mass, isCD);
        }

        public mSpring(float k = 1, bool isCD = false)
        {
            Initialize(k, 0, 1, isCD);
        }

        public void Initialize(float k, float c, float mass, bool isCD)
        {
            this.k = k;
            this.c = c;
            this.mass = mass;
            this.isCriticallyDamped = isCD;
        }
    }

    abstract public class MyCamera
    {
        protected CarCamera m_outerClassObject;
        abstract public void Update();
        abstract public Vector3 getCamLocation();

        abstract public Quaternion getCamRotation(Vector3 forward);

        abstract public Vector3 getTargetLocation();

        abstract public float getFOV();

        //for the moving camera
        abstract public float getMovingCameraTravel();
    }

    public class OrbitCamera : MyCamera
    {
        private Vector3 curTargetDir;
        private Queue<Vector3> TargetDirQueue;

        private float fFov = 0.0f;

        //Stores the camera info without further modification; e.g. not changed with FOV
        private Vector3 m_camLocation = Vector3.zero;
        private Vector3 m_camRotation = Vector3.zero;
        private Vector3 m_targetLocation = Vector3.zero;
        private Vector3 m_prevTargetLocation = Vector3.zero;
        private Vector3 m_targetRotation = Vector3.zero;

        private Vector3 m_camRelatedRotation = Vector3.zero;//for car shift to start position

        private Vector3 m_camVelocity = Vector3.zero;

        //Stores the camera info with further changes; i.e. changes won't affect the next frame
        private Vector3 m_camFinalLocation = Vector3.zero;

        //If the input force adding on this frame
        private bool m_bAccelForceAdding = true;
        private bool m_bBrakeForceAdding = true;

        public List<Vector3> OriDirs, kDirs, aDirs;
        public bool isRecordDir = false;

        public OrbitCamera(CarCamera outerObject)
        {
            m_outerClassObject = outerObject;
            m_camLocation = new Vector3(m_outerClassObject.fOriginalDistance, 0.0f, 0.0f);
            m_targetRotation.y = m_outerClassObject.transform.eulerAngles.y;

            //Setting target location
            m_targetLocation = m_outerClassObject.target.position + m_outerClassObject.target.up * m_outerClassObject.fTargetHeight;
            m_prevTargetLocation = m_targetLocation;

            //TestKalmanFilter();
            TargetDirQueue = new Queue<Vector3>();
        }
        
        
        public void UpdateTargerDir()
    {

        switch (m_outerClassObject.FollowRotationMode)
        {
            case FollowRotation.Model:
                curTargetDir = m_outerClassObject.target.eulerAngles + carToWorld(m_outerClassObject.targetOriginalRotation);
                break;
            case FollowRotation.Velocity:
                curTargetDir = getVelocityRot(m_outerClassObject.target.GetComponent<Rigidbody>().velocity, m_outerClassObject.target.up) + carToWorld(m_outerClassObject.targetOriginalRotation);
                break;
            case FollowRotation.Mix:
                m_outerClassObject.FollowRotationMixRatio = new float[3] { m_outerClassObject.FollowRotationMixRatioX, m_outerClassObject.FollowRotationMixRatioY, m_outerClassObject.FollowRotationMixRatioZ };
                Vector3 tempTargetDir = mSlerp(m_outerClassObject.target.eulerAngles, getVelocityRot(m_outerClassObject.target.GetComponent<Rigidbody>().velocity, m_outerClassObject.target.up), m_outerClassObject.FollowRotationMixRatio);
                curTargetDir = tempTargetDir + carToWorld(m_outerClassObject.targetOriginalRotation);
                break;
        }
        if (TargetDirQueue.Count >= m_outerClassObject.kalmanFilterSampleSize)
            TargetDirQueue.Dequeue();

        TargetDirQueue.Enqueue(curTargetDir);
        Vector3 kDir = KalmanFilter();
        if (m_outerClassObject.kalmanFilter)
            curTargetDir = kDir;
    }
        

        //Camera distance modified with spring force when key pressed
        public void setCamDistance_key()
        {
            if (m_outerClassObject.bIsAccelKey && m_bAccelForceAdding)
            {
                if (m_camLocation.x > m_outerClassObject.fOriginalDistance + m_outerClassObject.fMaxAccelDistance)
                {
                    m_bAccelForceAdding = false;
                }
                m_camLocation.x += getDeltaDistance(m_outerClassObject.fAccelKeyForce - springAcceleration(m_outerClassObject.fOriginalDistance, m_camLocation.x, m_outerClassObject.fDistanceSpringConstant), Time.deltaTime);
            }
            else if (m_outerClassObject.bIsBrakeKey && m_bBrakeForceAdding)
            {
                if (m_camLocation.x < m_outerClassObject.fOriginalDistance + m_outerClassObject.fMinAccelDistance)
                {
                    m_bBrakeForceAdding = false;
                }
                m_camLocation.x += getDeltaDistance(-m_outerClassObject.fBrakeKeyForce - springAcceleration(m_outerClassObject.fOriginalDistance, m_camLocation.x, m_outerClassObject.fDistanceSpringConstant), Time.deltaTime);
            }
            else if (!m_outerClassObject.bIsBrakeKey && !m_outerClassObject.bIsAccelKey)
            {
                m_camLocation.x += getDeltaDistance(-springAcceleration(m_outerClassObject.fOriginalDistance, m_camLocation.x, m_outerClassObject.fDistanceSpringConstant), Time.deltaTime);
            }
            if (!m_outerClassObject.bIsAccelKey && !m_bAccelForceAdding)
            {
                m_bAccelForceAdding = true;
            }
            if (!m_outerClassObject.bIsBrakeKey && !m_bBrakeForceAdding)
            {
                m_bBrakeForceAdding = true;
            }
            m_camLocation.x = Mathf.Min(m_camLocation.x, m_outerClassObject.fOriginalDistance + m_outerClassObject.fMaxAccelDistance);
            m_camLocation.x = Mathf.Max(m_camLocation.x, m_outerClassObject.fOriginalDistance + m_outerClassObject.fMinAccelDistance);
            m_camFinalLocation = m_camLocation;
        }

        //Camera FOV modified when velocity changed
        public void setCamFOV_velocity()
        {
            float speedFactor = Mathf.Clamp01(m_outerClassObject.currentVelocity_carAxis.magnitude / m_outerClassObject.fMaxSpeed);
            fFov = m_outerClassObject.curveFOV.Evaluate(speedFactor);
        }

        //Camera distance changing when FOV changed
        public void setCamDistance_FOV()
        {
            float minFOV = m_outerClassObject.curveFOV[0].value;
            float maxFOV = m_outerClassObject.curveFOV[m_outerClassObject.curveFOV.length - 1].value;
            m_camFinalLocation.x += m_outerClassObject.FOVAffectCamDistance.Evaluate((fFov - minFOV) / (maxFOV - minFOV));
        }

        float angleCorrect(float angle)
        {
            if (angle >= 360.0f) angle -= 360.0f;
            if (angle < 0.0f) angle += 360.0f;
            if (Mathf.Min(Mathf.Abs(angle), Mathf.Abs(angle - 360)) < 0.1f)
                angle = 0.01f;
            return angle;
        }

        Vector3 angleCorrect(Vector3 angles)
        {
            angles.x = angleCorrect(angles.x);
            angles.y = angleCorrect(angles.y);
            angles.z = angleCorrect(angles.z);

            return angles;
        }

        void angleCorrect(ref Vector3 angles)
        {
            angles = angleCorrect(angles);
        }

        Vector3 KalmanFilter()
        {
            float[] P = { 1, 1, 1 };
            float[] Q = { 10 ^ -4, 10 ^ -4, 10 ^ -4 };
            float[] R = { 10 ^ -1, 10 ^ -1, 10 ^ -1 };
            List<Vector3> TargetDirList = new List<Vector3>(TargetDirQueue);
            Vector3 kVector = angleCorrect(TargetDirList[0]);
            for (int i = 1; i < TargetDirQueue.Count; i++)
            {
                Vector3 realDir = angleCorrect(TargetDirList[i]);
                for (int j = 0; j < 3; j++)
                {
                    P[j] += Q[j];//
                    float k = P[j] / (P[j] + R[j]);//
                    float angleDiff = realDir[j] - kVector[j];
                    angleDiff = (Mathf.Abs(angleDiff) > 180) ? -Mathf.Sign(angleDiff) * Mathf.Abs((Mathf.Abs(angleDiff) - 360)) : angleDiff;
                    kVector[j] = kVector[j] + k * angleDiff;//
                    kVector[j] = angleCorrect(kVector[j]);
                    P[j] = (1 - k) * P[j];//
                }
            }
            return kVector;
        }

        void TestKalmanFilter()
        {
            float P = 1;
            float Q = 10 ^ -4;
            float R = 10 ^ -1;
            float[] X = { 2, 60, 5, 50, 40, 20, 1, 70, 10, 50 };
            float[] kVector = new float[10];
            kVector[0] = X[0];
            string s = "" + X[0].ToString("F2");
            string s1 = "" + kVector[0].ToString("F2");
            for (int i = 1; i < X.Length; i++)
            {
                kVector[i] = kVector[i - 1];
                float rX = X[i];
                P += Q;
                float k = P / (P + R);
                float angleDiff = rX - kVector[i];
                angleDiff = (Mathf.Abs(angleDiff) > 180) ? -Mathf.Sign(angleDiff) * Mathf.Abs((Mathf.Abs(angleDiff) - 360)) : angleDiff;
                kVector[i] = kVector[i] + k * angleDiff;
                P = (1 - k) * P;
                s += " " + X[i].ToString("F2");
                s1 += " " + kVector[i].ToString("F2");
            }

            Debug.Log(s);
            Debug.Log(s1);
        }

        Vector3 getVelocityRot(Vector3 velocity, Vector3 mUp)
        {
            Vector3 r = Vector3.zero;
            Vector3 defaultRot = new Vector3(0, 0, 0);
            Vector3 rFront = Quaternion.Euler(defaultRot) * Vector3.forward;
            Vector3 vRight = Vector3.Cross(mUp, velocity);
            Vector3 vUp = Vector3.Cross(velocity, vRight);
            r.x = Vector3.Angle(new Vector3(velocity.x, 0, velocity.z), velocity) * -Mathf.Sign(velocity.y);
            r.y = Vector3.Angle(new Vector3(velocity.x, 0, velocity.z), new Vector3(rFront.x, 0, rFront.z)) * Mathf.Sign(Vector3.Cross(Vector3.forward, velocity).y);
            Vector3 nUp = Quaternion.Euler(-r.x, -r.y, 0) * vUp;
            //r.z = Vector3.Angle(vUp, mUp);
            r.z = mUp.z;
            Vector3 r2 = r;
            //return r;
            return Quaternion.LookRotation(velocity, mUp).eulerAngles;
        }

        Vector3 mSlerp(Vector3 r1, Vector3 r2, float[] t)
        {
            angleCorrect(ref r1);
            angleCorrect(ref r2);
            Vector3 r = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                r[i] = Mathf.LerpAngle(r1[i], r2[i], t[i]);
            }
            return r;
        }

        //Rotation at target point when car turned
        //The coordinate system here follow Unity's, except the rotateSpringConstant
        public void setTargetRotation_turn()
        {
            Vector3 carEulerAngles = curTargetDir;
            Vector3 targetAngle = Vector3.zero;

            //clamp to 180 ~ -180
            float clampCX = (carEulerAngles.x > 180.0f ? carEulerAngles.x - 360.0f : carEulerAngles.x);
            float clampMinX = (m_outerClassObject.fMinTargetAngle_y > 180.0f ? m_outerClassObject.fMinTargetAngle_y - 360.0f : m_outerClassObject.fMinTargetAngle_y);
            float clampMaxX = (m_outerClassObject.fMaxTargetAngle_y > 180.0f ? m_outerClassObject.fMaxTargetAngle_y - 360.0f : m_outerClassObject.fMaxTargetAngle_y);
            if (clampCX < clampMinX)
            {
                if (m_outerClassObject.PitchEffect)
                {
                    carEulerAngles.x = m_outerClassObject.fMinTargetAngle_y + m_outerClassObject.PitchCurve.Evaluate((-clampCX - clampMinX) / (clampMaxX - clampMinX));
                }
                else
                {
                    carEulerAngles.x = m_outerClassObject.fMinTargetAngle_y;
                }
            }

            if (clampCX > clampMaxX)
            {
                carEulerAngles.x = m_outerClassObject.fMaxTargetAngle_y;
            }

            targetAngle.x = myDeltaAngle(m_targetRotation.x, carEulerAngles.x);
            targetAngle.y = myDeltaAngle(m_targetRotation.y, carEulerAngles.y);
            targetAngle.z = myDeltaAngle(m_targetRotation.z, carEulerAngles.z);

            m_camVelocity.x += TotalSpringAcceleration(targetAngle.x, m_outerClassObject.rotateSpringConstant.y, m_camVelocity.x, m_outerClassObject.rotateDampingConstant.y) * Time.deltaTime;
            m_camVelocity.y += TotalSpringAcceleration(targetAngle.y, m_outerClassObject.rotateSpringConstant.z, m_camVelocity.y, m_outerClassObject.rotateDampingConstant.z) * Time.deltaTime;
            m_camVelocity.z += TotalSpringAcceleration(targetAngle.z, m_outerClassObject.rotateSpringConstant.x, m_camVelocity.z, m_outerClassObject.rotateDampingConstant.x) * Time.deltaTime;
            m_targetRotation.x += m_camVelocity.x * Time.deltaTime;
            m_targetRotation.y += m_camVelocity.y * Time.deltaTime;
            m_targetRotation.z += m_camVelocity.z * Time.deltaTime;
            angleCorrect(ref m_targetRotation);

            m_camFinalLocation = Quaternion.Euler(0.0f, 0.0f, -m_targetRotation.y) * -Vector3.right * m_camFinalLocation.x;

            Vector3 camForward = Vector3.Cross(m_camFinalLocation.normalized, Vector3.forward);

            /*
            //clamp to 180 ~ -180
            float clampTX = (m_targetRotation.x > 180.0f ? m_targetRotation.x - 360.0f : m_targetRotation.x);
            float clampMinX = (m_outerClassObject.fMinTargetAngle_y > 180.0f ? m_outerClassObject.fMinTargetAngle_y - 360.0f : m_outerClassObject.fMinTargetAngle_y);
            float clampMaxX = (m_outerClassObject.fMaxTargetAngle_y > 180.0f ? m_outerClassObject.fMaxTargetAngle_y - 360.0f : m_outerClassObject.fMaxTargetAngle_y);
            if (clampTX < clampMinX)
            {

                m_targetRotation.x = m_outerClassObject.fMinTargetAngle_y;

                if (m_outerClassObject.PitchEffect)
                {
                    float clampCX = (carEulerAngles.x > 180.0f ? carEulerAngles.x - 360.0f : carEulerAngles.x);
                    m_outerClassObject.PitchEffectValue = m_outerClassObject.fMinTargetAngle_y + m_outerClassObject.PitchCurve.Evaluate((-clampCX - clampMinX) / (clampMaxX - clampMinX));
                    Debug.Log((clampCX - clampMinX) / (clampMaxX - clampMinX));
                }
            }

            if (clampTX > clampMaxX)
            {
                m_targetRotation.x = m_outerClassObject.fMaxTargetAngle_y;
            }
            */
            m_camRelatedRotation = targetAngle;

            m_camFinalLocation = Quaternion.AngleAxis(-m_targetRotation.x, camForward) * m_camFinalLocation;
        }

        public void setCamRelatedRotation()
        {
            UpdateTargerDir();
            m_targetRotation = curTargetDir + m_camRelatedRotation;
        }

        public void setCamRotation()
        {
            Vector3 carEulerAngles = curTargetDir;
            Vector3 camAngle = Vector3.zero;
            camAngle.z = myDeltaAngle(m_camRotation.z, carEulerAngles.z);

            m_camVelocity.z += TotalSpringAcceleration(camAngle.z, m_outerClassObject.rotateSpringConstant.x, m_camVelocity.z, m_outerClassObject.rotateDampingConstant.x) * Time.deltaTime;
            m_camRotation.z += m_camVelocity.z * Time.deltaTime;
            angleCorrect(ref m_camRotation);
        }


        //Calculate the target location using the car current position
        //The shaking compensation is done here
        public void setTargetLocation()
        {
            Vector3 curTargetLocation = m_outerClassObject.target.position + m_outerClassObject.target.up * m_outerClassObject.fTargetHeight;

            Vector3 forward = m_outerClassObject.target.forward;
            Vector3 right = Vector3.Cross(m_outerClassObject.target.GetComponent<Rigidbody>().velocity, m_outerClassObject.target.up).normalized;
            Vector3 up = m_outerClassObject.target.up;

            Vector3 delta_yzPlane = (m_prevTargetLocation - curTargetLocation) - Vector3.Project((m_prevTargetLocation - curTargetLocation), -forward);

            Vector3 targetDeltaDistance = Vector3.zero;

            targetDeltaDistance.y = Mathf.Min(m_outerClassObject.fMaxTargetDistance, Mathf.Abs(Vector3.Dot(delta_yzPlane, right)));
            targetDeltaDistance.z = Mathf.Min(m_outerClassObject.fMaxTargetDistance, Mathf.Abs(Vector3.Dot(delta_yzPlane, up)));

            targetDeltaDistance.y -= getDeltaDistance(springAcceleration(targetDeltaDistance.y, m_outerClassObject.targetSpringConstant.y), Time.deltaTime);
            targetDeltaDistance.z -= getDeltaDistance(springAcceleration(targetDeltaDistance.z, m_outerClassObject.targetSpringConstant.z), Time.deltaTime);

            m_prevTargetLocation = curTargetLocation
                + (Vector3.Project(delta_yzPlane.normalized, right) * targetDeltaDistance.y)
                + (Vector3.Project(delta_yzPlane.normalized, up) * targetDeltaDistance.z);

            if (m_outerClassObject.bIsNoise)
            {
                //Noise
                m_targetLocation = m_prevTargetLocation
                    + (Vector3.Project(delta_yzPlane.normalized, right) * UnityEngine.Random.Range(-m_outerClassObject.targetMaxNoise.y, m_outerClassObject.targetMaxNoise.y))
                    + (Vector3.Project(delta_yzPlane.normalized, up) * UnityEngine.Random.Range(-m_outerClassObject.targetMaxNoise.z, m_outerClassObject.targetMaxNoise.z));
            }
            else
            {
                m_targetLocation = m_prevTargetLocation;
            }
        }

        //Return the distance travelled by the acceleration
        public float getDeltaDistance(float a, float t)
        {
            return 0.5f * a * t * t;
        }

        public float getTargetAngle_kinematicEquation(float s, float a, float t)
        {
            return s - 0.5f * a * t * t;
        }

        public float TotalSpringAcceleration(float distance, float k, float velocity, float c)
        {
            float force = -k * distance - c * velocity;
            return force / m_outerClassObject.fMass;
        }

        //Return the acceleration from force, divided by camera mass
        public float springAcceleration(float distance, float k)
        {
            float force = k * distance;
            return force / m_outerClassObject.fMass;
        }

        //Return the acceleration from force, divided by camera mass
        public float springAcceleration(float x0, float x1, float k)
        {
            float force = k * (x1 - x0);
            return force / m_outerClassObject.fMass;
        }

        public override void Update()
        {
            UpdateTargerDir();

            setCamFOV_velocity();

            m_outerClassObject.readInput();

            setCamDistance_key();
            setCamDistance_FOV();
            setTargetRotation_turn();
            //setCamRotation();
            setTargetLocation();
        }

        public override Vector3 getCamLocation()
        {
            //Changing the coordinate system of cam location
            return m_targetLocation - new Vector3(m_camFinalLocation.y, m_camFinalLocation.z, -m_camFinalLocation.x);
        }

        public override Quaternion getCamRotation(Vector3 forward)
        {
            return Quaternion.AngleAxis(m_targetRotation.z, forward);
        }

        public override Vector3 getTargetLocation()
        {
            return m_targetLocation;
        }

        public override float getFOV()
        {
            if (m_outerClassObject.bIsManualFOV)
            {
                return m_outerClassObject.manualFOV;
            }
            else
            {
                return fFov;
            }
        }

        public override float getMovingCameraTravel()
        {
            throw new NotImplementedException();
        }

        //Used for calculating the delta angle between a and b, including sign
        float myDeltaAngle(float a, float b)
        {
            if (Mathf.Abs(a - b) > 180.0f)
            {
                if (a > b)
                {
                    return a - b - 360f;
                }
                else
                {
                    return a - (b - 360f);
                }
            }
            return a - b;
        }

        //Convert coordinates
        Vector3 worldToCar(Vector3 vec)
        {
            return new Vector3(vec.z, vec.x, vec.y);
        }

        Vector3 carToWorld(Vector3 vec)
        {
            return new Vector3(vec.y, vec.z, vec.x);
        }
    }

    public class FixateCamera : MyCamera
    {
        public FixateCamera(CarCamera outerObject)
        {
            m_outerClassObject = outerObject;
        }

        public override Vector3 getCamLocation()
        {
            return m_outerClassObject.transform.position;
        }

        public override Quaternion getCamRotation(Vector3 forward)
        {
            return Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }

        public override float getFOV()
        {
            return m_outerClassObject.manualFOV;
        }

        public override float getMovingCameraTravel()
        {
            throw new NotImplementedException();
        }

        public override Vector3 getTargetLocation()
        {
            return m_outerClassObject.target.position;
        }

        public override void Update()
        {

        }
    }

    void OnGUI()
    {
        if (ShowStatus)
        {
            GUIStyle guiStyle = new GUIStyle();
            guiStyle.fontSize = 20;
            guiStyle.normal.textColor = Color.red;
            int offsetX = (int)(SceneMainCamera.GetComponent<Camera>().rect.x * Screen.width);
            int offsetY = (int)((1.0f - SceneMainCamera.GetComponent<Camera>().rect.y - 0.5f) * Screen.height);
            GUI.Label(new Rect(offsetX + 5, offsetY + 5, 200, 20), ((multiCameraTest) ? (name + " - ") : "") + FollowRotationMode.ToString() + ((FollowRotationMode == FollowRotation.Mix) ? ":" + FollowRotationMixRatio[0] + ", " + FollowRotationMixRatio[1] + ", " + FollowRotationMixRatio[2] : ""), guiStyle);

            guiStyle.fontSize = 15;
            GUI.Label(new Rect(offsetX + 5, offsetY + 25, 200, 20), "Rotation: " + SceneMainCamera.transform.eulerAngles, guiStyle);
            /*
            GUI.Label(new Rect(offsetX + 5, offsetY + 25, 200, 20), "Spring: " + rotateSpringConstant.ToString(), guiStyle);
            GUI.Label(new Rect(offsetX + 5, offsetY + 45, 200, 20), "Damping: " + rotateDampingConstant.ToString(), guiStyle);
            GUI.Label(new Rect(offsetX + 5, offsetY + 65, 200, 20), "Kalman: " + kalmanFilter + ((kalmanFilter) ? "--- size:" + kalmanFilterSampleSize : ""), guiStyle);
            */
        }

    }
    
}


