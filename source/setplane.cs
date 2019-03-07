//=====================================
//      Motion Viewer
// Create by Vincent MEYRUEIS 2017
// INREV Dept ATI University Paris8
//      Version 1.2
//=====================================
​
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
​
public class MotionView : MonoBehaviour
{
​
	//Record
	public bool Record = true;
    public int FrameCount;
    public float BeginTime;
    public float DTime;
    public float MDTime;
    public GameObject MotionObj;
    public GameObject MotionRef; 
​
	//Data Viz Property
	public bool PosActive = true;
    public Color Pos_Col = Color.black;
    public Vector3 CurrentPos;
​
	public bool SpeedActive = false;
    [Range(0, 10)]
    public float GizmoSpeedScale = 1;
    public Color Speed_Col = Color.green;
    public Vector3 CurrentSpeed;
    public float CurrentSpeedMag;
​
	public bool AccActive = false;
    [Range(0, 10)]
    public float GizmoAccScale = 1;
    public Color Acc_Col = Color.red;
    public Vector3 CurrentAcc;
    public float CurrentAccMag;
​
	public bool JerkActive = false;
    [Range(0, 10)]
    public float GizmoJerkScale = 1;
    public Color Jerk_Col = Color.blue;
    public Vector3 CurrentJerk;
    public float CurrentJerkMag;
​
	public bool OrientationActive = false;
    [Range(0, 10)]
    public float GizmoQuatScale = 1;
    public Color Quat_Col = Color.yellow;
    public Quaternion CurrentQuat;
    public Vector3 CurrentQuatAxis;
    public float CurrentQuatAngle;
​
	//Trail property
	public bool Trail = true;
    [Range(0, 1)]
    public float TrailLinesWidth = 0.1f;
    public int TrailLength = 120;
​
	//Vector property
	public bool Vector = true;
    [Range(0, 1)]
    public float VectLinesWidth = 0.1f;
​
	public Material LinesMaterials;
​
	//Filter
	[Range(1, 120)]
    public int MovingAverageSample = 15;
​
	//RefValue
	Matrix4x4 RefMat;
    Matrix4x4 RefMatInv;
    Quaternion RefQuat;
    Vector3 RefPos;
​
	//Data
	List<float> Times = new List<float>();
    List<float> DTimes = new List<float>();
    List<Vector3> Pos = new List<Vector3>();
    List<Vector3> Speed = new List<Vector3>();
    List<Vector3> Acc = new List<Vector3>();
    List<Vector3> Jerk = new List<Vector3>();
    List<Quaternion> Quat = new List<Quaternion>();
​
	//Data Tag DEBUG put in Define 
	string Speed_Tag = "Speed";
    string Acc_Tag = "Acc";
    string Jerk_Tag = "Jerk";
    string Pos_Tag = "Pos";
    string Quat_Tag = "Orientation";
​
	//Data Trail 
	GameObject Pos_Trail;
    GameObject Speed_Trail;
    GameObject Acc_Trail;
    GameObject Jerk_Trail;
    GameObject Quat_Trail; 
​
	//Data Vectors
	GameObject Speed_Vect;
    GameObject Acc_Vect;
    GameObject Jerk_Vect;
    GameObject Quat_Vect;
​
	//Trail Object
	List<GameObject> TrailObjects = new List<GameObject>();
    List<GameObject> VectObjects = new List<GameObject>();
​
​
​
	// Use this for initialization
	void Start()
    {
        RecordInit();
​
​
​
	}
​
​
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
​
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
​
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
​
​
		//Draw Trail
		if (Trail)
        {
​
			//Draw Trails and vect
			if (PosActive)
            {
                Pos_Trail.SetActive(true);
                DrawPos_Trail();
            }
            else
                Pos_Trail.SetActive(false);
​
​
			if (SpeedActive)
            {
                Speed_Trail.SetActive(true);
                DrawSpeed_Trail();
            }
            else
            {
                Speed_Trail.SetActive(false);
            }
​
			if (AccActive)
            {
                Acc_Trail.SetActive(true);
                DrawAcc_Trail();
            }
            else
            {
                Acc_Trail.SetActive(false);
            }
​
			if (JerkActive)
            {
                Jerk_Trail.SetActive(true);
                DrawJerk_Trail();
            }
            else
            {
                Jerk_Trail.SetActive(false);
            }
​
​
			if (OrientationActive)
            {
                Quat_Trail.SetActive(true);
                DrawRotation_Trail();
            }
            else
            {
                Quat_Trail.SetActive(false);
            }
​
		}
        else
        {
            Pos_Trail.SetActive(false);
            Speed_Trail.SetActive(false);
            Acc_Trail.SetActive(false);
            Jerk_Trail.SetActive(false);
            Quat_Trail.SetActive(false);
        }
			
​
	}
​
​
​
	// Record and Compute Data
	void RecordData()
    {
​
		Times.Add(Time.time - BeginTime);
​
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
​
​
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
​
		//record Position
		CurrentPos = RefMat * (MotionObj.transform.position - RefPos);
        Pos.Add(CurrentPos);
​
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
​
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
        }
​
		
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
​
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
​
​
		FrameCount++;
    }
​
	//initialisation
	void RecordInit()
    {
​
		//Tracked Object
		if (!MotionObj)
        {
            MotionObj = gameObject;
        }
​
		Pos.Clear();
        Speed.Clear();
        Acc.Clear();
        Jerk.Clear();
        Quat.Clear();
        Times.Clear();
​
		FrameCount = 0;
​
		Pos_Trail = CreatTrailObject(Pos_Tag, Pos_Col);
        Speed_Trail = CreatTrailObject(Speed_Tag, Speed_Col);
        Acc_Trail = CreatTrailObject(Acc_Tag, Acc_Col);
        Jerk_Trail = CreatTrailObject(Jerk_Tag, Jerk_Col);
        Quat_Trail = CreatTrailObject(Quat_Tag, Quat_Col); 
​
		Speed_Vect = CreatVectObject(Speed_Tag, Speed_Col);
        Acc_Vect = CreatVectObject(Acc_Tag, Acc_Col);
        Jerk_Vect = CreatVectObject(Jerk_Tag, Jerk_Col);
        Quat_Vect = CreatVectObject(Quat_Tag, Quat_Col); 
​
		BeginTime = Time.time;
    }

    void DrawPos_Trail()
    {
        int Length = (Pos.Count < TrailLength) ? Pos.Count : TrailLength;
        LineRenderer Pos_Trail_Renderer = Pos_Trail.GetComponent<LineRenderer>();
        Pos_Trail_Renderer.positionCount = Length;
        for (int i = 0; i < Length; i++)
        {
            Vector3 TPos = RefMatInv * Pos[Pos.Count - 1 - i];
            Pos_Trail_Renderer.SetPosition(i, TPos + RefPos);
        }
        Pos_Trail_Renderer.startWidth = 0.01f * TrailLinesWidth;
        Pos_Trail_Renderer.endWidth = TrailLinesWidth;
    }
​
	void DrawSpeed_Trail()
    {
        int Length = (Speed.Count < TrailLength) ? Speed.Count : TrailLength;
        LineRenderer Speed_Trail_Renderer = Speed_Trail.GetComponent<LineRenderer>();
        Speed_Trail_Renderer.positionCount = Length;
        for (int i = 0; i < Length; i++)
        {
            Vector3 TSpeed = RefMatInv * (Pos[Pos.Count - 1 - i] + Speed[Speed.Count - 1 - i] * GizmoSpeedScale);
            Speed_Trail_Renderer.SetPosition(i, TSpeed + RefPos);
        }
        Speed_Trail_Renderer.startWidth = 0.01f * TrailLinesWidth;
        Speed_Trail_Renderer.endWidth = TrailLinesWidth;
    }

    void DrawAcc_Trail()
    {
        int Length = (Acc.Count < TrailLength) ? Acc.Count : TrailLength;
        LineRenderer Acc_Trail_Renderer = Acc_Trail.GetComponent<LineRenderer>();
        Acc_Trail_Renderer.positionCount = Length;
        for (int i = 0; i < Length; i++)
        {
            Vector3 TAcc = RefMatInv * (Pos[Pos.Count - 1 - i] + Acc[Acc.Count - 1 - i] * GizmoAccScale);
            Acc_Trail_Renderer.SetPosition(i, TAcc + RefPos);
        }
        Acc_Trail_Renderer.startWidth = 0.01f * TrailLinesWidth;
        Acc_Trail_Renderer.endWidth = TrailLinesWidth;
    }
​
	void DrawJerk_Trail()
    {
        int Length = (Jerk.Count < TrailLength) ? Jerk.Count : TrailLength;
        LineRenderer Jerk_Trail_Renderer = Jerk_Trail.GetComponent<LineRenderer>();
        Jerk_Trail_Renderer.positionCount = Length;
        for (int i = 0; i < Length; i++)
        {
            Vector3 TJerk = RefMatInv * (Pos[Pos.Count - 1 - i] + Jerk[Jerk.Count - 1 - i] * GizmoJerkScale);
            Jerk_Trail_Renderer.SetPosition(i, TJerk + RefPos);
        }
        Jerk_Trail_Renderer.startWidth = 0.01f * TrailLinesWidth;
        Jerk_Trail_Renderer.endWidth = TrailLinesWidth;
    }
​
	void DrawRotation_Trail()
    {
        int Length = (Quat.Count < TrailLength) ? Quat.Count : TrailLength;
        LineRenderer Quat_Trail_Renderer = Quat_Trail.GetComponent<LineRenderer>();
        Quat_Trail_Renderer.positionCount = Length - 1;
​
		for (int i = 0; i < Length - 1; i++)
        { //Debug
​
			Quaternion DQuat;
            Vector3 QuatAxis;
            float QuatAngle;
​
			if (Quat.Count < 2)
                DQuat = Quaternion.identity;
            else
                DQuat = Quaternion.Inverse(Quat[Quat.Count - 2 - i]) * Quat[Quat.Count - 1 - i];
​
			DQuat.ToAngleAxis(out QuatAngle, out QuatAxis);
            Vector3 TQuatAxis = RefMatInv * (Pos[Pos.Count - 1 - i] + QuatAxis.normalized * QuatAngle * GizmoQuatScale);
            Quat_Trail_Renderer.SetPosition(i, TQuatAxis + RefPos);
        }

        Quat_Trail_Renderer.startWidth = 0.01f * TrailLinesWidth;
        Quat_Trail_Renderer.endWidth = TrailLinesWidth;
    }	
​
	//Draw Vectors with Line render
	void DrawVector(GameObject Obj, Vector3 CurrentVect, float GizmoVectScale)
    {
​
		LineRenderer VectLine = Obj.GetComponent<LineRenderer>();
​
		CurrentVect *= GizmoVectScale;
​
		Vector3 TVect = RefMatInv * CurrentVect;
        VectLine.positionCount = 2;
        VectLine.SetPosition(0, Obj.transform.position);
        VectLine.SetPosition(1, Obj.transform.position + TVect);
​
		VectLine.startWidth = VectLinesWidth;
        VectLine.endWidth = 0.01f * VectLinesWidth;
        //	VectLine.widthMultiplier = 0.5f;
    }	
​
	//Filter MovingAverage (Vector)
	Vector3 MovingAverage(List<Vector3> VectList, int From, int Number)
    {
​
		Vector3 Res = Vector3.zero;
​
		if (Number <= 1)
            return VectList[From];
​
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
​
	//Filter MovingAverage (Float)
	float MovingAverage(List<float> List, int From, int Number)
    {
​
		float Res = 0.0f;
​
		if (Number <= 1)
            retu...
Collapse
 This snippet was truncated for display; see it in full

Message Input


Message yoann