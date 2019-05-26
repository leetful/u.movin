using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace u.movin
{
    public class Updater : MonoBehaviour
    {
        public Action fired;

        void Update()
        {
            fired?.Invoke();
        }
    }
}