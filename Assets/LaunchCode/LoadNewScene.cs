using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNewScene : MonoBehaviour
{
    public void LoadProspector()
    {
        SceneManager.LoadScene("__Prospector");
    }

    public void LoadPyramid()
    {
        SceneManager.LoadScene("_Pyramid");
    }
}
