using UnityEngine;

public class QuitButton : MonoBehaviour
{
    // This method is called when the button is clicked or tapped
    public void OnButtonClick()
    {
        // Quit the game
        Application.Quit();

        // Note: Application.Quit() only works in standalone builds, not in the Unity Editor
    }
}
