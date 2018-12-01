
using UnityEngine;

public class Startup : MonoBehaviour {

    Movin mov;
    Movin mov2;
    Movin mov3;
    Movin mov4;

    void Start () {

        //mov = new Movin(transform, "json/dupe3");
        //mov.Play();

        mov2 = new Movin(transform, "json/color", -100);
        mov2.Play();

        mov3 = new Movin(transform, "json/ship", 200);
        mov3.Play();

        //mov4 = new Movin(transform, "json/bag", 200);
        //mov4.Play();
    }

}
