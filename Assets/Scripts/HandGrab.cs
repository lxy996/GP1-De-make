using System.Collections;
using UnityEngine;

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
                    Throw(true);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (!rightBusy)
            {
                if (rightHeld == null) 
                    StartCoroutine(GrabRoutine(false));
                else 
                    Throw(false);
            }
        }
    }

    IEnumerator GrabRoutine(bool isLeft)
    {
        SetBusy(isLeft, true);

        Transform end;
        Transform hold;
        Vector3 originLocal;

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
            Vector3 targetLocal = end.parent.InverseTransformPoint(targetWorld);

            // === 2. Extend ===
            yield return MoveLocal(end, originLocal, targetLocal, extendTime);

            // === 3. Grab the target ===
            if (targetRb != null)
            {
                targetRb.isKinematic = true;
                targetRb.interpolation = RigidbodyInterpolation.None;

                targetRb.transform.SetParent(hold, false);
                targetRb.transform.localPosition = Vector3.zero;
                targetRb.transform.localRotation = Quaternion.identity;

                if (isLeft) leftHeld = targetRb;
                else rightHeld = targetRb;
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

        end.localPosition = originLocal;
        SetBusy(isLeft, false);
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
        
        held.isKinematic = false;
        held.interpolation = RigidbodyInterpolation.Interpolate;

        // Clear the initial velocity
        held.linearVelocity = Vector3.zero;
        held.angularVelocity = Vector3.zero;

        Vector3 dir = cam.transform.forward;
        held.AddForce(dir * throwForce, ForceMode.VelocityChange);

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

}
