using UnityEngine;
using UnityEngine.UI;
using DeckboundDungeon.Cards;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Principal")]
    [Tooltip("Canvas completo del menú de pausa")]
    public GameObject pauseMenuUI;

    [Tooltip("Botón para reanudar")]
    public Button resumeButton;
    [Tooltip("Botón para salir de la aplicación")]
    public Button quitButton;
    [Tooltip("Botón para mostrar/ocultar el deck")]
    public Button showDeckButton;
    [Tooltip("Botón para volver al menú de pausa")]
    public Button backButton;

    [Header("UI Deck")]
    [Tooltip("Panel que contiene el grid con las cartas actuales")]
    public GameObject deckPanel;
    [Tooltip("Transform padre que tiene el GridLayoutGroup")]
    public Transform deckGridContainer;
    [Tooltip("Prefab de tarjeta (el mismo de selección inicial)")]
    public GameObject cardPrefab;

    private bool isPaused = false;
    private GameManager gameManager;

    void Awake()
    {
        // encontremos el gestor de cartas para leer el deck actual
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
            Debug.LogError("[PauseMenuController] No encontré ningún GameManager en la escena");

        // hooks de botones
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);
        showDeckButton.onClick.AddListener(ToggleDeckPanel);
        backButton.onClick.AddListener(BackMenu);

        // estado inicial
        pauseMenuUI.SetActive(false);
        deckPanel.SetActive(false);
    }

    void Update()
    {
        // tecla Escape para abrir/cerrar pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        isPaused = true;
    }

    public void ResumeGame()
    {
        deckPanel.SetActive(false);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitGame()
    {
        // En el editor no hace nada, pero en build cierra la app
        Application.Quit();
    }

    private void ToggleDeckPanel()
    {
        bool show = !deckPanel.activeSelf;
        deckPanel.SetActive(show);

        if (show)
            PopulateDeckGrid();
    }

    private void BackMenu()
    {
        deckPanel.SetActive(false);
    }
    private void PopulateDeckGrid()
    {
        // limpiamos cualquier hijo previo
        foreach (Transform child in deckGridContainer)
            Destroy(child.gameObject);

        // asumimos que gameManager.startingDeck o bien otro listado es tu mazo actual
        foreach (var card in gameManager.startingDeck.OrderBy(c => c.cardName))
        {
            var cardData = Instantiate(cardPrefab, deckGridContainer);
            CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
            cardUI.setCardUI(card);

            var drag = cardData.GetComponent<CardDragDrop>();
            if (drag != null) Destroy(drag);
            //cd.cardData = card;
            //cd.isSelectable = false;
            // si tu CardDeck en modo selección activa border, lo desactivamos aquí:
            //cd.selectionBorder.SetActive(false);

        }
    }

    public void ShowCardsDeck()
    {
        deckPanel.SetActive(true);
        PopulateDeckGrid();
    }
}
