using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public float rotateSpeed = 18f;
    public GameObject blip;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject spawnedblip = GameObject.Instantiate(blip, other.transform.position, other.transform.rotation);
        spawnedblip.transform.parent = null;
    }
}
