using UnityEngine;

public class Teleport : MonoBehaviour
{
    private BugSpawner bugSpawner;

    private void Start()
    {
        // Find the BugSpawner in the scene
        bugSpawner = FindObjectOfType<BugSpawner>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kurby"))
        {
            // Check if the max flies per level have been eaten
            if (FlyController.fliesEaten >= bugSpawner.maxObject)
            {
                Debug.Log("Teleporting to next level as all flies are eaten.");
                SceneController.instance.NextLevel();
            }
            else
            {
                Debug.Log("Cannot teleport yet; eat all flies first.");
            }
        }
    }
}
