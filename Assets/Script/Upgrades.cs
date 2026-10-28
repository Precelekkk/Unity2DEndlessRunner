using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrades : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreCounter scoreCounter = other.GetComponent<ScoreCounter>();
            if (scoreCounter != null)
            {
            //    scoreCounter.IncreaseScore(20);
            }
            Destroy(gameObject);
        }
    }
}
