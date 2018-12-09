﻿using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{

    public float movementFrame = 10.0f;
    public float incrementRadius = 0.5f;
    public GameObject gamePlane;
    public NavMeshAgent agent;

    private readonly int IND_HORIZONTAL = 1;
    private readonly int IND_VERTICAL = 0;
    private readonly string[] AXIS_ARGS = { "Vertical", "Horizontal" };

    private Rigidbody rb;
    private float[] directions = { 1, 0 };
    private float spawnOffset = 0;
    private float SPAWN_POSITION_LIMIT = 0;

    private readonly int[] COORD_RANGE = new int[]{0, 1};

    private readonly int IND_UP = 0;
    private readonly int IND_DOWN = 1;
    private readonly int IND_LEFT = 2;
    private readonly int IND_RIGHT = 3;

    private static readonly int BIT_UP = 1;
    private static readonly int BIT_DOWN = 2;
    private static readonly int BIT_LEFT = 4;
    private static readonly int BIT_RIGHT = 8;

    private int[] BIT_DIR_ARRAY; // WARNING: BIT_LEFT AND RIGHT ARE REVERSED HERE
    private Vector3 staticCenter;
    private int currentDirection;
    private int validDirection; // use bitwise
    private Vector3 destination;

    public float mainIncrement;
    public float secondaryIncrement;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        BIT_DIR_ARRAY = new int[] { BIT_UP, BIT_DOWN, BIT_RIGHT, BIT_LEFT };
        currentDirection = BIT_RIGHT;
        validDirection = BIT_RIGHT | BIT_LEFT;
        staticCenter = CloneVector(transform.position);
    }

    private Vector3 CloneVector(Vector3 vector)
    {
        return new Vector3(vector.x, vector.y, vector.z);
    }

    private bool AndBitwise(int val1, int val2)
    {
        return (val1 & val2) > 0;
    }

    private void SetIndTarget(int direction, float increment, float x, float z,
        out float x2, out float z2)
    {
        if (AndBitwise(direction, BIT_UP))
        {
            z += increment;
        }
        else if (AndBitwise(direction, BIT_DOWN))
        {
            z -= increment;
        }

        if (AndBitwise(direction, BIT_RIGHT))
        {
            x += increment;
        }
        else if (AndBitwise(direction, BIT_LEFT))
        {
            x -= increment;
        }

        x2 = x;
        z2 = z;
    }

    private Vector3 SetTarget(int mainDirection, int secondaryDirection)
    {
        Vector3 currentPosition = transform.position;
        float x = currentPosition.x;
        float y = currentPosition.y;
        float z = currentPosition.z;

        SetIndTarget(mainDirection, mainIncrement, x, z, out x, out z);
        SetIndTarget(secondaryDirection, secondaryIncrement, x, z, out x, out z);

        return new Vector3(x,y,z);
    }

    private int[] horizontalOptions = new int[] { 0, BIT_LEFT, BIT_RIGHT };
    private int[] verticalOptions = new int[] { 0, BIT_UP, BIT_DOWN };
    public float sampleMovementRadius;

    // if returns false, is vertical
    // assumes only one direction
    private bool IsHorizontal(int direction)
    {
        return AndBitwise(direction, BIT_LEFT | BIT_RIGHT);
    }

    private void SetDestination(int mainBit)
    {
        int[] optionalBits = horizontalOptions;
        if (IsHorizontal(mainBit))
        {
            optionalBits = verticalOptions;
        } // else option already done

        foreach (int secondaryBit in optionalBits)
        {
            Vector3 tmpDestination = SetTarget(mainBit, secondaryBit);
            NavMeshHit hit;
            //print(tmpDestination.ToString());

            bool isHit = NavMesh.SamplePosition(tmpDestination, out hit, sampleMovementRadius, NavMesh.AllAreas);
            if (isHit)
            {
                //print("Detects hit");
                agent.SetDestination(hit.position);
                break;
            }
        }
    }

    private void CheckDestination(int directionBitwise)
    {
        int mainBit = 0;
        
        foreach (int bitDirection in BIT_DIR_ARRAY)
        {
            mainBit = directionBitwise & bitDirection;

            if (mainBit != 0)
            {
                SetDestination(mainBit);
                break;
            }
        }
    }

    private void Update()
    {
        // control
        int movementInput = 0;
        foreach (int index in COORD_RANGE)
        {
            float movement = Input.GetAxis(AXIS_ARGS[index]);
            if (movement != 0)
            {
                int factor = (movement > 0) ? 0 : 1;
                movementInput = movementInput | BIT_DIR_ARRAY[(index * 2) + factor];
            }
        }

        CheckDestination(movementInput);
    }

    void FixedUpdate()
    {   

    }
}
