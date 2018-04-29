using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

public class LoadAssetIFC : MonoBehaviour {
    const string IFCFile = "IFCBuilding.ifc";
	// Use this for initialization
	void Start () {
        var editor = new XbimEditorCredentials
        {
            ApplicationDevelopersName = "yuke",
            ApplicationFullName = "ifc_to_unity",
            ApplicationIdentifier = "Your app ID",
            ApplicationVersion = "4.0",
            //your user
            EditorsFamilyName = "Luo",
            EditorsGivenName = "Yuke",
            EditorsOrganisationName = "Educational study"
        };

        using (var model = IfcStore.Open(IFCFile))
        {
            var IFCProducts = model.Instances.OfType<IIfcProduct>();
            foreach (var ifcProduct in IFCProducts)
            {
                Debug.Log("ifcproductexpresstype: " + ifcProduct.ExpressType + " ,productlabel: " + ifcProduct.ObjectType +" ,productintID: " + ifcProduct.EntityLabel);
                
                if (ifcProduct.ExpressType.ToString() == "IfcSpace")
                {
                    GameObject go = GameObject.Find(ifcProduct.EntityLabel.ToString());
                    go.SetActive(false);
                }
                      
                
            }
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
