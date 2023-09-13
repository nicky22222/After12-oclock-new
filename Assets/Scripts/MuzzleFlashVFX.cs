using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class MuzzleFlashVFX : MonoBehaviour
{
    private ParticleSystem ps;
    
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
    }

    public void Play()
    {
        Debug.Log("Muzzle!");
        ps.Play(true);
    }
}
