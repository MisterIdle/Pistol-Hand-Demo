using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour
{
    // Get the particle system component
    private ParticleSystem ps;

    void Start()
    {
        Destroy(gameObject, 0.9f);
    }

    private void Update()
    {
        Color color = new Color(Random.value, Random.value, Random.value, 1.0f);
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = color;
    }
}
