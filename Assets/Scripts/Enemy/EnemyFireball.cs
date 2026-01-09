using UnityEngine;

public class EnemyFireball : MonoBehaviour
{
    [Header("References")]
    public EnemySensor sensor;
    public Transform firePoint;
    public Rigidbody fireballPrefab;

    [Header("Shoot")]
    public float shootCooldown = 1.2f;
    public float shootForce = 14f;

    [Header("Aim")]
    public float aimHeight = 0.2f;
    public float aimTurnSpeed = 12f;

    private float nextShootTime = 0f;
    void Awake()
    {
        if (sensor == null)
            sensor = GetComponent<EnemySensor>();
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
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) 
            return;

        Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * aimTurnSpeed);
    }

    void Shoot()
    {
        Vector3 aimPoint = sensor.target.position + Vector3.up * aimHeight;

        Rigidbody rb = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        // Avoid fireballs attacking the enemy itself
        Collider ballCol = rb.GetComponent<Collider>();
        if (ballCol != null)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var c in colliders)
                Physics.IgnoreCollision(ballCol, c, true);
        }

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

        // Clear the velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 direction = (aimPoint - firePoint.position).normalized;
        rb.AddForce(direction * shootForce, ForceMode.VelocityChange);
    }

}
