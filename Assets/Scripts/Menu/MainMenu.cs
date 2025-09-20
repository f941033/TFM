using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f); // tama�o al pasar el rat�n
    private Vector3 originalScale; // guardamos el tama�o original
    public GameObject panelInicial, gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        int reinicioDesdeGameOver = PlayerPrefs.GetInt("ReinicioDesdeGameOver", 0);
        if (reinicioDesdeGameOver == 1)
        {            
            if (panelInicial != null) panelInicial.SetActive(true);
            if(gameObject.name != "CanvasGameOver") gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("ReinicioDesdeGameOver", 0);
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
