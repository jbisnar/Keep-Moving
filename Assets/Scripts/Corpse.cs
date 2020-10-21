using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour
{
    public SpriteRenderer sr;
    float fadetime = 3f;
    float timealive;

    // Start is called before the first frame update
    void Start()
    {
        timealive = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timealive += Time.deltaTime;
        sr.color = Color.Lerp(Color.white,new Color(1,1,1,0),timealive/fadetime);
        if (timealive > fadetime)
        {
            Destroy(gameObject);
        }
    }
}
