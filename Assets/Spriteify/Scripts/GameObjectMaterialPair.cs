using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Spriteify
{
    [System.Serializable]
    public class GameObjectMaterialPair
    {
        /* CI: Represents a tuple (x,y) where 
         *  x is a GameObject and is well defined 
         *  y is a Material and is well defined 
         */
        public GameObject gameObject; // x
        public Material material; // y
    }
}