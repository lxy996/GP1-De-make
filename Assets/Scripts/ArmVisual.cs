using UnityEngine;

public class ArmVisual : MonoBehaviour
{
    public Transform origin; // LeftOrigin or RightOrigin
    public Transform end;    // LeftEnd or RightEnd

    [Header("Data")]
    public float originThickness = 0.125f;
    public float minThickness = 0.08f;
    public float originLength = 0.125f;
    public float minLength = 0.05f;

    void LateUpdate()
    {
        if (!origin || !end) 
            return;

        Vector3 a = origin.position;
        Vector3 b = end.position;
        Vector3 d = b - a;

        float len = d.magnitude;
        if (len < minLength) len = minLength;

        float t = Mathf.Clamp01(originLength / Mathf.Max(0.001f, len));  // Calculate the stretching ratio
        float thickness = Mathf.Lerp(minThickness, originThickness, t);  // Calculate the current thickness

        transform.position = a + d * 0.5f;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        transform.localScale = new Vector3(thickness, len, thickness);


    }
}
