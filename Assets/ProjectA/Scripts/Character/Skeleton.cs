using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO.Ports;

public class Skeleton : MonoBehaviour
{
    public MonsterGroup myGroup;
    public int GroupNumber = 0;
    public int Number = 0;
    public GameObject vCentrePrefabs;

    public Transform _player;

    float _chaseDist = 100f;
    float _attackDist = 2f;
    float _hangAboutDist = 10f;

    public float _sightAngle = 30f;
    public float _life = 5f;

    float _speed = 0;
    float _runSpeed  = 2f;
    float _walkSpeed = 1f;
    float _rotationSpeed = 4f;
    
    private Animator _animator;
    private bool _isDamage = false;
    private bool _isDead = false;
    private GameObject _damageEffect = null;
    private Vector3 _deadPos = Vector3.zero;
    
    private float neighbourDistance =6f;

    bool turning = false;

    int areaSize = 100;

    GameObject vCentreObject;

    //flocking
    bool bFlockingMoveOn = false;
    Vector3 vcentre = Vector3.zero;
    Vector3 vavoid = Vector3.zero;

    //A* value
    Vector3[] path;
    int targetIndex;

    float elapsedTime;
    void Start ()
    {
        _animator = GetComponent<Animator>();
        LoadEffect();

        if(vCentrePrefabs !=null)
            vCentreObject = (GameObject)Instantiate(vCentrePrefabs, transform.position, Quaternion.identity);
        
       PathRequestManager.RequestPath(transform.position, _player.position, OnPathFound);

        Number = GameInfomation.MonsterNumbering;
        GameInfomation.MonsterNumbering++;
    }
    
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length <= 0||_isDead)
            yield break;

        Vector3 currentwaypoint = path[0];
        while (true)
        {
            if (transform.position == currentwaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    targetIndex = 0;
                    yield break;
                }
                currentwaypoint = path[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentwaypoint, _speed * Time.deltaTime);
            yield return null;
        }

    }
    void LoadEffect()
    {
        _damageEffect = Resources.Load("Prefabs/Effect/skillAttack") as GameObject;
    }
	void Update ()
    {        
        if (_isDead /*|| myGroup == null*/)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            bFlockingMoveOn = !bFlockingMoveOn;
        }
        
        if (bFlockingMoveOn)
            FlockingMovement();
        
        NormalMovement();

        _player.GetComponent<Player>()._stateText.text = bFlockingMoveOn ? "Flocking On" : "Flocking Off";

        if(vCentreObject != null)
            vCentreObject.SetActive(bFlockingMoveOn);
    }
    void UpdatePath()
    {
        if (_isDead)
            return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime > 1f)
        {
            PathRequestManager.RequestPath(transform.position, _player.position, OnPathFound);
            elapsedTime = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name != "HitPoint")
            return;

        Player player =_player.GetComponent<Player>();

        if (_isDead)
            return;

        if (player.eCurrentState == STATE.ATTACK 
            && !_isDamage && player.isAttackPossible)
        {
            PlayDamageEffect(other.transform.position);
            Debug.Log(other.name);
            StartCoroutine(Damage(1f));
        }
    }
    IEnumerator Damage(float time)
    {
        _isDamage = true;

        _life -= 1;
        if (_life > 0)
        {
            _animator.SetTrigger("DAMAGE");
        }
        else
        {
            _animator.SetTrigger("DEATH");
            _isDead = true;
            _deadPos = transform.position;
            StartCoroutine(DeathDelay(3f));
        }
        yield return new WaitForSeconds(time);

        if(!_isDead)
            _isDamage = false;
    }

    IEnumerator DeathDelay(float time)
    {
        if (_player.GetComponent<Player>().inMonster.Contains(Number))
            _player.GetComponent<Player>().inMonster.Remove(Number);
        yield return new WaitForSeconds(time);
        //StartCoroutine(Death(5f));
        gameObject.SetActive(false);
    }
    IEnumerator Death(float time)
    {
        float elapsedTime = 0;
        float underSpeed  = 0.3f;
        while (elapsedTime<time)
        {
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;

            float yPOs = transform.position.y;
            yPOs -= underSpeed * Time.deltaTime;

            transform.position = new Vector3(_deadPos.x, yPOs, _deadPos.z);

        }
        StopCoroutine("FollowPath");
        gameObject.SetActive(false);
    }
    void PlayDamageEffect(Vector3 pos)
    {
        GameObject effect = Instantiate(_damageEffect);
        effect.transform.position = pos;
        effect.transform.localScale = new Vector3(10, 10, 10);
        StartCoroutine(DestroyEffect(effect,1f));
    }
    IEnumerator DestroyEffect(GameObject effect,float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(effect);
    }
    void Attack()
    {
        _animator.SetBool("IDLE", true);
        _animator.SetBool("WALKING", false);
        _animator.SetBool("RUN", false);
    }

    void NormalMovement()
    {
        Vector3 goalPos = bFlockingMoveOn ? vcentre : _player.position;
        Vector3 dir = goalPos - transform.position;
        float angle = Vector3.Angle(dir, transform.forward);
        float dist = Vector3.Distance(goalPos, transform.position);
        if (dist < _chaseDist && angle < _sightAngle)
        {
            dir.y = 0;
            _animator.SetBool("IDLE", false);

            if(!bFlockingMoveOn)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), _rotationSpeed * Time.deltaTime);
            
            if (dir.magnitude > _attackDist)
            {
                _speed = _walkSpeed;
                _animator.SetBool("RUN", false);
                _animator.SetBool("WALKING", true);
                _animator.SetBool("ATTACKING", false);
                UpdatePath();
                if (_player.GetComponent<Player>().inMonster.Contains(Number))
                {
                    _player.GetComponent<Player>().inMonster.Remove(Number);
                }
            }
            else
            {
                if (PathRequestManager.instance.IsProcessingPath)
                {
                    Debug.Log("StopCoroutine(FollowPath)");
                    StopCoroutine("FollowPath");
                }

                int rand = 1;//(Random.Range(0, 10000) / 5);
                if (rand == 1)
                {
                    _animator.SetBool("RUN", false);
                    _animator.SetBool("WALKING", false);
                    _animator.SetBool("ATTACKING", true);

                    if (!_player.GetComponent<Player>().inMonster.Contains(Number))
                        _player.GetComponent<Player>().inMonster.Add(Number);
                }
                else
                {
                    _animator.SetBool("RUN", false);
                    _animator.SetBool("WALKING", true);
                    _animator.SetBool("ATTACKING", false);
                }
            }
        }
        else
        {
            _animator.SetBool("IDLE", true);
            _animator.SetBool("WALKING", false);
            _animator.SetBool("RUN", false);
            _animator.SetBool("ATTACKING", false);
            if (_player.GetComponent<Player>().inMonster.Contains(Number))
            {
                _player.GetComponent<Player>().inMonster.Remove(Number);
            }
        }
    }

    //Flocking for Movement
    void FlockingMovement()
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
                _rotationSpeed * Time.deltaTime);
            _runSpeed = Random.Range(0.5f, 1f);
        }
        else
        {
            if (Random.Range(0, 5) < 1)
            {
                ApplyRules();
            }
        }
        
        transform.Translate(0, 0, Time.deltaTime * _runSpeed);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

    }

    void ApplyRules()
    {
        GameObject[] gos;
        gos = myGroup.monsterGroup.ToArray();

        float gSpeed = 0.1f;

        vcentre = Vector3.zero;
        vavoid = Vector3.zero;

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

                    Skeleton anotherFlock = go.GetComponent<Skeleton>();
                    gSpeed = gSpeed + anotherFlock._runSpeed;
                }

            }
        }

        if (groupSize > 0)
        {
            //Alignment
            vcentre = vcentre / groupSize + (goalPos - transform.position);

            if(vCentreObject !=null)
                vCentreObject.transform.position = vcentre;
            //_runSpeed = gSpeed / groupSize;

            Vector3 dir = (vcentre + vavoid) - transform.position;
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                     Quaternion.LookRotation(dir),
                                     _rotationSpeed * Time.deltaTime);
            }
        }
    }
    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
