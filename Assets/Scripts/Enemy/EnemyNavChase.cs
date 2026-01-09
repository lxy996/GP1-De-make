using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavChase : MonoBehaviour
{
    [Header("References")]
    public EnemySensor sensor;
    public NavMeshAgent agent;

    [Header("Repath")]
    public float repathInterval = 0.2f;
    private float nextRepathTime;

    [Header("Aggro")]
    public float aggroDuration = 3.0f;
    private float aggroUntilTime;

    public bool stopInAttackRange = true; // For melee enemies, there's no need to stop and attack.

    void Awake()
    {
        if (sensor == null)
            sensor = GetComponent<EnemySensor>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (sensor == null || sensor.target == null || agent == null)
            return;
        if (stopInAttackRange == true && sensor.InAttackRange == true)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        bool isAggroActive = Time.time < aggroUntilTime; // Determine whether to continue the chase

        if (sensor.InDetectRange == true && sensor.HasLineOfSight == true)
        {
            aggroUntilTime = Time.time + aggroDuration;
            isAggroActive = true;
        }

        // Chase conditions:
        // A. Within detection range + visible line of sight -> Chase
        // B. Aggro active + within detection range -> Chase even without visible line of sight (take a detour)

        bool shouldChase =
            (sensor.InDetectRange == true && sensor.HasLineOfSight == true) ||
            (isAggroActive == true && sensor.InDetectRange == true);


        if (shouldChase == false)
        {
        
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        agent.isStopped = false;

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;
            agent.SetDestination(sensor.target.position);
        }
    }
}
