using CloverGame.Cube;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    private Cube cube;

    // Use this for initialization
    void Start () {

        /*GameObject cubeGameObject = (GameObject)Resources.Load("Cube");

        GameObject obj = Instantiate(cubeGameObject);
        obj.transform.parent = this.transform;
        obj.transform.localPosition = Vector3.zero;

        cube = (Cube)obj.GetComponent("Cube");
        */
    }
	
	// Update is called once per frame
	void Update () {
         /* cube.gameObject.transform.localPosition = new Vector3( cube.transform.localPosition.x, 
                                                      cube.transform.localPosition.y + 0.01f,
                                                      20);
           */                                           
    }
}
