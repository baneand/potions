using UnityEngine;
using System.Collections;
public class scrollingScript : MonoBehaviour
{

    public float Speed;
public bool OffsetX = false;
public bool OffsetY = false;
public Renderer rend;

// Use this for initialization
void Start()
{
    rend = GetComponent<Renderer>();

}

// Update is called once per frame
void Update()
{

    float offset = Time.time * Speed;


    if (OffsetX == true)
    {
        rend.material.mainTextureOffset = new Vector2(offset, 0f);

    }
    else if (OffsetY == true)
    {
        rend.material.mainTextureOffset = new Vector2(0f, offset);
    }
    else if (OffsetX == true & OffsetY == true)
    {
        rend.material.mainTextureOffset = new Vector2(offset, offset);
    }

}
}