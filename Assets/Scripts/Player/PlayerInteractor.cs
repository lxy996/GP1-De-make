using UnityEngine;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 3.0f;  // The distance that player can interact
    public LayerMask interactLayer;     // The layer that player can interact
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Reference")]
    public TMP_Text promptText; // The context under the screen

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        CheckInteraction();
    }

    void CheckInteraction()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                // Show the context
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = interactable.GetInteractPrompt();
                }

                
                if (Input.GetKeyDown(interactKey))
                {
                    interactable.OnInteract(gameObject);
                }
                return;
            }
        }

        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}
