using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards.Buff;
using TMPro;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public GameObject prefabCard;
    public Transform panelCard;
    public Transform handTransform; //root of the hand position
    public Tilemap zonaValidaTilemap;
    private PlayerController player;
    public GameManager gameManager;

    [Header("Variables de la mano")]
    float fanSpread = 7.5f;
    float cardSpacing = -150f;
    float verticalSpacing = 100f;

    [Header("Variables del mazo")]
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();
    public List<GameObject> cardsInHand = new List<GameObject>();
    //public byte handSize = 4;
    public byte currentHandSize;
    public bool inMulligan = false;
    public bool mulliganUsed = false;
    [SerializeField] private bool mulliganToDiscard = true;
    public int mulliganSelectedCount = 0;
    public int MulliganSelectedCount => mulliganSelectedCount;
    public int MulliganMaxSelectable => drawPile.Count;
    public CardData lastCardUsed = null;
    public bool cardPlayedThisActionPhase { get; private set; }
    public void ResetCardPlayedFlagForAction() => cardPlayedThisActionPhase = false;
    private readonly List<MulliganSelectable> mulliganOrder = new List<MulliganSelectable>();

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        //currentHandSize = handSize;
    }

    public void ClearPanelCard()
    {
        foreach (Transform card in handTransform)
        {
            Destroy(card.gameObject);
        }
        cardsInHand.Clear();
    }

    void Update()
    {

    }

    public void DrawCard()
    {
        if (drawPile.Count == 0 && discardPile.Count > 0)
        {
            // Mezclar descartes y pasarlos al drawPile
            drawPile.AddRange(discardPile);
            Shuffle(drawPile);
            discardPile.Clear();
            gameManager.DeactivateDiscardPileImage();
            FindFirstObjectByType<GameManager>().UpdateTextNumberOfCardsDiscard();
        }
        else if (drawPile.Count == 0 && discardPile.Count == 0)
        {
            gameManager.ShowMessage("Expecting another card? Foolish…", 3);
            return;
        }

        var cardToDraw = drawPile[0];
        drawPile.RemoveAt(0);
        gameManager.textNumberOfCardsDeck.text = drawPile.Count.ToString();

        var cardData = Instantiate(prefabCard, handTransform.position, Quaternion.identity, handTransform);
        CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
        cardUI.SetCardUI(cardToDraw);

        var drag = cardData.GetComponent<CardDragDrop>();
        drag.dropTilemap = zonaValidaTilemap;
        drag.cardData = cardToDraw;
        drag.player = player;
        drag.Deck = this;
        
        if (cardToDraw.cardType == CardType.Hability || cardToDraw.cardType == CardType.Buff)
        {

            HabilityCardHandler hability = cardData.GetComponentInChildren<HabilityCardHandler>();
            hability.Initialize(cardToDraw, player);
        }

        cardsInHand.Add(cardData);
        UpdateHandVisuals();
    }

    public void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 75f, 0f);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));
            float normalizedPosition = (2f * i / (cardCount - 1) - 1f) / 2f; // Normalize card position between -1,1
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);

            //set card position
            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }

        // CRÍTICO: Después de recolocar, actualizar las posiciones base
        StartCoroutine(UpdateCardsBasePositionsAfterLayout());
    }

    // NUEVO: Actualizar posiciones base después de recolocación
    private IEnumerator UpdateCardsBasePositionsAfterLayout()
    {
        // Esperar 2 frames para que el layout se aplique completamente
        yield return null;
        yield return null;

        // Actualizar todas las posiciones base de las cartas en mano
        foreach (GameObject cardObj in cardsInHand)
        {
            if (cardObj != null)
            {
                CardHoverInHand hoverComponent = cardObj.GetComponent<CardHoverInHand>();
                if (hoverComponent != null)
                {
                    hoverComponent.ForceUpdateBasePosition();
                }
            }
        }
    }

    public void DrawFullHand()
    {
        while (cardsInHand.Count < currentHandSize && (drawPile.Count > 0 || discardPile.Count > 0))
        {
            DrawCard();
        }
    }

    public void CardPlayed(GameObject card, CardData cardData)
    {
        if (cardData.cardType != CardType.Hability && cardData.cardType != CardType.Buff)
        {
            cardsInHand.Remove(card);
            discardPile.Add(cardData);
            Destroy(card);
            UpdateHandVisuals(); // Esto ya incluye la actualización de posiciones base
            if (cardsInHand.Count == 0)
                gameManager.NextPreparationHand();
        }
        if (GameManager.CurrentPhase == DeckboundDungeon.GamePhase.GamePhase.Action)
        {
            cardPlayedThisActionPhase = true;
            FindFirstObjectByType<GameManager>()?.OnCardPlayedInAction();
        }
    }

    public void Shuffle<T>(List<T> list)
    {
        //Algoritmo de Fisher-Yates para que el shuffle sea siempre aleatorio
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void DrawFromDiscard(byte count)
    {
        while (count > 0 && discardPile.Count > 0)
        {
            CardData cardData = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            var cardObj = Instantiate(prefabCard, handTransform.position, Quaternion.identity, handTransform);
            var cardUI = cardObj.GetComponentInChildren<CardUI>();
            cardUI.SetCardUI(cardData);

            if (cardData.cardType == CardType.Trap || cardData.cardType == CardType.DeckEffect || cardData.cardType == CardType.Summon)
            {
                var drag = cardObj.GetComponent<CardDragDrop>();
                drag.dropTilemap = zonaValidaTilemap;
                drag.cardData = cardData;
                drag.player = player;
                drag.Deck = this;
            }
            else if (cardData.cardType== CardType.Buff || cardData.cardType == CardType.Hability) 
            {
                var drag = cardObj.GetComponent<CardDragDrop>();
                drag.dropTilemap = zonaValidaTilemap;
                drag.cardData = cardData;
                drag.player = player;
                drag.Deck = this;
                var hability = cardObj.GetComponentInChildren<HabilityCardHandler>();
                hability.Initialize(cardData, player);
            }

            cardsInHand.Add(cardObj);
            UpdateHandVisuals(); // Esto ya incluye la actualización de posiciones base
            count--;
        }
    }

    public void DrawLastCardUsed()
    {
        Debug.Log("Entro en el metodo de ultima carta usada");
        if (lastCardUsed != null)
        {
            Debug.Log("Entro en el if de ultima carta usada");
            CardData cardData = lastCardUsed;

            var cardObj = Instantiate(prefabCard, handTransform.position, Quaternion.identity, handTransform);
            var cardUI = cardObj.GetComponentInChildren<CardUI>();
            cardUI.SetCardUI(cardData);

            Debug.Log("la última carta cogida es: " + cardData.cardName);
            if (cardData.cardType == CardType.Trap || cardData.cardType == CardType.DeckEffect || cardData.cardType == CardType.Summon)
            {
                var drag = cardObj.GetComponent<CardDragDrop>();
                drag.dropTilemap = zonaValidaTilemap;
                drag.cardData = cardData;
                drag.player = player;
                drag.Deck = this;
            }
            else if (cardData.cardType == CardType.Buff || cardData.cardType == CardType.Hability)
            {
                var drag = cardObj.GetComponent<CardDragDrop>();
                drag.dropTilemap = zonaValidaTilemap;
                drag.cardData = cardData;
                drag.player = player;
                drag.Deck = this;
                var hability = cardObj.GetComponentInChildren<HabilityCardHandler>();
                hability.Initialize(cardData, player);
            }

            cardsInHand.Add(cardObj);
            UpdateHandVisuals(); // Esto ya incluye la actualización de posiciones base
        }
    }

    public void DiscardHand()
    {
        // suponemos que cardsInHand es la lista privada de GameObject de cartas
        foreach (var cardGO in cardsInHand)
        {
            // sacamos el CardData de cada carta
            var ui = cardGO.GetComponentInChildren<CardUI>();
            if (ui != null && ui.data != null)
                discardPile.Add(ui.data);

            Destroy(cardGO);
            UpdateHandVisuals();
        }
        cardsInHand.Clear();

        // avisa al GameManager para actualizar UI de pila de descarte, si quieres
        FindFirstObjectByType<GameManager>().UpdateTextNumberOfCardsDiscard();

        if (discardPile.Count > 0)
            gameManager.ActivateDiscardPileImage();
    }

    public void ResetMulliganForActionPhase()
    {
        inMulligan = false;
        mulliganUsed = false;
        mulliganSelectedCount = 0;
    }

    public void BeginMulligan()
    {
        if (mulliganUsed) { gameManager.ShowMessage("Your mulligan is spent… there is no turning back.", 3); return; }
        if (GameManager.CurrentPhase != DeckboundDungeon.GamePhase.GamePhase.Action) return;
        if (drawPile.Count == 0) { gameManager.ShowMessage("The deck is empty… and so is your hope.", 3); return; }

        foreach (var go in cardsInHand)
        {
            // 1) Bloquea interacción normal
            var drag = go.GetComponent<CardDragDrop>();
            if (drag) drag.enabled = false;

            var hab = go.GetComponentInChildren<HabilityCardHandler>(true);
            if (hab) hab.enabled = false;

            // 2) Activa/Inicializa el selectable del prefab (NO se crean GOs nuevos)
            var ui = go.GetComponentInChildren<CardUI>(true);
            var sel = go.GetComponent<MulliganSelectable>();
            if (!sel) sel = go.AddComponent<MulliganSelectable>(); // por si algún prefab aún no lo tiene

            sel.enabled = true;
            sel.Init(this, ui);
            sel.SetSelected(false, applyVisual: true); // apaga el halo por si acaso
        }

        mulliganOrder.Clear();
        inMulligan = true;
        mulliganSelectedCount = 0;

        Debug.Log("[Mulligan] Comienza selección. drawPile=" + drawPile.Count);
        gameManager.ShowMessage(
            "Sacrifice a card from your hand and replace it with another from the deck. \nIf you lack the courage, crawl away with Cancel.",
            6
        );
    }

    public void ConfirmMulligan()
    {
        if (!inMulligan) return;

        // 1) Recoger seleccionadas
        var toReplace = new List<GameObject>();
        foreach (var go in cardsInHand)
        {
            var sel = go.GetComponent<MulliganSelectable>();
            if (sel != null && sel.Selected) toReplace.Add(go);
        }

        int n = toReplace.Count;
        if (n == 0)
        {
            CancelMulligan();
            return;
        }

        if (n > drawPile.Count)
        {
            gameManager.ShowMessage("Looking for a mulligan? Too bad… there aren’t enough cards left for your misery.", 4);
            return;
        }

        // 2) Mover seleccionadas a descartes o al fondo del mazo y destruir sus GOs
        foreach (var go in toReplace)
        {
            cardsInHand.Remove(go);

            var ui = go.GetComponentInChildren<CardUI>(true);
            var data = ui ? ui.data : null;

            if (data != null)
            {
                if (mulliganToDiscard) discardPile.Add(data);
                else                   drawPile.Add(data);
            }

            Destroy(go);
        }

        // 3) UI de descarte
        gameManager.UpdateTextNumberOfCardsDiscard();
        if (discardPile.Count > 0) gameManager.ActivateDiscardPileImage();

        // 4) Robar nuevas cartas y recolocar
        for (int i = 0; i < n; i++) DrawCard();
        UpdateHandVisuals();

        // 5) Salir de mulligan: reactivar interacciones y apagar selectables/halos
        mulliganUsed = true;
        inMulligan = false;

        foreach (var go in cardsInHand)
        {
            var drag = go.GetComponent<CardDragDrop>(); if (drag) drag.enabled = true;
            var hab  = go.GetComponentInChildren<HabilityCardHandler>(true); if (hab) hab.enabled = true;

            var sel = go.GetComponent<MulliganSelectable>();
            if (sel != null)
            {
                sel.SetSelected(false, applyVisual: true); // apaga el halo (mulliganOverlay)
                sel.enabled = false;                       // sin clicks fuera del mulligan
            }
        }

        gameManager.OnMulliganFinished();
    }

    // Cancelar sin cambios
    public void CancelMulligan()
    {
        if (!inMulligan) return;

        inMulligan = false;
        mulliganSelectedCount = 0;

        foreach (var go in cardsInHand)
        {
            // Reactiva interacciones normales
            var drag = go.GetComponent<CardDragDrop>(); if (drag) drag.enabled = true;
            var hab  = go.GetComponentInChildren<HabilityCardHandler>(true); if (hab) hab.enabled = true;

            // Apaga halo y deshabilita selectable
            var sel = go.GetComponent<MulliganSelectable>();
            if (sel != null)
            {
                sel.SetSelected(false, applyVisual: true); // apaga el mulliganOverlay
                sel.enabled = false;
            }
        }

        gameManager.OnMulliganFinished();
    }

    // Llamado por MulliganSelectable al togglear
    public bool TryToggleSelect(MulliganSelectable sel, bool wantSelect)
    {
        if (!inMulligan) return false;

        if (wantSelect)
        {
            if (mulliganSelectedCount < MulliganMaxSelectable)
            {
                mulliganSelectedCount++;
                mulliganOrder.Add(sel);
                return true;
            }

            if (mulliganOrder.Count > 0)
            {
                var oldest = mulliganOrder[0];
                mulliganOrder.RemoveAt(0);

                mulliganSelectedCount = Mathf.Max(0, mulliganSelectedCount - 1);

                oldest.SetSelected(false, applyVisual: true);

                mulliganSelectedCount++;
                mulliganOrder.Add(sel);
                return true;
            }

            mulliganSelectedCount = 1;
            mulliganOrder.Clear();
            mulliganOrder.Add(sel);
            return true;
        }
        else
        {
            if (mulliganSelectedCount > 0)
                mulliganSelectedCount--;

            for (int i = mulliganOrder.Count - 1; i >= 0; i--)
            {
                if (mulliganOrder[i] == sel)
                {
                    mulliganOrder.RemoveAt(i);
                    break;
                }
            }
            return true;
        }
    }

    public void ForceCloseMulliganForEndWave()
    {
        if (!inMulligan) return;

        inMulligan = false;
        mulliganSelectedCount = 0;

        foreach (var go in cardsInHand)
        {
            // reactivar interacción normal
            var drag = go.GetComponent<CardDragDrop>();
            if (drag) drag.enabled = true;

            var hab = go.GetComponentInChildren<HabilityCardHandler>(true);
            if (hab) hab.enabled = true;

            // apagar selectable + bloqueo de raycast + halo
            var sel = go.GetComponent<MulliganSelectable>();
            if (sel != null) sel.ForceDisable();
        }
    }

    public void ClearLastCard()
    {
        lastCardUsed = null;
    }
}