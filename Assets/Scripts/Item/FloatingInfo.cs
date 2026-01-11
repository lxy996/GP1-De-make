using UnityEngine;
using TMPro; 

public class FloatingInfo : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject canvasRoot; 
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI priceText; // Only the store will appear

    private Transform targetCamera;

    void Start()
    {
        targetCamera = Camera.main.transform;
        
        if (canvasRoot != null) canvasRoot.SetActive(true);
    }

    public void Setup(string name, string desc, int price = -1)
    {
        if (nameText) nameText.text = name;
        if (descText) descText.text = desc;

        if (priceText)
        {
            if (price >= 0)
            {
                priceText.gameObject.SetActive(true);
                priceText.text = $"${price}";
            }
            else
            {
                priceText.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        
        if (canvasRoot != null && targetCamera != null)
        {
            // Make the UI face the player
            canvasRoot.transform.rotation = Quaternion.LookRotation(targetCamera.transform.forward);
        }
    }
}
