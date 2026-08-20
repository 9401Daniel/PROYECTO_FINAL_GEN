using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject hoverRayon;

    private void Awake()
    {
        hoverRayon.SetActive(false);
    }

    private void OnEnable()
    {
        hoverRayon.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverRayon.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverRayon.SetActive(false);
    }
}