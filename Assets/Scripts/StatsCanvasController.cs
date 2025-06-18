using UnityEngine;
using DeckboundDungeon.GamePhase;
using System;

public class StatsCanvasController : MonoBehaviour
{

    [SerializeField] private GameObject statsPanel;
    void OnEnable()
    {
        GameManager.OnPhaseChanged += HandlePhaseChanged;
        // Al iniciarse, sincroniza con la fase actual:
        HandlePhaseChanged(GameManager.CurrentPhase);
    }

    void OnDisable()
    {
        GameManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        Debug.Log("Estoy en la fase: " + phase);
        // Solo lo mostramos en Preparación y Acción:
        bool show = phase == GamePhase.Preparation || phase == GamePhase.Action || phase == GamePhase.Merchant;
        statsPanel.SetActive(show);
    }
}