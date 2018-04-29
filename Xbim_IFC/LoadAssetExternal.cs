using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadAssetExternal : MonoBehaviour {

    private string FBX_path = "C:/Users/luoyu/Documents/Revit_project/2018_03_Revit_Fbxport";
    private string FBX_name = "BuildingFbx_3ds.fbx";

    // Use this for initialization
    void Start ()
    {
        /* load AssetBundle
        AssetBundle bundle = AssetBundle.LoadFromFile(FBX_path);
        GameObject building = Instantiate(bundle.LoadAsset<GameObject>(FBX_name)) as GameObject;
        Debug.Log("load:" + bundle.ToString());
        */


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
