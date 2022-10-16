using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOV : MonoBehaviour
{
    // 시야 영역의 반지름
    public float viewRadius;

    //시야 각도
    [Range(0, 360)]
    public float viewAngle;

    // 마스크 2종
    public LayerMask targetMask, obstacleMask;

    // Target mask에 ray hit된 transform을 보관하는 리스트
    public List<Transform> visibleTargets = new List<Transform>();

    //샘플링 점의 비율 조정
    public float meshResolution;

    //폴리곤 매쉬를 구축하기 위한 Mesh, MeshFilter 추가
    Mesh viewMesh;
    public MeshFilter viewMeshFilter;


    public float maskCutawayDst = .1f;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;


        // 0.2초 간격으로 코루틴 호출
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void LateUpdate()
    {
        DrawFieldOfView();
    }

    void FindVisibleTargets() //시야가 가려질 target 찾기
    {
        visibleTargets.Clear();

        // viewRadius를 반지름으로 한 원 영역 내 targetMask 레이어인 콜라이더를 모두 가져옴
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++) //search된 target의 수 만큼 돌리기
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized; //타겟까지의 이동 방향

            // 플레이어와 forward와 target이 이루는 각이 설정한 각도 내라면
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position); //타겟과 거리

                // 타겟으로 가는 레이캐스트에 obstacleMask가 걸리지 않으면 visibleTargets에 Add
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }
    void DrawFieldOfView()
    {
        //RoundToInt = 반올림 
        //샘플링할 점 = 설정 각도 * 샘플링 비율
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        // 샘플링할 점과 샘플링으로 나뉘어지는 각의 크기를 구함
        float stepAngleSize = viewAngle / stepCount;

        //ViewCast로 폴리곤을 구성할 정점을 얻어 viewPoints라는 Vector3 리스트에 정점들을 넣을 수 있도록 함.
        List<Vector3> viewPoints = new List<Vector3>();

        ViewCastInfo prevViewCast = new ViewCastInfo();

        //샘플링 할 점까지 반복
        for (int i = 0; i <= stepCount; i++)
        {

            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle); //폴리곤을 구성할 정점 설정.  


            // i가 0이면 prevViewCast에 아무 값이 없어 정점 보간을 할 수 없으므로 건너뛴다.
            if (i != 0)
            {
                bool edgeDstThresholdExceed = Mathf.Abs(prevViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                
                // 둘 중 한 raycast가 장애물을 만나지 않았거나 두 raycast가 서로 다른 장애물에 hit 된 것이라면(edgeDstThresholdExceed 여부로 계산)
                if (prevViewCast.hit != newViewCast.hit || (prevViewCast.hit && newViewCast.hit && edgeDstThresholdExceed))
                {
                    Edge e = FindEdge(prevViewCast, newViewCast);
                    
                    // zero가 아닌 정점을 추가함
                    if (e.PointA != Vector3.zero)
                    {
                        viewPoints.Add(e.PointA);
                    }

                    if (e.PointB != Vector3.zero)
                    {
                        viewPoints.Add(e.PointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.point); //ray가 도달한 위치를 viewPoints 벡터 리스트에 넣음.
            prevViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.forward * maskCutawayDst;
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    public struct ViewCastInfo //구조체 
    {
        public bool hit; //raycast가 hit 판정인지 
        public Vector3 point; //ray가 마지막으로 도달한 위치 
        public float dst; //ray와 점사이의 길이
        public float angle; //해당 ray가 이루는 각의 크기

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;      //raycast가 hit 판정인지
            point = _point;  //ray가 마지막으로 도달한 위치 
            dst = _dst;      //ray와 점사이의 길이
            angle = _angle;  //해당 ray가 이루는 각의 크기
        }
    }

    //raycast 결과를 ViewCastInfo로 반환하는 메서드 ViewCast 
    //메서드 : 코드들을 묶어놓은 코드 블록
    //viewCast : 폴리곤을 구성할 정점을 구축하는 메서드
    ViewCastInfo ViewCast(float globalAngle)
    {
        //이때 어느 방향으로 raycast를 할지는 오브젝트의 y축 오일러 각(y축 기준으로 각도 변환)을 인자로 받아 
        //DirFromAngle 메서드로 방향벡터를 만든다.
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask)) //ray가 방해물에 걸리면
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle); // hit 판정, hit된 위치, hit사이의 길이, ray
        }

        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    // y축 오일러 각을 3차원 방향 벡터로 변환한다.
    // 원본과 구현이 살짝 다름에 주의. 결과는 같다.
    public Vector3 DirFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }
        //90도에서 angleDegrees를 빼준 이유 : 새로운 ray의 x가 0으로 가기때문에 90-x로 각도로 바뀐다.
        //x = cos , z = sin  
        return new Vector3(Mathf.Cos((-angleDegrees + 90) * Mathf.Deg2Rad), 0, Mathf.Sin((-angleDegrees + 90) * Mathf.Deg2Rad));
    }

    public struct Edge
    {
        public Vector3 PointA, PointB;
        public Edge(Vector3 _PointA, Vector3 _PointB)
        {
            PointA = _PointA;
            PointB = _PointB;
        }
    }

    public int edgeResolveIterations;
    public float edgeDstThreshold;

    Edge FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = minAngle + (maxAngle - minAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            bool edgeDstThresholdExceed = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceed)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new Edge(minPoint, maxPoint);
    }
}

