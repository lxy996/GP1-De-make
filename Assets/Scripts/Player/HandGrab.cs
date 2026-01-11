using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HandGrab : MonoBehaviour
{
    [Header("Camera")]
    public Camera cam;
    public Transform camRoot;

    [Header("Right Hand")]
    public Transform rightEnd;
    public Transform rightHoldPoint;
    Vector3 rightRestLocal;

    [Header("Left Hand")]
    public Transform leftEnd;
    public Transform leftHoldPoint;
    Vector3 leftRestLocal;

    [Header("Grab Detect")]
    public LayerMask grabbableMask;
    public float grabDistance = 3.0f;        // Maximum grabbing distance
    public float grabRadius = 0.3f;          // The radius of the area to be grabbed and detected.

    [Header("Empty Punch")]
    public Vector3 emptyPunchLocalOffset = new Vector3(0.05f, 0.05f, 0.05f); 
    public float emptyExtendTime = 0.03f;
    public float emptyRetractTime = 0.05f;

    [Header("Time")]
    public float extendTime = 0.04f;
    public float retractTime = 0.06f;

    [Header("Throw")]
    public float throwForce = 16f;

    Rigidbody leftHeld;  
    Rigidbody rightHeld;

    bool leftBusy;
    bool rightBusy;

    void Awake()
    {
        rightRestLocal = rightEnd.localPosition;
        leftRestLocal = leftEnd.localPosition;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!leftBusy)
            {
                if (leftHeld == null) 
                    StartCoroutine(GrabRoutine(true));
                else
                    UseOrThrow(true);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (!rightBusy)
            {
                if (rightHeld == null) 
                    StartCoroutine(GrabRoutine(false));
                else 
                    UseOrThrow(false);
            }
        }
    }

    IEnumerator GrabRoutine(bool isLeft)
    {
        SetBusy(isLeft, true);

        Transform end;
        Transform hold;
        Vector3 originLocal;

        bool grabbed = false;
        ProjectileOwner lockOwner = null;

        if (isLeft)
        {
            end = leftEnd;
            hold = leftHoldPoint;
            originLocal = leftRestLocal;
        }
        else
        {
            end = rightEnd;
            hold = rightHoldPoint;
            originLocal = rightRestLocal;
        }

        

        // === 1. Detect the target to be grabbed ===
        if (FindBestTarget(out Rigidbody targetRb, out Vector3 targetWorld))
        {
            if (!CanPlayerGrab(targetRb))
            {
                end.localPosition = originLocal;
                SetBusy(isLeft, false);
                yield break;
            }

            lockOwner = targetRb.GetComponent<ProjectileOwner>();
            if (lockOwner != null)
            {
                if (lockOwner.isHeld)
                {
                    end.localPosition = originLocal;
                    SetBusy(isLeft, false);
                    yield break;
                }
                lockOwner.isHeld = true;      // Avoid grabbing the same ball with both hands at the same time
                lockOwner.holder = hold;      // Declare the owner of the ball
            }

            Vector3 targetLocal = end.parent.InverseTransformPoint(targetWorld);

            // === 2. Extend ===
            yield return MoveLocal(end, originLocal, targetLocal, extendTime);

            // === 3. Grab the target ===
            if (targetRb != null)
            {
                // Marking ownership
                var ownerComp = targetRb.GetComponent<ProjectileOwner>();
                if (ownerComp != null) ownerComp.owner = Team.Player;
                
                // Turn off damage
                var dmgComp = targetRb.GetComponent<ProjectileDamage>();
                if (dmgComp != null) dmgComp.armed = false;

                // Clear the velocity
                targetRb.linearVelocity = Vector3.zero;
                targetRb.angularVelocity = Vector3.zero;

                targetRb.isKinematic = true;
                // To prevent the ball from floating in the air after being caught
                targetRb.interpolation = RigidbodyInterpolation.None;

                targetRb.transform.SetParent(hold, false);
                targetRb.transform.localPosition = Vector3.zero;
                targetRb.transform.localRotation = Quaternion.identity;

                if (isLeft) leftHeld = targetRb;
                else rightHeld = targetRb;

                grabbed = true;

                HandleSpecialGrabLogic(targetRb);

                TryConvertHeldItem(isLeft);
            }

            // === 4. Retract ===
            yield return MoveLocal(end, targetLocal, originLocal, retractTime);
        }
        else
        {
            // === Empty Punch ===

            Vector3 offset = emptyPunchLocalOffset;
            
            if (isLeft)
            {
                offset.x = Mathf.Abs(offset.x);
            }
            else
            {
                offset.x = -Mathf.Abs(offset.x);
            }

            Vector3 punchLocal = originLocal + offset; // Calculate the target position of the empty punch
            
            yield return MoveLocal(end, originLocal, punchLocal, emptyExtendTime);
            yield return MoveLocal(end, punchLocal, originLocal, emptyRetractTime);
        }

        // Unify unlocking here if the grab fails for any reason
        if (!grabbed && lockOwner != null)
        {
            lockOwner.isHeld = false;
            lockOwner.holder = null;
        }

        end.localPosition = originLocal;
        SetBusy(isLeft, false);
    }

    void UseOrThrow(bool isLeft)
    {
        Rigidbody held;

        if (isLeft)
        {
            held = leftHeld;
        }
        else
        {
            held = rightHeld;
        }

        if (held == null)
            return;

        // Firstly, determine whether it can be used
        IHandUsable usable = held.GetComponent<IHandUsable>();
        if (usable != null)
        {
            bool used = usable.Use(this, isLeft);
            if (used)
            {
                // if used the item, clear the held
                if (isLeft) 
                    leftHeld = null;
                else 
                    rightHeld = null;

                return;
            }
        }

        Throw(isLeft);
    }

    void Throw(bool isLeft)
    {
        Rigidbody held;

        if (isLeft)
        {
            held = leftHeld;
        }
        else
        {
            held = rightHeld;
        }

        if (held == null) 
            return;

        held.transform.SetParent(null, true);
        
        var ownerComp = held.GetComponent<ProjectileOwner>();
        if (ownerComp != null)
        {
            ownerComp.owner = Team.Player;
            ownerComp.isHeld = false;
            ownerComp.holder = null;
        }
        var dmgComp = held.GetComponent<ProjectileDamage>();
        if (dmgComp != null)
        {
            dmgComp.armed = true;
            PlayerRelics relics = GetComponent<PlayerRelics>();
            if (relics != null && relics.attackMultiplier > 1.0f)
            {
                // Increase damage by attackMultiplier
                dmgComp.damage *= relics.attackMultiplier;

            }
        }
            

        held.isKinematic = false;
        held.interpolation = RigidbodyInterpolation.Interpolate;

        // Clear the initial velocity
        held.linearVelocity = Vector3.zero;
        held.angularVelocity = Vector3.zero;

        Vector3 dir = cam.transform.forward;
        held.AddForce(dir * throwForce, ForceMode.VelocityChange);

        // To count the number of throw
        if (GameLevelManager.Instance)
        {
            GameLevelManager.Instance.RegisterThrow();
        }

        if (isLeft) 
            leftHeld = null;
        else 
            rightHeld = null;
    }


    // Logic for detecting the best target


    bool FindBestTarget(out Rigidbody bestRb, out Vector3 bestPoint)
    {
        bestRb = null;
        bestPoint = Vector3.zero;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            grabRadius,
            grabDistance,
            grabbableMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0) 
            return false;

        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.rigidbody == null) 
                continue;

            // Avoid hand-to-hand conflicts or players/enemies stealing the ball from each other's hands.
            ProjectileOwner ownerComp = hit.rigidbody.GetComponent<ProjectileOwner>();
            if (ownerComp != null && ownerComp.isHeld)
                continue;

            // Detect whether there are obstructions in front of the target.
            Vector3 from = cam.transform.position;
            Vector3 to = hit.rigidbody.worldCenterOfMass;
            Vector3 dir = to - from;

            if (Physics.Raycast(from, dir.normalized, out RaycastHit block, dir.magnitude))
            {
                if (block.rigidbody != hit.rigidbody)
                    continue; // There are obstructions
            }

            if (hit.distance < bestDist)
            {
                bestDist = hit.distance;
                bestRb = hit.rigidbody;
                bestPoint = hit.rigidbody.worldCenterOfMass;
            }
        }

        return bestRb != null;
    }

    void SetBusy(bool isLeft, bool v)
    {
        if (isLeft) 
            leftBusy = v;
        else 
            rightBusy = v;
    }

    // Make the movement of arms more juicy
    IEnumerator MoveLocal(Transform t, Vector3 a, Vector3 b, float time)
    {
        float s = 0f;
        while (s < 1f)
        {
            s += Time.deltaTime / Mathf.Max(0.001f, time);
            float k = s * s * (3f - 2f * s);  // SmoothStep
            t.localPosition = Vector3.Lerp(a, b, k);
            yield return null;
        }
        t.localPosition = b;
    }
    bool CanPlayerGrab(Rigidbody rb)
    {
        GrabbableItem grab = rb.GetComponent<GrabbableItem>();
        if (grab == null) 
            return false;

        if (!grab.playerCanPick) 
            return false;

        // Furniture: need the relic that allow player to grab furniture
        if (grab.requirement == GrabbableItem.PickupRequirement.RequiresRelic)
        {
            PlayerRelics relic = GetComponent<PlayerRelics>();
            if (relic == null) 
                return false;

            
            if (!relic.canPickFurniture) 
                return false;
        }

        return true;
    }

    private void TryConvertHeldItem(bool isLeft)
    {
        PlayerRelics relics = GetComponent<PlayerRelics>();
        if (relics == null) 
            return;

        Rigidbody held;

        if (isLeft)
        {
            held = leftHeld;
        }
        else
        {
            held = rightHeld;
        }

        if (held == null) 
            return;

        GrabbableItem item = held.GetComponent<GrabbableItem>();
        if (item == null) 
            return;

        // Only these kinds of items can be converted
        bool canConvert =
            item.type == GrabbableType.Ball ||
            item.type == GrabbableType.Rock ||
            item.type == GrabbableType.Slime ||
            item.type == GrabbableType.Bottle ||
            item.type == GrabbableType.Book;

        if (canConvert == false) 
            return;

        // These kinds of items cannot be converted
        if (item.type == GrabbableType.Furniture) 
            return;
        if (item.type == GrabbableType.Potion) 
            return;
        if (item.type == GrabbableType.Key) 
            return;
        if (item.type == GrabbableType.Relic) 
            return;

        // Try to convert to bomb
        if (TryTriggerRelic(isLeft, held, relics.convertToBombChance, relics.bombPrefab))
            return;
        // Try to convert to potion
        if (TryTriggerRelic(isLeft, held, relics.convertToPotionChance, relics.potionPrefab))
            return;

    }
    private bool TryTriggerRelic(bool isLeft, Rigidbody currentHeld, float chance, Rigidbody targetPrefab)
    {
        if (targetPrefab == null || chance <= 0f) 
            return false;

        float roll = Random.value;
        if (roll <= chance) 
        {
            ReplaceHeldItem(isLeft, currentHeld, targetPrefab);
            return true;
        }
        return false;
    }

    private void ReplaceHeldItem(bool isLeft, Rigidbody oldItem, Rigidbody newPrefab)
    {
        Transform holdPoint;

        if (isLeft)
        {
            holdPoint = leftHoldPoint;
        }
        else
        {
            holdPoint = rightHoldPoint;
        }

        if (oldItem != null) 
            Destroy(oldItem.gameObject);

        // Instantiate a new item in the hand
        Rigidbody newItem = Instantiate(newPrefab);

        newItem.isKinematic = true;
        newItem.interpolation = RigidbodyInterpolation.None;

        newItem.transform.SetParent(holdPoint, false);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;

        ProjectileOwner ownerComp = newItem.GetComponent<ProjectileOwner>();
        if (ownerComp != null)
        {
            ownerComp.owner = Team.Player;
            ownerComp.isHeld = true;
            ownerComp.holder = holdPoint;
        }

        ProjectileDamage dmgComp = newItem.GetComponent<ProjectileDamage>();
        if (dmgComp != null) dmgComp.armed = false;

        if (isLeft) leftHeld = newItem;
        else rightHeld = newItem;
    }

    public bool TrySpawnItemInRightHand(Rigidbody itemPrefab)
    {
        
        if (rightBusy || rightHeld != null)
            return false;

        Rigidbody newItem = Instantiate(itemPrefab);
        newItem.isKinematic = true;
        newItem.interpolation = RigidbodyInterpolation.None;

        newItem.transform.SetParent(rightHoldPoint, false);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;

        ProjectileOwner ownerComp = newItem.GetComponent<ProjectileOwner>();
        if (ownerComp != null)
        {
            ownerComp.owner = Team.Player;
            ownerComp.isHeld = true;
            ownerComp.holder = rightHoldPoint;
        }

        ProjectileDamage dmgComp = newItem.GetComponent<ProjectileDamage>();
        if (dmgComp != null) dmgComp.armed = false;

        rightHeld = newItem;

        return true;
    }

    private void HandleSpecialGrabLogic(Rigidbody rb)
    {
        GrabbableItem item = rb.GetComponent<GrabbableItem>();
        if (item == null) 
            return;

        // If the item is slime
        if (item.type == GrabbableType.Slime) 
        {
            // Turn off the ai of slime
            var navChase = rb.GetComponent<EnemyNavChase>();
            if (navChase != null) navChase.enabled = false;

            var touchDamage = rb.GetComponent<EnemyTouchDamage>();
            if (touchDamage != null) touchDamage.enabled = false;

            var agent = rb.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;

        }
    }

}
