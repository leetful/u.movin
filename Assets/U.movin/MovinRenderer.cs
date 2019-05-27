using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MovinRenderer : MonoBehaviour
{
    private Movin mov;
    bool shouldUpdate = false;

    [SerializeField]
    string resourcePath = "json/";

    [SerializeField]
    float scale = 0.1f;

    [SerializeField]
    int sortingLayer = 0;

    [SerializeField]
    float strokeWidth = 0.5f;

    [SerializeField]
    bool loop = true;

    [Range(0.01f, 1)]
    public float quality = 0.1f;        // Lower is better quality (more vertices)


    void Start () {
        RenderMovin();
    }

    void ClearChildren(){
        for (int i = 0; i < transform.childCount; i++){
            Object.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        mov = null;
    }

    void RenderMovin() {
        ClearChildren();

        mov = new Movin(transform, resourcePath, sortingLayer, scale, strokeWidth, loop, quality);
        mov.Play();

    }

    void OnValidate() {
        shouldUpdate = true;
    }

    void Update(){
        if (shouldUpdate){
            RenderMovin();
            shouldUpdate = false;
        }
    }

}
