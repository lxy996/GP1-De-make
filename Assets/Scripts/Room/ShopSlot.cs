using UnityEngine;

public enum ShopType { Bomb, Potion, Relic }
public class ShopSlot : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public Transform itemSpawnPoint;
    public int price = 5;
    public ShopType type;

    [Header("Item Prefabs")]
    public GameObject bombPrefab;
    public GameObject potionPrefab;

    [Header("UI")]
    public GameObject floatingUiPrefab;

    private GameObject displayItem;
    private GameObject realItemPrefab;
    private FloatingInfo currentUI;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip buySound;
    public AudioClip noMoneySound;

    private bool isSold = false;

    void Start()
    {

        GenerateShopItem();
    }

    void GenerateShopItem()
    {

        string name = "";
        string desc = "";

        switch (type)
        {
            case ShopType.Bomb:

                realItemPrefab = bombPrefab;
                price = 10;
                name = "Bomb";
                desc = "Deals AoE explosive damage.";
                break;


            case ShopType.Potion:

                realItemPrefab = potionPrefab;
                price = 10;
                name = "Health Potion";
                desc = "Restores Health to maximum.";
                break;


            case ShopType.Relic:

                if (RelicDatabase.Instance != null)
                {
                    realItemPrefab = RelicDatabase.Instance.GetRandomRelic(new System.Random());
                    if (realItemPrefab != null)
                    {
                        RelicItem data = realItemPrefab.GetComponent<RelicItem>();
                        if (data != null)
                        {
                            price = 40;
                            name = data.relicName;
                            desc = data.description;
                        }
                    }
                }
                break;
        }

        if (realItemPrefab != null)
        {
            displayItem = Instantiate(realItemPrefab, itemSpawnPoint.position, Quaternion.identity);

            var rb = displayItem.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            var grab = displayItem.GetComponent<GrabbableItem>();
            if (grab) grab.playerCanPick = false;

            if (floatingUiPrefab != null)
            {

                GameObject uiObj = Instantiate(floatingUiPrefab, displayItem.transform.position + Vector3.up * 0.8f, Quaternion.identity);

                uiObj.transform.SetParent(transform);
                currentUI = uiObj.GetComponent<FloatingInfo>();

                if (currentUI != null)
                {
                    currentUI.Setup(name, desc, price);
                }

            }
        }
    }


    public string GetInteractPrompt()
    {
        if (isSold) 
            return "";
        return $"[E] Buy ({price} G)";
    }

    public void OnInteract(GameObject interactor)
    {
        if (isSold) return;

        if (GameLevelManager.Instance != null)
        {
            if (GameLevelManager.Instance.TrySpendGold(price))
            {
                BuySuccess();
            }
            else
            {
                if (audioSource && noMoneySound) audioSource.PlayOneShot(noMoneySound);

            }
        }
    }

    void BuySuccess()
    {
        isSold = true;

        if (displayItem != null) Destroy(displayItem);
        if (currentUI != null) Destroy(currentUI.gameObject);

        if (realItemPrefab != null)
        {
            GameObject realThing = Instantiate(realItemPrefab, itemSpawnPoint.position, Quaternion.identity);


            var rb = realThing.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;

            var grab = realThing.GetComponent<GrabbableItem>();
            if (grab) grab.playerCanPick = true;


            RelicItem data = realItemPrefab.GetComponent<RelicItem>();
            if (data && RelicDatabase.Instance != null)
            {
                RelicDatabase.Instance.MarkRelicAsObtained(data.relicName);
            }
        }

        if (audioSource && noMoneySound) audioSource.PlayOneShot(buySound);
    }
}
