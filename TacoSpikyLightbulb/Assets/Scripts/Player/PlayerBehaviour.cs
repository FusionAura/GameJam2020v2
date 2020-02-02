using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public bool alive = true, MoveToTarget = false, OnLadder = false, YourWinner = false;
    private Hero AnimationScript;
    public Transform Destination;
    private Quaternion lastDirection;
    public GameController controller;

    [Range(1f,10)]
    public float walkSpeed = 1f;

    // Start is called before the first frame update
    private void Awake()
    {
        AnimationScript = GetComponent<Hero>();
        lastDirection = Quaternion.Euler(0f, 180f, 0f);
    }

    private void Update()
    {
        if (MoveToTarget == true)
        {
            AnimationScript.PlayAnimation("run");
            float step = walkSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, Destination.position, step);

            if (Vector3.Distance(transform.position, Destination.position) > 0.001f)
            {
                Vector3 relativePos = Destination.position - transform.position;

                // the second argument, upwards, defaults to Vector3.up
                Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                transform.rotation = rotation;
                lastDirection = rotation;
            }
            else
            {
                MoveToTarget = false;
                AnimationScript.PlayAnimation("stand");
            }
        }
        else
        {
            transform.rotation = lastDirection;
        }
    }

}
