using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AirStrikeAttack : Attack
{
    private GameObject Plane;
    private Vector3 Direction;
    protected override void InstantiateProjectile()
    {
        GameObject PlanePrefab = Resources.Load<GameObject>("Models/colonel/PlanePrefab");
        Plane = Instantiate(PlanePrefab);
        Plane.SetActive(false);
    }

    protected override void StartProjectile()
    {
        // Arbitrary distance backward from the attacker, should be off screen.
        Vector3 StartPosition = gameObject.transform.position
        - gameObject.transform.forward * 30
        + gameObject.transform.up * 30;
        Plane.transform.position = StartPosition;
        this.Direction = ((TargetPosition + Vector3.down) - StartPosition).normalized;
        Plane.transform.rotation = Quaternion.LookRotation(Direction);
        Plane.SetActive(true);
    }

    protected override void StopProjectile()
    {
        Plane.SetActive(false);
    }

    protected override void UpdateProjectile()
    {
        if (Plane.activeSelf)
        {
            float t = (SimulationTime - FireStartTime) / MaxFireTime;
            if (t < 0)
                t = 0;
            float MetersPerSecond = Time.deltaTime * 200f;
            Vector3 NewDir = ((TargetPosition + Vector3.down) - Plane.transform.position);
            const float Dist = 3;
            float Magnitude = NewDir.magnitude;
            Vector3 currentDirection = Direction;
            // When plane is far away from the target we want to adapt its trajectory towards the target
            if (Magnitude <= Dist)
            {
                // When the plane is close, we want it to turn down
                float _t = Magnitude / Dist;
                currentDirection = (Direction + Vector3.down * (1 - _t)).normalized;
            }
            Plane.transform.position += MetersPerSecond * currentDirection;
            Plane.transform.rotation = Quaternion.LookRotation(currentDirection);
        }
    }
}
