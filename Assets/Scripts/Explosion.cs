using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Delete", .2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            other.GetComponent<Friendly>().Kill();
        }
    }

    IEnumerator Delete(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
