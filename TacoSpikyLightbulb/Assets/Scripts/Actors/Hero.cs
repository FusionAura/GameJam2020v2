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

        // Pickup Beer
        if (Input.GetKeyDown(KeyCode.C))
        {
            PickupGameObject(GameObject.Find("obj_beer"));
        }

        // Pickup Broom
        if (Input.GetKeyDown(KeyCode.D))
        {
            PickupGameObject(GameObject.Find("obj_broom"));
        }

        // Swing Broom
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Temp. Set the position to be directly under the light.
            var lightbulb = GameObject.Find("obj_lightbulb");
            Vector3 underTheLightbulb = new Vector3(lightbulb.transform.position.x, transform.position.y, lightbulb.transform.position.z);
            transform.position = underTheLightbulb;

            PlayAnimation("hit-up", () => {
                lightbulb.GetComponent<Rigidbody>().useGravity = true;
            }, -0.2f);
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

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "danger")
        {
            Explode();
            Destroy(GetComponent<BoxCollider>());
        }
    }

    /// <summary>
    /// Play the interacting animation, then execute onComplete.
    /// </summary>
    /// <param name="onComplete"></param>
    public void Interact(System.Action onComplete)
    {
        PlayAnimation("interact", onComplete, -0.2f);
    }

    /// <summary>
    /// Plays the interacting animation, then picks up the specified GO, then stands.
    /// </summary>
    /// <param name="go"></param>
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
    
    /// <summary>
    /// What do you think?
    /// </summary>
    public void Explode()
    {
        var vms = GetComponentsInChildren<VecModel>();
        foreach (var e in vms)
            e.Explode();
    }

    /// <summary>
    /// Play the specified animation clip.
    /// </summary>
    /// <param name="animationName"></param>
    public void PlayAnimation(string animationName)
    {
        cAnimation.CrossFade(animationName, 0.2f);
    }

    /// <summary>
    /// Play the specified animation clip, then call onComplete().
    /// onComplete can be called sooner or later by setting timeMod.
    /// </summary>
    /// <param name="animationName"></param>
    /// <param name="onComplete"></param>
    /// <param name="timeMod"></param>
    public void PlayAnimation(string animationName, System.Action onComplete, float timeMod = -0.2f)
    {
        PlayAnimation(animationName);

        AddTimeout(onComplete, cAnimation.GetClip(animationName).length + timeMod);
    }

    /// <summary>
    /// Call callback() after a delay.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="delay"></param>
    public void AddTimeout(System.Action callback, float delay)
    {
        timeouts.Add(new Timeout(callback, delay));
    }
}
