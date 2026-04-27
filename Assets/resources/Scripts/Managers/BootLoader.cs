using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [Header("Setup")]
    public GameObject appSystemsPrefab; // Drag your _AppSystems prefab here

    private void Start()
    {
        // 1. Spawn the persistent managers if they don't exist yet
        if (GameFlowManager.Instance == null)
        {
            Instantiate(appSystemsPrefab);
        }

        // 2. Load your Main Menu
        SceneManager.LoadSceneAsync("Home");
    }
}