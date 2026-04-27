using UnityEngine;
using UnityEngine.EventSystems; // Required for handling Hover/Click events
using UnityEngine.UI;

public class VRButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float clickScale = 0.95f;
    public float animationSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button btn;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        btn = GetComponent<Button>();
    }

    private void Update()
    {
        // Smoothly animate to the target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    // --- HOVER EVENTS ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return; // Ignore if disabled

        targetScale = originalScale * hoverScale; // Scale Up
        AudioManager.Instance?.PlayHover(); // Play Sound
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale; // Reset Scale
    }

    // --- CLICK EVENTS ---
    public void OnPointerDown(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;

        targetScale = originalScale * clickScale; // Scale Down (Press effect)
        AudioManager.Instance?.PlayClick(); // Play Sound
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale; // Reset Scale
    }
}