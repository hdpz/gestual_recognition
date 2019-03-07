//=====================================
//        Motion Viewer Ribbon
// Create by Vincent MEYRUEIS 2018
// INREV Dept ATI University Paris8
//            Version 1.2
//=====================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Setplane : MonoBehaviour
{

    //Record
    public bool Record = true;
    public int FrameCount;
    public float BeginTime;
    public float DTime;
    public float MDTime;
    public GameObject MotionObj;
    public GameObject MotionRef;

    //Data Viz Property
    public bool PosActive = true;
    public Color Pos_Col = Color.black;
    public Vector3 CurrentPos;
    [Range(10, 200)]
    public int NbSpeedsPlane = 80;
    [Range(0, 0.5f)]
    public float QuantilePlane = 0.30f;
    [Range(0, 1.0f)]
    public float ThreshPlane = 0.35f;
    public float PlaneWidth = 10;
    public float PlaneHeight = 10;
    public GameObject PlaneViz = null;
    public bool PlaneMov = false;

    public bool SpeedActive = false;
    [Range(0, 10)]
    public float GizmoSpeedScale = 1;
    public Color Speed_Col = Color.green;
    public Vector3 CurrentSpeed;
    public float CurrentSpeedMag;

    public bool AccActive = false;
    [Range(0, 10)]
    public float GizmoAccScale = 1;
    public Color Acc_Col = Color.red;
    public Vector3 CurrentAcc;
    public float CurrentAccMag;

    public bool JerkActive = false;
    [Range(0, 10)]
    public float GizmoJerkScale = 1;
    public Color Jerk_Col = Color.blue;
    public Vector3 CurrentJerk;
    public float CurrentJerkMag;

    public bool OrientationActive = false;
    [Range(0, 10)]
    public float GizmoQuatScale = 1;
    public Color Quat_Col = Color.yellow;
    public Quaternion CurrentQuat;
    public Vector3 CurrentQuatAxis;
    public float CurrentQuatAngle;

    //Trail property
    public bool Trail = true;
    [Range(0, 1)]
    public float TrailLinesWidth = 0.1f;
    public int TrailLength = 360;
    public AnimationCurve Alpha;

    //Vector property
    public bool Vector = true;
    [Range(0, 1)]
    public float VectLinesWidth = 0.1f;

    public Material LinesMaterials;
    public Material RibbonMaterials;

    //Filter
    [Range(1, 120)]
    public int MovingAverageSample = 15;

    //RefValue
    Matrix4x4 RefMat;
    Matrix4x4 RefMatInv;
    Quaternion RefQuat;
    Vector3 RefPos;

    //Data
    List<float> Times = new List<float>();
    List<float> DTimes = new List<float>();
    List<Vector3> Pos = new List<Vector3>();
    List<Vector3> Speed = new List<Vector3>();
    List<Vector3> Acc = new List<Vector3>();
    List<Vector3> Jerk = new List<Vector3>();
    List<Quaternion> Quat = new List<Quaternion>();

    //Data Tag DEBUG put in Define 
    string Speed_Tag = "Speed";
    string Acc_Tag = "Acc";
    string Jerk_Tag = "Jerk";
    string Pos_Tag = "Pos";
    string Quat_Tag = "Orientation";

    //Data Trail 
    GameObject Pos_Trail;
    GameObject Speed_Ribbon;
    GameObject Acc_Ribbon;
    GameObject Jerk_Ribbon;
    GameObject Quat_Trail;

    //Data Vectors
    GameObject Speed_Vect;
    GameObject Acc_Vect;
    GameObject Jerk_Vect;
    GameObject Quat_Vect;

    //Trail Object
    List<GameObject> TrailObjects = new List<GameObject>();
    List<GameObject> VectObjects = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        RecordInit();
        PlaneViz = GameObject.CreatePrimitive(PrimitiveType.Plane);
        PlaneViz.transform.localScale = new Vector3(0, 0, 0);
    }


    void Update()
    {

        if (Record)
            RecordData();

        // Draw Vector
        if (Vector)
        {
            if (SpeedActive)
            {
                Speed_Vect.SetActive(true);
                DrawVector(Speed_Vect, Speed[Speed.Count - 1], GizmoSpeedScale);
            }
            else
            {
                Speed_Vect.SetActive(false);
            }

            if (AccActive)
            {
                Acc_Vect.SetActive(true);
                DrawVector(Acc_Vect, Acc[Acc.Count - 1], GizmoAccScale);
            }
            else
            {
                Acc_Vect.SetActive(false);
            }

            if (JerkActive)
            {
                Jerk_Vect.SetActive(true);
                DrawVector(Jerk_Vect, Jerk[Jerk.Count - 1], GizmoJerkScale);
            }
            else
            {
                Jerk_Vect.SetActive(false);
            }
            if (OrientationActive)
            {
                Vector3 QuatAxis;
                float QuatAngle;

                Quaternion DQuat;
                if (Quat.Count < 2)
                    DQuat = Quaternion.identity;
                else
                    DQuat = Quaternion.Inverse(Quat[Quat.Count - 2]) * Quat[Quat.Count - 1];

                DQuat.ToAngleAxis(out QuatAngle, out QuatAxis);
                Quat_Vect.SetActive(true);
                DrawVector(Quat_Vect, QuatAxis * QuatAngle, GizmoQuatScale);
            }
            else
            {
                Quat_Vect.SetActive(false);
            }
        }
        else
        {
            Speed_Vect.SetActive(false);
            Acc_Vect.SetActive(false);
            Jerk_Vect.SetActive(false);
            Quat_Vect.SetActive(false);
        }


        //Draw Trail
        if (Trail)
        {

            //Draw Trails and vect
            if (PosActive)
            {
                Pos_Trail.SetActive(true);
                Draw_Trail(Pos, Pos_Trail, 1);
            }
            else
                Pos_Trail.SetActive(false);


            if (SpeedActive)
            {
                Speed_Ribbon.SetActive(true);
                Draw_Ribbon(Speed, Pos, Speed_Ribbon, GizmoSpeedScale);
            }
            else
            {
                Speed_Ribbon.SetActive(false);
            }

            if (AccActive)
            {
                Acc_Ribbon.SetActive(true);
                Draw_Ribbon(Acc, Pos, Acc_Ribbon, GizmoAccScale);
            }
            else
            {
                Acc_Ribbon.SetActive(false);
            }

            if (JerkActive)
            {
                Jerk_Ribbon.SetActive(true);
                Draw_Ribbon(Jerk, Pos, Jerk_Ribbon, GizmoJerkScale);
            }
            else
            {
                Jerk_Ribbon.SetActive(false);
            }


            if (OrientationActive)
            {
                Quat_Trail.SetActive(true);

            }
            else
            {
                Quat_Trail.SetActive(false);
            }

        }
        else
        {
            Pos_Trail.SetActive(false);
            Speed_Ribbon.SetActive(false);
            Acc_Ribbon.SetActive(false);
            Jerk_Ribbon.SetActive(false);
            Quat_Trail.SetActive(false);
        }


    }

    // Record and Compute Data
    void RecordData()
    {

        Times.Add(Time.time - BeginTime);

        //record Delta Times
        if (Times.Count < 2)
            DTimes.Add(0.0f);
        else
        {
            DTime = Times[Times.Count - 1] - Times[Times.Count - 2];
            DTimes.Add(DTime);
        }

        MDTime = MovingAverage(DTimes, DTimes.Count - 1, MovingAverageSample);
        //DTime = Time.deltaTime;
        //DTime = 0.0165f;


        //Referential
        if (MotionRef)
        {
            RefMat = MotionRef.transform.worldToLocalMatrix;
            RefMatInv = MotionRef.transform.localToWorldMatrix;
            RefQuat = MotionRef.transform.rotation;
            RefPos = MotionRef.transform.position;
        }
        else
        {
            RefMat = Matrix4x4.identity;
            RefMatInv = Matrix4x4.identity;
            RefQuat = Quaternion.identity;
            RefPos = Vector3.zero;
        }

        //record Position
        CurrentPos = RefMat * (MotionObj.transform.position - RefPos);
        Pos.Add(CurrentPos);

        //record Quaternion
        if (Quat.Count < 2)
            Quat.Add(Quaternion.identity);
        else
        {
            Quat.Add(Quaternion.Inverse(RefQuat) * MotionObj.transform.rotation);
            CurrentQuat = Quaternion.Inverse(Quat[Quat.Count - 2]) * Quat[Quat.Count - 1];
            CurrentQuat.ToAngleAxis(out CurrentQuatAngle, out CurrentQuatAxis);
            CurrentQuatAngle = CurrentQuatAngle * Mathf.Rad2Deg;
        }

        //record Speed
        if (Pos.Count < 2)
            Speed.Add(Vector3.zero);
        else
        {
            Vector3 Pos1 = MovingAverage(Pos, Pos.Count - 1, MovingAverageSample);
            Vector3 Pos2 = MovingAverage(Pos, Pos.Count - 2, MovingAverageSample);
            CurrentSpeed = (Pos1 - Pos2) / (MDTime);
            CurrentSpeedMag = CurrentSpeed.magnitude;
            Speed.Add(CurrentSpeed);
            //Detect plane
            if (Speed.Count > NbSpeedsPlane)
            {
                Draw_Plane(Pos, Speed, Speed.Count - NbSpeedsPlane, Speed.Count - 1);
            }
        }


        //record Acc
        if (Speed.Count < 2)
            Acc.Add(Vector3.zero);
        else
        {
            Vector3 Speed1 = MovingAverage(Speed, Speed.Count - 1, MovingAverageSample);
            Vector3 Speed2 = MovingAverage(Speed, Speed.Count - 2, MovingAverageSample);
            CurrentAcc = (Speed1 - Speed2) / (MDTime);
            CurrentAccMag = CurrentAcc.magnitude;
            Acc.Add(CurrentAcc);
        }

        //record Jerk
        if (Acc.Count < 2)
            Jerk.Add(Vector3.zero);
        else
        {
            Vector3 Acc1 = MovingAverage(Acc, Acc.Count - 1, MovingAverageSample);
            Vector3 Acc2 = MovingAverage(Acc, Acc.Count - 2, MovingAverageSample);
            CurrentJerk = (Acc1 - Acc2) / (MDTime);
            CurrentJerkMag = CurrentJerk.magnitude;
            Jerk.Add(CurrentJerk);
        }

        FrameCount++;
    }

    void Draw_Plane(List<Vector3> posVec, List<Vector3> speedVec, int startIndex, int endIndex)
    {
        Vector3 speedStart = speedVec[startIndex];
        Vector3 norm, norm_mean, pos;
        List<Vector3> norms = new List<Vector3>();

        for (int i = startIndex; i < endIndex-1; i++)
        {
            norm = Compute_Vect_Prod(speedVec[i], speedVec[i+1]);
            norm.Normalize();
            norms.Add(norm);
        }
        norm_mean = GetMeanVector(norms);
        PlaneMov = Detect_Plane(norm_mean, norms);

        if (PlaneMov)
        {
            norm = GetMeanVector(norms);
            pos = posVec[posVec.Count - 1];

            PlaneViz.transform.name = "PlaneViz";
            PlaneViz.transform.up = norm;
            PlaneViz.transform.position = new Vector3(pos.x, pos.y, pos.z);
            PlaneViz.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        }
        else
        {
            PlaneViz.transform.localScale = new Vector3(0f, 0f, 0f);
        }

    }

    Vector3 Compute_Vect_Prod(Vector3 a, Vector3 b)
    {
        Vector3 res = new Vector3();
        res.Set((a[1] * b[2]) - (a[2] * b[1]), (a[2] * b[0]) - (b[2] * a[0]), (a[0] * b[1]) - (b[0] * a[1]));
        return res;
    }

    bool Detect_Plane(Vector3 norm_mean, List<Vector3> norms)
    {
        float[] arr_mag = new float[norms.Count];
        for (int i=0; i<norms.Count; i++)
        {
            arr_mag[i] = Compute_Vect_Prod(norm_mean, norms[i]).magnitude;
        }

        Array.Sort(arr_mag);

        foreach (float mag in arr_mag)
        {
            print(mag.ToString());
        }

        int high_lim = (int)((1-QuantilePlane) * norms.Count);

        float mean_mag = 0;
        for (int i=0; i<high_lim; i++)
        {
            mean_mag += arr_mag[i]/high_lim;
        }
        print("Mean mag");
        print(mean_mag.ToString());
        return (mean_mag < ThreshPlane);
    }

    //initialisation
    void RecordInit()
    {

        //Tracked Object
        if (!MotionObj)
        {
            MotionObj = gameObject;
        }

        Pos.Clear();
        Speed.Clear();
        Acc.Clear();
        Jerk.Clear();
        Quat.Clear();
        Times.Clear();

        FrameCount = 0;

        Pos_Trail = CreatTrailObject(Pos_Tag, Pos_Col);
        Speed_Ribbon = CreatRibbonObject(Speed_Tag, Speed_Col);
        Acc_Ribbon = CreatRibbonObject(Acc_Tag, Acc_Col);
        Jerk_Ribbon = CreatRibbonObject(Jerk_Tag, Jerk_Col);
        Quat_Trail = CreatRibbonObject(Quat_Tag, Quat_Col);

        Speed_Vect = CreatVectObject(Speed_Tag, Speed_Col);
        Acc_Vect = CreatVectObject(Acc_Tag, Acc_Col);
        Jerk_Vect = CreatVectObject(Jerk_Tag, Jerk_Col);
        Quat_Vect = CreatVectObject(Quat_Tag, Quat_Col);

        BeginTime = Time.time;
    }




    //DrawRibbon
    void Draw_Ribbon(List<Vector3> Vectors, List<Vector3> Positions, GameObject RibbonObj, float DrawScale)
    {
        if (Vectors.Count != Positions.Count)
        {
            //Erreur Count
            Debug.Log("Error: Vectors/Position Count");
            return;
        }

        int Length = (Vectors.Count < TrailLength) ? Vectors.Count : TrailLength;

        RibbonRenderer Ribbon_Renderer = RibbonObj.GetComponent<RibbonRenderer>();

        Ribbon_Renderer.Vectors.Clear();
        Ribbon_Renderer.Positions.Clear();

        for (int i = 0; i < Length; i++)
        {
            Vector3 TVect = RefMatInv * (Vectors[Vectors.Count - 1 - i] * DrawScale); //DEBUG Test Legth -> Vector.count-1
            Vector3 TPos = RefMatInv * (Positions[Positions.Count - 1 - i]);

            Ribbon_Renderer.Vectors.Add(TVect);
            Ribbon_Renderer.Positions.Add(TPos + RefPos);
        }
        Ribbon_Renderer.Alpha = Alpha;

    }

    //DrawTrail with Position
    void Draw_Trail(List<Vector3> Vectors, List<Vector3> Positions, GameObject TrailObj, float DrawScale)
    {
        int Length = (Vectors.Count < TrailLength) ? Vectors.Count : TrailLength;
        LineRenderer Trail_Renderer = TrailObj.GetComponent<LineRenderer>();
        Trail_Renderer.positionCount = Length;
        for (int i = 0; i < Length; i++)
        {
            Vector3 TVect = RefMatInv * (Positions[Positions.Count - 1 - i] + Vectors[Vectors.Count - 1 - i] * DrawScale);
            Trail_Renderer.SetPosition(i, TVect + RefPos);
        }
        Trail_Renderer.startWidth = 0.01f * TrailLinesWidth;
        Trail_Renderer.endWidth = TrailLinesWidth;
    }

    //DrawTrail without Position
    void Draw_Trail(List<Vector3> Vectors, GameObject TrailObj, float DrawScale)
    {
        int Length = (Vectors.Count < TrailLength) ? Vectors.Count : TrailLength;
        LineRenderer Trail_Renderer = TrailObj.GetComponent<LineRenderer>();
        Trail_Renderer.positionCount = Length;
        for (int i = 0; i < Length; i++)
        {
            Vector3 TVect = RefMatInv * (Vectors[Vectors.Count - 1 - i] * DrawScale);
            Trail_Renderer.SetPosition(i, TVect + RefPos);
        }
        Trail_Renderer.startWidth = 0.01f * TrailLinesWidth;
        Trail_Renderer.endWidth = TrailLinesWidth;
    }   

    Vector3 GetMeanVector(List<Vector3> vectors)
    {
        if (vectors.Count == 0)
            return Vector3.zero;
        float x = 0f;
        float y = 0f;
        float z = 0f;
        foreach (Vector3 vec in vectors)
        {
            x += vec.x;
            y += vec.y;
            z += vec.z;
        }
        return new Vector3(x / vectors.Count, y / vectors.Count, z / vectors.Count);
    }

    //DEBUG
    void DrawRotation_Trail()
    {
        int Length = (Quat.Count < TrailLength) ? Quat.Count : TrailLength;
        LineRenderer Quat_Trail_Renderer = Quat_Trail.GetComponent<LineRenderer>();
        Quat_Trail_Renderer.positionCount = Length - 1;

        for (int i = 0; i < Length - 1; i++)
        { //Debug

            Quaternion DQuat;
            Vector3 QuatAxis;
            float QuatAngle;

            if (Quat.Count < 2)
                DQuat = Quaternion.identity;
            else
                DQuat = Quaternion.Inverse(Quat[Quat.Count - 2 - i]) * Quat[Quat.Count - 1 - i];

            DQuat.ToAngleAxis(out QuatAngle, out QuatAxis);
            Vector3 TQuatAxis = RefMatInv * (Pos[Pos.Count - 1 - i] + QuatAxis.normalized * QuatAngle * GizmoQuatScale);
            Quat_Trail_Renderer.SetPosition(i, TQuatAxis + RefPos);
        }

        Quat_Trail_Renderer.startWidth = 0.01f * TrailLinesWidth;
        Quat_Trail_Renderer.endWidth = TrailLinesWidth;
    }



    //Draw Vectors with Line render
    void DrawVector(GameObject Obj, Vector3 CurrentVect, float GizmoVectScale)
    {

        LineRenderer VectLine = Obj.GetComponent<LineRenderer>();

        CurrentVect *= GizmoVectScale;

        Vector3 TVect = RefMatInv * CurrentVect;
        VectLine.positionCount = 2;
        VectLine.SetPosition(0, Obj.transform.position);
        VectLine.SetPosition(1, Obj.transform.position + TVect);

        VectLine.startWidth = VectLinesWidth;
        VectLine.endWidth = 0.01f * VectLinesWidth;
        //	VectLine.widthMultiplier = 0.5f;
    }


    //Filter MovingAverage (Vector)
    Vector3 MovingAverage(List<Vector3> VectList, int From, int Number)
    {

        Vector3 Res = Vector3.zero;

        if (Number <= 1)
            return VectList[From];

        if (From < Number)
        {
            for (int i = From; i < VectList.Count; i++)
            {
                Res += VectList[i];
            }
            Res /= (VectList.Count - From);
        }
        else
        {
            for (int i = 0; i < Number; i++)
            {
                Res += VectList[From - i];
            }
            Res /= Number;
        }
        return Res;
    }

    //Filter MovingAverage (Float)
    float MovingAverage(List<float> List, int From, int Number)
    {

        float Res = 0.0f;

        if (Number <= 1)
            return List[From];

        if (From < Number)
        {
            for (int i = From; i < (List.Count); i++)
            {
                Res += List[i];
            }
            Res /= (List.Count - From);
        }
        else
        {
            for (int i = 0; i < Number; i++)
            {
                Res += List[From - i];
            }
            Res /= Number;
        }
        return Res;
    }


    //Compute Gradiant for Trail Fade Color
    Gradient SetColorGradient(Color Col)
    {
        Gradient ColGrad = new Gradient();
        GradientColorKey[] KeysColor = new GradientColorKey[2];
        KeysColor[0].color = Col;
        KeysColor[0].time = 0;
        KeysColor[1].color = Col;
        KeysColor[1].time = 1;

        GradientAlphaKey[] KeysAlf = new GradientAlphaKey[2];
        KeysAlf[0].alpha = 1;
        KeysAlf[0].time = 0;
        KeysAlf[1].alpha = 0;
        KeysAlf[1].time = 1;

        ColGrad.SetKeys(KeysColor, KeysAlf);
        return ColGrad;
    }

    //Compute Gradiant for Vector Full Color
    Gradient SetVectColorGradient(Color Col)
    {
        Gradient ColGrad = new Gradient();
        GradientColorKey[] KeysColor = new GradientColorKey[2];
        KeysColor[0].color = Col;
        KeysColor[0].time = 0;
        KeysColor[1].color = Col;
        KeysColor[1].time = 1;

        GradientAlphaKey[] KeysAlf = new GradientAlphaKey[2];
        KeysAlf[0].alpha = 1;
        KeysAlf[0].time = 0;
        KeysAlf[1].alpha = 1;
        KeysAlf[1].time = 1;

        ColGrad.SetKeys(KeysColor, KeysAlf);
        return ColGrad;
    }

    //Creat RibbonTrail Object Parent to track Object
    GameObject CreatRibbonObject(string Name, Color Col)
    {

        GameObject RibbonObject = new GameObject();
        RibbonObject.transform.position = MotionObj.transform.position;
        RibbonObject.transform.rotation = MotionObj.transform.rotation;
        RibbonObject.name = Name + "_Ribbon";
        RibbonObject.transform.parent = MotionObj.gameObject.transform;
        RibbonRenderer Ribbon = RibbonObject.AddComponent<RibbonRenderer>();

        Ribbon.Material = RibbonMaterials;

        Ribbon.Color = SetColorGradient(Col);

        TrailObjects.Add(RibbonObject);

        return RibbonObject;
    }

    //Creat Trail Object Parent to track Object
    GameObject CreatTrailObject(string Name, Color Col)
    {

        GameObject TrailObject = new GameObject();
        TrailObject.transform.position = MotionObj.transform.position;
        TrailObject.transform.rotation = MotionObj.transform.rotation;
        TrailObject.name = Name + "_Trail";
        TrailObject.transform.parent = MotionObj.gameObject.transform;
        LineRenderer Lines = TrailObject.AddComponent<LineRenderer>();
        Lines.material = LinesMaterials;

        Lines.colorGradient = SetColorGradient(Col);

        Lines.receiveShadows = false;
        Lines.useWorldSpace = true;

        TrailObjects.Add(TrailObject);

        return TrailObject;
    }


    //Creat Vector Object Parent to track Object
    GameObject CreatVectObject(string Name, Color Col)
    {

        GameObject VectObject = new GameObject();
        VectObject.transform.position = MotionObj.transform.position;
        VectObject.transform.rotation = MotionObj.transform.rotation;
        VectObject.name = Name + "_Vect";
        VectObject.transform.parent = MotionObj.gameObject.transform;
        LineRenderer Lines = VectObject.AddComponent<LineRenderer>();
        Lines.material = LinesMaterials;

        Lines.colorGradient = SetVectColorGradient(Col);

        Lines.receiveShadows = false;
        Lines.useWorldSpace = true;

        VectObjects.Add(VectObject);

        return VectObject;
    }


    // DEBUG TO USE
    Vector3 Deriv(List<Vector3> Value, float DTime)
    {

        if (Value.Count < 2)
            return Vector3.zero;
        return (Value[Value.Count - 1] - Value[Value.Count - 2]) / DTime;
    }

}
