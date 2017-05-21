using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTexture2DArray : MonoBehaviour {

    public RT2DARRAY.RenderTexture2DArray RT2DArray;
    public Material LeftQuadMat;
    public Material RightQuadMat;

    public Transform Cube;
    public Transform Capsule;
    public Transform renderCamera;

	// Use this for initialization
	void Start () {
        LeftQuadMat.SetTexture("_MyArr", RT2DArray.renderTexture);
        RightQuadMat.SetTexture("_MyArr", RT2DArray.renderTexture);
        Input.gyro.enabled = true;
    }
	
	// Update is called once per frame
	void Update () {
        Cube.Rotate(Vector3.up, 1, Space.World);
        Capsule.Rotate(Vector3.right, 1, Space.World);
        renderCamera.rotation = Input.gyro.attitude;
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
