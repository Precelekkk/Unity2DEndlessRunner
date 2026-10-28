using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    // This method is called when the button is clicked or tapped
    public void OnButtonClick()
    {
        // Check if the "Play" scene is loaded
        if (SceneManager.GetSceneByName("PlaySec").isLoaded)
        {
            // Unload the "Play" scene
            SceneManager.UnloadSceneAsync("PlaySec");
        }

        // Load the "Play" scene again
        SceneManager.LoadScene("PlaySec");
    }
}