using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    // public Transform[] points; (배열)
    public List<Transform> points = new List<Transform>(); //List가 쓰기 좋음.

    // 몬스터를 미리 생성해 저장할 리스트 자료형
    public List<GameObject> monsterPool = new List<GameObject>();

    // 오브젝트 풀에 생성될 최대 몬승터
    public int maxMonsters = 10;

    public GameObject monster;

    public float createTime = 3.0f; // 생성 시간
    bool isGameOver;

    public static GameManager instance = null;

    public TMP_Text scoreText;

    int totScore = 0;

    public bool IsGameOver
    {
        get  // 읽기
        {
            return IsGameOver;
        }

        set  // 쓰기
        {
            IsGameOver = value;
            if (isGameOver)
            {
                CancelInvoke("CreateMonster");
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        //새로 생성된 인스턴스는 삭제
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject); //다른씬 넘어가도 삭제하지않고 유지
    }
    void Start()
    {
        CreateMonsterPool();

        //SpawnPointGroup 게임오브젝트의 TransForm 컴포넌트 추출
        Transform spawnPointGroup = GameObject.Find("SpawnPointGroup")?.transform;

        //points = spawnPointGroup?.GetComponentInChildren<Transform>(); (배열기준)

        //SpawnPointGroup 안 하위 객체들의 Transform을 points에 하나씩 넣어준다.
        foreach (Transform point in spawnPointGroup)
        {
            points.Add(point);
        }

        InvokeRepeating("CreateMonster", 2.0f, createTime);

        //SpawnPointGroup 하위에 있는 모든 차일드 게임 오브젝트의 Transform 컴포넌트 추출 (자동으로 리스트 받기)
        //spawnPointGroup?.GetComponentsInChildren<Transform>(points);

        
        totScore = PlayerPrefs.GetInt("TOT_SCORE", 0);
        DisplayScore(0);
    }

    void CreateMonster()
    {
        // 몬스터 특정 위치 산출
        int idx = Random.Range(0, points.Count);
        //Instantiate(monster, points[idx].position, points[idx].rotation);

        GameObject _monster = GetMonsterInPool();
        _monster?.transform.SetPositionAndRotation(points[idx].position,points[idx].rotation);
        _monster?.SetActive(true);

    }

    void CreateMonsterPool() // 리스폰 
    {
        for (int i = 0; i < maxMonsters; i++)
        {
            //몬스터 생성
            var _monster = Instantiate<GameObject>(monster);
            _monster.name = $"Monster_{i:00}";
            _monster.SetActive(false);
            //생성된 몬스터를 오브젝트 풀에 추가
            monsterPool.Add(_monster);
        }
    }

    public GameObject GetMonsterInPool()
    {
        foreach (var _monster in monsterPool)
        {
            //비활성화 여부로 사용 가능한 몬스터를 판단
            if (_monster.activeSelf == false)
            {
                return _monster;
            }
        }
        return null;
    }

    public void DisplayScore(int score)
    {
        totScore += score;
        scoreText.text = $"<color=#00ff00>SCORE: </color> <color=#ff0000>{totScore:#,##0}</color>";

        PlayerPrefs.SetInt("TOT_SCORE",totScore);
    } 
}
