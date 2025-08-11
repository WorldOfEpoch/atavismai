using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Atavism
{
    [Obsolete]
    public class ChangeCharacterScene : MonoBehaviour
    {

        public void ChangeScene(string sceneName)
        {
            //		Application.LoadLevel(sceneName);
            SceneManager.LoadScene(sceneName);

        }
    }
}