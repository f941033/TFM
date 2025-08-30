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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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



    // Update is called once per frame
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
            gameManager.ShowMessage("No hay cartas que robar!", 2);
            return;
        }

        var cardToDraw = drawPile[0];
        drawPile.RemoveAt(0);


        gameManager.textNumberOfCardsDeck.text = drawPile.Count.ToString();

        // ------------- INSTANCIAR LA CARTA ------------- //
        var cardData = Instantiate(prefabCard, handTransform.position, Quaternion.identity, handTransform);
        CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
        cardUI.SetCardUI(cardToDraw);

        if (cardToDraw.cardType == CardType.Trap || cardToDraw.cardType == CardType.DeckEffect || cardToDraw.cardType == CardType.Summon)
        {
            var drag = cardData.GetComponent<CardDragDrop>();
            drag.dropTilemap = zonaValidaTilemap;
            drag.cardData = cardToDraw;
            drag.player = player;
            drag.Deck = this;
        }
        else if (cardToDraw.cardType == CardType.Hability)
        {
            var drag = cardData.GetComponent<CardDragDrop>();
            drag.dropTilemap = zonaValidaTilemap;
            drag.cardData = cardToDraw;
            drag.player = player;
            drag.Deck = this;

            HabilityCardHandler hability = cardData.GetComponentInChildren<HabilityCardHandler>();
            hability.Initialize(cardToDraw, player);
        }
        else if (cardToDraw is BuffCardData buffData)
        {
            Destroy(cardData.GetComponent<CardDragDrop>());
            HabilityCardHandler hability = cardData.GetComponentInChildren<HabilityCardHandler>();
            hability.Initialize(buffData, player);
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
        if (cardData.cardType != CardType.Hability)
        {
            cardsInHand.Remove(card);
            discardPile.Add(cardData);
            Destroy(card);
            UpdateHandVisuals();
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

            Debug.Log("la carta cogida de descartes es: " + cardData.cardName);
            if (cardData.cardType == CardType.Trap || cardData.cardType == CardType.DeckEffect || cardData.cardType == CardType.Hability || cardData.cardType == CardType.Summon)
            {
                var drag = cardObj.GetComponent<CardDragDrop>();
                drag.dropTilemap = zonaValidaTilemap;
                drag.cardData = cardData;
                drag.player = player;
                drag.Deck = this;
            }
            else if (cardData is BuffCardData buffData)
            {
                Destroy(cardObj.GetComponent<CardDragDrop>());
                var hability = cardObj.GetComponentInChildren<HabilityCardHandler>();
                hability.Initialize(buffData, player);
            }

            cardsInHand.Add(cardObj);
            UpdateHandVisuals();
            count--;
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
        if (mulliganUsed) { FindFirstObjectByType<GameManager>().ShowMessage("Ya hiciste mulligan.", 2); return; }
        if (GameManager.CurrentPhase != DeckboundDungeon.GamePhase.GamePhase.Action) return;
        if (drawPile.Count == 0) { FindFirstObjectByType<GameManager>().ShowMessage("No quedan cartas en el mazo.", 2); return; }

        foreach (var go in cardsInHand)
        {
            // 1) Bloquea interacción normal
            var drag = go.GetComponent<CardDragDrop>();
            if (drag) drag.enabled = false;

            var hab = go.GetComponentInChildren<HabilityCardHandler>(true);
            if (hab) hab.enabled = false;

            // 2) Overlay que captura el click (arriba del todo)
            var overlayTr = go.transform.Find("MulliganOverlay");
            GameObject overlay;
            if (!overlayTr)
            {
                overlay = new GameObject("MulliganOverlay", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)overlay.transform;
                rt.SetParent(go.transform, false);
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                overlay.transform.SetAsLastSibling();

                var img = overlay.GetComponent<Image>();
                img.color = new Color(0, 0, 0, 0); 
                img.raycastTarget = true;

                overlay.AddComponent<MulliganSelectable>();
            }
            else
            {
                overlay = overlayTr.gameObject;
                overlay.transform.SetAsLastSibling(); // asegúrate que está arriba
                overlay.SetActive(true);
            }

            // 3) Inicializa el selector en el OVERLAY (no en la raíz)
            var cardUI = go.GetComponentInChildren<CardUI>();
            var sel = overlay.GetComponent<MulliganSelectable>();
            sel.Init(this, cardUI);
            sel.SetSelected(false, applyVisual: true);
        }

        inMulligan = true;
        mulliganSelectedCount = 0;

        Debug.Log("[Mulligan] Comienza selección. drawPile=" + drawPile.Count);
        FindFirstObjectByType<GameManager>().ShowMessage("Selecciona cartas y confirma.", 2);
    }

    public void ConfirmMulligan()
    {
        if (!inMulligan) return;

        var toReplace = new List<GameObject>();
        foreach (var go in cardsInHand)
        {
            var sel = go.GetComponentInChildren<MulliganSelectable>();
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
            FindFirstObjectByType<GameManager>().ShowMessage("No hay suficientes cartas en el mazo para ese mulligan.", 2);
            return;
        }

        // 1) Mover seleccionadas a descartes o al fondo del mazo
        foreach (var go in toReplace)
        {
            cardsInHand.Remove(go);

            var ui = go.GetComponentInChildren<CardUI>();
            var data = ui ? ui.data : null;

            if (data != null)
            {
                if (mulliganToDiscard)
                    discardPile.Add(data);
                else
                    drawPile.Add(data);
            }

            Destroy(go);
        }

        var gm = FindFirstObjectByType<GameManager>();
        gm.UpdateTextNumberOfCardsDiscard();
        if (discardPile.Count > 0) gm.ActivateDiscardPileImage();

        for (int i = 0; i < n; i++)
            DrawCard();

        UpdateHandVisuals();

        // 3) Salir de mulligan
        mulliganUsed = true;
        inMulligan = false;

        // Rehabilitar drag y cardHandler
        foreach (var go in cardsInHand)
        {
            var drag = go.GetComponent<CardDragDrop>();
            if (drag) drag.enabled = true;

            var hab = go.GetComponentInChildren<HabilityCardHandler>(true);
            if (hab) hab.enabled = true;

            // Elimina overlay si existe
            var overlay = go.transform.Find("MulliganOverlay");
            if (overlay) overlay.gameObject.SetActive(false); // o Destroy(overlay.gameObject);

            // Si dejaste algún MulliganSelectable en raíz (no debería ya), límpialo
            var selRoot = go.GetComponent<MulliganSelectable>();
            if (selRoot) Destroy(selRoot);
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
            var drag = go.GetComponent<CardDragDrop>();
            if (drag) drag.enabled = true;

            var hab = go.GetComponentInChildren<HabilityCardHandler>(true);
            if (hab) hab.enabled = true;

            // Elimina overlay si existe
            var overlay = go.transform.Find("MulliganOverlay");
            if (overlay) overlay.gameObject.SetActive(false); // o Destroy(overlay.gameObject);

            // Si dejaste algún MulliganSelectable en raíz (no debería ya), límpialo
            var selRoot = go.GetComponent<MulliganSelectable>();
            if (selRoot) Destroy(selRoot);
        }

        gameManager.OnMulliganFinished();
    }

    // Llamado por MulliganSelectable al togglear
    public bool TryToggleSelect(MulliganSelectable sel, bool wantSelect)
    {
        if (!inMulligan) return false;

        if (wantSelect)
        {
            if (mulliganSelectedCount >= drawPile.Count)
                return false;

            mulliganSelectedCount++;
            return true;
        }
        else
        {
            mulliganSelectedCount = Mathf.Max(0, mulliganSelectedCount - 1);
            return true;
        }
    }
}
