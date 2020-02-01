using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    /// <summary>
    /// The GameObject to parent GameObjects too while they're being held.
    /// </summary>
    public GameObject ItemHoldParent;


    private struct Timeout
    {
        public System.Action Callback;
        public float CallTime;

        public Timeout(System.Action Callback, float Duration)
        {
            this.Callback = Callback;
            this.CallTime = Time.time + Duration;
        }

        public bool Execute()
        {
            if (Time.time >= CallTime) {
                Callback();
                return true;
            }

            return false;
        }
    }
    private List<Timeout> timeouts;

    private GameObject item = null;
    private Animation cAnimation;

    // Start is called before the first frame update
    void Start()
    {
        timeouts = new List<Timeout>();
        cAnimation = GetComponentInChildren<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        // Run
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayAnimation("run");
        }

        // Explode
        if (Input.GetKeyDown(KeyCode.B))
        {
            Explode();
        }

        // Pickup GameObject
        if (Input.GetKeyDown(KeyCode.C))
        {
            PickupGameObject(GameObject.FindGameObjectWithTag("item"));
        }

        for (var i = 0; i < timeouts.Count; i++)
        {
            if (timeouts[i].Execute())
            {
                timeouts.RemoveAt(i);
                i--;
            }
        }
    }

    public void Interact(System.Action onComplete)
    {
        PlayAnimation("interact", onComplete, -0.2f);
    }

    public void PickupGameObject(GameObject go)
    {
        Interact(() => {
            ParentItemToMe(go);
        });
    }

    private void ParentItemToMe(GameObject go)
    {
        go.transform.parent = ItemHoldParent.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        item = go;
        //cAnimation.
        PlayAnimation("stand");
    }
    
    public void Explode()
    {
        var vms = GetComponentsInChildren<VecModel>();
        foreach (var e in vms)
            e.Explode();
    }

    public void PlayAnimation(string animationName)
    {
        cAnimation.CrossFade(animationName, 0.2f);
    }

    public void PlayAnimation(string animationName, System.Action onComplete, float timeMod = 0f)
    {
        PlayAnimation(animationName);

        AddTimeout(onComplete, cAnimation.GetClip(animationName).length + timeMod);
    }

    public void AddTimeout(System.Action callback, float delay)
    {
        timeouts.Add(new Timeout(callback, delay));
    }
}
