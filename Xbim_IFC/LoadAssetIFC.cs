using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc2x3.MaterialResource;
using System;
using System.Linq;
using UnityEngine.UI;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.PropertyResource;

public class LoadAssetIFC : MonoBehaviour {
    const string IFCFile = "IFCBuilding.ifc";
    private string SelectedIFCProduct;
    private IfcStore model;
    public GameObject UI_prefab;
    private LineRenderer line;
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



        model = IfcStore.Open(IFCFile);
        //store the class as global, impossible to store a class, the reference is lost when using ends by dispose
        //forgo using is a violation of the xbim, wcgw ?
        //openning file that large takes big amount of time its better to reuse it rather than dispose and recreate, 20s freeze when using(open)
        var IFCProducts = model.Instances.OfType<IIfcProduct>();
       
        foreach (var IFCSpace in model.Instances.OfType<IIfcSpace>())
        {
            GameObject go = GameObject.Find(IFCSpace.EntityLabel.ToString());
            go.SetActive(false);
        }
        foreach (var IFCProduct in model.Instances.OfType<IIfcLightFixture>())
        {
            Debug.Log(IFCProduct.ExpressType + IFCProduct.EntityLabel.ToString()+ " Registered");
            GameObject go = GameObject.Find(IFCProduct.EntityLabel.ToString());
            go.name += "_baked";
            Light lt = go.AddComponent<Light>();
            lt.range = 10f;
            lt.shadows = LightShadows.Hard;
            lt.lightmapBakeType = LightmapBakeType.Baked;

            IEnumerable<IfcRelDefinesByProperties> relations = IFCProduct.IsDefinedBy.OfType<IfcRelDefinesByProperties>();
            foreach (IfcRelDefinesByProperties rel in relations)
            {
                IfcPropertySet pSet = rel.RelatingPropertyDefinition as IfcPropertySet;
                if (pSet == null) continue;
                foreach (IfcProperty prop in pSet.HasProperties)
                {
                    IfcPropertySingleValue singleVal = prop as IfcPropertySingleValue;
                    if (singleVal == null) continue;

                    if (singleVal.Name == "Luminous Intensity")
                    {
                        lt.intensity = float.Parse(singleVal.NominalValue.ToString())/10;
                    }
                    else
                    {
                        lt.intensity = 10f;
                    }
                }
            }
        }
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseClick();
        }
    }

    private void OnMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {

            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow, 2f);

            GameObject go = GameObject.Find(hit.transform.name);
            SelectedIFCProduct = go.name;
            Debug.Log("Go hit: " + SelectedIFCProduct);

            //find the spawn point of the info board prefab
            try
            {
                GameObject cl = GameObject.Find("UI_IFCSelect_TextInfo(Clone)").gameObject;
                GameObject ln = GameObject.Find("Line").gameObject;
                if (cl != null)
                    Destroy(cl);
                if (ln != null)
                    Destroy(ln);
            }
            catch{ }

            Debug.DrawRay(hit.point, hit.normal, Color.red, 5f);
            GameObject InfoBoard = Instantiate(UI_prefab, hit.point + hit.normal * 2f, Quaternion.LookRotation(Vector3.Cross(hit.normal, Vector3.up)));
            //Quaternion.LookRotation(Vector3.Cross(hit.normal,Vector3.up))
            //create a new empty gameobject and line renderer component
            line = new GameObject("Line").AddComponent<LineRenderer>();
            //set the number of points to the line
            line.SetPosition(0,hit.point);
            line.SetPosition(1, hit.point+hit.normal);
            line.startColor = Color.cyan;
            //set the width
            line.startWidth = 0.01f;
            

            Text IFC_info = InfoBoard.transform.Find("IFCProduct_text").gameObject.GetComponent<Text>();
            Text IFC_Mat = InfoBoard.transform.Find("IFCMaterial_text").gameObject.GetComponent<Text>();
            Text IFC_Prop = InfoBoard.transform.Find("IFCProperty_text").gameObject.GetComponent<Text>();
            //GameObject myclone = Instantiate(go, transform.position, new Quaternion(0, 0, 0, 0));
            string AirGap="";
            int Product_IntSel;
            if (int.TryParse(SelectedIFCProduct, out Product_IntSel))
            {
                var Product_Sel = model.Instances.FirstOrDefault<IIfcProduct>(w => w.EntityLabel == Product_IntSel);
                var ProductMaterial = Product_Sel.Material;

                IFC_info.text = "GUID: "+ Product_Sel.GlobalId;
                IFC_info.text += "\nName: "+ Product_Sel.Name;
                IFC_info.text += "\nType: "+ Product_Sel.ExpressType;
                IFC_info.text += "\nEntityLabel: "+ Product_Sel.EntityLabel;
                IFC_info.text += "\nDescription: "+ Product_Sel.Description;
                IFC_info.text += "\nMaterial(s):\n--------------------\n";

                IFC_Mat.text = "";
                //read ifc material structure
                try
                {
                    switch (ProductMaterial.ExpressType.ToString())
                    {
                        case "IfcMaterialLayerSetUsage":
                            var IFCProductLayerSetUsage = model.Instances.FirstOrDefault<IIfcMaterialLayerSetUsage>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                            IFC_Mat.text += $"{IFCProductLayerSetUsage.ForLayerSet.LayerSetName}, Total thickness: {IFCProductLayerSetUsage.ForLayerSet.TotalThickness}mm\n";
                            foreach (var Material_layer in IFCProductLayerSetUsage.ForLayerSet.MaterialLayers)
                            {//layer has no name(all optional otherthan thickness), but its associated material has
                                if (Material_layer.IsVentilated.HasValue)
                                    AirGap = "-Air Gap Layer";
                                IFC_Mat.text += $"{Material_layer.Material.Name}{AirGap}, {Material_layer.LayerThickness}mm\n";
                            }
                            break;

                        case "IfcMaterialLayerSet":
                            var IFCProductLayerSet = model.Instances.FirstOrDefault<IIfcMaterialLayerSet>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                            IFC_Mat.text += $"{IFCProductLayerSet.LayerSetName}, Total thickness: {IFCProductLayerSet.TotalThickness}mm\n";
                            foreach (var Material_layer in IFCProductLayerSet.MaterialLayers)
                            {
                                IFC_Mat.text += $"{Material_layer.Material.Name}, {Material_layer.LayerThickness}mm\n";
                            }
                            break;

                        case "IfcMaterialLayer":
                            var IFCProductLayer = model.Instances.FirstOrDefault<IIfcMaterialLayer>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                            IFC_Mat.text += $"{IFCProductLayer.Material.Name}, Thickness: {IFCProductLayer.LayerThickness}mm\n";
                            break;

                        case "IfcMaterial":
                            var IFCProductMaterial = model.Instances.FirstOrDefault<IIfcMaterial>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                            IFC_Mat.text += $"{IFCProductMaterial.Name}\n";
                            break;

                        case "IfcMaterialList":
                            var IFCProductMaterialList = model.Instances.FirstOrDefault<IIfcMaterialList>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                            foreach (var Material in IFCProductMaterialList.Materials)
                            {
                                IFC_Mat.text += $"{Material.Name}\n";
                            }
                            break;

                        default:// more stuctual types coming soon
                            break;
                    }

                }
                catch (Exception e) { throw e; }

                //read ifc TypeObject properties
                try
                {
                    IFC_Prop.text = "Properties";
                    IEnumerable<IfcRelDefinesByProperties> relations = Product_Sel.IsDefinedBy.OfType<IfcRelDefinesByProperties>();
                    foreach (IfcRelDefinesByProperties rel in relations)
                    {
                        IfcPropertySet pSet = rel.RelatingPropertyDefinition as IfcPropertySet;
                        if (pSet == null) continue;
                        foreach (IfcProperty prop in pSet.HasProperties)
                        {
                            IfcPropertySingleValue singleVal = prop as IfcPropertySingleValue;
                            if (singleVal == null) continue;
                            IFC_Prop.text += $"\n{singleVal.Name}, {singleVal.NominalValue}";
                            try
                            {
                                IFC_Prop.text += $", {singleVal.Unit.FullName}";
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                { throw ex; }
                



            }


        }
        else Debug.Log("hit nothing");
    }
}
