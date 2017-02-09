using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using CnControls;


public enum STATE
{
    IDLE,
    COMBAT_MODE,
    ATTACK,
    RUN,
}

public class Player : MonoBehaviour
{
    public float _attackMoveSpeed = 0.5f;
    public float _runSpeed = 0.5f;
    float _speed = 0f;

    public STATE eCurrentState = STATE.IDLE;
    public STATE ePrevState    = STATE.IDLE;

    public Text _stateText = null;
    public Text _TimeText = null;
    public Text _TimeText2 = null;

    public bool isAttackPossible = false;

    bool isCombo = false;
    Animator _playerAnimator;
    AnimatorStateInfo _currentState;
    AnimatorClipInfo[] _currentClip;
    
    static int _maxCombo = 4;

    public int _currentComboIndex = 0;
    string[] _comboParam = new string[_maxCombo];
    string[] _comboClip = new string[_maxCombo];
    float[] _comboAnimationTime = new float[_maxCombo];
    bool[] _comboSuccess = new bool[_maxCombo];
    
    float _attackTime = 0;
    float _elapsedTime = 0;
   
    bool _bPlay = false;
    bool _attackMotion = false;
    bool _startCombo = false;

    //A* value
    Vector3[] path;
    int targetIndex;

    Vector3 movePostion;
    
    float _autoAttackTime = 0.1f;
    float _autoAttackElapsedTime = 0;

    public List<int> inMonster = new List<int>();
    void Start ()
    {
        _comboParam[0] = "WALK_LEFT_SWING";
        _comboParam[1] = "WALK_RIGHT_SWING";
        _comboParam[2] = "WALK_TRUST";
        _comboParam[3] = "5X_COMBO_MOVE";

        _comboClip[0] = "RM_1H_walk_left_swing";
        _comboClip[1] = "RM_1H_walk_right_swing";
        _comboClip[2] = "RM_1H_walk_trust";
        _comboClip[3] = "RM_1H_5X_Combo_move_forward";

        InitCombo();
        
        _playerAnimator = GetComponent<Animator>();
        _playerAnimator.SetBool("COMBAT_MODE",true);
        eCurrentState = STATE.COMBAT_MODE;

        StartCoroutine(StartCount(1.5f));
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
        if (path.Length <= 0)
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
                   
                    _speed = 0;
                    eCurrentState = STATE.COMBAT_MODE;
                    ReturnCombatMode();
                    yield break;
                }
                currentwaypoint = path[targetIndex];
            }

            _speed = _runSpeed;
            _playerAnimator.SetFloat("RUN", _speed);
            _playerAnimator.SetBool("COMBAT_MODE", false);
            if (_speed > 0f)
            {
                //Rotation
                Vector3 dir = new Vector3(currentwaypoint.x, transform.position.y, currentwaypoint.z) - transform.position;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.1f);
                }

                eCurrentState = STATE.RUN;
                InitCombo();
            }

            transform.position = Vector3.MoveTowards(transform.position, currentwaypoint, _speed * Time.deltaTime);
       

            yield return null;
        }
    }

    void Update ()
    {
        if (!_bPlay)
            return;

        _currentState = _playerAnimator.GetCurrentAnimatorStateInfo(0);
        _currentClip = _playerAnimator.GetCurrentAnimatorClipInfo(0);

        

        UpdateAttack();
        UpdateMovement();
        UpdateRotation();

        //UpdateInput();

        ePrevState = eCurrentState;

        _stateText.text = eCurrentState.ToString();
        //_TimeText.text = _currentComboIndex.ToString();
        _TimeText2.text = _attackTime.ToString();
  
    }
    
    IEnumerator StartCount(float time)
    {
        yield return new WaitForSeconds(time);
        _bPlay = true;
    }

    void UpdateMovement()
    {
        //Run
        if (eCurrentState != STATE.ATTACK)
        {
            if (inputVector.x != 0 || inputVector.y != 0)
            {
                _speed = _runSpeed;
                _playerAnimator.SetFloat("RUN", _speed);
                _playerAnimator.SetBool("COMBAT_MODE", false);
            }
            if (_speed > 0f)
            {
                Vector3 dir = new Vector3(inputVector.x, 0f, inputVector.y);
                transform.position  = transform.position + ((dir.normalized * _speed) * Time.deltaTime);

                eCurrentState = STATE.RUN;
                InitCombo();
            }
        }

        if (inputVector.x == 0 && inputVector.y == 0 
            && eCurrentState != STATE.ATTACK)
        {
            _speed = 0;
            eCurrentState = STATE.COMBAT_MODE;

            if (eCurrentState != ePrevState)
            {
                ReturnCombatMode();
            }
        }
    }
    void UpdateRotation()
    {
        //Rotation
        Vector3 dir = new Vector3(inputVector.x, 0f, inputVector.y);
        //if (inputVector.x != 0)
        //{
        //    transform.Rotate(Vector3.up * ((100f * Time.deltaTime) * inputVector.x));
        //}
        //else
        {
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.15f);
            }
        }
    }
    void Attack()
    {
        StopCoroutine("FollowPath");
        eCurrentState = STATE.ATTACK;
        _elapsedTime = 0;
        _playerAnimator.SetTrigger(_comboParam[0]);
        _attackTime = GetAnimationtime(_comboClip[_currentComboIndex]) / _playerAnimator.speed; //_comboAnimationTime[_currentComboIndex];
        _startCombo = true;
        Debug.Log(_currentState.length.ToString());
    }
    void ComboAttack()
    {
        int comboTag = _currentComboIndex + 1;
        string comboAnimationTag = "COMBO" + comboTag.ToString("00");
        if (_currentState.IsTag(comboAnimationTag) && _currentComboIndex < _maxCombo - 1)
        {
            if (_currentComboIndex < _maxCombo - 1)
            {
                isCombo = true;
                _currentComboIndex += 1;
                eCurrentState = STATE.ATTACK;
                _playerAnimator.SetTrigger(_comboParam[_currentComboIndex]);
                _attackTime += GetAnimationtime(_comboClip[_currentComboIndex]) / _playerAnimator.speed; // _comboAnimationTime[_currentComboIndex];
                _speed = 0;
                _playerAnimator.SetFloat("RUN", _speed);
                _playerAnimator.SetBool("COMBAT_MODE", false);
            }
        }
    }
    void ReturnCombatMode()
    {
        Debug.Log("ReturnCombatMode()");
        eCurrentState = STATE.COMBAT_MODE;
        
        isCombo = false;
        _attackMotion = false;
        InitCombo();

        _playerAnimator.SetFloat("RUN", _speed);
        _playerAnimator.SetBool("COMBAT_MODE", true);
    }
    public void StartComboAnimation(int index)
    {
        isCombo = false;
        _attackMotion = true;
        
        Debug.Log("StartAnimation: " + index.ToString() + "(" + _attackTime.ToString() + ")");
    }
    public void DashStart(int index)
    {
        if(index !=3)
            StartCoroutine(Dash(0.25f,0.5f));
        else
            StartCoroutine(Dash(0.1f, 1f));
    }
    

    IEnumerator Dash(float time, float dist)
    {
        float elapsedTime = 0;
        float dashSpeed = dist / time;
        while (elapsedTime < time)
        {
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;

            transform.position += (transform.forward.normalized * dashSpeed) * Time.deltaTime;
        }
    }
    public void AttackPossible()
    {
        isAttackPossible = true;
    }

    public void EndComboAnimation(int index)
    {
        _attackMotion = false;
        isAttackPossible = false;
        if (!isCombo)
        {
            eCurrentState = STATE.COMBAT_MODE;
        }
        Debug.Log("EndAnimation: " + index.ToString());
    }
    private static Vector2 inputVector
    {
        get
        {
            
            //float x = Mathf.Round(Input.GetAxis("Horizontal"));
            //float y = Mathf.Round(Input.GetAxis("Vertical"));
            float x = Mathf.Round(CnInputManager.GetAxis("Horizontal"));
            float y = Mathf.Round(CnInputManager.GetAxis("Vertical"));

            return new Vector2(x, y);
        }
    }
    void AutoAttack()
    {
        if (inMonster.Count > 0)
        {
            _autoAttackElapsedTime += Time.deltaTime;
            if (_autoAttackElapsedTime > _autoAttackTime)
            {
                if (eCurrentState != STATE.ATTACK && !_attackMotion)
                {
                    Debug.Log("Attack");
                    Attack();
                }

                if (!_startCombo)
                    return;

                if (_elapsedTime < _attackTime)
                {
                    ComboAttack();
                }
                _autoAttackElapsedTime = 0;
            }
        }
    }
    void UpdateAttack()
    {
        AutoAttack();
        if (eCurrentState != STATE.ATTACK && !_attackMotion)
        {
            if (Input.GetKeyDown(KeyCode.A) /*|| CrossPlatformInputManager.GetButtonUp("Fire1")*/)
            {
                Debug.Log("Attack");
                Attack();
            }
        }

        if (eCurrentState == STATE.ATTACK)
        {
            ComboChecking();
            if (_elapsedTime < _attackTime)
            {
                _elapsedTime += Time.deltaTime;
            }
        }
    }

    void ComboChecking()
    {
        if (!_startCombo)
            return;

        if (_elapsedTime < _attackTime)
        {
            if (Input.GetKeyDown(KeyCode.A)/* || CrossPlatformInputManager.GetButtonUp("Fire1")*/)
            {
                ComboAttack();
            }
        }
        else
        {
            if(_speed>=_runSpeed)
                eCurrentState = STATE.RUN;
            else
                eCurrentState = STATE.COMBAT_MODE;

            _attackMotion = false;
        }
    }
    void InitCombo()
    {
        _attackTime = 0;
        _currentComboIndex = 0;

        isCombo = false;
        _startCombo = false;
    }

    float GetAnimationtime(string name)
    {
        float time = 0f;
        RuntimeAnimatorController ac = _playerAnimator.runtimeAnimatorController;
        for (int i = 0; i < ac.animationClips.Length; i++)                
        {
            if (ac.animationClips[i].name == name)       
            {
                time = ac.animationClips[i].length;
            }
        }
        return time;
    }

    void UpdateInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray.origin, ray.direction, out hit))
            {
                movePostion = hit.point;
                PathRequestManager.RequestPath(transform.position, movePostion, OnPathFound);
                Debug.Log(hit.transform.name + " hitto hitto");
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
