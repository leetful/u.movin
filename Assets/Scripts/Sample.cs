
using UnityEngine;

public class Sample : MonoBehaviour {
    
    Movin samurai;
    string str = "samurai";

    void Start () {

        samurai = new Movin(transform, "json/samurai");
        samurai.Play();

    }

    void Update(){
        if (Input.GetMouseButtonDown(0)){
            str = (str == "samurai") ? "samurai2" : "samurai";
            samurai.Blend("json/" + str, 10f); 
        }
    }

}
