using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFollow : MonoBehaviour {

    public Transform CameraOrbit;
    private Transform Target;

    public GameObject scrb;
    public Transform joistk; // empty image to catch mouse drag 

    public float sensitivity;
    void Start()
    {
        Target = GameObject.Find("WexbimModel").transform;
        CameraOrbit.position = Target.position;
    }
    // Update is called once per frame
    void Update ()
    {
        Vector3 current = joistk.position;

        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0);

        transform.LookAt(Target.position);

        gameObject.GetComponent<Camera>().orthographicSize = 50f-scrb.GetComponent<Slider>().value;

        CameraOrbit.rotation = Quaternion.Euler( (current.y) * sensitivity, (current.x) * sensitivity, 0f);

    }
}
