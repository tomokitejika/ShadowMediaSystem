using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModelScript : MonoBehaviour {

    public List<GameObject> bones;

    //Generator genarator;
    //public Human modeldata { get; set; }

	// Use this for initialization
	void Start () 
    {
        //this.genarator = GameObject.FindGameObjectWithTag("Generator").GetComponent<Generator>();	 
        
	}
	
	// Update is called once per frame
	public void move (Human human) {
        try
        {
            //this.transform.position = this.genarator.robotBone.bones[0].position;
            this.transform.position = human.bones[0].position;

            for(int i= 0; i<this.bones.Count;i++)
            {              
                if(i != 12 && i != 16)
                {
                    //this.bones[i].transform.rotation= this.genarator.robotBone.bones[i].quaternion;
                    this.bones[i].transform.rotation = human.bones[i].quaternion;
                }
            }          
        }
        catch
        {
            Debug.Log("Erorr:ModelScript");
        }
	
	}

    public void move(Human human, Vector3 vec)
    { 
        try
        {
            //this.transform.position = this.genarator.robotBone.bones[0].position;
            this.transform.position = vec;

            for (int i = 0; i < this.bones.Count; i++)
            {
                if (i != 12 && i != 16)
                {
                    //this.bones[i].transform.rotation= this.genarator.robotBone.bones[i].quaternion;
                    this.bones[i].transform.rotation = human.bones[i].quaternion;
                }
            }
        }
        catch
        {
            Debug.Log("Erorr:ModelScript");
        }

    }

}