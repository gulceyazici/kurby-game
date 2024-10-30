using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance; //there is only one instance of scenecontroller shared
                                            //across the program so this makes it to be accessible everywhere
                                            //in the program without creating new instance of scenecontroller

    private void Awake()
    {
        if (instance == null) // no other scenecontroller is created 
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // not destroy this scenecontroller when loading new scene
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void NextLevel()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
