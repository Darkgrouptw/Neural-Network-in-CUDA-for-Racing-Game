using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stablizer : MonoBehaviour 
{
    private RaycastHit hit;
  

    private Quaternion lastQuater;
    void Start()
    {
        lastQuater = this.transform.rotation;
    }

    void Update()
    {
        this.transform.rotation = GetRotation();
        lastQuater = this.transform.rotation;
    }
    Quaternion GetRotation()
    {
        return Quaternion.Slerp(lastQuater, this.transform.rotation, 0.5f);
    }

    public void ReCached()
    {
        lastQuater = this.transform.rotation;
    }
}
