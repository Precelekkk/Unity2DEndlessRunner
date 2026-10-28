using UnityEngine;

public class TextPowerUpPopUp : MonoBehaviour
{
    public GameObject popUpText; // Assign the pop-up text object in the Unity editor
    public GameObject playerModel; // Assign the player model in the Unity editor
    public GameObject powerUpPrefab; // Assign the PowerUp prefab in the Unity editor

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == playerModel && other.gameObject.CompareTag("Player") && gameObject == powerUpPrefab)
        {
            if (popUpText != null)
            {
                popUpText.SetActive(true);
                Invoke("HidePopUpText", 5f);
            }
        }
    }

    private void HidePopUpText()
    {
        if (popUpText != null)
        {
            popUpText.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (popUpText != null)
        {
            popUpText.SetActive(false);
        }
    }
}