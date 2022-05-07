using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMovement : MonoBehaviour
{
    Queue<BrainMovement> MovementQueue = new Queue<BrainMovement>();
    //public delegate void OnConsumeQueueItem(int lasting);
    public float TimeBetweenConsumptions = 0.5f;
    public float speed = .3f;
    public float jumpStrength = 100;
    private float lastConsumption = 0;
    private bool jumping = true;
    public event Action<int> ConsumedItem;  // hier
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        if (lastConsumption + TimeBetweenConsumptions < Time.time)
        {
            if (MovementQueue.Count > 0)
            {
                BrainMovement current = MovementQueue.Dequeue();
                Move(current);
                OnConsumeQueueItem(MovementQueue.Count);  // hier
                lastConsumption = Time.time;
            }
        }
    }

    void Move(BrainMovement movement)
    {
        if (movement.Right)
        {
            rb.velocity += new Vector3(speed, 0, 0);
        }
        if (movement.Left)
        {
            rb.velocity += new Vector3(-speed, 0, 0);
        }
        if (!jumping && movement.Jump)
        {
            jumping = true;
            rb.velocity += new Vector3(0, speed * 2 * jumpStrength * TimeBetweenConsumptions, 0);
        }
    }

    public void ResetSpeed()
    {
        rb.angularDrag = 0;
        rb.angularVelocity = new Vector3(0, 0, 0);
        rb.velocity = new Vector3(0, 0, 0);
    }
    public void AddMovementToQueue(BrainMovement movement)
    {
        MovementQueue.Enqueue(movement);
    }
    // hier
    protected void OnConsumeQueueItem(int lasting)
    {
        ConsumedItem?.Invoke(lasting);
    }
    private void OnCollisionEnter(Collision collision)
    {
        jumping = false;
    }
}
