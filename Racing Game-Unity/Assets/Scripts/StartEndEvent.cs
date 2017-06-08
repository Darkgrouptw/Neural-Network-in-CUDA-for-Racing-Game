using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartEndEvent : MonoBehaviour {

    void OnTriggerEnter(Collider car)
    {
        if(car.tag == "Player")
        {
            car.GetComponentInChildren<DataRecorder>().writeFile();
            print("Data saved");
        }
    }
}
