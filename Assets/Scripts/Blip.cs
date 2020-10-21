using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blip : MonoBehaviour
{
    public SpriteRenderer sr;
    float fadetime = 3.5f;
    float timealive;
    float mortarDelay = 5f;
    public GameObject explosion;

    // Start is called before the first frame update
    void Start()
    {
        timealive = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timealive += Time.deltaTime;
        sr.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), timealive / fadetime);
        if (timealive > mortarDelay)
        {
            GameObject spawnedexp = GameObject.Instantiate(explosion, transform.position, transform.rotation);
            spawnedexp.transform.parent = null;
            Destroy(gameObject);
        }
    }
}
