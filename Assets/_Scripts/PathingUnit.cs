﻿using UnityEngine;
using System.Collections;

public class PathingUnit : SelectableUnit
{

    public Transform target;
    public float speed = 10f;
    Vector3[] path;
    public int pathWaypointIndex;
    public bool isPathing;
    public int debugPathLength;

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
    }

    protected override void Update ()
    {

        base.Update();

        if (!isPathing && target != null)
        {
            StartNewPath();
        }
        else 
        {

        }
        
    }

    void StartNewPath ()
    {
        Debug.Log("go");
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        isPathing = true;
    }

    public void OnPathFound (Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            //pathWaypointIndex = 0;
            path = newPath;
            debugPathLength = path.Length;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath ()
    {
        pathWaypointIndex = 0;
        Vector3 currentWaypoint = path[0];

        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                pathWaypointIndex++;
                if (pathWaypointIndex >= path.Length)
                {
                    target = null;
                    isPathing = false;
                    yield break;
                }
                currentWaypoint = path[pathWaypointIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, (speed) * Time.deltaTime);
            yield return null;
        }
    }
    public void OnDrawGizmos ()
    {
        if (null != path)
        {
            for (int i = pathWaypointIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(path[i], Vector3.one);
                if (i == pathWaypointIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}