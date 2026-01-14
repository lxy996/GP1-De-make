using UnityEngine;

public class FinalRoomCheck : MonoBehaviour
{
    [Header("Settings")]
    public BoxCollider roomTrigger;
    public PortalController portalController;
    public GameObject portal;
    public LayerMask enemyLayerMask;
    public float checkInterval = 0.5f;

    private bool requireEnemyPresenceFirst = true; // Make sure that at least one enemy existed here.

    private float nextCheckTime;
    private bool portalOpened;
    private bool hasWitnessedEnemy = false;

    private readonly Collider[] overlapCache = new Collider[32];
    void Awake()
    {
        portal.SetActive (false);
        if (roomTrigger == null) roomTrigger = GetComponent<BoxCollider>();
        if (roomTrigger != null) roomTrigger.isTrigger = true;
    }
    void Update()
    {
        if (portalOpened) 
            return;

        if (Time.time < nextCheckTime) 
            return;
        nextCheckTime = Time.time + checkInterval;

        bool hasEnemy = CheckEnemies();

        if (hasEnemy)
        {
            // There have been enemies here
            hasWitnessedEnemy = true;
        }
        else
        {
            // There have been enemies here and they were all killed
            if (hasWitnessedEnemy || !requireEnemyPresenceFirst)
            {
                OpenPortal();
            }
        }

    }
    void OpenPortal()
    {
        portalOpened = true;
        portal.SetActive(true);
        
        if (portalController != null)
            portalController.Open();
    }
    private bool CheckEnemies()
    {
        if (roomTrigger == null) 
            return false;

        Vector3 worldCenter = roomTrigger.transform.TransformPoint(roomTrigger.center);

        Vector3 lossyScale = roomTrigger.transform.lossyScale;
        Vector3 absScale = new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
        
        Vector3 halfExtents = Vector3.Scale(roomTrigger.size * 0.5f, absScale);

        int count = Physics.OverlapBoxNonAlloc(
            worldCenter,
            halfExtents,
            overlapCache,
            roomTrigger.transform.rotation,
            enemyLayerMask,
            QueryTriggerInteraction.Ignore
        );

        return count > 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (roomTrigger == null) return;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.matrix = Matrix4x4.TRS(
            roomTrigger.transform.TransformPoint(roomTrigger.center),
            roomTrigger.transform.rotation,
            roomTrigger.transform.lossyScale
        );
        Gizmos.DrawCube(Vector3.zero, roomTrigger.size);
    }

}
