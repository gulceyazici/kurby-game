using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyController : MonoBehaviour
{
    public static int fliesEaten = 0;
    public int maxFliesPerLevel = 5;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kurby"))
        {
            if (fliesEaten < maxFliesPerLevel) // Only proceed if limit isn't reached
            {
                fliesEaten++;
                Score.score++;                // Update score if applicable
                Destroy(gameObject);          // Destroy fly object
                Debug.Log("Fly eaten. Total flies eaten: " + fliesEaten);

                if (fliesEaten >= maxFliesPerLevel)
                {
                    Debug.Log("Reached max flies eaten for this level.");
                }
            }
            else
            {
                Debug.Log("Fly limit reached; cannot eat more this level.");
            }
        }
    }

    // Optional: Reset fliesEaten for new levels
    public static void ResetFlyCounter()
    {
        fliesEaten = 0;
    }
}
