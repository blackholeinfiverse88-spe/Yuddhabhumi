using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VRCameraWarning : MonoBehaviour
{
    [Header("Target to Watch")]
    public BaseHealth targetBase; // Drag your Player Base object here!

    [Header("UI References")]
    public Image warningImage; // The Red Vignette

    [Header("Settings")]
    [Tooltip("Percentage (0-1) to start flashing. 0.3 = 30%")]
    public float lowHealthThreshold = 0.3f; 
    public float flashSpeed = 3f;
    public float maxAlpha = 0.4f;

    private Coroutine flashRoutine;

    void Start()
    {
        // Ensure the warning is hidden at start
        if (warningImage != null)
        {
            SetAlpha(0); 
        }

        // Auto-find base if not assigned (Convenience)
        if (targetBase == null)
        {
            GameObject playerBaseObj = GameObject.FindWithTag("PlayerBase");
            if (playerBaseObj) targetBase = playerBaseObj.GetComponent<BaseHealth>();
        }
    }

    void Update()
    {
        if (targetBase == null) return;

        // 1. Calculate Health Percentage
        float hpPercent = targetBase.currentHealth / targetBase.maxHealth;

        // 2. Check Threshold
        // If HP is low (but alive), start flashing
        if (hpPercent <= lowHealthThreshold && targetBase.currentHealth > 0)
        {
            if (flashRoutine == null)
                flashRoutine = StartCoroutine(FlashRoutine());
        }
        else
        {
            // Otherwise (Health is high OR Base is dead), stop flashing
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
                SetAlpha(0); // Reset to invisible
            }
        }
    }

    IEnumerator FlashRoutine()
    {
        while (true)
        {
            // Smooth Sine-wave pulse
            float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f * maxAlpha;
            SetAlpha(alpha);
            yield return null;
        }
    }

    void SetAlpha(float a)
    {
        if (warningImage != null)
        {
            Color c = warningImage.color;
            c.a = a;
            warningImage.color = c;
        }
    }
}