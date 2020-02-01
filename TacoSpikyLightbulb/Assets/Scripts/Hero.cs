using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void Explode()
    {
        var vms = GetComponentsInChildren<VecModel>();
        foreach (var e in vms)
            e.Explode();
    }

    public void PlayAnimation(string animationName)
    {
        var animation = GetComponentInChildren<Animation>();
        animation.CrossFade(animationName, 0.2f);
    }
}
