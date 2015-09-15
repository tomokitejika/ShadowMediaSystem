using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;


public class Generator : MonoBehaviour {

    public GameObject modelofRobot;

    GameObject model;
    CIPCReceiver cipc;
    Human robotBone;
    Human target;
    public List<Human> List_Humans;

    System.Threading.Timer timer;
    Time time;

    //パラメータ等
    #region
    int datalength;
    int timerIntervalMillsec;
    Vector3[] PositionofRobot;
    Vector3[] PositionofHuman;
    Vector3 Vec;
    float[] alfa;
    float initLength;
    int S;
    float[] Ka;
    float[] Kb;
    int secondPosition;
    int secondStep;
    #endregion

    // Use this for initialization
	void Start () 
    {
        this.cipc = GameObject.FindGameObjectWithTag("CIPC").GetComponent<CIPCReceiver>();
        this.robotBone = new Human();

        this.List_Humans = new List<Human>();
        this.robotBone = new Human();

        Debug.Log("Generator");
        this.model = Instantiate(this.modelofRobot);
        this.model.transform.parent = this.transform;


        this.Init();
	}
	
	// Update is called once per frame
	void Update () 
    {
        //test用
        #region
        /*
        if (this.cipc.List_Humans.Count != 0)
        {
            this.List_Humans = this.cipc.List_Humans;
            //this.robotBone = this.List_Humans[0];
        }
        */
        #endregion

        //List_Humanにrobotactionを追加
        this.RobotAction();
	}

    //影ロボットの動き
    //相澤モデル
    void Init()
    {
        this.timerIntervalMillsec = 100;

        //データ格納配列
        this.datalength = 3;
        this.PositionofHuman = new Vector3[this.datalength];
        this.PositionofRobot = new Vector3[this.datalength];
        
        //影ロボット最初の移動距離[m]
        this.initLength = 0.1f;
        this.Vec = Vector3.right;
        for (int i = 0; i < this.PositionofRobot.Length; i++)
        {
            this.PositionofRobot[i] = new Vector3(10,0,0) - this.Vec * this.initLength * i * Time.deltaTime;
            this.PositionofHuman[i] = new Vector3(0,0,0)  + this.Vec * this.initLength * i * Time.deltaTime;
        }
        this.model.transform.position = this.PositionofRobot[1];
        //定数初期値
        this.Ka = new float[2];
        this.Kb = new float[2];
        this.Ka[0] = 1;
        this.Ka[1] = 0.25f;
        this.Kb[0] = 1;
        this.Kb[1] = 0.25f;
        this.S = 1;
    }

    void RobotAction()
    {
        try
        {
            //Debug.Log(this.cipc.List_Humans.Count.ToString());
            //データ取得
            
            if (this.cipc.List_Humans.Count != 0)
            {
                //Debug.Log("RobotAction");

                this.robotBone = this.cipc.List_Humans[0];
                //this.robotBone.bones[0].position += Vector3.up;
                
                //データ入れ替え
                this.ReplaceData(this.datalength, ref this.PositionofRobot);
                this.ReplaceData(this.datalength, ref this.PositionofHuman);
                this.AddNewDataToArray(this.cipc.List_Humans[0].bones[0].position, this.datalength, ref this.PositionofHuman);
                //this.AddNewDataToArray(Vector3.zero, this.datalength, ref this.PositionofHuman);                
                //Debug.Log("Data:" + this.PositionofHuman[0].ToString());

                //速度算出
                Vector3 velocityofRobot = (this.PositionofRobot[1] - this.PositionofRobot[0]) / Time.deltaTime;
                Vector3 velocityofHuman = (this.PositionofHuman[1] - this.PositionofHuman[0]) / Time.deltaTime;
                //Debug.Log("Robot[0] :" + this.PositionofRobot[0].ToString());
                //Debug.Log("Robot[1] :" + this.PositionofRobot[1].ToString());


                //移動ベクトル
                this.Vec = this.PositionofHuman[1] - this.PositionofRobot[1];
                this.Vec /= this.Vec.magnitude;

                //計算
                //自分の位置予測
                Vector3 nextRobotPosition = PositionofRobot[0] + 2 * this.S * velocityofRobot * Time.deltaTime;
                //Debug.Log("Next Robot Pt :" + nextRobotPosition.ToString());
                //相手の位置予測
                Vector3 nextHumanPosition = this.PositionofHuman[1] + (this.PositionofHuman[1] - this.PositionofHuman[0]) * Time.deltaTime;
                //予測感覚
                Vector3 interval = nextRobotPosition - nextHumanPosition;
                //Debug.Log("Interval:" + interval.magnitude.ToString());
                //評価関数
                float alfa = (this.Ka[0] * interval.magnitude - this.Ka[1] / interval.magnitude);
                //Debug.Log("Alfa:" + alfa.ToString());
                
                //移動
                Vector3 vector = this.PositionofRobot[1] + new Vector3(alfa * this.Vec.x, 0, alfa * this.Vec.z) * Time.deltaTime;
               
                //算出したデータを保存
                this.AddNewDataToArray(vector, this.datalength, ref this.PositionofRobot);

                //モデル操作
                this.model.GetComponent<ModelScript>().move(this.robotBone, vector);   
                                                
            }
            
        }
        catch
        {
            Debug.Log("Error:RobotAction");
        }
        
    }
    
    void ReplaceData(int lengthofArray, ref Vector3[] array)
    {
        //n+1（最後）のデータは計算後に出す
        //n（最後から2つ目）が取得データ
        for (int i = 0; i < lengthofArray - 1; i++)
        {
            array[i] = array[i + 1];
        }
        //array[lengthofArray-1] = newData;
    }

    void AddNewDataToArray(Vector3 newData, int lengthofArray, ref Vector3[] array)
    {
        array[lengthofArray - 1] = newData;
    }

    void OnAppLicatinQuit()
    {
        //this.timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        this.timer.Dispose();
    }
}


