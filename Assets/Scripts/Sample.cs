
using UnityEngine;

public class Sample : MonoBehaviour {

    void Start () {

        Movin mov = new Movin(transform, "json/samurai");
        mov.Play();

    }

}
