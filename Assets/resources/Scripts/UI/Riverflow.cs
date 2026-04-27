using UnityEngine;

public class WaterFlow : MonoBehaviour
{
    public float speed = 0.5f;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offset = Time.time * speed;
        rend.material.mainTextureOffset = new Vector2(0, offset);
    }
}