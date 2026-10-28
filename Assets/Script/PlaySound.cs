using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public AudioSource ButtonClick;

    public void PlayOnClick()
    {
        ButtonClick.Play();
    }
}