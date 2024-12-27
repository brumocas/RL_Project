using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Method to load a specific scene by name
    public void LoadSceneByName(string sceneName)
    {
        Debug.Log("Loading Scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // Method to load a specific scene by index
    public void LoadSceneByIndex(int sceneIndex)
    {
        Debug.Log("Loading Scene Index: " + sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }
    
    // Example methods for your three scenes
    public void LoadMenu()
    {
        Debug.Log("Loading Menu...");
        SceneManager.LoadScene(0); // Replace with the actual scene index or name
    }

    // Example methods for your three scenes
    public void LoadScene1()
    {
        Debug.Log("Loading Scene 1...");
        SceneManager.LoadScene(1); // Replace with the actual scene index or name
    }

    public void LoadScene2()
    {
        Debug.Log("Loading Scene 2...");
        SceneManager.LoadScene(2); // Replace with the actual scene index or name
    }
    
    
    public void LoadScene3()
    {
        Debug.Log("Loading Scene 3...");
        SceneManager.LoadScene(3); // Replace with the actual scene index or name
    }
    
    // Quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}

