using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.Burst.Intrinsics;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyThrowerFSM : MonoBehaviour
{
    public enum State { Idle, SeekItem, GoToItem, ChasePlayer, Aim, Throw, Recover }

    [Header("References")]
    public EnemySensor sensor;
    public NavMeshAgent agent;
    public Transform holdPoint;
    public GameObject idle;
    public GameObject Aim;
    public AudioSource audioSource;
    public AudioClip throwClip;

    [Header("Scan")]
    public LayerMask grabbableMask;
    public float searchRadius = 10f;
    public float pickupDistance = 1.4f;
    public float repathInterval = 0.2f;
    private float nextRepathTime;

    [Header("Scan Interval")]
    public float itemScanInterval = 0.15f;   
    private float nextItemScanTime;

    [Header("Throw")]
    public float throwForce = 16f;
    public float aimHeight = 0.2f;
    public float turnSpeed = 10f;
    public float selfCollisionIgnoreTime = 0.3f;

   [Header("Aim Timing")]
    public float aimDuration = 0.35f;         // Time required for aiming action
    private float aimReadyTime;

    [Header("Recover")]
    public float recoverDuration = 0.2f;      // Achieve the effect of pausing briefly after throwing
    private float recoverUntilTime;

    [Header("Behavior")]
    public bool chasePlayerWhenNoItem = true;

    private State state = State.Idle;

    private Rigidbody held;
    private Rigidbody targetItem;

    void Awake()
    {
        if (sensor == null) sensor = GetComponent<EnemySensor>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        UpdateExpression();
    }

    void Update()
    {
        if (sensor == null) return;
        if (sensor.target == null) return;
        if (agent == null) return;

        switch (state)
        {
            case State.Idle:
                {

                    agent.isStopped = true;
                    
                    agent.ResetPath();

                    if (held != null)
                    {
                        ChangeState(State.Aim);
                        EnterAimState();
                        break;
                    }

                    if (Time.time >= nextItemScanTime && sensor.InDetectRange)
                    {
                        ChangeState(State.SeekItem);
                    }
                    else if (chasePlayerWhenNoItem && sensor.InDetectRange)
                    {
                        ChangeState(State.ChasePlayer);
                    }

                    break;
                }

            case State.SeekItem:
                {

                    if (Time.time < nextItemScanTime)
                    {
                        ChangeState(State.Idle);
                        break;
                    }

                    idle.SetActive(true);
                    Aim.SetActive(false);

                    nextItemScanTime = Time.time + itemScanInterval;
                    // Prioritize finding items that can be grab
                    targetItem = FindBestItem();

                    if (targetItem != null)
                    {
                        ChangeState(State.GoToItem);
                    }
                    else
                    {
                        // Chase players when don't have grabbable
                        if (chasePlayerWhenNoItem && sensor.InDetectRange)
                        {
                            ChangeState(State.ChasePlayer);
                        }
                        else
                        {
                            agent.isStopped = true;
                            agent.ResetPath();
                            ChangeState(State.Idle);
                        }
                    }
                    break;
                }

            case State.GoToItem:
                {
                    if (targetItem == null)
                    {
                        ChangeState(State.SeekItem);
                        break;
                    }

                    idle.SetActive(true);
                    Aim.SetActive(false);

                    // If the item has already been taken by someone else, change target.
                    ProjectileOwner ownerComp = targetItem.GetComponent<ProjectileOwner>();
                    if (ownerComp != null && ownerComp.isHeld)
                    {
                        targetItem = null;
                        ChangeState(State.SeekItem);
                        break;
                    }

                    agent.isStopped = false;

                    if (Time.time >= nextRepathTime)
                    {
                        nextRepathTime = Time.time + repathInterval;
                        agent.SetDestination(targetItem.position);
                    }

                    float distanceToItem = Vector3.Distance(transform.position, targetItem.position);
                    if (distanceToItem <= pickupDistance)
                    {
                        TryPickup(targetItem);
                        targetItem = null;

                        if (held != null)
                        {
                            ChangeState(State.Aim);
                            EnterAimState();
                        }

                        else
                        {
                            ChangeState(State.SeekItem);
                        }
                    }

                    break;
                }

            case State.ChasePlayer:
                {


                    if (held != null)
                    {
                        // If enemy has the ball, just chase the player until it enters attack range and have vision of player.
                        agent.isStopped = false;

                        if (Time.time >= nextRepathTime)
                        {
                            nextRepathTime = Time.time + repathInterval;
                            agent.SetDestination(sensor.target.position);
                        }

                        
                        if (sensor.InAttackRange && sensor.HasLineOfSight)
                        {
                            ChangeState(State.Aim);
                            EnterAimState();
                        }

                        break;
                    }

                    if (sensor.InDetectRange == false)
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                        ChangeState(State.Idle);
                        break;
                    }

                    // Chase Player
                    agent.isStopped = false;
                    if (Time.time >= nextRepathTime)
                    {
                        nextRepathTime = Time.time + repathInterval;
                        agent.SetDestination(sensor.target.position);
                    }

                    // When enemy is chasing player, it will still try to find grabbable. If find, go to pick it
                    if (Time.time >= nextItemScanTime)
                    {
                        nextItemScanTime = Time.time + itemScanInterval;

                        Rigidbody newItem = FindBestItem();
                        if (newItem != null)
                        {
                            targetItem = newItem;
                            ChangeState(State.GoToItem);
                            break;
                        }
                    }

                    // Theoretically, don't have grabbable items in hand, but if have, enter the aim state
                    if (held != null)
                    {
                        ChangeState(State.Aim);
                        EnterAimState();
                    }

                    break;
                }

            case State.Aim:
                {

                    agent.isStopped = true;

                    if (held == null)
                    {
                        ChangeState(State.SeekItem);
                        break;
                    }

                    FaceTarget(sensor.target.position);

                    if (!sensor.InAttackRange || !sensor.HasLineOfSight)
                    {
                        ChangeState(State.ChasePlayer);
                        break;
                    }

                    bool isAimFinished = Time.time >= aimReadyTime;
                    bool canThrow = isAimFinished && sensor.InAttackRange && sensor.HasLineOfSight;

                    if (canThrow)
                    {
                        ChangeState(State.Throw);
                    }


                    break;
                }

            case State.Throw:
                {

                    FaceTarget(sensor.target.position);
                    DoThrow();

                    // Pause after throwing, enemy may be juicier
                    if (recoverDuration > 0f)
                    {
                        recoverUntilTime = Time.time + recoverDuration;
                        ChangeState(State.Recover);
                    }
                    else
                    {

                        ChangeState(State.SeekItem);

                    }

                    break;
                }

            case State.Recover:
                {


                    agent.isStopped = true;

                    if (Time.time >= recoverUntilTime)
                    {
                        ChangeState(State.SeekItem);
                    }

                    break;
                }
        }
    }

    private void EnterAimState()
    {
        //state = State.Aim;
        aimReadyTime = Time.time + aimDuration;
        agent.ResetPath();
    }

    private Rigidbody FindBestItem()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, searchRadius, grabbableMask, QueryTriggerInteraction.Ignore);

        float bestSqr = float.MaxValue;
        Rigidbody bestRb = null;

        for (int i = 0; i < cols.Length; i++)
        {
            Rigidbody rb = cols[i].attachedRigidbody;
            if (rb == null) 
                continue;

            // Enemy can only grab the items with the tag: enemyCanPick
            GrabbableItem item = rb.GetComponent<GrabbableItem>();
            if (item == null) 
                continue;
            if (item.enemyCanPick == false) 
                continue;

            ProjectileOwner ownerComp = rb.GetComponent<ProjectileOwner>();
            if (ownerComp != null && ownerComp.isHeld) 
                continue;

            ProjectileDamage dmg = rb.GetComponent<ProjectileDamage>();
            if (dmg != null && dmg.armed)
                continue;

            // There must be a passable path between the enemy and the grabbable item
            NavMeshPath path = new NavMeshPath();
            bool hasPath = agent.CalculatePath(rb.position, path);
            if (hasPath == false) 
                continue;
            if (path.status != NavMeshPathStatus.PathComplete) // Calculate whether the path is passable or not
                continue;

            // Compare to find the best grabbable item
            float sqr = (rb.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                bestRb = rb;
            }
        }

        return bestRb;
    }

    private void TryPickup(Rigidbody rb)
    {
        if (rb == null) 
            return;
        if (holdPoint == null) 
            return;

        ProjectileOwner ownerComp = rb.GetComponent<ProjectileOwner>();
        if (ownerComp != null)
        {
            if (ownerComp.isHeld) 
                return;
            ownerComp.isHeld = true;
            ownerComp.holder = holdPoint;
            ownerComp.owner = Team.Enemy;
        }

        ProjectileDamage dmg = rb.GetComponent<ProjectileDamage>();
        if (dmg != null) dmg.armed = false;

        rb.linearVelocity = Vector3.zero;         
        rb.angularVelocity = Vector3.zero;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.isKinematic = true;

        rb.transform.SetParent(holdPoint, false);
        rb.transform.localPosition = Vector3.zero;
        rb.transform.localRotation = Quaternion.identity;

        held = rb;

        IgnoreSelfCollision(rb, true);
    }

    private void DoThrow()
    {
        if (held == null) 
            return;
        if (holdPoint == null) 
            return;

        Rigidbody rb = held;
        held = null;

        rb.transform.SetParent(null, true);

        ProjectileOwner ownerComp = rb.GetComponent<ProjectileOwner>();
        if (ownerComp != null)
        {
            ownerComp.owner = Team.Enemy;
            ownerComp.isHeld = false;
            ownerComp.holder = null;
        }

        ProjectileDamage dmg = rb.GetComponent<ProjectileDamage>();
        if (dmg != null) dmg.armed = true;

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.linearVelocity = Vector3.zero;          
        rb.angularVelocity = Vector3.zero;

        // Avoid enemy being hit by the ball from itself
        IgnoreSelfCollision(rb, true);
        StartCoroutine(RestoreSelfCollision(rb, selfCollisionIgnoreTime));

        Vector3 aimPoint = sensor.target.position + Vector3.up * aimHeight;
        Vector3 direction = (aimPoint - holdPoint.position).normalized;

        rb.AddForce(direction * throwForce, ForceMode.VelocityChange);
        audioSource.PlayOneShot(throwClip);

    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) 
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
    }
    
    // Avoid enemy being hit by the ball from itself
    private void IgnoreSelfCollision(Rigidbody rb, bool ignore)
    {
        Collider ballCol = rb.GetComponent<Collider>();
        if (!ballCol) 
            return;

        Collider[] myCols = GetComponentsInChildren<Collider>();
        foreach (var c in myCols)
        {
            if (c && c != ballCol)
            {
                Physics.IgnoreCollision(ballCol, c, ignore);
            }
                
        }
    }

    private IEnumerator RestoreSelfCollision(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null)
            IgnoreSelfCollision(rb, false);
    }

    private void ChangeState(State newState)
    {
        if (state == newState) 
            return; 

        state = newState;

        
        UpdateExpression();
    }
    private void UpdateExpression()
    {
        
        bool isAiming = (state == State.Aim || state == State.Throw || state == State.ChasePlayer || state == State.Recover);


        if (idle != null) idle.SetActive(!isAiming);
        if (Aim != null) Aim.SetActive(isAiming);
    }

}
