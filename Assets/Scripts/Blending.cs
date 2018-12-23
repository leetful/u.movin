
using UnityEngine;

public class Blending : MonoBehaviour {
    
    Movin s;
    int n = 0;  
    string str = "";

    void Start () {

        s = new Movin(transform, "json/circle", quality: 0.1f);
        s.Play();

        // s.Blend("json/triangle", 60f);

        // s.OnComplete = ()=>{ 
            // if (n == 1) str = "square";
            // if (n == 2) str = "circle";
            // if (n == 0) str = "triangle";

            // s.Blend("json/" + str, 20f); 

            // n = n >= 2 ? 0 : n + 1;
        // };
        
    }

    void Update(){
        if (Input.GetMouseButtonDown(0)){
            if (n == 1) str = "square";
            if (n == 2) str = "circle";
            if (n == 0) str = "triangle";

            s.Blend("json/" + str, 40f); 

            n = n >= 2 ? 0 : n + 1;
        }
    }

}
