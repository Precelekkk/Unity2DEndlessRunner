using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeButton : MonoBehaviour
{
    // This method is called when the button is clicked or tapped
    public void OnButtonClick()
    {
        // Load the specified scene
        SceneManager.LoadScene("Score");
    }
}   