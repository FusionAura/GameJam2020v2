using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public bool alive = true, MoveToTarget = false, OnLadder = false, YourWinner = false;

    public Transform Destination;

    [Range(1f,10)]
    public float walkSpeed = 1f;

    // Start is called before the first frame update

    private void Update()
    {
        if (MoveToTarget == true)
        {

            float step = walkSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, Destination.position, step);
            if (Vector3.Distance(transform.position, Destination.position) < 0.001f)
            {
                MoveToTarget = false;
            }
        }
    }

}
