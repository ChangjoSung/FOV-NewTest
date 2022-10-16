using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarScript : MonoBehaviour
{
    public GameObject m_fogOfWarPlane;
    public Transform m_player;
    public LayerMask m_fogLayer;

    [Range(0, 20)]
    public float m_radius = 5f;//지름

    private float m_radiusSqr { get { return m_radius * m_radius; } }//넓이

    Mesh m_mesh;

    public List<Vector3> vertices = new List<Vector3>();
    Vector3[] m_vertices; //점
    Color[] m_colors;
    void Start()
    {
        /*for (int i = 0; i < vertices.Length; i++)
        {
            vertices.Add(m_fogOfWarPlane.transform.TransformPoint(m_vertices[i]));
        }*/
            
        Initialize();
    }
    void Update()
    {
        Ray r = new Ray(transform.position, m_player.position - transform.position);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 1000, m_fogLayer, QueryTriggerInteraction.Collide)) //카메라 레이어가 안개 레이어를 거치면
        {
            for (int i = 0; i < m_vertices.Length; i++)
            {   
                
                Vector3 v = m_fogOfWarPlane.transform.TransformPoint(m_vertices[i]); 
                float dist = Vector3.SqrMagnitude(v - hit.point);
                if (dist < m_radiusSqr)
                {
                    float alpha = Mathf.Min(m_colors[i].a, dist / m_radiusSqr); //둘 중 최솟값 출력
                    m_colors[i].a = alpha;
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
