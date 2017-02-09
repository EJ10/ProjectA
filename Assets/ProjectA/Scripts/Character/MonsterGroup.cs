using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterGroup : MonoBehaviour
{
    public Transform player;
    public int GroupNumber = 0;
    public int numMonster = 0;
    private GameObject monsterPrefab;
    private List<Vector3> monstersPosition = new List<Vector3>();

    [SerializeField]public List<GameObject> monsterGroup = new List<GameObject>();
    void Start ()
    {
        GroupNumber = GameInfomation.MonsterGroupNumbering;
        GameInfomation.MonsterGroupNumbering++;
        LoadMonsterPosition();

        monsterPrefab = Resources.Load("Prefabs/Character/Skeleton") as GameObject;
        //monsterPrefab = Resources.Load("Prefabs/Character/SkeletonForFlock") as GameObject;
        
        numMonster = monstersPosition.Count;
        monsterGroup.Clear();
        for (int i = 0; i < numMonster; i++)
        {
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
            monster.transform.SetParent(transform);
            monster.name = "Skeleton_" + i.ToString();
            monster.transform.position = monstersPosition[i];
            monster.transform.rotation = transform.rotation;//Quaternion.identity;
            monster.GetComponent<Skeleton>()._player = player;
            monster.GetComponent<Skeleton>().myGroup = this;

            monsterGroup.Add(monster);
        }
    }
	
	void Update ()
    {
	
	}

    void LoadMonsterPosition()
    {
        monstersPosition.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform monsterPos =transform.GetChild(i);
            if (monsterPos.name == "MonsterPosition")
                monstersPosition.Add(monsterPos.position);

            Destroy(monsterPos.gameObject);
        }
    }
}
