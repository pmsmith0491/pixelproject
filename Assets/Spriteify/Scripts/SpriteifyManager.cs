using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteifyManager : MonoBehaviour
{

    List<Transform> spriteTargets;
    
    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void addTransform(Transform target)
    {
        if(!spriteTargets.Contains(target))
            spriteTargets.Add(target);
    }

    void removeTransform(Transform target)
    {
        if(spriteTargets.Contains(target))
            spriteTargets.Remove(target);
    }

}
