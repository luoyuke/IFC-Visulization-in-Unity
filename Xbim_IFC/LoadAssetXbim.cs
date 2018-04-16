using System;
using Xbim.Ifc;
using Xbim.Common.XbimExtensions;
using System.Linq;
using System.IO;
using UnityEngine;
using Xbim.Common.Geometry;

public class LoadAssetXbim : MonoBehaviour
{
    bool swch = false;
    // Use this for initialization
    public float Flo_convert(double input)
    {
        float result = (float)input;
        if (float.IsPositiveInfinity(result))
        {
            result = float.MaxValue;
        }
        else if (float.IsNegativeInfinity(result))
        {
            result = float.MinValue;
        }
        return result;
    }

    void Start()
    {
        Material true_mat = new Material(Shader.Find("Standard"));

        GameObject parent = new GameObject("WexbimModel");
        GameObject go;
        Mesh mesh;

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

        const string fileName = "IFCBuilding";
        //const string fileName = "Rvt_ifc_Wall";
        //const string fileName = "rac_basic_sample_project";
        //const string fileName = "walltemp";
        //const string fileName = "dooooooors";

        using (var fs = new FileStream(fileName+".wexBIM", FileMode.Open, FileAccess.Read))
        {
            using (var br = new BinaryReader(fs))
            {
                var magicNumber = br.ReadInt32();
                if (magicNumber != 94132117) throw new ArgumentException( "Magic number mismatch.");
                //   Assert.IsTrue(magicNumber == IfcStore.WexBimId);
                var version = br.ReadByte();
                var shapeCount = br.ReadInt32();
                var vertexCount = br.ReadInt32();
                var triangleCount = br.ReadInt32();
                var matrixCount = br.ReadInt32();
                var productCount = br.ReadInt32();
                var styleCount = br.ReadInt32();
                var meter = br.ReadSingle();
                //    Assert.IsTrue(meter > 0);
                var regionCount = br.ReadInt16();
                

                for (int i = 0; i < regionCount; i++)
                {
                    var population = br.ReadInt32();
                    var centreX = br.ReadSingle();
                    var centreY = br.ReadSingle();
                    var centreZ = br.ReadSingle();
                    var boundsBytes = br.ReadBytes(6 * sizeof(float));
                    var modelBounds = XbimRect3D.FromArray(boundsBytes);
                }

                
                for (int i = 0; i < styleCount; i++)
                {
                    var styleId = br.ReadInt32();
                    var red = br.ReadSingle();
                    var green = br.ReadSingle();
                    var blue = br.ReadSingle();
                    var alpha = br.ReadSingle();
                 
                    true_mat.SetColor(styleId, new Color(red, green, blue, alpha));
                }

                for (int i = 0; i < productCount; i++)
                {
                    var productLabel = br.ReadInt32();
                    var productType = br.ReadInt16();
                    var boxBytes = br.ReadBytes(6 * sizeof(float));
                    XbimRect3D bb = XbimRect3D.FromArray(boxBytes);
                }
                for (int i = 0; i < shapeCount; i++)
                {
                    var shapeRepetition = br.ReadInt32();
                    //   Assert.IsTrue(shapeRepetition > 0);

                    if (shapeRepetition > 1)
                    {
                        GameObject[] repes = new GameObject[shapeRepetition];
                        int[] ColorStyleId = new int[shapeRepetition];
                        for (int j = 0; j < shapeRepetition; j++)
                        {
                            var ifcProductLabel = br.ReadInt32();
                            var instanceTypeId = br.ReadInt16();
                            var instanceLabel = br.ReadInt32();
                            var styleId = br.ReadInt32();
                            var transform = XbimMatrix3D.FromArray(br.ReadBytes(sizeof(double) * 16));

                            
                            go = new GameObject(ifcProductLabel.ToString());
                            go.transform.parent = parent.transform;
                            repes[j] = go;
                            
                            //order of ro/po makes no diference
                            go.transform.rotation = new Quaternion(Flo_convert(transform.GetRotationQuaternion().X), Flo_convert(transform.GetRotationQuaternion().Y), Flo_convert(transform.GetRotationQuaternion().Z), Flo_convert(transform.GetRotationQuaternion().W));
                            go.transform.position = new Vector3(Flo_convert(transform.Translation.X / meter), Flo_convert(transform.Translation.Y / meter), Flo_convert(transform.Translation.Z / meter));

                            ColorStyleId[j] = styleId;
                           
                        }
                        var triangulation = br.ReadShapeTriangulation();
       
                        int TriangleIndexCount = 0;
                        

                        foreach (var face in triangulation.Faces)
                        {
                            foreach (var triIndex in face.Indices)
                            {
                                ++TriangleIndexCount;
                            }
                        }
                        
                        int VerticesCount = triangulation.Vertices.Count;
                        int FaceCount = triangulation.Faces.Count;
                        
                       Vector3[] vertices = new Vector3[VerticesCount]; //these eventully write into unity gameobject
                        int[] triangleIndices = new int[TriangleIndexCount];

                        //Debug.Log("styleId:" + styleId + ", ifcProductLabel: " + ifcProductLabel + ", faceCount: " + FaceCount + ", TriangleIndexCount: " + TriangleIndexCount);
                        int TriangleIndex;
                        foreach (var repete in repes)
                        {
                            int id = 0;
   
                            TriangleIndex = 0;
                            
                            for (int j = 0; j < VerticesCount; j++)
                            {
                                var vert = new Vector3(Flo_convert(triangulation.Vertices[j].X), Flo_convert(triangulation.Vertices[j].Y), Flo_convert(triangulation.Vertices[j].Z));
                                vertices[j] = vert / meter;
                            }
                            
                            for (int j = 0; j < FaceCount; j++)
                            {
                                for (int k = 0; k < triangulation.Faces[j].Indices.Count; k++)
                                {
                                    triangleIndices[TriangleIndex++] = triangulation.Faces[j].Indices[k];
                                }
                            }

                            repete.AddComponent<MeshFilter>();
                            repete.AddComponent<MeshRenderer>();
                            mesh = new Mesh() ;
                            repete.GetComponent<MeshFilter>().mesh = mesh;
                            
                            MeshRenderer meshRenderer = repete.GetComponent<MeshRenderer>();
                            meshRenderer.material.color = true_mat.GetColor(ColorStyleId[id++]);

                            mesh.vertices = vertices;// in unity vertices must defined before triangles
                            mesh.triangles = triangleIndices;

                            mesh.RecalculateNormals();
                            repete.AddComponent<MeshCollider>();
                        }
                        
                        //    Assert.IsTrue(triangulation.Vertices.Count > 0, "Number of vertices should be greater than zero");

                    }
                    else if (shapeRepetition == 1)
                    {
                        var ifcProductLabel = br.ReadInt32();
                        var instanceTypeId = br.ReadInt16();
                        var instanceLabel = br.ReadInt32();
                        var styleId = br.ReadInt32();
                        XbimShapeTriangulation triangulation = br.ReadShapeTriangulation();
                        //     Assert.IsTrue(triangulation.Vertices.Count > 0, "Number of vertices should be greater than zero");

                        go = new GameObject(ifcProductLabel.ToString());
                        go.transform.parent = parent.transform;
                        mesh = new Mesh();

                        go.AddComponent<MeshFilter>();
                        go.AddComponent<MeshRenderer>();
                        go.GetComponent<MeshFilter>().mesh = mesh;
                        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
                        meshRenderer.material.color = true_mat.GetColor(styleId);

                        int TriangleIndexCount = 0;
                        int TriangleIndex = 0;

                        foreach (var face in triangulation.Faces)
                        {
                            foreach (var triIndex in face.Indices)
                            {
                                ++TriangleIndexCount;
                            }
                        }

                        int VerticesCount = triangulation.Vertices.Count;
                        int FaceCount = triangulation.Faces.Count;

                        Vector3[] vertices = new Vector3[VerticesCount]; //these eventully write into unity gameobject
                        int[] triangleIndices = new int[TriangleIndexCount];
                        //Debug.Log("styleId:" + styleId + ", ifcProductLabel: " + ifcProductLabel + ", faceCount: " + FaceCount + ", TriangleIndexCount: " + TriangleIndexCount);

                        for (int j = 0; j < VerticesCount; j++)
                        {
                            vertices[j] = new Vector3(Flo_convert(triangulation.Vertices[j].X), Flo_convert(triangulation.Vertices[j].Y), Flo_convert(triangulation.Vertices[j].Z));
                            vertices[j] /= meter;
                        }
                        mesh.vertices = vertices;

                        for (int j = 0; j < FaceCount; j++)
                        {
                            for (int k = 0; k < triangulation.Faces[j].Indices.Count; k++)
                            {
                                triangleIndices[TriangleIndex++] = triangulation.Faces[j].Indices[k];
                            }
                        }
                        mesh.triangles = triangleIndices;
                        mesh.RecalculateNormals();
                        go.AddComponent<MeshCollider>();

                    }
                    
                }

        
            }
        }
        Debug.Log("finish start");
    }
    private void Update()
    {

        if (!swch)
        {
            GameObject go = GameObject.Find("WexbimModel");
            go.transform.Rotate(-90f,0,0); //first generate Wexbim XYZ then rotate the parent to XZY
            
            foreach (var child in go.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.name == "WexbimModel")
                    continue;
                var goo = child.gameObject;                 //shader will always be stadart instance in start(), update it here to make it right
                MeshRenderer meshRenderer = goo.GetComponent<MeshRenderer>();
              

                if (meshRenderer.material.color.a <.95f)    //only rewrite the shader when alpha make a difference
                    meshRenderer.material.shader = Shader.Find("Transparent/VertexLit");
                else meshRenderer.material.shader = Shader.Find("CullOffStandard"); //custome shader to render both sides of the face 
            }
            swch = true;
        }
    }
}
