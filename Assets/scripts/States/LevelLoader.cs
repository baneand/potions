using UnityEngine;
using UnityEngine.SceneManagement;

/* This class loads levels in the game. Usually controlled by button presses on UI elements.
*/
public class LevelLoader : MonoBehaviour {

    //This loads a level based on the name of the actual scene (not a path name)
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
