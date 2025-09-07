using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f); // tamaño al pasar el ratón
    private Vector3 originalScale; // guardamos el tamaño original

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("ReinicioDesdeGameOver", 1);
        SceneManager.LoadScene("Game");
    }
}
