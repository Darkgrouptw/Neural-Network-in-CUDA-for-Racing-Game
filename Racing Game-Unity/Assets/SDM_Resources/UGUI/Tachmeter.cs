using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tachmeter : MonoBehaviour {
    public Drivetrain Target;
    public Text Speed;
    public Image Gear;
    public Image RPM;

    public Sprite[] GearImage;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Speed.text = "" + (int)(Target.GetComponent<Rigidbody>().velocity.magnitude * 3.6f);
        if((Target.gear - 2) >= 0 && (Target.gear - 2) < GearImage.Length)
            Gear.sprite = GearImage[Target.gear - 2];
        RPM.fillAmount = Target.rpm / Target.maxRPM * 0.76f;
    }
}
