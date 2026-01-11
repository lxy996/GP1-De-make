using UnityEngine;

public class PortalController : MonoBehaviour
{
    public GameObject visualEffect;

    private bool isOpen = false;

    void Start()
    {
        
        if (visualEffect) visualEffect.SetActive(false);
        GetComponent<Collider>().enabled = false; 
    }

    public void Open()
    {
        if (isOpen) 
            return;
        isOpen = true;

        if (visualEffect) visualEffect.SetActive(true);
        GetComponent<Collider>().enabled = true; 
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            GameLevelManager.Instance.FinishLevel();

        }
    }
}
