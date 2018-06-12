using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnoFacing : MonoBehaviour {
    private GameObject SecondCamera;
    // Use this for initialization
    void Start () {
        SecondCamera = GameObject.Find("CameraOrbit").transform.Find("SecondCamera").gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.LookRotation(transform.position - SecondCamera.transform.position);
    }
}
