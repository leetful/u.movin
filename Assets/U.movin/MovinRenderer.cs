using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovinRenderer : MonoBehaviour
{
    [SerializeField]
    public string resourcesPath = "json/samurai";

    void Start () {

        Movin mov = new Movin(transform, resourcesPath);
        mov.Play();

    }   

}
