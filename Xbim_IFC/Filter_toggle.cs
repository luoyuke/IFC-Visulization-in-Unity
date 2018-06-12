using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.MeasureResource;

public class Filter_toggle : MonoBehaviour {

    private float ValMax, ValMin;
    IEnumerable<LoadAssetIFC.Sorting_IFCSpace> MList = new List<LoadAssetIFC.Sorting_IFCSpace>();

    public void IFCTypeFilter(int ind)
    {
        GameObject go = GameObject.Find("Initializing emvironment");
        LoadAssetIFC IfcScript = go.GetComponent<LoadAssetIFC>();
        GameObject Lego = GameObject.Find("Legend");
        switch (ind)
        {
            case 0://Arch
                IfcScript.Filter_IsArch = !IfcScript.Filter_IsArch;
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Arch"))
                    {
                        obj.GetComponent<MeshRenderer>().enabled = IfcScript.Filter_IsArch;
                        obj.GetComponent<MeshCollider>().enabled = IfcScript.Filter_IsArch;
                    }
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Arch_light"))
                    {
                        obj.GetComponent<MeshRenderer>().enabled = IfcScript.Filter_IsArch;
                        obj.GetComponent<MeshCollider>().enabled = IfcScript.Filter_IsArch;
                        if (IfcScript.Filter_IsLighting)
                        {
                            obj.GetComponent<Light>().enabled = false;
                        }
                    }
                break;
            case 1://MEP
                    IfcScript.Filter_IsMep = !IfcScript.Filter_IsMep;
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MEP"))
                    {
                        obj.GetComponent<MeshRenderer>().enabled = IfcScript.Filter_IsMep;
                        obj.GetComponent<MeshCollider>().enabled = IfcScript.Filter_IsMep;
                    } 
                break;
            case 2://Stru
                    IfcScript.Filter_IsStru = !IfcScript.Filter_IsStru;
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Stru"))
                    {
                        obj.GetComponent<MeshRenderer>().enabled = IfcScript.Filter_IsStru;
                        obj.GetComponent<MeshCollider>().enabled = IfcScript.Filter_IsStru;
                    }
                break;
            case 3://DayNight
                Light lt = GameObject.Find("Directional Light").GetComponent<Light>();
                if (IfcScript.Filter_DayNight)
                {
                    RenderSettings.skybox = Resources.Load("SkyNight", typeof(Material)) as Material;
                    RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
                    RenderSettings.defaultReflectionResolution = 128;
                    lt.intensity = 0f;
                    RenderSettings.ambientIntensity = 0f;
                    //RenderSettings.reflectionIntensity = .5f;
                }
                else
                {
                    RenderSettings.skybox = Resources.Load("SkyNoon", typeof(Material)) as Material;
                    lt.intensity = 1f;
                    RenderSettings.ambientIntensity = .5f;
                    
                }
                IfcScript.Filter_DayNight = !IfcScript.Filter_DayNight;
                break;
            case 4://Space X-Ray
                IfcScript.Filter_IsSpace = !IfcScript.Filter_IsSpace;
                if (!IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = false;
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("XRay"))
                {
                    obj.GetComponent<MeshRenderer>().enabled = IfcScript.Filter_IsSpace;
                }

                IEnumerable<GameObject> CombineList = GameObject.FindGameObjectsWithTag("Arch").Concat(GameObject.FindGameObjectsWithTag("Stru")).Concat(GameObject.FindGameObjectsWithTag("MEP"));

                if (IfcScript.Filter_IsSpace)
                {
                    foreach (GameObject obj in CombineList)
                    {
                        Color col = obj.GetComponent<MeshRenderer>().material.color;
                        Color Ncol = new Color(col.r, col.g, col.b, col.a/20f);
                        obj.GetComponent<MeshRenderer>().material.color = Ncol;
                        obj.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 3);
                        
                        obj.GetComponent<MeshRenderer>().material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        obj.GetComponent<MeshRenderer>().material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        obj.GetComponent<MeshRenderer>().material.SetInt("_ZWrite", 0);
                        obj.GetComponent<MeshRenderer>().material.DisableKeyword("_ALPHATEST_ON");
                        obj.GetComponent<MeshRenderer>().material.DisableKeyword("_ALPHABLEND_ON");
                        obj.GetComponent<MeshRenderer>().material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                        obj.GetComponent<MeshRenderer>().material.renderQueue = 3000;

                    }
                }
                else if(!IfcScript.Filter_IsSpace)
                {
                    foreach (GameObject obj in CombineList)
                    {
                        Color col = obj.GetComponent<MeshRenderer>().material.color;
                        Color Ncol = new Color(col.r, col.g, col.b, col.a * 20f);
                        obj.GetComponent<MeshRenderer>().material.color = Ncol;
                        obj.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 0);
                        
                        obj.GetComponent<MeshRenderer>().material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        obj.GetComponent<MeshRenderer>().material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        obj.GetComponent<MeshRenderer>().material.SetInt("_ZWrite", 1);
                        obj.GetComponent<MeshRenderer>().material.DisableKeyword("_ALPHATEST_ON");
                        obj.GetComponent<MeshRenderer>().material.DisableKeyword("_ALPHABLEND_ON");
                        obj.GetComponent<MeshRenderer>().material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                        obj.GetComponent<MeshRenderer>().material.renderQueue = 2000;

                    }
                }
                break;
            case 5://light switch
                if(IfcScript.Filter_IsArch)
                {
                    IfcScript.Filter_IsLighting = !IfcScript.Filter_IsLighting;
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Arch_light"))
                    {
                        obj.GetComponent<Light>().enabled = IfcScript.Filter_IsLighting;
                    }
                }
                else
                {
                    IfcScript.Filter_IsLighting = !IfcScript.Filter_IsLighting;
                }
                break;
            default:
                break;

        }
    }

    public void IFCSpaceSorting(int ind)
    {
        GameObject go = GameObject.Find("Initializing emvironment");
        LoadAssetIFC IfcScript = go.GetComponent<LoadAssetIFC>();

        GameObject Lego = GameObject.Find("Legend");
        Text legend_text = Lego.transform.Find("Legend_range").GetComponent<Text>(); //to display range
        legend_text.text = "";
        Text legend_title = Lego.transform.Find("Legend_title").GetComponent<Text>(); //to display title/unit
        legend_title.text = "";

        Color Def_color = new Color(); //default space color is light blue
        ColorUtility.TryParseHtmlString("#ADD9E666", out Def_color);
        string[] Color_Gradient = { "#0500ff66", "#0032ff66", "#00d4ff66", "#3eff0066", "#FFd20066", "#FF6e0066", "#FF0a0066", "#FF009066", "#FF04F066", "#FF0EF066" };

        switch (ind)
        {
            case 0:
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("XRay"))
                {
                    obj.GetComponent<MeshRenderer>().material.color = Def_color;
                }
                legend_text.text = "";
                legend_title.text = "";
                Lego.GetComponent<Canvas>().enabled = false;
                break;
            case 1:
                //CoolingLoad
                MList = IfcScript.SpaceList.OrderBy(o => float.Parse(o.C_CoolingLoad.ToString())); //more appropriate use Icompare
                ValMax = MList.Max(o => float.Parse(o.C_CoolingLoad.ToString()));
                ValMin = MList.Min(o => float.Parse(o.C_CoolingLoad.ToString()));
                legend_title.text = "Cooling Load                       Watt";
                if (IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = true;
                for (int i = 0; i <10; i++)
                {//subgroup values using Linq
                    var querySpaces = from qs in MList
                                      where float.Parse(qs.C_CoolingLoad.ToString()) >= ValMin + (ValMax - ValMin) / 10 * i
                                      && float.Parse(qs.C_CoolingLoad.ToString()) <= ValMin + (ValMax - ValMin) / 10 * (i + 1)
                                      select qs;
                    legend_text.text += $"{(ValMin + (ValMax - ValMin) / 10 * i).ToString("0.00")} - {(ValMin + (ValMax - ValMin) / 10 * (i + 1)).ToString("0.00")}\n";
                    foreach (var qspace in querySpaces)
                    {
                        var objects = GameObject.FindGameObjectsWithTag("XRay").Where(obj => obj.name == qspace.C_EntityLabel.ToString());
                        if (objects == null) continue;

                        Color color = new Color();
                        ColorUtility.TryParseHtmlString(Color_Gradient[i], out color);
                        foreach (var obj in objects)
                        {
                            obj.GetComponent<MeshRenderer>().material.color = color;
                        }
                    }
                }
                break;
            case 2:
                //HeatingLoad
                MList = IfcScript.SpaceList.OrderBy(o => float.Parse(o.C_HeatingLoad.ToString())); //more appropriate use Icompare
                ValMax = MList.Max(o => float.Parse(o.C_HeatingLoad.ToString()));
                ValMin = MList.Min(o => float.Parse(o.C_HeatingLoad.ToString()));
                legend_title.text = "Heating Load                       Watt";
                if (IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = true;
                for (int i = 0; i < 10; i++)
                {//subgroup values using Linq
                    var querySpaces = from qs in MList
                                      where float.Parse(qs.C_HeatingLoad.ToString()) >= ValMin + (ValMax - ValMin) / 10 * i
                                      && float.Parse(qs.C_HeatingLoad.ToString()) <= ValMin + (ValMax - ValMin) / 10 * (i + 1)
                                      select qs;
                    legend_text.text += $"{(ValMin + (ValMax - ValMin) / 10 * i).ToString("0.00")} - {(ValMin + (ValMax - ValMin) / 10 * (i + 1)).ToString("0.00")}\n";
                    foreach (var qspace in querySpaces)
                    {
                        var objects = GameObject.FindGameObjectsWithTag("XRay").Where(obj => obj.name == qspace.C_EntityLabel.ToString());
                        if (objects == null) continue;

                        Color color = new Color();
                        ColorUtility.TryParseHtmlString(Color_Gradient[i], out color);
                        foreach (var obj in objects)
                        {
                            obj.GetComponent<MeshRenderer>().material.color = color;
                        }
                    }
                }
                break;
            case 3:
                //LightingLoad
                MList = IfcScript.SpaceList.OrderBy(o => float.Parse(o.C_LightingLoad.ToString())); //more appropriate use Icompare
                ValMax = MList.Max(o => float.Parse(o.C_LightingLoad.ToString()));
                ValMin = MList.Min(o => float.Parse(o.C_LightingLoad.ToString()));
                legend_title.text = "Lighting Load                       Watt";
                if (IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = true;
                for (int i = 0; i < 10; i++)
                {//subgroup values using Linq
                    var querySpaces = from qs in MList
                                      where float.Parse(qs.C_LightingLoad.ToString()) >= ValMin + (ValMax - ValMin) / 10 * i
                                      && float.Parse(qs.C_LightingLoad.ToString()) <= ValMin + (ValMax - ValMin) / 10 * (i + 1)
                                      select qs;
                    legend_text.text += $"{(ValMin + (ValMax - ValMin) / 10 * i).ToString("0.00")} - {(ValMin + (ValMax - ValMin) / 10 * (i + 1)).ToString("0.00")}\n";
                    foreach (var qspace in querySpaces)
                    {
                        var objects = GameObject.FindGameObjectsWithTag("XRay").Where(obj => obj.name == qspace.C_EntityLabel.ToString());
                        if (objects == null) continue;

                        Color color = new Color();
                        ColorUtility.TryParseHtmlString(Color_Gradient[i], out color);
                        foreach (var obj in objects)
                        {
                            obj.GetComponent<MeshRenderer>().material.color = color;
                        }
                    }
                }
                break;
            case 4:
                //PowerLoad
                MList = IfcScript.SpaceList.OrderBy(o => float.Parse(o.C_PowerLoad.ToString())); //more appropriate use Icompare
                ValMax = MList.Max(o => float.Parse(o.C_PowerLoad.ToString()));
                ValMin = MList.Min(o => float.Parse(o.C_PowerLoad.ToString()));
                legend_title.text = "Power Load                       Watt";
                if (IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = true;
                for (int i = 0; i < 10; i++)
                {//subgroup values using Linq
                    var querySpaces = from qs in MList
                                      where float.Parse(qs.C_PowerLoad.ToString()) >= ValMin + (ValMax - ValMin) / 10 * i
                                      && float.Parse(qs.C_PowerLoad.ToString()) <= ValMin + (ValMax - ValMin) / 10 * (i + 1)
                                      select qs;
                    legend_text.text += $"{(ValMin + (ValMax - ValMin) / 10 * i).ToString("0.00")} - {(ValMin + (ValMax - ValMin) / 10 * (i + 1)).ToString("0.00")}\n";
                    foreach (var qspace in querySpaces)
                    {
                        var objects = GameObject.FindGameObjectsWithTag("XRay").Where(obj => obj.name == qspace.C_EntityLabel.ToString());
                        if (objects == null) continue;

                        Color color = new Color();
                        ColorUtility.TryParseHtmlString(Color_Gradient[i], out color);
                        foreach (var obj in objects)
                        {
                            obj.GetComponent<MeshRenderer>().material.color = color;
                        }
                    }
                }
                break;
            case 5:
                //CoolingLoad_pa
                MList = IfcScript.SpaceList.OrderBy(o => float.Parse(o.C_CoolingLoad_pa.ToString())); //more appropriate use Icompare
                ValMax = MList.Max(o => float.Parse(o.C_CoolingLoad_pa.ToString()));
                ValMin = MList.Min(o => float.Parse(o.C_CoolingLoad_pa.ToString()));
                legend_title.text = "Cooling Load per Area  Watt/Sq.Meter";
                if (IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = true;
                for (int i = 0; i < 10; i++)
                {//subgroup values using Linq
                    var querySpaces = from qs in MList
                                      where float.Parse(qs.C_CoolingLoad_pa.ToString()) >= ValMin + (ValMax - ValMin) / 10 * i
                                      && float.Parse(qs.C_CoolingLoad_pa.ToString()) <= ValMin + (ValMax - ValMin) / 10 * (i + 1)
                                      select qs;
                    legend_text.text += $"{(ValMin + (ValMax - ValMin) / 10 * i).ToString("0.00")} - {(ValMin + (ValMax - ValMin) / 10 * (i + 1)).ToString("0.00")}\n";
                    foreach (var qspace in querySpaces)
                    {
                        var objects = GameObject.FindGameObjectsWithTag("XRay").Where(obj => obj.name == qspace.C_EntityLabel.ToString());
                        if (objects == null) continue;

                        Color color = new Color();
                        ColorUtility.TryParseHtmlString(Color_Gradient[i], out color);
                        foreach (var obj in objects)
                        {
                            obj.GetComponent<MeshRenderer>().material.color = color;
                        }
                    }
                }
                break;
            case 6:
                //HeatingLoad_pa
                MList = IfcScript.SpaceList.OrderBy(o => float.Parse(o.C_HeatingLoad_pa.ToString())); //more appropriate use Icompare
                ValMax = MList.Max(o => float.Parse(o.C_HeatingLoad_pa.ToString()));
                ValMin = MList.Min(o => float.Parse(o.C_HeatingLoad_pa.ToString()));
                legend_title.text = "Heating Load per Area  Watt/Sq.Meter";
                if (IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = true;
                for (int i = 0; i < 10; i++)
                {//subgroup values using Linq
                    var querySpaces = from qs in MList
                                      where float.Parse(qs.C_HeatingLoad_pa.ToString()) >= ValMin + (ValMax - ValMin) / 10 * i
                                      && float.Parse(qs.C_HeatingLoad_pa.ToString()) <= ValMin + (ValMax - ValMin) / 10 * (i + 1)
                                      select qs;
                    legend_text.text += $"{(ValMin + (ValMax - ValMin) / 10 * i).ToString("0.00")} - {(ValMin + (ValMax - ValMin) / 10 * (i + 1)).ToString("0.00")}\n";
                    foreach (var qspace in querySpaces)
                    {
                        var objects = GameObject.FindGameObjectsWithTag("XRay").Where(obj => obj.name == qspace.C_EntityLabel.ToString());
                        if (objects == null) continue;

                        Color color = new Color();
                        ColorUtility.TryParseHtmlString(Color_Gradient[i], out color);
                        foreach (var obj in objects)
                        {
                            obj.GetComponent<MeshRenderer>().material.color = color;
                        }
                    }
                }
                break;
            case 7:
                //LightingLoad_pa
                MList = IfcScript.SpaceList.OrderBy(o => float.Parse(o.C_LightingLoad_pa.ToString())); //more appropriate use Icompare
                ValMax = MList.Max(o => float.Parse(o.C_LightingLoad_pa.ToString()));
                ValMin = MList.Min(o => float.Parse(o.C_LightingLoad_pa.ToString()));
                legend_title.text = "Lighting Load per Area  Watt/Sq.Meter";
                if (IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = true;
                for (int i = 0; i < 10; i++)
                {//subgroup values using Linq
                    var querySpaces = from qs in MList
                                      where float.Parse(qs.C_LightingLoad_pa.ToString()) >= ValMin + (ValMax - ValMin) / 10 * i
                                      && float.Parse(qs.C_LightingLoad_pa.ToString()) <= ValMin + (ValMax - ValMin) / 10 * (i + 1)
                                      select qs;
                    legend_text.text += $"{(ValMin + (ValMax - ValMin) / 10 * i).ToString("0.00")} - {(ValMin + (ValMax - ValMin) / 10 * (i + 1)).ToString("0.00")}\n";
                    foreach (var qspace in querySpaces)
                    {
                        var objects = GameObject.FindGameObjectsWithTag("XRay").Where(obj => obj.name == qspace.C_EntityLabel.ToString());
                        if (objects == null) continue;

                        Color color = new Color();
                        ColorUtility.TryParseHtmlString(Color_Gradient[i], out color);
                        foreach (var obj in objects)
                        {
                            obj.GetComponent<MeshRenderer>().material.color = color;
                        }
                    }
                }
                break;
            case 8:
                //PowerLoad_pa
                MList = IfcScript.SpaceList.OrderBy(o => float.Parse(o.C_PowerLoad_pa.ToString())); //more appropriate use Icompare
                ValMax = MList.Max(o => float.Parse(o.C_PowerLoad_pa.ToString()));
                ValMin = MList.Min(o => float.Parse(o.C_PowerLoad_pa.ToString()));
                legend_title.text = "Power Load per Area  Watt/Sq.Meter";
                if (IfcScript.Filter_IsSpace)
                    Lego.GetComponent<Canvas>().enabled = true;
                for (int i = 0; i < 10; i++)
                {//subgroup values using Linq
                    var querySpaces = from qs in MList
                                      where float.Parse(qs.C_PowerLoad_pa.ToString()) >= ValMin + (ValMax - ValMin) / 10 * i
                                      && float.Parse(qs.C_PowerLoad_pa.ToString()) <= ValMin + (ValMax - ValMin) / 10 * (i + 1)
                                      select qs;
                    legend_text.text += $"{(ValMin + (ValMax - ValMin) / 10 * i).ToString("0.00")} - {(ValMin + (ValMax - ValMin) / 10 * (i + 1)).ToString("0.00")}\n";
                    foreach (var qspace in querySpaces)
                    {
                        var objects = GameObject.FindGameObjectsWithTag("XRay").Where(obj => obj.name == qspace.C_EntityLabel.ToString());
                        if (objects == null) continue;

                        Color color = new Color();
                        ColorUtility.TryParseHtmlString(Color_Gradient[i], out color);
                        foreach (var obj in objects)
                        {
                            obj.GetComponent<MeshRenderer>().material.color = color;
                        }
                    }
                }
                break;
            default:
                break;


        }
    }

}
