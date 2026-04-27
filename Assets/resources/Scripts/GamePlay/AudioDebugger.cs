using UnityEngine;

public class AudioDebugger : MonoBehaviour
{
    void Update()
    {
        // Press keys to test sounds
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Testing: Hover Sound");
            if (AudioManager.Instance) AudioManager.Instance.PlayHover();
            else Debug.LogError("NO AUDIO MANAGER FOUND!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Testing: Victory Sound");
            if (AudioManager.Instance) AudioManager.Instance.PlayVictory();
            else Debug.LogError("NO AUDIO MANAGER FOUND!");
        }

        // This line caused the error before because AudioManager didn't have the definition
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Testing: Defeat Sound");
            if (AudioManager.Instance) AudioManager.Instance.PlayDefeat();
            else Debug.LogError("NO AUDIO MANAGER FOUND!");
        }
    }
}