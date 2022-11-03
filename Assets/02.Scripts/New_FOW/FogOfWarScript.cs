using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarScript : MonoBehaviour
{
    #region 변수
        
    [Range(0, 100)]
    public float m_radius;//반지름
    private float m_radiusSqr { get { return m_radius * m_radius; } }//넓이

    [Range(0,360)]
    public int m_angle;    
    #endregion

    public GameObject m_fogOfWarPlane;
    public Transform m_player;
    public LayerMask m_fogLayer; //안개 레이어 

    Mesh m_mesh;
    

    public Vector3[] vertices; //점
    public Vector3[] m_vertices; //점

    public Color[] m_colors;


    void Start()
    {
        
        for (int i = 0; i < vertices.Length; i++)
        {
            m_vertices[i] = m_fogOfWarPlane.transform.TransformPoint(vertices[i]);
        }

        Initialize();

    }
    
    void Update()
    {
        Ray r = new Ray(transform.position, m_player.position - transform.position);
        
        Debug.DrawRay(r.origin, r.direction * Mathf.Infinity, Color.gray, 1f);
        
        RaycastHit hit;
        
        //카메라 레이어가 안개 레이어를 거치면
        if (Physics.Raycast(r, out hit, Mathf.Infinity , m_fogLayer, QueryTriggerInteraction.Collide))//트리거와 충돌할지 안할지 결정 
        {
            for (int i = 0; i < m_vertices.Length; i++)
            {   
                float dist = Vector3.SqrMagnitude(m_vertices[i] - hit.point); //계산된 거리의 제곱값 계산
                
                if (dist < m_radiusSqr)
                {
                    float alpha = Mathf.Min(m_colors[i].a, dist / m_radiusSqr); //둘 중 최솟값 출력
                    m_colors[i].a = alpha;
                }

                else if (dist >= m_radiusSqr)
                {
                    m_colors[i].a = 1;
                }
            }
            UpdateColor();
        }
    }

    void Initialize()
    {
        m_mesh = m_fogOfWarPlane.GetComponent<MeshFilter>().mesh;
        m_vertices = m_mesh.vertices;
        m_colors = new Color[m_vertices.Length];
        for (int i = 0; i < m_colors.Length; i++)
        {
            m_colors[i] = Color.black;
        }
        UpdateColor();
    }

    void UpdateColor()
    {
        m_mesh.colors = m_colors;
    }

}
