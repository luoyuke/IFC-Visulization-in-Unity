using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.MaterialResource;
using System.Linq;

public class Analysis : MonoBehaviour {
    private int CullingMask;
    public GameObject Ana_Prefab;
    public GameObject Ano_Prefab;
    private Dictionary<double,float> SaturatedPressureTable;
    public class Ana_Layer
    {
        public string MaterialName { set; get; }
        public float Thickness { set; get; }
        public float Permeability { set; get; }
        public float ThermalConductivity { set; get; }

        public Ana_Layer(string MaterialName, float Thickness, float Permeability, float ThermalConductivity)
        {
            this.MaterialName = MaterialName;
            this.Thickness = Thickness;
            this.Permeability = Permeability;
            this.ThermalConductivity = ThermalConductivity;

        }
        public Ana_Layer() { }
    }
    public float TemperatureExternal, TemperatureInternal;

    public void IsAnalysing(bool click)
    {
        GameObject go = GameObject.Find("Initializing emvironment");
        LoadAssetIFC IfcScript = go.GetComponent<LoadAssetIFC>();

        GameObject SecondCamera = GameObject.Find("CameraOrbit").transform.Find("SecondCamera").gameObject;
        CullingMask = SecondCamera.GetComponent<Camera>().cullingMask;
        SecondCamera.GetComponent<Camera>().cullingMask = 1 << 11;

        try
        {
            GameObject LastBatch = GameObject.Find("CompoundWall").gameObject;
            if (LastBatch !=null)
                Destroy(LastBatch);
        }
        catch { }
        try
        {
            GameObject LastBatchLine = GameObject.Find("Thermo_Liner").gameObject;
            if (LastBatchLine != null)
                Destroy(LastBatchLine);
        }
        catch { }
        try
        {
            GameObject LastBatchLine = GameObject.Find("Hygro_Liner").gameObject;
            if (LastBatchLine != null)
                Destroy(LastBatchLine);
        }
        catch { }

        GameObject CompoundWall = new GameObject("CompoundWall");
        
        List<Ana_Layer> AnalyticalLayer = new List<Ana_Layer>();
        float TotalThickness = 0f;
        float ThermalTransmittance = 0f;
        int EntityLabel_Select = int.Parse(IfcScript.RayCastSelect.name.ToString());
        
        
        var Product_Sel = IfcScript.model.Instances.FirstOrDefault<IIfcProduct>(w => w.EntityLabel == EntityLabel_Select);
        IEnumerable<IfcRelDefinesByProperties> relations = Product_Sel.IsDefinedBy.OfType<IfcRelDefinesByProperties>();
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
                    case "ThermalTransmittance"://without Rsi Rse
                        ThermalTransmittance = float.Parse(singleVal.NominalValue.ToString());
                        break;
                    default:
                        break;

                }
            }
        }

        var ProductMaterial = Product_Sel.Material;
        if (ProductMaterial != null)
        {
            try
            {
                switch (ProductMaterial.ExpressType.ToString())
                {
                    case "IfcMaterialLayerSetUsage":
                        var IFCProductLayerSetUsage = IfcScript.model.Instances.FirstOrDefault<IIfcMaterialLayerSetUsage>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                        TotalThickness = float.Parse(IFCProductLayerSetUsage.ForLayerSet.TotalThickness.ToString());
                        foreach (var Material_layer in IFCProductLayerSetUsage.ForLayerSet.MaterialLayers)
                        {//layer has no name(all optional otherthan thickness), but its associated material has
                            Ana_Layer Alayer_SU = new Ana_Layer();
                            Alayer_SU.MaterialName = Material_layer.Material.Name;
                            Alayer_SU.Thickness = float.Parse(Material_layer.LayerThickness.ToString());

                            foreach (var IfcProperty in Material_layer.Material.HasProperties)
                            {
                                var singleVals = IfcProperty.Properties.OfType<IfcPropertySingleValue>();
                                if (singleVals == null) continue;
                                foreach (var singleVal in singleVals)
                                {
                                    switch (singleVal.Name)
                                    {
                                        case "ThermalConductivity":
                                            Alayer_SU.ThermalConductivity = float.Parse(singleVal.NominalValue.ToString());
                                            break;
                                        case "VaporPermeability":
                                            Alayer_SU.Permeability = float.Parse(singleVal.NominalValue.ToString());
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            AnalyticalLayer.Add(Alayer_SU);
                        }
                        break;

                    case "IfcMaterialLayerSet":
                        var IFCProductLayerSet = IfcScript.model.Instances.FirstOrDefault<IIfcMaterialLayerSet>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                        TotalThickness = float.Parse(IFCProductLayerSet.TotalThickness.ToString());
                        foreach (var Material_layer in IFCProductLayerSet.MaterialLayers)
                        {
                            Ana_Layer Alayer_S = new Ana_Layer();
                            Alayer_S.MaterialName = Material_layer.Material.Name;
                            Alayer_S.Thickness = float.Parse(Material_layer.LayerThickness.ToString());

                            foreach (var IfcProperty in Material_layer.Material.HasProperties)
                            {
                                var singleVals = IfcProperty.Properties.OfType<IfcPropertySingleValue>();
                                if (singleVals == null) continue;
                                foreach (var singleVal in singleVals)
                                {
                                  
                                    switch (singleVal.Name)
                                    {
                                        case "ThermalConductivity":
                                            Alayer_S.ThermalConductivity = float.Parse(singleVal.NominalValue.ToString());
                                            break;
                                        case "VaporPermeability":
                                            Alayer_S.Permeability = float.Parse(singleVal.NominalValue.ToString());
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            AnalyticalLayer.Add(Alayer_S);
                        }
                        break;

                    case "IfcMaterialLayer":
                        var IFCProductLayer = IfcScript.model.Instances.FirstOrDefault<IIfcMaterialLayer>(m => m.EntityLabel == ProductMaterial.EntityLabel);
                        Ana_Layer Alayer = new Ana_Layer();
                        Alayer.MaterialName = IFCProductLayer.Material.Name;
                        Alayer.Thickness = float.Parse(IFCProductLayer.LayerThickness.ToString());
                        TotalThickness = float.Parse(IFCProductLayer.LayerThickness.ToString());
                        foreach (var IfcProperty in IFCProductLayer.Material.HasProperties)
                        {
                            var singleVals = IfcProperty.Properties.OfType<IfcPropertySingleValue>();
                            if (singleVals == null) continue;
                            foreach (var singleVal in singleVals)
                            {
                                switch (singleVal.Name)
                                {
                                    case "ThermalConductivity":
                                        Alayer.ThermalConductivity = float.Parse(singleVal.NominalValue.ToString());
                                        break;
                                    case "VaporPermeability":
                                        Alayer.Permeability = float.Parse(singleVal.NominalValue.ToString());
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        AnalyticalLayer.Add(Alayer);
                        break;
                }
            }
            catch { }
        }
        else
        {
            foreach(var type in Product_Sel.IsTypedBy)
            {
                var MaterialLayerSet = IfcScript.model.Instances.FirstOrDefault<IIfcMaterialLayerSet>(m => m.EntityLabel == type.RelatingType.Material.EntityLabel);
                if (MaterialLayerSet != null)
                {
                    TotalThickness = float.Parse(MaterialLayerSet.TotalThickness.ToString());
                    foreach (var Material_layer in MaterialLayerSet.MaterialLayers)
                    {
                        Ana_Layer Alayer_S = new Ana_Layer();
                        Alayer_S.MaterialName = Material_layer.Material.Name;
                        Alayer_S.Thickness = float.Parse(Material_layer.LayerThickness.ToString());
                        foreach (var IfcProperty in Material_layer.Material.HasProperties)
                        {
                            var singleVals = IfcProperty.Properties.OfType<IfcPropertySingleValue>();
                            if (singleVals == null) continue;
                            foreach (var singleVal in singleVals)
                            {

                                switch (singleVal.Name)
                                {
                                    case "ThermalConductivity":
                                        Alayer_S.ThermalConductivity = float.Parse(singleVal.NominalValue.ToString());
                                        break;
                                    case "VaporPermeability":
                                        Alayer_S.Permeability = float.Parse(singleVal.NominalValue.ToString());
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        AnalyticalLayer.Add(Alayer_S);
                    }
                    break;
                }

                var MaterialLayerSetUsage = IfcScript.model.Instances.FirstOrDefault<IIfcMaterialLayerSetUsage>(m => m.EntityLabel == type.RelatingType.Material.EntityLabel);
                if (MaterialLayerSetUsage != null)
                {
                    TotalThickness = float.Parse(MaterialLayerSetUsage.ForLayerSet.TotalThickness.ToString());
                    foreach (var Material_layer in MaterialLayerSetUsage.ForLayerSet.MaterialLayers)
                    {
                        Ana_Layer Alayer_SU = new Ana_Layer();
                        Alayer_SU.MaterialName = Material_layer.Material.Name;
                        Alayer_SU.Thickness = float.Parse(Material_layer.LayerThickness.ToString());
                        foreach (var IfcProperty in Material_layer.Material.HasProperties)
                        {
                            var singleVals = IfcProperty.Properties.OfType<IfcPropertySingleValue>();
                            if (singleVals == null) continue;
                            foreach (var singleVal in singleVals)
                            {

                                switch (singleVal.Name)
                                {
                                    case "ThermalConductivity":
                                        Alayer_SU.ThermalConductivity = float.Parse(singleVal.NominalValue.ToString());
                                        break;
                                    case "VaporPermeability":
                                        Alayer_SU.Permeability = float.Parse(singleVal.NominalValue.ToString());
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        AnalyticalLayer.Add(Alayer_SU);
                    }
                    break;
                }

                var MaterialLayer = IfcScript.model.Instances.FirstOrDefault<IIfcMaterialLayer>(m => m.EntityLabel == type.RelatingType.Material.EntityLabel);
                if (MaterialLayer != null)
                {
                    Ana_Layer Alayer = new Ana_Layer();
                    Alayer.MaterialName = MaterialLayer.Material.Name;
                    Alayer.Thickness = float.Parse(MaterialLayer.LayerThickness.ToString());
                    TotalThickness = float.Parse(MaterialLayer.LayerThickness.ToString());
                    foreach (var IfcProperty in MaterialLayer.Material.HasProperties)
                    {
                        var singleVals = IfcProperty.Properties.OfType<IfcPropertySingleValue>();
                        if (singleVals == null) continue;
                        foreach (var singleVal in singleVals)
                        {
                            switch (singleVal.Name)
                            {
                                case "ThermalConductivity":
                                    Alayer.ThermalConductivity = float.Parse(singleVal.NominalValue.ToString());
                                    break;
                                case "VaporPermeability":
                                    Alayer.Permeability = float.Parse(singleVal.NominalValue.ToString());
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    AnalyticalLayer.Add(Alayer);
                    break;
                }
            }
            //var ProductType = Product_Sel.IsTypedBy.OfType<>
        }
        //implement slider 
        TemperatureExternal = -5f;
        TemperatureInternal = 20f;

        AnalyticalLayer.Reverse();//reverse list order 'Exterior --> Interior' 
        float OffSet_X = 0f;
        float SumThickness = 0f;

        Debug.Log($"ThermalTransmittance before Rsi Rse: {ThermalTransmittance}");
        ThermalTransmittance = 1/(1/ThermalTransmittance + 0.25f + 0.04f); //Rsi = 0.13 Rse = 0.04 -- Vertical U-Value from IFC doesnt cover this 2 heat convect-coeff

        float R_Value = 0f;
        float SumRValue = 0.25f; //Rsi = 0.13 whereas in Vapor diffusion Rsi = 0.25
        float TempDropRatio = SumRValue * ThermalTransmittance;
        float[] Array_TDropRatio = new float[AnalyticalLayer.Count+1];

        float[] Mu_Value = new float[AnalyticalLayer.Count];
        float SumSd = 0f;
        float[] Array_SdAccumRatio = new float[AnalyticalLayer.Count];
        float[] Arrary_Sd = new float[AnalyticalLayer.Count];

        int i = 0; //index for layer
        Array_TDropRatio[0] = TempDropRatio;

        float clip = 1f; //formatting space
        foreach (var layer in AnalyticalLayer)
        {
            //Debug.Log("layer class- " + layer.MaterialName + ", ThermalConductivity: " + layer.ThermalConductivity + ", Permeability" + layer.Permeability);
            GameObject goos = Instantiate(Ana_Prefab);
            goos.name = layer.MaterialName;
            goos.transform.parent = CompoundWall.transform;
            goos.layer = 11;
            goos.GetComponent<MeshRenderer>().material.color = new Color(.8f+.1f*clip,.8f + .1f * clip, .8f + .1f * clip, .8f + .1f * clip);

            //OffSet solution
            goos.transform.localScale = new Vector3(layer.Thickness/10f, 15f, 10f);
            OffSet_X = SumThickness + layer.Thickness/2f;
            SumThickness += layer.Thickness;
            StartCoroutine(MovetoPos(goos, new Vector3(OffSet_X-TotalThickness/2, 0, 0) / 10f, 2f));

            clip *= -1;
            //Hanging Tags Annotation Prefab
            GameObject goostag = Instantiate(Ano_Prefab, new Vector3((OffSet_X - TotalThickness / 2) / 10f, 8f+clip, -5f), new Quaternion());
            goostag.transform.SetParent(CompoundWall.transform);
            goostag.layer = 11;
            Text HangTag = goostag.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
            HangTag.text = $"{layer.MaterialName}\nThickness:{layer.Thickness}";
            //Thermal-Hygro Tags
            GameObject goostag2 = Instantiate(Ano_Prefab, new Vector3((OffSet_X - TotalThickness / 2) / 10f, 10f + clip, -2f), new Quaternion());
            goostag2.transform.SetParent(CompoundWall.transform);
            goostag2.layer = 11;
            Text HangTag2 = goostag2.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
            HangTag2.text = $"Conductivity:{layer.ThermalConductivity.ToString("F3")}\nPermeability:{layer.Permeability.ToString("F2")}";

            //annotation & liner
            GameObject Liner = new GameObject("Anno_Line");
            LineRenderer line = Liner.AddComponent<LineRenderer>();
            Liner.transform.parent = CompoundWall.transform;
            Liner.gameObject.layer = 11;
            line.SetPosition(0, new Vector3((OffSet_X - TotalThickness / 2) / 10f, 5f, -5f));
            line.SetPosition(1, new Vector3((OffSet_X - TotalThickness / 2) / 10f, 8f+clip,-5f));
            line.startColor = Color.black;
            line.startWidth = 0.03f;
            Material mat = new Material(Shader.Find("UI/Default"));
            line.material = mat;
            //liner 2
            GameObject Liner2 = new GameObject("Anno_Line2");
            LineRenderer line2 = Liner2.AddComponent<LineRenderer>();
            Liner2.transform.parent = CompoundWall.transform;
            Liner2.gameObject.layer = 11;
            line2.SetPosition(0, new Vector3((OffSet_X - TotalThickness / 2) / 10f, 5f, -2f));
            line2.SetPosition(1, new Vector3((OffSet_X - TotalThickness / 2) / 10f, 10f + clip, -2f));
            line2.startColor = Color.black;
            line2.startWidth = 0.03f;
            line2.material = mat;

            //Thermal Liner
            if (layer.ThermalConductivity == 0)
                layer.ThermalConductivity = 1000f;//k = 1000 is very conductive to ignore the fact that some layers K-value are not assigned

            R_Value = layer.Thickness / layer.ThermalConductivity/1000f;
            SumRValue = SumRValue + R_Value;
            TempDropRatio = SumRValue * ThermalTransmittance;//2nd last, Rse = 0.04 should be the last
            Debug.Log($"Temp.Drop.Ratio: {TempDropRatio},layer.Thickness  {layer.Thickness}, layer.ThermalConductivity {layer.ThermalConductivity}");
            if (TempDropRatio>=1)
            {
                TempDropRatio = 0;
                Debug.Log($"Warming: IFC Material {layer.MaterialName}'s Thermal Properties are not valid. U-Value doesn't add up.");
            }
            Array_TDropRatio[++i] = TempDropRatio;

            //Hygro Liner
            if (layer.Permeability == 0)
                layer.Permeability = 1000000f;//P = 1.000.000 is very permeable to ignore the fact that some layers P-value are not assigned
            Mu_Value[i-1] = 200f / layer.Permeability;

            SumSd = SumSd + Mu_Value[i-1] * layer.Thickness / 1000f;
           
        }

        //this time for the Sd ratio
        float SdAccum = 0f;
        for ( int k =0; k< AnalyticalLayer.Count;k++) 
        {
            Arrary_Sd[k] = Mu_Value[k] * AnalyticalLayer[k].Thickness / 1000f;

            SdAccum = SdAccum + Arrary_Sd[k];
            Array_SdAccumRatio[k] = SdAccum / SumSd;
            Debug.Log($"Sd.Accu.Ratio: {Array_SdAccumRatio[k]},layer.Permeability {AnalyticalLayer[k].Permeability},SdAccum {SdAccum}");
        }

        StartCoroutine(DrawTempLiner(TemperatureExternal, TemperatureInternal, Array_TDropRatio, TotalThickness, AnalyticalLayer));

        //Hygroscopic analysis
        StartCoroutine(DrawMoistLiner(TemperatureExternal, TemperatureInternal, Array_SdAccumRatio,Array_TDropRatio, Arrary_Sd, SumSd,TotalThickness, AnalyticalLayer));
        //Debug.Log($"SaturatedPressureTables 21.5 {SaturatedPressureTable[21.5]}"); read table like this, cap for 1 decimal

    }

    IEnumerator MovetoPos(GameObject g, Vector3 newPos, float time)
    {
        float elapsedTime = 0;
        Vector3 startPos = new Vector3();
        while (elapsedTime < time)
        {
            g.transform.position = Vector3.Lerp(startPos, newPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= time)
                g.transform.position = newPos;
            yield return null;
        }
        yield return null;
    }

    IEnumerator DrawTempLiner(float Te, float Ti, float[] Ts, float TotalThickness, List<Ana_Layer> AnalyticalLayer)
    {
        float Del_T = Ti - Te;

        GameObject LinerT = new GameObject("Thermo_Liner");
        LineRenderer lineT = LinerT.AddComponent<LineRenderer>();
        //LinerT.transform.parent = GameObject.Find("CompoundWall").transform;
        LinerT.gameObject.layer = 11;

        lineT.positionCount = AnalyticalLayer.Count+5;
        lineT.SetPosition(0, new Vector3((-10f - TotalThickness / 2) / 10f, 4f, -5.1f));//StartPoint Indoor Temperture
        GameObject Temptag = Instantiate(Ano_Prefab, new Vector3((-15f - TotalThickness / 2) / 10f, 4f, -5.1f), new Quaternion());
        Temptag.transform.SetParent(LinerT.transform);
        Temptag.layer = 11;
        Text HangTag = Temptag.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
        HangTag.text = $"{Ti.ToString("F1")}°C|2337Pa";

        lineT.SetPosition(1, new Vector3((-5f-TotalThickness/2) / 10f, 4f , -5.1f));//EndPoint Indoor Temperture - surface

        float OffSet = 0f;
        float SaturatedPressure = 0f;
        double Temperture = 0;
        for (int j =0; j< AnalyticalLayer.Count; j++)
        {
            Temperture = System.Math.Round(Ti - Ts[j] * Del_T, 1);

            if (Temperture > -11 && Temperture < 31)
                try { SaturatedPressure = SaturatedPressureTable[Temperture]; } catch { }

            float yj = ((float)Temperture - Te) * 0.3f - 3.5f; // old--> y = 4f - Ts[j]*Del_T*0.3f
            lineT.SetPosition(j+2,new Vector3((OffSet - TotalThickness / 2) /10f , yj, -5.1f));

            GameObject Temptage = Instantiate(Ano_Prefab, new Vector3((OffSet - TotalThickness / 2) / 10f, yj+1f, -5.1f), new Quaternion());
            Temptage.transform.SetParent(LinerT.transform);
            Temptage.layer = 11;
            Text HangTage = Temptage.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
            HangTage.text = $"{(Ti - Ts[j] * Del_T).ToString("F1")}°C|{SaturatedPressure}Pa";

            OffSet = OffSet + AnalyticalLayer[j].Thickness;
            yield return null;
        }
        //// old--> y = 4f -Ts[AnalyticalLayer.Count] * Del_T * 0.3f
        float jj = (1 - Ts[AnalyticalLayer.Count]) * Del_T * 0.3f - 3.5f;
        lineT.SetPosition(AnalyticalLayer.Count + 2, new Vector3((OffSet - TotalThickness / 2) / 10f, jj , -5.1f));//StartPoint outdoor Temperture - surface

        Temperture = System.Math.Round(Ti - Ts[AnalyticalLayer.Count] * Del_T, 1);
        if (Temperture > -11 && Temperture < 31)
            try { SaturatedPressure = SaturatedPressureTable[Temperture]; } catch { }

        GameObject Temptag2 = Instantiate(Ano_Prefab, new Vector3((OffSet - TotalThickness / 2) / 10f, jj+1f, -5.1f), new Quaternion());
        Temptag2.transform.SetParent(LinerT.transform);
        Temptag2.layer = 11;
        Text HangTag2 = Temptag2.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
        HangTag2.text = $"{(Ti - Ts[AnalyticalLayer.Count] * Del_T).ToString("F2")}°C|{SaturatedPressure}Pa";

        lineT.SetPosition(AnalyticalLayer.Count + 3, new Vector3((5f+ TotalThickness / 2) / 10f, 4f - Del_T * 0.3f, -5.1f));//EndPoint outdoor Temperture
        lineT.SetPosition(AnalyticalLayer.Count + 4, new Vector3((10f+TotalThickness / 2) / 10f, 4f - Del_T * 0.3f, -5.1f));//EndPoint outdoor Temperture

        GameObject Temptag3 = Instantiate(Ano_Prefab, new Vector3((15f+TotalThickness / 2) / 10f, 4f - Del_T * 0.3f, -5.1f), new Quaternion());
        Temptag3.transform.SetParent(LinerT.transform);
        Temptag3.layer = 11;
        Text HangTag3 = Temptag3.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
        HangTag3.text = $"{(Te).ToString("F1")}°C|401Pa";

        lineT.startColor = Color.red;
        lineT.startWidth = 0.3f;
        lineT.endWidth = 0.3f;
        lineT.endColor = Color.blue;

        Material mate = new Material(Shader.Find("UI/Default"));
        lineT.material = mate;

        yield return null;
    }

    IEnumerator LoadSaturatedPressureVaporTable()
    {//pass along the value to 2 layer coroutine
        
        Dictionary<double, float> SaturatedPressureTable = new Dictionary<double, float>();
        int Temperture_Whole=0, Temperture_decimal=0, minues = 1;
        float Pressure=0;
        //var TableString = Resources.Load<TextAsset>("Tabelle C1").ToString(); can be read as string

        string tablePath = Application.dataPath + "/Resources/Tabelle C1.csv"; //Application.dataPath defines relative path of file in Build
        //still needed be copied to resources folder manuelly afet build completes.
        //better solution will be wraping the string into asset, and no more copish 

        string[] tableText = File.ReadAllLines(tablePath);
        foreach(var line in tableText)
        {
            string[] LineValue = line.Split(","[0]);
            Temperture_Whole = int.Parse(LineValue[0].ToString());

            Temperture_decimal = -1;
            foreach(var Value in LineValue)
            {
                if (Temperture_decimal == -1)
                {
                    ++Temperture_decimal;
                    continue;
                }

                Pressure = float.Parse(Value.ToString());
                try
                { SaturatedPressureTable.Add(Temperture_Whole + (double)Temperture_decimal / 10 * minues, Pressure); }
                catch { }
                ++Temperture_decimal;
            }

            if (Temperture_Whole == 0)
                minues = -1;//get into negative celcius 

            yield return null;
        }
        this.SaturatedPressureTable = SaturatedPressureTable;
        Debug.Log("Table Loading completes" + Time.time);
        
        GameObject.Find("Button_ThermHygro").GetComponent<Button>().interactable = true;
        yield return null;
    }

    IEnumerator DrawMoistLiner(float Te, float Ti, float[] Hs,float[] Ts,float[] Sd_value, float SumSd ,float TotalThickness, List<Ana_Layer> AnalyticalLayer)
    {
        float Del_T = Ti - Te;
        float g_div_theta = (1168-321)/SumSd;

        GameObject LinerHS = new GameObject("Hygro_Liner");
        LineRenderer lineHS = LinerHS.AddComponent<LineRenderer>();
        //LinerT.transform.parent = GameObject.Find("CompoundWall").transform;
        LinerHS.gameObject.layer = 11;

        lineHS.positionCount = AnalyticalLayer.Count + 4;
        lineHS.SetPosition(0, new Vector3((-10f - TotalThickness / 2) / 10f, -0.5473f, -5.1f));//StartPoint Indoor Temperture

        GameObject Temptag = Instantiate(Ano_Prefab, new Vector3((-10f - TotalThickness / 2) / 10f, -0.5473f+1f, -5.1f), new Quaternion());
        Temptag.transform.SetParent(LinerHS.transform);
        Temptag.layer = 11;
        Text HangTag = Temptag.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
        HangTag.text = $"Pi:1168Pa";

        lineHS.SetPosition(1, new Vector3((-5f - TotalThickness / 2) / 10f, -0.5473f, -5.1f));//EndPoint Indoor Temperture - surface same Pi as indoor

        float OffSet = 0f;
        float SaturatedPressure = 2337f;
        float VaporPressure = 1168f;

        for (int j = 0; j < AnalyticalLayer.Count; j++)
        {
            double Temperture = System.Math.Round(Ti - Ts[j] * Del_T, 1);
            if (Temperture > -11 && Temperture < 31)
                try { SaturatedPressure = SaturatedPressureTable[Temperture]; } catch { }

            if (j == 0)
            {
                VaporPressure = 1168f;
            }
            else
            {
                VaporPressure = VaporPressure - g_div_theta * Sd_value[j-1];
            }
            bool isCondensing = false;
            if (VaporPressure > SaturatedPressure)
            {
                VaporPressure = SaturatedPressure;
                g_div_theta = (VaporPressure - 321f) / (SumSd * (1-Hs[j-1]));
                Debug.Log("Diffuse Water in layer" + AnalyticalLayer[j].MaterialName);
                isCondensing = true;
            }

            Debug.Log($"layer {j}, Temp {Temperture}, vaporP {VaporPressure}, SatP {SaturatedPressure}, SumSd {SumSd}");
            //a proportional VaporPressure drop pegged to SaturatedPressure/Temperature
            float yj = (VaporPressure - 321f) / 2016f * 7.8423f - 3.8423f;
            if(isCondensing)
            {//Pi value of Y-axie will be overwritten by Temp-Psat Y-axie
                yj = ((float)Temperture - Te) * 0.3f - 3.5f;
            }
            lineHS.SetPosition(j + 2, new Vector3((OffSet - TotalThickness / 2) / 10f, yj, -5.1f));

            GameObject Temptage = Instantiate(Ano_Prefab, new Vector3((OffSet - TotalThickness / 2) / 10f, yj-1f, -5.1f), new Quaternion());
            Temptage.transform.SetParent(lineHS.transform);
            Temptage.layer = 11;
            Text HangTage = Temptage.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
            HangTage.text = $"P{j}:{VaporPressure.ToString("F0")}Pa";

            if (isCondensing)
            {
                HangTage.text += "\nDew Formation Occurs.";
            }

            OffSet = OffSet + AnalyticalLayer[j].Thickness;
            yield return null;
        }

        lineHS.SetPosition(AnalyticalLayer.Count+2, new Vector3((5f + TotalThickness / 2) / 10f, -3.8423f, -5.1f));
        lineHS.SetPosition(AnalyticalLayer.Count + 3, new Vector3((10f + TotalThickness / 2) / 10f, -3.8423f, -5.1f));

        GameObject Temptag2 = Instantiate(Ano_Prefab, new Vector3((10f + TotalThickness / 2) / 10f, -3.8423f - 2f, -5.1f), new Quaternion());
        Temptag2.transform.SetParent(LinerHS.transform);
        Temptag2.layer = 11;
        Text HangTag2 = Temptag2.transform.Find("Anno_tx").gameObject.GetComponent<Text>();
        HangTag2.text = $"Pe:321Pa";


        lineHS.startColor = Color.yellow;
        lineHS.startWidth = 0.3f;
        lineHS.endWidth = 0.3f;
        lineHS.endColor = Color.yellow;

        Material mateh = new Material(Shader.Find("UI/Default"));
        lineHS.material = mateh;

        yield return null;
    }

    private void Start()
    {
        StartCoroutine(LoadSaturatedPressureVaporTable());
    }
}
