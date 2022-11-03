using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    float speed = 8.0f;
    public Vector3 movePoint;

    new Rigidbody rigidbody;

    //camera (z,y,속도)
    Camera viewCamera;
    [Range(2.0f, 20.0f)]
    public float Cam_Z = 10.0f;

    [Range(0.0f, 100.0f)]
    public float Cam_Y = 15.0f;
    Vector3 velocity = Vector3.zero;

    public LayerMask Ignorelayer;

    //이동할 위치값
    Vector3 thisUpdatePoint;

    //업데이트 될 플레이어 각도
    float rotDegree;

    //public List<Transform> visibleTargets;// Target mask에 ray hit된 transform을 보관하는 리스트
    //public LayerMask targetMask, obstacleMask; //타겟 레이어, 장애물 레이어
    
    void Start()
    {
        rigidbody = transform.GetComponent<Rigidbody>();
        viewCamera = Camera.main;
        //FindTarget();
    }

    /*void FindTarget()
    {

        // viewRadius를 반지름으로 한 원 영역 내 targetMask 레이어인 콜라이더를 모두 가져옴
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, 20 , targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++) //search된 target의 수 만큼 돌리기
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized; //타겟까지의 이동 방향

            // 플레이어와 forward와 target이 이루는 각이 설정한 각도 내라면
            if (Vector3.Angle(transform.forward, dirToTarget) < 360 / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position); //타겟과 거리

                // 타겟으로 가는 레이캐스트에 obstacleMask가 걸리지 않으면 visibleTargets에 Add
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }*/
    void Update()
    {
        //오른쪽 버튼 누를 시
        if (Input.GetMouseButtonDown(1))
        {
            // ray로 마우스 위치 world 좌표로 받기.
            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);

            //광선 그려주기
            Debug.DrawRay(ray.origin, ray.direction * Mathf.Infinity, Color.green, 1f);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, ~(Ignorelayer)))
            {
                //레어와 닿은 곳이 이동 포인트
                movePoint = raycastHit.point;

                //각도
                movePoint.y = 0f;
                float dx = movePoint.x - rigidbody.position.x;
                float dz = movePoint.z - rigidbody.position.z;
                rotDegree = -(Mathf.Rad2Deg * Mathf.Atan2(dz, dx) - 90); //tan-1(dz/dx) = 각도

                Debug.Log("movepoint : " + movePoint.ToString());
            }
        }
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(rigidbody.position, movePoint) >= 0.1f)
        {
            // thisUpdatePoint 는 이번 업데이트(프레임) 에서 이동할 포인트를 담는 변수다.
            // 이동할 방향(이동할 곳-현재 위치) 곱하기 속도를 해서 이동할 위치값을 계산한다.
            thisUpdatePoint = (movePoint - rigidbody.position).normalized * speed;

            //플레이어 이동 및 회전
            rigidbody.MovePosition(rigidbody.position + thisUpdatePoint * Time.fixedDeltaTime);
            rigidbody.MoveRotation(Quaternion.Euler(0f, rotDegree, 0f));

            Debug.Log(rotDegree.ToString());
        }
    }

    void LateUpdate() {
        //카메라 이동
        Vector3 pos = new Vector3(rigidbody.position.x ,rigidbody.position.y + Cam_Y, rigidbody.position.z - Cam_Z);
        viewCamera.transform.position =  Vector3.SmoothDamp(viewCamera.transform.position, pos, ref velocity, 0.25f);
    }
}




