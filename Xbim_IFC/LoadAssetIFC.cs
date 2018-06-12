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
using UnityEngine.EventSystems;
using Xbim.Ifc4.MeasureResource;

public class LoadAssetIFC : MonoBehaviour {
    const string IFCFile = "IFCBuilding.ifc";
    private string SelectedIFCProduct;
    public IfcStore model;
    public GameObject UI_prefab;
    public GameObject RayCastSelect;

    private LineRenderer line;
    private int SecondCamCullMask;


    public bool Filter_IsArch = true;
    public bool Filter_IsMep = true;
    public bool Filter_IsStru = true;
    public bool Filter_DayNight = true;
    public bool Filter_IsSpace = false;
    public bool Filter_IsLighting = false;
    public List<Sorting_IFCSpace> SpaceList = new List<Sorting_IFCSpace>();

    public class Sorting_IFCSpace
    {
        private IfcLabel? Name;
        private int EntityLabel;
        private IfcValue CoolingLoad;
        private IfcValue HeatingLoad;
        private IfcValue LightingLoad;
        private IfcValue PowerLoad;
        private IfcValue CoolingLoad_pa;
        private IfcValue HeatingLoad_pa;
        private IfcValue LightingLoad_pa;
        private IfcValue PowerLoad_pa;

        public Sorting_IFCSpace(IfcLabel? Name, int EntityLabel, IfcValue CoolingLoad, IfcValue HeatingLoad, IfcValue LightingLoad, IfcValue PowerLoad, IfcValue CoolingLoad_pa, IfcValue HeatingLoad_pa, IfcValue LightingLoad_pa, IfcValue PowerLoad_pa)
        {
            this.Name = Name;
            this.EntityLabel = EntityLabel;
            this.CoolingLoad = CoolingLoad;
            this.HeatingLoad = HeatingLoad;
            this.LightingLoad = LightingLoad;
            this.PowerLoad = PowerLoad;
            this.CoolingLoad_pa = CoolingLoad_pa;
            this.HeatingLoad_pa = HeatingLoad_pa;
            this.LightingLoad_pa = LightingLoad_pa;
            this.PowerLoad_pa = PowerLoad_pa;
        }

        public Sorting_IFCSpace() { }

        public IfcLabel? C_Name
        {
            get { return Name; }
            set { Name = value; }
        }

        public int C_EntityLabel
        {
            get { return EntityLabel; }
            set { EntityLabel = value; }
        }

        public IfcValue C_CoolingLoad
        {
            get { return CoolingLoad; }
            set { CoolingLoad = value; }
        }
        public IfcValue C_HeatingLoad
        {
            get { return HeatingLoad; }
            set { HeatingLoad = value; }
        }
        public IfcValue C_PowerLoad
        {
            get { return PowerLoad; }
            set { PowerLoad = value; }
        }
        public IfcValue C_LightingLoad
        {
            get { return LightingLoad; }
            set { LightingLoad = value; }
        }
        public IfcValue C_CoolingLoad_pa
        {
            get { return CoolingLoad_pa; }
            set { CoolingLoad_pa = value; }
        }
        public IfcValue C_HeatingLoad_pa
        {
            get { return HeatingLoad_pa; }
            set { HeatingLoad_pa = value; }
        }
        public IfcValue C_LightingLoad_pa
        {
            get { return LightingLoad_pa; }
            set { LightingLoad_pa = value; }
        }
        public IfcValue C_PowerLoad_pa
        {
            get { return PowerLoad_pa; }
            set { PowerLoad_pa = value; }
        }

        public double ITS { get; set; }
    };
    // Use this for initialization
    void Start () {
        GameObject SecondCamera = GameObject.Find("CameraOrbit").transform.Find("SecondCamera").gameObject;
        SecondCamCullMask = SecondCamera.GetComponent<Camera>().cullingMask;

        /* no need to save in ifc - read only
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
        };*/
        model = IfcStore.Open(IFCFile);
        //store the class as global, impossible to store a class, the reference is lost when using ends by dispose
        //forgo using is a violation of the xbim, wcgw ?
        //openning file that large takes big amount of time its better to reuse it rather than dispose and recreate, 20s freeze when using(open)

        GameObject Togo = GameObject.Find("Tablet_UI"); //block filter function before initializing is done
        Togo.transform.Find("Filter_Arch").GetComponent<Toggle>().interactable = false;
        Togo.transform.Find("Filter_MEP").GetComponent<Toggle>().interactable = false;
        Togo.transform.Find("Filter_Stru").GetComponent<Toggle>().interactable = false;
        Togo.transform.Find("Switch_Day").GetComponent<Toggle>().interactable = false;
        Togo.transform.Find("Filter_Space").GetComponent<Toggle>().interactable = false;
        Togo.transform.Find("Switch_Light").GetComponent<Toggle>().interactable = false;
        Togo.transform.Find("Dropdown_SpaceSorting").GetComponent<Dropdown>().interactable = false;
        Togo.transform.Find("Button_ThermHygro").GetComponent<Button>().interactable = false;
        StartCoroutine(YieldingWork());
    }

    //coroutine for background initializing
    IEnumerator YieldingWork() 
    {
        GameObject Togo = GameObject.Find("Tablet_UI");
        bool workDone = false;
        yield return null;

        while (!workDone)
        {
            Debug.Log("Coro starts: ");

            // Do Work... yield return null;
            Physics.IgnoreLayerCollision(8, 9);//noclid with player
            //Architect Taging
            foreach (var IFCDoor in model.Instances.OfType<IIfcDoor>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCDoor.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {
                        goo.tag = "Arch";
                        goo.layer = 9;//noclid layer for door/space must have collider for raycast
                    }

                }
                catch { }
                yield return null;
            }
            foreach (var IFCSpace in model.Instances.OfType<IIfcSpace>())
            {
                IfcValue CoolingLoad, HeatingLoad, LightingLoad, PowerLoad, CoolingLoad_pa, HeatingLoad_pa, LightingLoad_pa, PowerLoad_pa;
                CoolingLoad = HeatingLoad = LightingLoad = PowerLoad = CoolingLoad_pa = HeatingLoad_pa = LightingLoad_pa = PowerLoad_pa = new IfcReal();

                bool isSpace = false;
                Sorting_IFCSpace mySpace = new Sorting_IFCSpace();
                IEnumerable<IfcRelDefinesByProperties> relations = IFCSpace.IsDefinedBy.OfType<IfcRelDefinesByProperties>();
                foreach (IfcRelDefinesByProperties rel in relations)
                {
                    IfcPropertySet pSet = rel.RelatingPropertyDefinition as IfcPropertySet;
                    if (pSet == null) continue;
                    foreach (IfcProperty prop in pSet.HasProperties)
                    {
                        IfcPropertySingleValue singleVal = prop as IfcPropertySingleValue;
                        if (singleVal == null) continue;
                        switch (singleVal.Name)
                        {
                            case "Reference":
                                if (singleVal.NominalValue.ToString().Contains("Space"))
                                    isSpace = true;
                                break;
                            case "Calculated Cooling Load":
                                CoolingLoad = singleVal.NominalValue;
                                break;
                            case "Calculated Heating Load":
                                HeatingLoad = singleVal.NominalValue;
                                break;
                            case "Specified Lighting Load":
                                LightingLoad = singleVal.NominalValue;
                                break;
                            case "Specified Power Load":
                                PowerLoad = singleVal.NominalValue;
                                break;
                            case "Calculated Cooling Load per area":
                                CoolingLoad_pa = singleVal.NominalValue;
                                break;
                            case "Calculated Heating Load per area":
                                HeatingLoad_pa = singleVal.NominalValue;
                                break;
                            case "Specified Lighting Load per area":
                                LightingLoad_pa = singleVal.NominalValue;
                                break;
                            case "Specified Power Load per area":
                                PowerLoad_pa = singleVal.NominalValue;
                                break;
                            default:
                                break;
                        }
                    }
                }

                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCSpace.EntityLabel.ToString());
                if (!isSpace)
                {
                    try
                    {
                        foreach (GameObject goo in objects)
                        {
                            goo.SetActive(false);
                        }
                    }
                    catch { }
                }
                else try
                    {
                        SpaceList.Add(new Sorting_IFCSpace(IFCSpace.Name, IFCSpace.EntityLabel, CoolingLoad, HeatingLoad, LightingLoad, PowerLoad, CoolingLoad_pa, HeatingLoad_pa, LightingLoad_pa, PowerLoad_pa));

                        foreach (GameObject goo in objects)
                        {
                            goo.tag = "XRay";
                            goo.layer = 9;
                            goo.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                    catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcCurtainWall>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {
                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcChimney>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {
                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcRoof>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {
                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcWall>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcWindow>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcPlate>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcFurniture>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcMember>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcBuildingElementProxy>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcShadingDevice>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Arch";
                    }
                }
                catch { }
                yield return null;
            }

            //MEP Taging
            foreach (var IFCprop in model.Instances.OfType<IIfcDistributionElement>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {
                        goo.tag = "MEP";
                    }
                }
                catch { }
                yield return null;
            }
            //Lighting is part of MEP component, to overwrite MEP to Arch here for Arch's Convience
            foreach (var IFCProduct in model.Instances.OfType<IIfcLightFixture>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCProduct.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Arch_light"; //because the name change, can't reference them with name later to tag them.
                        Light lt = goo.AddComponent<Light>();
                        lt.range = 10f;
                        lt.shadows = LightShadows.Hard;
                        lt.intensity = .5f;
                        goo.GetComponent<Light>().enabled = false;

                    }
                }
                catch { }
                yield return null;
            }
            
            //Structual Taging
            foreach (var IFCprop in model.Instances.OfType<IIfcBeam>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";

                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcFooting>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcColumn>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";

                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcSlab>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";

                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcRailing>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcRamp>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";

                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcStair>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcStairFlight>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcCovering>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcSite>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcPile>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";
                    }
                }
                catch { }
                yield return null;
            }
            foreach (var IFCprop in model.Instances.OfType<IIfcRampFlight>())
            {
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == IFCprop.EntityLabel.ToString());
                try
                {
                    foreach (GameObject goo in objects)
                    {

                        goo.tag = "Stru";
                    }
                }
                catch { }
                yield return null;
            }

            Togo.transform.Find("Filter_Arch").GetComponent<Toggle>().interactable = true;
            Togo.transform.Find("Filter_MEP").GetComponent<Toggle>().interactable = true;
            Togo.transform.Find("Filter_Stru").GetComponent<Toggle>().interactable = true;
            Togo.transform.Find("Switch_Day").GetComponent<Toggle>().interactable = true;
            Togo.transform.Find("Filter_Space").GetComponent<Toggle>().interactable = true;
            Togo.transform.Find("Switch_Light").GetComponent<Toggle>().interactable = true;
            Togo.transform.Find("Dropdown_SpaceSorting").GetComponent<Dropdown>().interactable = true;
            workDone = true; //Go taging completes
        }
        if (workDone)
            Debug.Log("Coro completes in " + Time.time);
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
    {//Input.mousePosition

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {

                Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow, 2f);

                GameObject go = GameObject.Find(hit.transform.name);
                SelectedIFCProduct = go.name;
                Debug.Log("Go hit: " + SelectedIFCProduct);

                RayCastSelect = go;
                //find the spawn point of the info board prefab
                try
                {
                    GameObject cl = GameObject.Find("UI_IFCSelect_TextInfo(Clone)").gameObject;
                    GameObject ln = GameObject.Find("Line").gameObject;
                    if (cl != null)
                    {
                        Destroy(cl);
                    }
                    if (ln != null)
                        Destroy(ln);
                }
                catch { }

                try
                {
                    GameObject cw = GameObject.Find("CompoundWall").gameObject;
                    if (cw != null)
                    {
                        GameObject SecondCamera = GameObject.Find("CameraOrbit").transform.Find("SecondCamera").gameObject;
                        SecondCamera.GetComponent<Camera>().cullingMask = SecondCamCullMask;
                        Destroy(cw);
                    }
                    GameObject LastBatchLine = GameObject.Find("Thermo_Liner").gameObject;
                    if (LastBatchLine != null)
                        Destroy(LastBatchLine);
                    
                    GameObject LastBatchLine2 = GameObject.Find("Hygro_Liner").gameObject;
                    if (LastBatchLine2 != null)
                        Destroy(LastBatchLine2);
                }
                catch { }

                Debug.DrawRay(hit.point, hit.normal, Color.red, 5f);
                GameObject InfoBoard = Instantiate(UI_prefab, hit.point + hit.normal * 2f, Quaternion.LookRotation(Vector3.Cross(hit.normal, Vector3.up)));
                //Quaternion.LookRotation(Vector3.Cross(hit.normal,Vector3.up))
                //create a new empty gameobject and line renderer component
                line = new GameObject("Line").AddComponent<LineRenderer>();
                //set the number of points to the line
                line.SetPosition(0, hit.point);
                line.SetPosition(1, hit.point + hit.normal);
                line.startColor = Color.cyan;
                //set the width
                line.startWidth = 0.01f;
                Material mat = new Material(Shader.Find("Standard"));
                line.material = mat;

                Text IFC_info = InfoBoard.transform.Find("IFCProduct_text").gameObject.GetComponent<Text>();
                Text IFC_Mat = InfoBoard.transform.Find("IFCMaterial_text").gameObject.GetComponent<Text>();
                Text IFC_Prop = InfoBoard.transform.Find("Scrollrect").Find("IFCProperty_text").gameObject.GetComponent<Text>();
                IFC_Prop.color = new Color(0, 0, 0, 0);


                //GameObject myclone = Instantiate(go, transform.position, new Quaternion(0, 0, 0, 0));
                string AirGap = "";
                int Product_IntSel;
                if (int.TryParse(SelectedIFCProduct, out Product_IntSel))
                {
                    var Product_Sel = model.Instances.FirstOrDefault<IIfcProduct>(w => w.EntityLabel == Product_IntSel);
                    var ProductMaterial = Product_Sel.Material;
                    GameObject.Find("Tablet_UI").transform.Find("Button_ThermHygro").GetComponent<Button>().interactable = true;

                    IFC_info.text = "GUID: " + Product_Sel.GlobalId;
                    IFC_info.text += "\nName: " + Product_Sel.Name;
                    IFC_info.text += "\nType: " + Product_Sel.ExpressType;
                    IFC_info.text += "\nEntityLabel: " + Product_Sel.EntityLabel;
                    IFC_info.text += "\nDescription: " + Product_Sel.Description;
                    IFC_info.text += "\nMaterial(s):\n--------------------\n";

                    IFC_Mat.text = "";
                    //read ifc material structure
                    if (ProductMaterial != null)
                    {
                        try
                        {
                            switch (ProductMaterial.ExpressType.ToString())
                            {
                                case "IfcMaterialLayerSetUsage":
                                    var IFCProductLayerSetUsage = model.Instances.FirstOrDefault<IIfcMaterialLayerSetUsage>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                                    IFC_Mat.text += $"{IFCProductLayerSetUsage.ForLayerSet.LayerSetName}, Total thickness: {IFCProductLayerSetUsage.ForLayerSet.TotalThickness}\n";
                                    foreach (var Material_layer in IFCProductLayerSetUsage.ForLayerSet.MaterialLayers)
                                    {//layer has no name(all optional otherthan thickness), but its associated material has
                                        if (Material_layer.IsVentilated.HasValue)
                                            AirGap = "-Air Gap Layer";
                                        IFC_Mat.text += $"{Material_layer.Material.Name}{AirGap}, {Material_layer.LayerThickness}\n";
                                    }
                                    break;

                                case "IfcMaterialLayerSet":
                                    var IFCProductLayerSet = model.Instances.FirstOrDefault<IIfcMaterialLayerSet>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                                    IFC_Mat.text += $"{IFCProductLayerSet.LayerSetName}, Total thickness: {IFCProductLayerSet.TotalThickness}\n";
                                    foreach (var Material_layer in IFCProductLayerSet.MaterialLayers)
                                    {
                                        IFC_Mat.text += $"{Material_layer.Material.Name}, {Material_layer.LayerThickness}\n";
                                    }
                                    break;

                                case "IfcMaterialLayer":
                                    var IFCProductLayer = model.Instances.FirstOrDefault<IIfcMaterialLayer>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                                    IFC_Mat.text += $"{IFCProductLayer.Material.Name}, Thickness: {IFCProductLayer.LayerThickness}\n";
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
                    }
                    else
                    {
                        foreach (var type in Product_Sel.IsTypedBy)
                        {
                            var MaterialLayerSet = model.Instances.FirstOrDefault<IIfcMaterialLayerSet>(m => m.EntityLabel == type.RelatingType.Material.EntityLabel);
                            if (MaterialLayerSet != null)
                            {
                                IFC_Mat.text += $"{MaterialLayerSet.LayerSetName}, Total thickness: {MaterialLayerSet.TotalThickness}\n";
                                foreach (var Material_layer in MaterialLayerSet.MaterialLayers)
                                {
                                    IFC_Mat.text += $"{Material_layer.Material.Name}, {Material_layer.LayerThickness}\n";
                                }
                            }

                            var MaterialLayerSetUsage = model.Instances.FirstOrDefault<IIfcMaterialLayerSetUsage>(m => m.EntityLabel == type.RelatingType.Material.EntityLabel);
                            if (MaterialLayerSetUsage != null)
                            {
                                IFC_Mat.text += $"{MaterialLayerSetUsage.ForLayerSet.LayerSetName}, Total thickness: {MaterialLayerSetUsage.ForLayerSet.TotalThickness}\n";
                                foreach (var Material_layer in MaterialLayerSetUsage.ForLayerSet.MaterialLayers)
                                {//layer has no name(all optional otherthan thickness), but its associated material has
                                    if (Material_layer.IsVentilated.HasValue)
                                        AirGap = "-Air Gap Layer";
                                    IFC_Mat.text += $"{Material_layer.Material.Name}{AirGap}, {Material_layer.LayerThickness}\n";
                                }
                            }

                            var MaterialLayer = model.Instances.FirstOrDefault<IIfcMaterialLayer>(m => m.EntityLabel == type.RelatingType.Material.EntityLabel);
                            if (MaterialLayer != null)
                            {
                                IFC_Mat.text += $"{MaterialLayer.Material.Name}, Thickness: {MaterialLayer.LayerThickness}\n";
                            }
                         }
                    }
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

                                //IFC_Prop.text += $", {singleVal.Unit.FullName}";

                            }
                        }
                    }
                    catch (Exception ex)
                    { throw ex; }




                }


            }
            else
            {
                Debug.Log("hit nothing");
                GameObject.Find("Tablet_UI").transform.Find("Button_ThermHygro").GetComponent<Button>().interactable = false;
            }
        }
    }
}
