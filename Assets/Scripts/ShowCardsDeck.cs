using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowCardsDeck : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public PauseMenuController pauseMenuController;
    private Color originalColor;
    public void OnPointerClick(PointerEventData eventData)
    {
        pauseMenuController.ShowCardsDeck();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //GetComponent<Image>().color = Color.magenta;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().color = originalColor;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = GetComponent<Image>().color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
