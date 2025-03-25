using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject cardPrefab;
    public Transform hoverLayer;
    private GameObject hoverInstance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void OnPointerEnter(PointerEventData eventHover)
    {
        hoverInstance = Instantiate(cardPrefab, hoverLayer);
        hoverInstance.transform.localScale = transform.localScale *1.5f;
        hoverInstance.transform.SetAsLastSibling();
        RectTransform originalT = GetComponent<RectTransform>();
        RectTransform copyT = hoverInstance.GetComponent<RectTransform>();

        Vector3 worldPos = originalT.position;
        copyT.position = worldPos;
    }

    public void OnPointerExit(PointerEventData eventHover)
    {
       if(hoverInstance != null)
            Destroy(hoverInstance);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
