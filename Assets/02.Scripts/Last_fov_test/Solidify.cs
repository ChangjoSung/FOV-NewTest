using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Solidify : MonoBehaviour
{
    public Shader flatShader;
    Camera cam;
    void OnEnable()
    {
        cam = GetComponent<Camera>();
        
        //flatShader로 바꾸는데 여기서 바꿀 셰이더는 유니티 기본 셰이더의 Color 셰이더이다. 
        //Color 셰이더는 기본 RGB 값이 (1, 1, 1)이라 보이는 모든 오브젝트가 하얀색으로 바뀐다.
        cam.SetReplacementShader(flatShader, ""); 
    }

}