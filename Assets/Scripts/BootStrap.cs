using UnityEngine;
using SupersonicWisdomSDK;
using UnityEngine.SceneManagement;

public class BootStrap : MonoBehaviour
{
    private void Awake()
    {
        // Subscribe
        SupersonicWisdom.Api.AddOnReadyListener(()=> SceneManager.LoadScene("Game"));
        // Then initialize
        SupersonicWisdom.Api.Initialize();
    }
}
