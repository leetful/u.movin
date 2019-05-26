using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bust : MonoBehaviour {
    
    void Start () {

        Movin m = new Movin(transform, "json/bust", quality:0.2f);
        m.Play();

    }

    void Update(){
        
    }

}
