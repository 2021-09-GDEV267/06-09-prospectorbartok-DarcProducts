using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public void Load(int indx) => SceneManager.LoadScene(indx);

    public void Load(string name) => SceneManager.LoadScene(name);
}