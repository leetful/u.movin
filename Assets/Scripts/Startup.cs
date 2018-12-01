
using UnityEngine;

public class Startup : MonoBehaviour {

    Movin mov;

    void Start () {

        mov = new Movin(transform, "json/bag");
        mov.Play();
    }

}
