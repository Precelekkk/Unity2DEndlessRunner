using UnityEngine;
using UnityEngine.UI;

public class AnimationPlayOnClick : MonoBehaviour
{
    public Animation anim; // Reference to the Animation component
    public string animationClipName = "YourAnimationClipName"; // Replace with the actual name of your animation clip

    void Start()
    {
        // Make sure to assign the Animation component to 'anim' in the Unity Editor
        if (anim == null)
        {
            Debug.LogError("Animation component is not assigned.");
        }

        // Attach the button click event to the method PlayAnimation
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayAnimation);
        }
        else
        {
            Debug.LogError("Button component is not found.");
        }
    }

    void PlayAnimation()
    {
        // Check if the animation component and clip are assigned
        if (anim != null && !string.IsNullOrEmpty(animationClipName))
        {
            // Play the specified animation clip
            anim.Play(animationClipName);
        }
        else
        {
            Debug.LogError("Animation component or clip name is not set.");
        }
    }
}