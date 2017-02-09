using UnityEngine;
using System.Collections;

public class Flock : MonoBehaviour
{
    public MonsterGroup myGroup;
    public GameObject vCentrePrefabs;

    GameObject vCentreObject;

    public float speed = 0.001f;
    float rotationSpeed = 4.0f;
    Vector3 averageHeading;
    Vector3 averagePostion;
    float neighbourDistance = 6.0f;

    bool turning = false;

    int areaSize = 20;

    void Start ()
    {
        speed = Random.Range(0.5f,1f);
        vCentreObject = (GameObject)Instantiate(vCentrePrefabs, transform.position, Quaternion.identity);
    }
	

	void Update ()
    {
        if (myGroup == null)
            return;



        if (Vector3.Distance(transform.position, Vector3.zero) >= areaSize)
        {
            turning = true;
        }
        else
        {
            turning = false;
        }
        if (turning)
        {
            Vector3 dir = Vector3.zero - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir),
                rotationSpeed * Time.deltaTime);
            speed = Random.Range(0.5f, 1f);
        }
        else
        {
            if (Random.Range(0, 5) < 1)
            {
                ApplyRules();
            }
        }
        transform.Translate(0, 0, Time.deltaTime * speed);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

    }

    void ApplyRules()
    {
        GameObject[] gos;
        gos = myGroup.monsterGroup.ToArray();

        Vector3 vcentre = Vector3.zero;
        Vector3 vavoid = Vector3.zero;

        float gSpeed = 0.1f;

        Vector3 goalPos = myGroup.player.position;

        float dist;
        int groupSize = 0;
        foreach (GameObject go in gos)
        {
            if (go != gameObject)
            {
                dist = Vector3.Distance(go.transform.position, transform.position);
                if (dist <= neighbourDistance)
                {
                    //Cohesion
                    vcentre += go.transform.position;
                    groupSize++;

                    if (dist < 1.0f)
                    {
                        //Separation
                        vavoid += transform.position - go.transform.position;
                    }

                    Flock anotherFlock = go.GetComponent<Flock>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }

            }
        }

        if (groupSize > 0)
        {
            //Alignment
            vcentre = vcentre/groupSize + (goalPos - transform.position);
            vCentreObject.transform.position = vcentre;
            speed = gSpeed / groupSize;

            Vector3 dir = (vcentre + vavoid) - transform.position;
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                     Quaternion.LookRotation(dir),
                                     rotationSpeed * Time.deltaTime);
            }
        }
    }
}
