using UnityEngine;
using UnityEngine.SceneManagement;

/* This class is responsible for exiting a level to the menu. Should be located in each level.
*/
public class MenuLoader : MonoBehaviour {

    //Name of the menu scene to load
    private const string MenuName = "StartScreen";

    // Load to the menu here
    public void LoadMenu ()
    {
        SceneManager.LoadScene(MenuName);
	}
	
}
