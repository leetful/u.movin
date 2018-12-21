
using UnityEngine;

public class Sample : MonoBehaviour {

    Movin mov;

    void Start () {

        mov = new Movin(transform, "json/precomp");
        mov.Play();

    }

}
