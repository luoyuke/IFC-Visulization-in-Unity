using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Button_Property : MonoBehaviour {
    
    private int Sel_original_layer;
    private int CullingMask;
    private GameObject go_clone;

    public void IsDisplayProp(bool ProIsPropertyClickedp)
    {
        Text IFC_info = GameObject.Find("IFCProduct_text").GetComponent<Text>();
        Text IFC_Mat = GameObject.Find("IFCMaterial_text").GetComponent<Text>();
        Text IFC_Prop = GameObject.Find("IFCProperty_text").GetComponent<Text>();

        Button bt = GameObject.Find("UI_IFCSelect_TextInfo(Clone)").transform.Find("Button").GetComponent<Button>();
        Button bt_b = GameObject.Find("UI_IFCSelect_TextInfo(Clone)").transform.Find("Button_back").GetComponent<Button>();
        

        if (ProIsPropertyClickedp)
        {
            IFC_Prop.color = new Color(255f, 255f, 255f, 1f);
            IFC_info.color = new Color(255f, 255f, 255f, 0f);
            IFC_Mat.color = new Color(255f, 255f, 255f, 0f);
            
            bt.gameObject.SetActive(false);
            bt_b.gameObject.SetActive(true);
        }
        else
        {
            IFC_Prop.color = new Color(255f, 255f, 255f, 0f);
            IFC_info.color = new Color(255f, 255f, 255f, 1f);
            IFC_Mat.color = new Color(255f, 255f, 255f, 1f);

            bt.gameObject.SetActive(true);
            bt_b.gameObject.SetActive(false);
        }
       
    }

    public void IsInspecting(bool click)
    {
        GameObject go = GameObject.Find("Initializing emvironment");
        LoadAssetIFC IfcScript = go.GetComponent<LoadAssetIFC>();

        GameObject SecondCamera = GameObject.Find("CameraOrbit").transform.Find("SecondCamera").gameObject;

        Button bt = GameObject.Find("UI_IFCSelect_TextInfo(Clone)").transform.Find("Button_Inspect").GetComponent<Button>();
        Button bt_b = GameObject.Find("UI_IFCSelect_TextInfo(Clone)").transform.Find("Button_Inspect_back").GetComponent<Button>();

        if (click)
        {
            //Sel_original_layer = IfcScript.RayCastSelect.layer;
            //IfcScript.RayCastSelect.layer = 10;
            go_clone = Instantiate(IfcScript.RayCastSelect, IfcScript.RayCastSelect.transform);
            go_clone.layer = 10;
            go_clone.transform.parent = GameObject.Find("UI_IFCSelect_TextInfo(Clone)").transform;

            CullingMask = SecondCamera.GetComponent<Camera>().cullingMask;
            SecondCamera.GetComponent<Camera>().cullingMask = 1<<10;

            bt.gameObject.SetActive(false);
            bt_b.gameObject.SetActive(true);
        }
        else
        {
            //IfcScript.RayCastSelect.layer = Sel_original_layer;
            SecondCamera.GetComponent<Camera>().cullingMask = CullingMask;
            Destroy(go_clone);

            bt.gameObject.SetActive(true);
            bt_b.gameObject.SetActive(false);
        }
    }

    }
