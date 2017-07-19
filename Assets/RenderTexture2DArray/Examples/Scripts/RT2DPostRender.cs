using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RT2DPostRender : MonoBehaviour {

    private Material fadeMaterial = null;

    void Awake()
    {
        fadeMaterial = new Material(Shader.Find("Unlit/Color"));
    }

    void OnPostRender()
    {
        fadeMaterial.SetPass(0);
        GL.PushMatrix();
        //GL.LoadOrtho();
        //Use below instead!
        RT2DARRAY.GL.LoadOrtho(fadeMaterial);

        GL.Color(fadeMaterial.color);
        GL.Begin(GL.QUADS);
        GL.Vertex3(0.4f, 0.4f, -3);
        GL.Vertex3(0.4f, 0.6f, -3);
        GL.Vertex3(0.6f, 0.6f, -3);
        GL.Vertex3(0.6f, 0.4f, -3);
        GL.End();
        GL.Flush();
        GL.PopMatrix();
    }

}
