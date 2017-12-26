using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

    public Texture2D map;
    public Transform floor;
    [Header("Prefabs")]
    public GameObject wall;

	void Start () {
        Generate();
	}

    public void Generate() {
        int width = map.width;
        int height = map.height;
        floor.localScale = new Vector3(width, 1, height);
        Vector3 offset = new Vector3(-width * 0.5f, 0, -height * 0.5f);
        Color32[] pixels = map.GetPixels32();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 c = pixels[x + y * width];
                if (c.r == c.b && c.r == c.g)
                {
                    if (c.r == 255)
                        continue;
                    else
                    {
                        float h = - c.r / 255.0f;
                        Instantiate(wall, new Vector3(x, h, y) + offset, Quaternion.identity, transform);
                    }
                }
            }
        }
    }

    public void Despawn()
    {
        for (int i = transform.childCount-1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child != floor)
                Destroy(child.gameObject);
        }
    }
}
