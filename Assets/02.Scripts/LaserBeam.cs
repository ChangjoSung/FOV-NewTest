using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    Transform tr;
    LineRenderer line;
    RaycastHit hit;
    void Start()
    {
        tr = GetComponent<Transform>();
        line = GetComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.enabled = false;
        
        line.startWidth = 0.5f;
        line.endWidth = 0.02f;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(tr.position + (Vector3.up * 0.02f), tr.forward);

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue);

        if(Input.GetMouseButton(0))
        {
            line.SetPosition(0,tr.InverseTransformPoint(ray.origin)); // position(월드기준)을 호출자의 지역좌표계 기준으로 바꿔 반환

            //어떤 물체에 광선이 맞았을 때의 위치가 광선의 끝 점이 되도록 설정
            if(Physics.Raycast(ray,out hit,100.0f))
            {
                line.SetPosition(1,tr.InverseTransformPoint(hit.point)); //시작 점 = 0, 다음 점 = 1
            } 
            else{
                line.SetPosition(1,tr.InverseTransformPoint(ray.GetPoint(100.0f)));
            }

            StartCoroutine(this.showLaserBeam());
        }
    }

    IEnumerator showLaserBeam()
    {
        line.enabled = true;
        yield return new WaitForSeconds(3.0f);
        line.enabled = false;
    }
}
