using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f); // tamaño al pasar el ratón
    //private Vector3 originalScale; // guardamos el tamaño original
    public GameObject panelInicial;
    public AudioClip gameOverClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int startFromMainMenu = PlayerPrefs.GetInt("StartFromMainMenu", 1); // 1 es el valor por defecto

        if (startFromMainMenu == 0)
        {
            if (panelInicial != null) panelInicial.SetActive(true);
            if(gameObject.name == "CanvasMainMenu") gameObject.SetActive(false);
        }

        if (gameObject.name == "CanvasGameOver" && gameOverClip != null) AudioSettings.Instance.PlayMusic(gameOverClip,true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // INICIAR DESDE MAIN MENU
    public void StartGame()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("StartFromMainMenu", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        PlayerPrefs.DeleteKey("StartFromMainMenu");
        PlayerPrefs.Save();
        Application.Quit();
    }

    // INICIAR DESDE PANEL INICIAL
    public void RestartGame()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("StartFromMainMenu", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
    }
}
