using UnityEngine;
using System.Collections;

public class EnemyFireball : MonoBehaviour
{
    [Header("References")]
    public EnemySensor sensor;
    public Transform firePoint;
    public Rigidbody fireballPrefab;
    public GameObject Idle;
    public GameObject Aim;

    [Header("Shoot")]
    public float shootCooldown = 1.2f;
    public float shootForce = 14f;
    public float selfCollisionIgnoreTime = 0.25f;

    [Header("Aim")]
    public float aimHeight = 2.0f;
    public float aimTurnSpeed = 12f;

    private float nextShootTime = 0f;
    void Awake()
    {
        if (sensor == null)
            sensor = GetComponent<EnemySensor>();
        if(Idle != null && Aim != null)
        {
            Idle.SetActive(true);
            Aim.SetActive(false);
        }
    }

    void Update()
    {
        if (!sensor || !sensor.target || !firePoint || !fireballPrefab) 
            return;

        bool canShoot = sensor.InAttackRange && sensor.HasLineOfSight;
        if (!canShoot) 
            return;

        FaceTarget(sensor.target.position);

        if (Time.time >= nextShootTime)
        {
            nextShootTime = Time.time + shootCooldown;
            Shoot();
        }

    }
    void FaceTarget(Vector3 targetPos)
    {
        
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) 
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * aimTurnSpeed);
    }

    void Shoot()
    {
        if (Idle != null && Aim != null)
        {
            Idle.SetActive(false);
            Aim.SetActive(true);
        }

        Vector3 aimPoint = sensor.target.position + Vector3.up * aimHeight;

        Rigidbody rb = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        ProjectileOwner owner = rb.GetComponent<ProjectileOwner>();
        if (owner != null)
        {
            owner.owner = Team.Enemy;
            owner.isHeld = false;
            owner.holder = null;
        }

        // Turn on damage
        ProjectileDamage dmg = rb.GetComponent<ProjectileDamage>();
        if (dmg != null) dmg.armed = true;

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Clear the velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        IgnoreSelfCollision(rb, true);
        StartCoroutine(RestoreSelfCollision(rb, selfCollisionIgnoreTime));

        Vector3 dir = (aimPoint - firePoint.position).normalized;
        rb.AddForce(dir * shootForce, ForceMode.VelocityChange);

        if (Idle != null && Aim != null)
        {
            Idle.SetActive(true);
            Aim.SetActive(false);
        }
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

}
