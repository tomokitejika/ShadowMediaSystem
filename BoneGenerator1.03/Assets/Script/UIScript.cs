using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIScript : MonoBehaviour {

    public List<Camera> List_Camera;
    public GameObject Light;

    RenderTexture texture;

	// Use this for initialization
	void Start () {
        this.List_Camera = new List<Camera>();
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void Changed_LightPositonX(float x)
    {
        Vector3 Pos = new Vector3(x,this.Light.transform.position.y, this.Light.transform.position.z);
        this.Light.transform.position = Pos;
    }
    public void Changed_LightPositonY(float y)
    {
        Vector3 Pos = new Vector3(this.Light.transform.position.x, y, this.Light.transform.position.z);
        this.Light.transform.position = Pos;
    }
    public void Changed_LightPositonZ(float z)
    {
        Vector3 Pos = new Vector3(this.Light.transform.position.x, this.Light.transform.position.y, z);
        this.Light.transform.position = Pos;
    }

}


