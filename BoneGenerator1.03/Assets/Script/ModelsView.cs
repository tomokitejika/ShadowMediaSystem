using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModelsView : MonoBehaviour {

    public GameObject model;

    CIPCReceiver cipc;
    List<GameObject> List_models;
    Generator generator;

    

	// Use this for initialization
	void Start () 
    {
        this.cipc = GameObject.FindGameObjectWithTag("CIPC").GetComponent<CIPCReceiver>();

        //this.generator = GameObject.FindGameObjectWithTag("Generator").GetComponent<Generator>();
        Debug.Log("Models View");
        this.List_models = new List<GameObject>();

        
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Debug.Log("ModelView");

        if (this.cipc.List_Humans.Count != 0)
        {
            if (this.List_models.Count == 0 )
            {
                this.List_models.Clear();

                for (int i = 0; i < this.cipc.List_Humans.Count; i++)
                {
                    this.List_models.Add(Instantiate(this.model));
                    this.List_models[i].transform.parent = this.transform;
                }

            }
            for (int i = 0; i < this.cipc.List_Humans.Count; i++)
            {
                this.List_models[i].GetComponent<ModelScript>().move(this.cipc.List_Humans[i]);
            }

        }
        else if (this.cipc.List_Humans.Count == 0)
        {
            for (int i = 0; i < this.List_models.Count; i++)
            {
                Destroy(this.List_models[i]);
            }
            this.List_models.Clear();
        }
	}

    
}
