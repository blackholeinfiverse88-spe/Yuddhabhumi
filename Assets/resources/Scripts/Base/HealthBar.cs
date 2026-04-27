using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Transform target;
    public Vector3 offset = new Vector3(0, 5f, 0);

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + offset;
        transform.LookAt(Camera.main.transform);
    }

    public void SetHealth(float current, float max)
    {
        slider.maxValue = max;
        slider.value = current;
    }
}
