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
        public System.Action<float> OnStep;
        public float CallTime;

        private float duration;

        public Timeout(System.Action Callback, float Duration)
        {
            this.Callback = Callback;
            this.CallTime = Time.time + Duration;
            this.OnStep = null;

            this.duration = Duration;
        }

        public Timeout(System.Action Callback, System.Action<float> OnStep, float Duration)
        {
            this.Callback = Callback;
            this.CallTime = Time.time + Duration;
            this.OnStep = OnStep;

            this.duration = Duration;
        }

        public bool Execute()
        {
            if (this.OnStep != null)
            {
                var p = 1f - Mathf.Clamp((CallTime - Time.time) / duration, 0, 1f); // Percentage of the duration that this Timeout is at.
                OnStep(p);
            }

            if (Time.time >= CallTime) {
                if (Callback != null) Callback();
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
        if (Input.GetKeyDown(KeyCode.A))
        {
            PickupGameObject(GameObject.Find("obj_ladder"));
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            DrinkCurItem();
        }

        /*
         * // Run
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
            
        }

        // Swing Broom at bulb
        if (Input.GetKeyDown(KeyCode.E))
        {

        }*/

        // Swing Broom at ladder
        /*if (Input.GetKeyDown(KeyCode.F))
        {
            Fall();
        }

        // Open fridge
        if (Input.GetKeyDown(KeyCode.G))
        {

        }

        // Drop current item.
        if (Input.GetKeyDown(KeyCode.H))
        {
           // DropCurrentGameObject();
        }*/

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
            return;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "obj_landmine")
        {
            Explode(50f);
            other.gameObject.GetComponent<VecModel>().Explode(50f);
            return;
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
    /// Returns false if no item is held.
    /// </summary>
    public bool DrinkCurItem()
    {
        if (!item) return false;

        PlayAnimation("drink", () => { 
            
            item.GetComponent<VecModel>().Explode();

            PlayAnimation("stand");
        });

        return true;
    }

    public void HitLadder()
    {
        // Temp. Set the position to be directly under the light.
        var ladder = GameObject.Find("obj_ladder");

        PlayAnimation("hit-up", () => {
            ladder.GetComponent<Rigidbody>().useGravity = true;
            PlayAnimation("stand");
        }, -0.2f);
    }

    public void Fall()
    {
        // Temp. Set the position to be directly under the light.
        var ladder = GameObject.Find("obj_ladder");

        PlayAnimation("fall");
    }

    /// <summary>
    /// Returns false if he can't.
    /// </summary>
    /// <returns></returns>
    public bool PickupLadder()
    {
        var ladder = GameObject.Find("obj_ladder");
        if (ladder.GetComponent<Rigidbody>().useGravity)
        {
            PickupGameObject(ladder);
            return true;
        }

        return false;
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

    public void DropCurrentGameObject()
    {
        if (item)
        {
            item.transform.parent = null;
            var rb = item.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.useGravity = true;
                rb.detectCollisions = true;
            }

            item = null;
        }
    }

    private void ParentItemToMe(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity = false;
            rb.detectCollisions = false;
        }

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
    public void Explode(float force = 0f)
    {
        GetComponent<PlayerBehaviour>().alive = false;
        var vms = GetComponentsInChildren<VecModel>();
        foreach (var e in vms)
            e.Explode(force);

        Destroy(GetComponent<BoxCollider>());
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

    /// <summary>
    /// Calls callback every frame of in duration.
    /// </summary>
    /// <param name="onStep"></param>
    /// <param name="duration"></param>
    public void AddTimeoutOnStep(System.Action<float> callback, float duration)
    {
        timeouts.Add(new Timeout(null, callback, duration));
    }
}
