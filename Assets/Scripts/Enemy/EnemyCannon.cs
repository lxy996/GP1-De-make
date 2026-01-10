using System.Collections;
using UnityEngine;

public class EnemyCannon : MonoBehaviour
{
    [Header("References")]
    public EnemySensor sensor;              
    public Transform bombPoint;             
    public Rigidbody cannoBombPrefab;
    public Transform yawPivot;   // Horizontal axis
    public Transform pitchPivot; // Pitch axis

    [Header("Shoot")]
    public float shootCooldown = 1.6f;
    public float flightTime = 0.65f;        
    public float aimHeight = 0.6f;     
    public float maxLaunchSpeed = 22f;      // Speed limit
    public float selfCollisionIgnoreTime = 0.25f;   // avoid hitting itself

    [Header("Aim")]
    public float turnSpeed = 8f;
    public float minPitch = -25f;   // Maximum downward and upward angles
    public float maxPitch = 45f;


    private float nextShootTime;
    void Awake()
    {
        if (sensor == null) sensor = GetComponent<EnemySensor>();
        if (yawPivot == null) yawPivot = transform;
    }

    void Update()
    {
        if (!sensor || !sensor.target || !bombPoint || !cannoBombPrefab) 
            return;

        bool canShoot = sensor.InAttackRange && sensor.HasLineOfSight;
        if (!canShoot)
            return;

        // Calculate the initial velocity of the projectile
        Vector3 aimPoint = sensor.target.position + Vector3.up * aimHeight;
        Vector3 v0 = CalculateVelocity(bombPoint.position, aimPoint, flightTime);
        
        // Limit the maximum velocity of the projectile
        float speed = v0.magnitude;
        if (speed > maxLaunchSpeed)
            v0 = v0.normalized * maxLaunchSpeed;

        FaceTarget(sensor.target.position, v0);

        if (Time.time >= nextShootTime)
        {
            nextShootTime = Time.time + shootCooldown;
            ShootBomb(v0);
        }
    }

    // Unlike other enemies, the cannon needs to turn and adjust its angle simultaneously.
    private void FaceTarget(Vector3 targetPos, Vector3 v0)
    {

        Vector3 dir = targetPos - yawPivot.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        yawPivot.rotation = Quaternion.Slerp(yawPivot.rotation, targetRot, Time.deltaTime * turnSpeed);

        if (pitchPivot == null) 
            return;

        // Change the velocity from global to local
        Vector3 localV = yawPivot.InverseTransformDirection(v0);

        // Calculate the angle of y/x (here is y/z as Z is the canno's front axis)
        float pitchDeg = Mathf.Atan2(localV.y, localV.z) * Mathf.Rad2Deg;

        // Control the maximum and minimum angle
        pitchDeg = Mathf.Clamp(pitchDeg, minPitch, maxPitch);

        // Make the canno just change its X axis' angle
        Quaternion pitchRot = Quaternion.Euler(pitchDeg, 0f, 0f);

        // Change the pitch angle
        pitchPivot.localRotation = Quaternion.Slerp(pitchPivot.localRotation, pitchRot, Time.deltaTime * turnSpeed);
    }

    void ShootBomb(Vector3 v0)
    {

        Rigidbody rb = Instantiate(cannoBombPrefab, bombPoint.position, Quaternion.identity);

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

        // Give the bomb initial velocity
        rb.linearVelocity = v0;

        IgnoreSelfCollision(rb, true);
        StartCoroutine(RestoreSelfCollision(rb, selfCollisionIgnoreTime));
    }

    Vector3 CalculateVelocity(Vector3 from, Vector3 to, float t)
    {
        // v = (жд - 0.5 g t^2) / t
        Vector3 delta = to - from;
        Vector3 g = Physics.gravity;

        return (delta - 0.5f * g * t * t) / Mathf.Max(0.001f, t);
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
