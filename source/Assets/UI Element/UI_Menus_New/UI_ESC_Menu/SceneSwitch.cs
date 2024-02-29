using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneSwitch : MonoBehaviour{
	  public void GoToScene(string  sceneName){
	       SceneManager.LoadScene(sceneName);
	  }
	       
	  public void QuitApp() {
		  #if UNITY_EDITOR
               UnityEditor.EditorApplication.isPlaying = false;
          #else
				Application.Quit();
		  #endif
		  Debug.Log("Application has Quit");
	  }
}
