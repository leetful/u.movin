
using UnityEngine;

public class Startup : MonoBehaviour {

    Movin mov;

    void Start () {
        //mov = new Movin(transform, "json/racer");
        //mov = new Movin(transform, "json/anim-test");
        //mov = new Movin(transform, "json/anim-test2");
        //mov = new Movin(transform, "json/anim-test3");
        //mov = new Movin(transform, "json/anim-test4");
        //mov = new Movin(transform, "json/anim-test5");
        //mov = new Movin(transform, "json/anim-test6");
        //mov = new Movin(transform, "json/anim-test7");
        //mov = new Movin(transform, "json/anim-test8");
        mov = new Movin(transform, "json/anim-test9");
        mov.Play();
    }

}
