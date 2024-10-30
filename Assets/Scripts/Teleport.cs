using UnityEngine;

public class Teleport : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if(collision.CompareTag("Kurby"))
        {
            SceneController.instance.NextLevel();

        }
    }
}
