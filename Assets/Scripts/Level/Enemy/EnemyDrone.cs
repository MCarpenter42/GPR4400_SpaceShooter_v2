using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class EnemyDrone : CoreFunc
{
    #region [ GENERAL PARAMETERS ]

    private GameObject player;
    private Transform worldSpace;

    // MODEL PARTS
    [Header("Model Parts")]
    [SerializeField] GameObject body;
    private MeshRenderer meshBody;
    [SerializeField] GameObject eyes;
    private MeshRenderer meshEyes;
    [SerializeField] GameObject weaponLeft;
    [SerializeField] GameObject weaponRight;
    [SerializeField] GameObject sightCone;
    private Light sightConeLight;

    // COLOURS
    [Header("Colours")]
    [SerializeField] Color bodyNormal;
    [SerializeField] Color bodyDamaged;
    [SerializeField] Color eyeIdle;
    [SerializeField] Color eyeAlert;
    [SerializeField] Color eyeAttacking;
    [SerializeField] Color eyeDead;
    private float eyeLightColourShit = 0.20f;

    // MOVEMENT & ROTATION

    [Header("Movement & Rotation")]
    [SerializeField] float maxMoveSpeed = 2.0f;
    [SerializeField] float patrolSpeed = 0.8f;
    private Vector3 targetMovePos = Vector3.zero;
    private bool moveTo = true;
    private float currentSpeed;
    private bool isMoving = false;
    private Vector3 moveDir;

    [SerializeField] float wallBufferDistance;
    private LayerMask wallLayerMask;
    private Collider[] touchedWalls = new Collider[10];
    private GameObject[] closestWalls = new GameObject[3];
    private bool closeToWall = false;

    [SerializeField] GameObject rotationController;
    [SerializeField] GameObject rotationPointer;
    [SerializeField] GameObject lookPointer;
    private float maxLookAngle = 30.0f;
    private Vector3 patrolRotation;
    private Coroutine setRotate;
    private bool inSetRotate = false;

    // ATTACKING

    [Header("Weapons")]
    [SerializeField] GameObject firePointLeft;
    [SerializeField] GameObject firePointRight;
    private Coroutine weaponsMove;
    private bool weaponsOut;
    [SerializeField] GameObject projectilePrefab;

    // HEALTH

    [Header("Health")]
    [SerializeField] int healthMax = 8;
    private int healthCurrent;

    // AUDIO

    [Header("Audio")]
    public AudioSource hum;
    public AudioSource alert;

    #endregion

    #region [ BEHAVIOUR ]

    // BEHAVIOUR

    [Header("Behaviour")]
    [SerializeField] bool patrolByDefault = true;
    [HideInInspector] public List<Vector3> patrolPoints = new List<Vector3>();
    private int patrolDirection = 1;
    private int nextPatrolIndex;
    private Vector3 nextPatrolPoint;
    private Vector3 returnPoint;

    [SerializeField] float sightConeAngle = 70.0f;
    private bool playerSpotted = false;
    [SerializeField] float sightlineBreakTime = 1.0f;
    private Coroutine sightlineBreak;

    private bool isAlert = false;

    [SerializeField] float attackRangeMin = 3.0f;
    [SerializeField] float attackRangeMax = 15.0f;
    [SerializeField] float detectionRange = 30.0f;
    private bool inDetectRange;

    [SerializeField] float fireRate = 1.25f;
    private Coroutine firingCooldown;
    private bool weaponsOnCooldown = false;
    private GameObject[] firePoints = new GameObject[2];
    private int nextFireSide = 0;

    private Coroutine search;
    private bool searchComplete = false;

    private bool isAlive = true;

    #endregion

    #region [ STATE CONTROL ]

    private EnemyAIState currentState;
    [HideInInspector] public bool canChangeState = true;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Awake()
    {
        GetComponents();
    }

    void Start()
    {
        if (patrolPoints.Count > 0)
        {
            transform.position = patrolPoints[0];
            nextPatrolIndex = 1;
            nextPatrolPoint = patrolPoints[1];
            rotationController.transform.LookAt(nextPatrolPoint);
        }
    }
	
    void Update()
    {
        if (isAlive)
        {
            StateBehaviour();
            Movement();
            Rotation();
            HumVolumeScale();
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void GetComponents()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        worldSpace = GameObject.FindGameObjectWithTag("WorldSpace").transform;

        meshBody = body.GetComponent<MeshRenderer>();
        meshBody.material.color = bodyNormal;
        meshEyes = eyes.GetComponent<MeshRenderer>();

        sightConeLight = sightCone.transform.GetChild(0).GetComponent<Light>();
        sightConeLight.range = detectionRange + 5.0f;
        sightConeLight.spotAngle = sightConeAngle;

        if (attackRangeMin < 0.0f)
        {
            attackRangeMin = 0.0f;
        }

        if (attackRangeMax - attackRangeMin <= 0.5f)
        {
            attackRangeMax = attackRangeMin + 0.5f;
        }

        currentSpeed = patrolSpeed;

        wallLayerMask = LayerMask.GetMask("Walls");

        healthCurrent = healthMax;

        firePoints[0] = firePointLeft;
        firePoints[1] = firePointRight;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool CheckState(EnemyAIState newState)
    {
        if (NewStateValid(newState) && canChangeState)
        {
            switch (currentState)
            {
                case EnemyAIState.Idle:
                    switch (newState)
                    {
                        case EnemyAIState.Patrolling:
                            return patrolByDefault;

                        case EnemyAIState.Dead:
                            return true;

                        default:
                            return false;
                    }
                    
                case EnemyAIState.Patrolling:
                    switch (newState)
                    {
                        case EnemyAIState.Idle:
                            return !patrolByDefault;

                        case EnemyAIState.Alerted:
                            return CanSeePlayer();

                        case EnemyAIState.Dead:
                            return true;

                        default:
                            return false;
                    }

                case EnemyAIState.Alerted:
                    switch (newState)
                    {
                        case EnemyAIState.Searching:
                            return !CanSeePlayer();

                        case EnemyAIState.Following:
                            return (CanSeePlayer() && !InRange());
                            
                        case EnemyAIState.Attacking:
                            return (CanSeePlayer() && InRange());

                        case EnemyAIState.Dead:
                            return true;

                        default:
                            return false;
                    }

                case EnemyAIState.Following:
                    switch (newState)
                    {
                        case EnemyAIState.Searching:
                            return !CanSeePlayer();

                        case EnemyAIState.Attacking:
                            return InRange();

                        case EnemyAIState.Returning:
                            return (!CanSeePlayer() && Tether());

                        case EnemyAIState.Dead:
                            return true;

                        default:
                            return false;
                    }

                case EnemyAIState.Attacking:
                    switch (newState)
                    {
                        case EnemyAIState.Following:
                            return !InRange();

                        case EnemyAIState.Searching:
                            return !CanSeePlayer();

                        case EnemyAIState.Returning:
                            return Tether();

                        case EnemyAIState.Dead:
                            return true;

                        default:
                            return false;
                    }

                case EnemyAIState.Searching:
                    switch (newState)
                    {
                        case EnemyAIState.Returning:
                            return !CanSeePlayer();

                        case EnemyAIState.Following:
                            return (CanSeePlayer() && !InRange());
                            
                        case EnemyAIState.Attacking:
                            return (CanSeePlayer() && InRange());

                        case EnemyAIState.Dead:
                            return true;

                        default:
                            return false;
                    }

                case EnemyAIState.Returning:
                    switch (newState)
                    {
                        case EnemyAIState.Idle:
                            return !patrolByDefault;

                        case EnemyAIState.Patrolling:
                            return patrolByDefault;

                        case EnemyAIState.Dead:
                            return true;

                        default:
                            return false;
                    }

                default:
                    return false;
            }
        }
        else
        {
            return false;
        }
    }

    private bool NewStateValid(EnemyAIState newState)
    {
        switch (currentState)
            {
                case EnemyAIState.Idle:
                    switch (newState)
                    {
                        case EnemyAIState.Dead:
                            return !isAlive;

                        case EnemyAIState.Patrolling:
                            if (patrolPoints.Count > 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }

                        case EnemyAIState.Idle:
                        case EnemyAIState.Alerted:
                        case EnemyAIState.Searching:
                        case EnemyAIState.Following:
                        case EnemyAIState.Attacking:
                        case EnemyAIState.Returning:
                        default:
                            return false;
                    }

                case EnemyAIState.Patrolling:
                    switch (newState)
                    {
                        case EnemyAIState.Dead:
                            return !isAlive;

                        case EnemyAIState.Idle:
                        case EnemyAIState.Alerted:
                            return true;

                        case EnemyAIState.Patrolling:
                        case EnemyAIState.Searching:
                        case EnemyAIState.Following:
                        case EnemyAIState.Attacking:
                        case EnemyAIState.Returning:
                        default:
                            return false;
                    }

                case EnemyAIState.Alerted:
                    switch (newState)
                    {
                        case EnemyAIState.Dead:
                            return !isAlive;

                        case EnemyAIState.Searching:
                        case EnemyAIState.Following:
                        case EnemyAIState.Attacking:
                            return true;

                        case EnemyAIState.Idle:
                        case EnemyAIState.Patrolling:
                        case EnemyAIState.Alerted:
                        case EnemyAIState.Returning:
                        default:
                            return false;
                    }

                case EnemyAIState.Following:
                    switch (newState)
                    {
                        case EnemyAIState.Dead:
                            return !isAlive;

                        case EnemyAIState.Searching:
                        case EnemyAIState.Attacking:
                        case EnemyAIState.Returning:
                            return true;

                        case EnemyAIState.Idle:
                        case EnemyAIState.Patrolling:
                        case EnemyAIState.Alerted:
                        case EnemyAIState.Following:
                        default:
                            return false;
                    }

                case EnemyAIState.Attacking:
                    switch (newState)
                    {
                        case EnemyAIState.Dead:
                            return !isAlive;

                        case EnemyAIState.Following:
                        case EnemyAIState.Searching:
                        case EnemyAIState.Returning:
                            return true;

                        case EnemyAIState.Idle:
                        case EnemyAIState.Patrolling:
                        case EnemyAIState.Alerted:
                        case EnemyAIState.Attacking:
                        default:
                            return false;
                    }

                case EnemyAIState.Searching:
                    switch (newState)
                    {
                        case EnemyAIState.Dead:
                            return !isAlive;

                        case EnemyAIState.Following:
                        case EnemyAIState.Attacking:
                        case EnemyAIState.Returning:
                            return true;

                        case EnemyAIState.Idle:
                        case EnemyAIState.Patrolling:
                        case EnemyAIState.Alerted:
                        case EnemyAIState.Searching:
                        default:
                            return false;
                    }

                case EnemyAIState.Returning:
                    switch (newState)
                    {
                        case EnemyAIState.Dead:
                            return !isAlive;

                        case EnemyAIState.Idle:
                        case EnemyAIState.Patrolling:
                            return true;

                        case EnemyAIState.Alerted:
                        case EnemyAIState.Searching:
                        case EnemyAIState.Following:
                        case EnemyAIState.Attacking:
                        case EnemyAIState.Returning:
                        default:
                            return false;
                    }

                default:
                    return false;
            }
    }

    public void ChangeState(EnemyAIState newState)
    {
        InterruptRotate();
        moveTo = true;
        switch (newState)
        {
            case EnemyAIState.Idle:
                SetValuesOnStateChange(true, false, false, 0.0f, eyeIdle, false);
                break;

            case EnemyAIState.Patrolling:
                SetValuesOnStateChange(true, false, true, patrolSpeed, eyeIdle, false);
                break;

            case EnemyAIState.Alerted:
                SetValuesOnStateChange(true, true, false, 0.0f, eyeAlert);
                AlertSound();
                break;

            case EnemyAIState.Following:
                SetValuesOnStateChange(true, true, true, maxMoveSpeed, eyeAttacking, true);
                if ((transform.position - player.transform.position).magnitude < attackRangeMin)
                {
                    moveTo = false;
                }
                break;

            case EnemyAIState.Attacking:
                SetValuesOnStateChange(true, true, false, 0.0f, eyeAttacking, true);
                break;

            case EnemyAIState.Searching:
                SetValuesOnStateChange(false, false, false, 0.0f, eyeAlert);
                searchComplete = false;
                search = StartCoroutine(Search());
                break;

            case EnemyAIState.Returning:
                SetValuesOnStateChange(false, false, false, maxMoveSpeed, eyeAlert, false);
                DoSetRotation(GetFacing(returnPoint));
                break;

            case EnemyAIState.Dead:
                SetValuesOnStateChange(false, false, false, 0.0f, eyeDead);
                DoSetRotation(GetFacing(returnPoint));
                break;

            default:
                break;
        }
        if (currentState == EnemyAIState.Patrolling)
        {
            returnPoint = transform.position;
        }
        if (currentState == EnemyAIState.Searching)
        {
            search = null;
        }
        if (currentState == EnemyAIState.Returning)
        {
            isMoving = false;
            DoSetRotation(patrolRotation);
        }
        currentState = newState;
    }

    private void StateBehaviour()
    {
        switch (currentState)
        {
            case EnemyAIState.Patrolling:
                if ((nextPatrolPoint - transform.position).magnitude < 0.1f)
                {
                    NextPatrolPoint();
                }
                targetMovePos = nextPatrolPoint;
                break;

            case EnemyAIState.Following:
                targetMovePos = player.transform.position;
                break;

            case EnemyAIState.Attacking:
                targetMovePos = player.transform.position;
                if (weaponsOut)
                {
                    Attack();
                }
                break;

            case EnemyAIState.Searching:
                if (search != null && CanSeePlayer())
                {
                    StopCoroutine(search);
                    searchComplete = true;
                    canChangeState = true;
                }
                break;

            case EnemyAIState.Returning:
                targetMovePos = returnPoint;
                float distance = (returnPoint - transform.position).magnitude;
                if (distance < 0.1f)
                {
                    canChangeState = true;
                }
                break;

            case EnemyAIState.Idle:
            case EnemyAIState.Alerted:
            default:
                break;
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void Movement()
    {
        moveDir = Vector3.zero;
        if (isMoving)
        {
            moveDir = GetMoveDir(targetMovePos) * ToInt(moveTo, 1, -1);
        }

        int wallCount = Physics.OverlapSphereNonAlloc(transform.position, wallBufferDistance, touchedWalls, wallLayerMask);
        if (wallCount > 0)
        {
            closeToWall = true;

            if (wallCount > 3)
            {
                WallProximity();
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (touchedWalls[i] != null)
                    {
                        closestWalls[i] = touchedWalls[i].gameObject;
                    }
                }
            }

            Vector3 moveAway = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                GameObject wall = closestWalls[i];
                if (wall != null)
                {
                    Vector3 normal = -wall.transform.right;
                    if (moveDir.magnitude > 0.0f)
                    {
                        if (Vector3.Dot(moveDir, normal) == -1.0f)
                        {
                            Vector3 dirRel = wall.transform.InverseTransformDirection(moveDir);
                            dirRel.x *= -1.0f;
                            moveDir = wall.transform.TransformDirection(dirRel);
                        }
                        if (Vector3.Dot(moveDir, normal) < 0.0f)
                        {
                            Vector3 dirRel = wall.transform.InverseTransformDirection(moveDir);
                            float mgn = dirRel.magnitude;
                            dirRel.x = 0.0f;
                            dirRel = dirRel.normalized;
                            moveDir = wall.transform.TransformDirection(dirRel * mgn);
                        }
                    }
                    else
                    {
                        moveAway -= wall.transform.right;
                    }
                }
            }

            if (moveDir.magnitude == 0.0f)
            {
                moveDir += moveAway.normalized;
            }
        }
        else
        {
            closeToWall = false;
        }

        if (moveDir.magnitude > 0.0f)
        {
            transform.position += moveDir * Time.deltaTime * currentSpeed;
        }
    }

    private void Rotation()
    {
        if (CanSeePlayer())
        {
            LookAtPlayer();

            Vector3 aimDir = GetFacing(targetMovePos);
            Vector3 currentDir = rotationController.transform.eulerAngles;

            if (isMoving || closeToWall)
            {
                aimDir = Vector3.Lerp(currentDir, aimDir, 0.6f);
            }

            rotationController.transform.eulerAngles = aimDir;
        }
    }

    private void LookAtPlayer()
    {
        lookPointer.transform.LookAt(player.transform.position);

        Vector3 lookDir = RestrictVectorBounds(lookPointer.transform.localEulerAngles);

        if (!(Mathf.Abs(lookDir.x) > maxLookAngle || Mathf.Abs(lookDir.y) > maxLookAngle))
        {
            sightCone.transform.localEulerAngles = lookDir;
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void SetValuesOnStateChange(bool canChangeState, bool isAlert, bool isMoving, float currentSpeed, Color eyeColour)
    {
        this.canChangeState = canChangeState;
        this.isAlert = isAlert;
        this.isMoving = isMoving;
        this.currentSpeed = currentSpeed;
        SetEyeColour(eyeColour);
    }
    
    private void SetValuesOnStateChange(bool canChangeState, bool isAlert, bool isMoving, float currentSpeed, Color eyeColour, bool deployWeapons)
    {
        this.canChangeState = canChangeState;
        this.isAlert = isAlert;
        this.isMoving = isMoving;
        this.currentSpeed = currentSpeed;
        SetEyeColour(eyeColour);
        MoveWeapons(deployWeapons);
    }

    private void SetEyeColour(Color eyeColour)
    {
        meshEyes.material.color = eyeColour;
        meshEyes.material.SetColor("_EmissionColor", eyeColour);
        sightConeLight.color = ShiftColour(eyeColour, eyeLightColourShit);
    }

    private void MoveWeapons(bool outPos)
    {
        if (weaponsOut != outPos)
        {
            if (weaponsMove != null)
            {
                StopCoroutine(weaponsMove);
            }
            weaponsMove = StartCoroutine(WeaponsMove(outPos));
        }
    }

    private IEnumerator WeaponsMove(bool outPos)
    {
        float posMulti = 0.19f * (float)ToInt(outPos);

        Vector3 startPosL = weaponLeft.transform.localPosition;
        Vector3 targetPosL = new Vector3(posMulti * 1.0f, 0.0f, 0.0f);
        Vector3 startPosR = weaponRight.transform.localPosition;
        Vector3 targetPosR = new Vector3(posMulti * -1.0f, 0.0f, 0.0f);

        float aDuration = 0.20f;
        int aFrames = 20;
        float aFrameTime = aDuration / (float)aFrames;

        for (int i = 1; i <= aFrames; i++)
        {
            float delta = (float)i / (float)aFrames;
            yield return new WaitForSeconds(aFrameTime);
            weaponLeft.transform.localPosition = Vector3.Lerp(startPosL, targetPosL, delta);
            weaponRight.transform.localPosition = Vector3.Lerp(startPosR, targetPosR, delta);
        }

        weaponsOut = outPos;
    }
    
    private bool CanSeePlayer()
    {
        bool canSee = false;

        Vector3 pos = sightCone.transform.position;
        Vector3 disp = player.transform.position - sightCone.transform.position;
        Vector3 facing = disp.normalized * detectionRange;

        if (Vector3.Angle(sightCone.transform.forward, facing) < sightConeAngle / 2.0f)
        {
            RaycastHit hit;
            if (Physics.Raycast(pos, facing, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Player") && disp.magnitude < detectionRange)
                {
                    canSee = true;
                }
            }
        }

        if (canSee)
        {
            playerSpotted = true;
            if (sightlineBreak != null)
            {
                StopCoroutine(sightlineBreak);
            }
            return true;
        }
        else
        {
            if (playerSpotted)
            {
                if (sightlineBreak == null)
                {
                    sightlineBreak = StartCoroutine(SightlineBreak());
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private IEnumerator SightlineBreak()
    {
        yield return new WaitForSeconds(sightlineBreakTime);
        playerSpotted = false;
        sightlineBreak = null;
    }

    private IEnumerator Search()
    {
        float aDuration = 1.5f;
        float aFrames = 150;
        float aFrameTime = aDuration / (float)aFrames;

        float lookAngle = maxLookAngle * 0.8f;

        // To centre
        Vector3 eyeRotStart = sightCone.transform.localEulerAngles;
        eyeRotStart = RestrictVectorBounds(eyeRotStart);
        Vector3 eyeRotTarget = Vector3.zero;
        for (int i = 1; i <= aFrames / 5; i++)
        {
            float delta = (float)i / (float)(aFrames / 5);
            yield return new WaitForSeconds(aFrameTime);
            sightCone.transform.localEulerAngles = Vector3.Lerp(eyeRotStart, eyeRotTarget, CosInterpDelta(delta));
        }

        // To top left
        eyeRotStart = sightCone.transform.localEulerAngles;
        eyeRotStart = RestrictVectorBounds(eyeRotStart);
        eyeRotTarget = new Vector3(-lookAngle, -lookAngle, 0.0f);
        for (int i = 1; i <= aFrames / 2; i++)
        {
            float delta = (float)i / (float)(aFrames / 2);
            yield return new WaitForSeconds(aFrameTime);
            sightCone.transform.localEulerAngles = Vector3.Lerp(eyeRotStart, eyeRotTarget, CosInterpDelta(delta));
        }
        
        // To top right
        eyeRotStart = sightCone.transform.localEulerAngles;
        eyeRotStart = RestrictVectorBounds(eyeRotStart);
        eyeRotTarget = new Vector3(-lookAngle, lookAngle, 0.0f);
        for (int i = 1; i <= aFrames; i++)
        {
            float delta = (float)i / (float)(aFrames);
            yield return new WaitForSeconds(aFrameTime);
            sightCone.transform.localEulerAngles = Vector3.Lerp(eyeRotStart, eyeRotTarget, CosInterpDelta(delta));
        }
        
        // To bottom left
        eyeRotStart = sightCone.transform.localEulerAngles;
        eyeRotStart = RestrictVectorBounds(eyeRotStart);
        eyeRotTarget = new Vector3(lookAngle, -lookAngle, 0.0f);
        for (int i = 1; i <= aFrames; i++)
        {
            float delta = (float)i / (float)(aFrames);
            yield return new WaitForSeconds(aFrameTime);
            sightCone.transform.localEulerAngles = Vector3.Lerp(eyeRotStart, eyeRotTarget, CosInterpDelta(delta));
        }
        
        // To bottom right
        eyeRotStart = sightCone.transform.localEulerAngles;
        eyeRotStart = RestrictVectorBounds(eyeRotStart);
        eyeRotTarget = new Vector3(lookAngle, lookAngle, 0.0f);
        for (int i = 1; i <= aFrames; i++)
        {
            float delta = (float)i / (float)(aFrames);
            yield return new WaitForSeconds(aFrameTime);
            sightCone.transform.localEulerAngles = Vector3.Lerp(eyeRotStart, eyeRotTarget, CosInterpDelta(delta));
        }

        // To centre
        eyeRotStart = sightCone.transform.localEulerAngles;
        eyeRotStart = RestrictVectorBounds(eyeRotStart);
        eyeRotTarget = Vector3.zero;
        for (int i = 1; i <= aFrames / 2; i++)
        {
            float delta = (float)i / (float)(aFrames / 2);
            yield return new WaitForSeconds(aFrameTime);
            sightCone.transform.localEulerAngles = Vector3.Lerp(eyeRotStart, eyeRotTarget, CosInterpDelta(delta));
        }

        canChangeState = true;
        search = null;
        searchComplete = true;
    }

    private bool InRange()
    {
        float distance = (transform.position - player.transform.position).magnitude;
        if (distance >= attackRangeMin && distance <= attackRangeMax)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool Tether()
    {
        return false;
    }

    private void WallProximity()
    {
        ClearArray(closestWalls);
        int wallsAdded = 0;
        foreach (Collider col in touchedWalls)
        {
            if (col != null)
            {
                GameObject wall = col.gameObject;
                float dist = (wall.transform.position - transform.position).magnitude;
                switch (wallsAdded)
                {
                    case 0:
                        closestWalls[0] = wall;
                        wallsAdded = 1;
                        break;
                    case 1:
                        if (dist < (closestWalls[0].transform.position - transform.position).magnitude || closestWalls[0] == null)
                        {
                            closestWalls[1] = closestWalls[0];
                            closestWalls[0] = wall;
                        }
                        else
                        {
                            closestWalls[2] = wall;
                        }
                        wallsAdded = 2;
                        break;
                    case 2:
                        if (dist < (closestWalls[0].transform.position - transform.position).magnitude || closestWalls[1] == null)
                        {
                            closestWalls[2] = closestWalls[1];
                            closestWalls[1] = closestWalls[0];
                            closestWalls[0] = wall;
                        }
                        else if (dist < (closestWalls[1].transform.position - transform.position).magnitude)
                        {
                            closestWalls[2] = closestWalls[1];
                            closestWalls[1] = wall;
                        }
                        else
                        {
                            closestWalls[2] = wall;
                        }
                        wallsAdded = 3;
                        break;
                    case 3:
                        if (dist < (closestWalls[0].transform.position - transform.position).magnitude || closestWalls[2] == null)
                        {
                            closestWalls[2] = closestWalls[1];
                            closestWalls[1] = closestWalls[0];
                            closestWalls[0] = wall;
                        }
                        else if (dist < (closestWalls[1].transform.position - transform.position).magnitude)
                        {
                            closestWalls[2] = closestWalls[1];
                            closestWalls[1] = wall;
                        }
                        else if (dist < (closestWalls[2].transform.position - transform.position).magnitude)
                        {
                            closestWalls[2] = wall;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private Vector3 GetMoveDir(Vector3 targetPoint)
    {
        Vector3 displacement = targetPoint - transform.position;
        return displacement.normalized;
    }

    private Vector3 GetFacing(Vector3 targetPoint)
    {
        rotationPointer.transform.LookAt(targetPoint);
        return rotationPointer.transform.eulerAngles;
    }

    private void NextPatrolPoint()
    {
        if (nextPatrolIndex == patrolPoints.Count - 1)
        {
            patrolDirection = -1;
        }
        else if (nextPatrolIndex == 0)
        {
            patrolDirection = 1;
        }

        nextPatrolIndex += patrolDirection;
        nextPatrolPoint = patrolPoints[nextPatrolIndex];

        patrolRotation = GetFacing(nextPatrolPoint);
        if (rotationController.transform.eulerAngles != patrolRotation)
        {
            isMoving = false;
            DoSetRotation(patrolRotation);
        }
    }

    private void DoSetRotation(Vector3 targetRot)
    {
        inSetRotate = true;
        setRotate = StartCoroutine(SetRotate(targetRot));
    }

    private IEnumerator SetRotate(Vector3 targetRot)
    {
        Vector3 rotStart = RestrictVectorBounds(rotationController.transform.eulerAngles);
        Vector3 rotEnd = RestrictVectorBounds(targetRot);
        rotEnd.z = rotStart.z;

        if (rotStart != rotEnd)
        {
            float aDuration = 1.5f;
            float aFrames = 150;
            float aFrameTime = aDuration / (float)aFrames;

            for (int i = 1; i <= aFrames; i++)
            {
                float delta = (float)i / (float)aFrames;
                yield return new WaitForSeconds(aFrameTime);
                rotationController.transform.eulerAngles = Vector3.Lerp(rotStart, rotEnd, CosInterpDelta(delta));
            }
        }

        isMoving = true;
        inSetRotate = false;
        setRotate = null;
    }
    
    private IEnumerator HitRotate(Vector3 targetRot)
    {
        Vector3 rotStart = RestrictVectorBounds(rotationController.transform.eulerAngles);
        Vector3 rotEnd = RestrictVectorBounds(targetRot);
        rotEnd.z = rotStart.z;

        if (rotStart != rotEnd)
        {
            float aDuration = 0.3f;
            float aFrames = 30;
            float aFrameTime = aDuration / (float)aFrames;

            for (int i = 1; i <= aFrames; i++)
            {
                float delta = (float)i / (float)aFrames;
                yield return new WaitForSeconds(aFrameTime);
                rotationController.transform.eulerAngles = Vector3.Lerp(rotStart, rotEnd, CosInterpDelta(delta));
            }
        }

        isMoving = true;
        inSetRotate = false;
        setRotate = null;

        canChangeState = true;
    }
    
    private void InterruptRotate()
    {
        if (setRotate != null)
        {
            StopCoroutine(setRotate);
        }
        inSetRotate = false;
    }

    private void AlertSound()
    {
        float volumeMulti = 0.2f;
        float distanceScale = 2.0f * volumeMulti / (player.transform.position - transform.position).magnitude;
        alert.volume = volumeMulti + distanceScale;
        alert.pitch = 1.0f;
        alert.PlayOneShot(alert.clip);
    }

    private void HumVolumeScale()
    {
        float distanceScale = 1.0f / (player.transform.position - transform.position).magnitude;
        distanceScale -= 0.02f;
        distanceScale *= 0.18f;
        if (distanceScale < 0.0f)
        {
            distanceScale = 0.0f;
        }
        hum.volume = distanceScale;
    }

    private IEnumerator StopHum()
    {
        float startPitch = hum.pitch;
        for (int i = 1; i <= 100; i++)
        {
            float delta = (float)i / 100.0f;
            yield return new WaitForSeconds(0.02f);
            hum.pitch = startPitch * (1.5625f - delta ) * 0.64f;
        }
        hum.Stop();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    private void Attack()
    {
        if (!weaponsOnCooldown)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoints[nextFireSide].transform, false);
            projectile.GetComponent<EnemyRocket>().player = player;
            projectile.GetComponent<EnemyRocket>().DisableCollider();
            projectile.transform.SetParent(worldSpace);
            nextFireSide = ToInt(!ToBool(nextFireSide));
            weaponsOnCooldown = true;
            firingCooldown = StartCoroutine(FiringCooldown());
        }
    }

    private IEnumerator FiringCooldown()
    {
        float fireInterval = 1.0f / fireRate;
        yield return new WaitForSeconds(fireInterval);
        weaponsOnCooldown = false;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void HitByRocket(int dmg)
    {
        TakeDamage(dmg);
    }

    public bool TakeDamage(int dmg)
    {
        healthCurrent -= dmg;
        DoColourFlash(meshBody.material, bodyNormal, bodyDamaged, 0.25f, false);
        if (healthCurrent <= 0)
        {
            OnDeath();
            return true;
        }
        else
        {
            if (NewStateValid(EnemyAIState.Alerted))
            {
                if (setRotate != null)
                {
                    StopCoroutine(setRotate);
                }
                ChangeState(EnemyAIState.Following);
                AlertSound();
                canChangeState = false;
                isMoving = false;
                setRotate = StartCoroutine(HitRotate(GetFacing(player.transform.position)));
            }
            return false;
        }
    }

    public void OnDeath()
    {
        isAlive = false;
        sightCone.SetActive(false);
        StartCoroutine(StopHum());
    }
}
