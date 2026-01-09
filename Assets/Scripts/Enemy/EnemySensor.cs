using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    public Transform target; // Player
    public float detectRange = 12f;
    public float attackRange = 6f;
    public LayerMask obstacleMask; // Obstacle + Door + MovableObstacle

    public bool InDetectRange { get; private set; }
    public bool HasLineOfSight { get; private set; }
    public bool InAttackRange { get; private set; }

    void Update()
    {
        if (target == null)
        {
            InDetectRange = HasLineOfSight = InAttackRange = false;
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position); // caculate the distance from enemy to player
        InDetectRange = dist <= detectRange;
        InAttackRange = dist <= attackRange;

        if (InDetectRange == false)
        {
            HasLineOfSight = false;
            return;
        }

        Vector3 from = transform.position + Vector3.up * 0.8f;
        Vector3 to = target.position + Vector3.up * 0.8f;
        Vector3 direction = to - from;

        bool hitObstacle = Physics.Raycast(
            from,
            direction.normalized,
            direction.magnitude,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        if (hitObstacle == true)
        {
            HasLineOfSight = false;
        }
        else
        {
            HasLineOfSight = true;
        }
    }

}
