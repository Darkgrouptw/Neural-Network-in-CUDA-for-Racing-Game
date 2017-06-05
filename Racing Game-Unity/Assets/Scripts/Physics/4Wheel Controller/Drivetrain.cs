using UnityEngine;
using System.Collections;

// 模擬引擎
// 施加 Torque 給 Wheel
[RequireComponent(typeof(CurveManager))]
public class Drivetrain : MonoBehaviour 
{
	// 要被驅動的輪子
    public Wheel[] poweredWheels;
    private float ForwardMaxSpeed;
    private float BackwardMaxSpeed;
    private float NitrousUpSpeed;
    private const float SpeedGap = 50;
	
	// The gear ratios, including neutral (0) and reverse (negative) gears
	public float[] gearRatios;
	
	// The final drive ratio, which is multiplied to each gear ratio
	public float finalDriveRatio = 3.23f;

    // 轉動的 RPM
	public float minRPM = 800;
	public float maxRPM = 15000;


    // engine inertia (how fast the engine spins up), in kg * m^2
    // 引擎的慣性
    public float engineInertia = 0.3f;
	
	// engine's friction coefficients - these cause the engine to slow down, and cause engine braking.

	// constant friction coefficient
	public float engineBaseFriction = 25f;
	// linear friction coefficient (higher friction when engine spins higher)
	public float engineRPMFriction = 0.02f;

	// Engine orientation (typically either Vector3.forward or Vector3.right). 
	// This determines how the car body moves as the engine revs up.	
	public Vector3 engineOrientation = Vector3.forward;
	
	// Coefficient determining how muchg torque is transfered between the wheels when they move at 
	// different speeds, to simulate differential locking.
	public float differentialLockCoefficient = 0;
	
	// inputs
	// engine throttle
	public float throttle = 0;
	// engine throttle without traction control (used for automatic gear shifting)
	public float throttleInput = 0;
	
	// shift gears automatically?
	public bool automatic = true;

	// state
	public int gear = 2;
	public float rpm;
	public float slipRatio = 0.0f;
	float engineAngularVelo;
    public float torqueFactor2 = 1.0f; 

    private GUIStyle style;

    private AnimationCurve[] EngineTorque;

	float Sqr (float x) { return x*x; }

    // SDM -------------------------------------------------------------------------

    // state
    [System.NonSerialized]
    public bool bShiftUp = false;
    [System.NonSerialized]
    public bool bShiftDown = false;
    [System.NonSerialized]
    public float brake = 0;

    // Level 設定
    private float MaxLevel;
    private const float MaxSpeedGap = 100;
    private float SpeedLevel;
    private float AccelLevel;

    // NitrousFactor
    private float MaxNitrousFactor = 2.0f;

    // 利用 Curve 來拉 Torque
    public float CalcEngineTorque()
    {
        // 大於0會屬於前進，等於0屬於後退
        float AccelFactor, NitrousFactor, currentSpeed;
        AccelFactor = (float)Mathf.Clamp(1, AccelLevel, MaxLevel) / MaxLevel;
        NitrousFactor = 1;

        // 現在的速度
        currentSpeed = this.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        if (gear > 0)
        {
            float FinalForwardMaxSpeed = MaxSpeedGap * (float)Mathf.Clamp(1, SpeedLevel, MaxLevel) / MaxLevel + ForwardMaxSpeed - MaxSpeedGap;

            // 是否有用到氮氣
            if (CarController.UseNitrous)
            {
                FinalForwardMaxSpeed += NitrousUpSpeed;

                if (currentSpeed < ForwardMaxSpeed)
                    NitrousFactor = (1 - (float)Mathf.Clamp(1, currentSpeed, ForwardMaxSpeed) / ForwardMaxSpeed) * MaxNitrousFactor + 1;
            }

            float FixedTorque = Mathf.Clamp01((FinalForwardMaxSpeed - currentSpeed) / 50) * AccelFactor * NitrousFactor;
            return torqueFactor2 * EngineTorque[gear - 1].Evaluate(rpm) * FixedTorque;
        }
        else
        {
            float FixedTorque = Mathf.Clamp01((BackwardMaxSpeed - currentSpeed) / 50) * AccelFactor;
            return torqueFactor2 * EngineTorque[gear].Evaluate(rpm) * FixedTorque;
        }
    }

    void Start()
    {
        CurveManager cm = this.GetComponent<CurveManager>();
        CarController cc = this.GetComponent<CarController>();
        EngineTorque = cm.gear;
        ForwardMaxSpeed = cc.ForwardMaxSpeed;
        BackwardMaxSpeed = cc.BackwardMaxSpeed;
        NitrousUpSpeed = cc.NitrousUpSpeed;

        SpeedLevel = this.GetComponent<CarController>().SpeedLevel;
        AccelLevel = this.GetComponent<CarController>().AccelLevel;
        MaxLevel = this.GetComponent<CarController>().MaxLevel;
    }
	
	void FixedUpdate () 
	{
		float ratio = gearRatios[gear] * finalDriveRatio;
		float inertia = engineInertia * Sqr(ratio);
		float engineFrictionTorque = engineBaseFriction + rpm * engineRPMFriction;
		float engineTorque = (CalcEngineTorque() + Mathf.Abs(engineFrictionTorque)) * throttle;
		slipRatio = 0.0f;		
		
		if (ratio == 0)
		{
			// Neutral gear - just rev up engine
			float engineAngularAcceleration = (engineTorque-engineFrictionTorque) / engineInertia;
			engineAngularVelo += engineAngularAcceleration * Time.deltaTime;
			
			// Apply torque to car body
            
			GetComponent<Rigidbody>().AddTorque(-engineOrientation * engineTorque);
        }
		else
		{
			float drivetrainFraction = 1.0f/poweredWheels.Length;
			float averageAngularVelo = 0;	
			foreach(Wheel w in poweredWheels)
				averageAngularVelo += w.angularVelocity * drivetrainFraction;

			// Apply torque to wheels
			foreach(Wheel w in poweredWheels)
			{
				float lockingTorque = (averageAngularVelo - w.angularVelocity) * differentialLockCoefficient;
				w.drivetrainInertia = inertia * drivetrainFraction;
				w.driveFrictionTorque = engineFrictionTorque * Mathf.Abs(ratio) * drivetrainFraction;
				w.driveTorque = engineTorque * ratio * drivetrainFraction + lockingTorque;

				slipRatio += w.slipRatio * drivetrainFraction;
			}
			
			// update engine angular velo
			engineAngularVelo = averageAngularVelo * ratio;
		}
		
		// update state
		slipRatio *= Mathf.Sign ( ratio );
		rpm = engineAngularVelo * (60.0f / (2*Mathf.PI));
		
		// very simple simulation of clutch - just pretend we are at a higher rpm.
		float minClutchRPM = minRPM;
		if (gear == 2)
			minClutchRPM += throttle * 3000;
        if (rpm < minClutchRPM)
            rpm = minClutchRPM;
        else if (rpm >= maxRPM)
            rpm = maxRPM;
			
		// Automatic gear shifting. Bases shift points on throttle input and rpm.
        if (automatic)
        {
            if (throttleInput > 0.8f && rpm >= maxRPM * (0.5f + 0.5f * throttleInput) && gear < EngineTorque.Length)
                ShiftUp();
            else if ((rpm <= maxRPM * (0.25f + 0.4f * throttleInput) || throttleInput < 0) && gear > 2)
                ShiftDown();
            if (throttleInput < 0 && rpm <= minRPM)
                gear = (gear == 0 ? 2 : 0);
        }
    }
		
	public void ShiftUp () {
		if (gear < gearRatios.Length - 1)
            gear ++;
	}

	public void ShiftDown () {
		if (gear > 0)
			gear --;
	}
}
