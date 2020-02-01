using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public bool alive = true, MoveToTarget = false, OnLadder = false, YourWinner = false;
    private Hero AnimationScript;
    public Transform Destination;
    private Quaternion lastDirection;


    [Range(1f,10)]
    public float walkSpeed = 1f;

    // Start is called before the first frame update
    private void Awake()
    {
        AnimationScript = GetComponent<Hero>();
    }

    private void Update()
    {
        if (MoveToTarget == true)
        {
            AnimationScript.PlayAnimation("run");
            float step = walkSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, Destination.position, step);



            Vector3 relativePos = Destination.position - transform.position;

            // the second argument, upwards, defaults to Vector3.up
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = rotation;
            lastDirection = rotation;


            if (Vector3.Distance(transform.position, Destination.position) < 0.001f)
            {
                MoveToTarget = false;
                AnimationScript.PlayAnimation("stand");
            }
            else
            {
                lastDirection = rotation;
            }
        }
        else
        {
            transform.rotation = lastDirection;
        }
    }

}
