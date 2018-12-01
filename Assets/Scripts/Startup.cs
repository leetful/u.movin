
using UnityEngine;

public class Startup : MonoBehaviour {

    Movin mov;

    void Start () {

        mov = new Movin(transform, "json/dupe2");
        mov.Play();
    }

}
