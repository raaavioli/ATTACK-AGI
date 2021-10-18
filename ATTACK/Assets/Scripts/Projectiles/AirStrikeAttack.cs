using UnityEngine;

public class AirStrikeAttack : Attack
{
    private GameObject Plane;

    protected override int GetMaxTargets()
    {
        return 1;
    }

    protected override void InstantiateProjectile()
    {
        GameObject PlanePrefab = Resources.Load<GameObject>("Models/colonel/PlanePrefab");
        Plane = Instantiate(PlanePrefab);
        Plane.SetActive(false);
    }

    private Vector3 GetPlaneStartPosition()
    {
        // Arbitrary distance backward from the attacker, should be off screen.
        return AttackSource.transform.position
        - AttackSource.transform.forward * 30
        + AttackSource.transform.up * 30;
    }

    protected override void StartProjectile()
    {
        Plane.transform.position = GetPlaneStartPosition();
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
            float t = (SimulationTime - ChargeTime) / MaxFireTime;
            if (t < 0)
                t = 0;
            Vector3 StartPosition = GetPlaneStartPosition();
            Vector3 TargetPosition = TargetPositions[0] + Vector3.down;
            Plane.transform.position = StartPosition * (1 - t) + TargetPosition * t;
            Plane.transform.rotation = Quaternion.LookRotation(TargetPosition - StartPosition);
            if (t > 0.95)
            {
                Plane.GetComponent<ObjectShatter>().Shatter();
            }
        }
    }

    private void OnDisable()
    {
        if (Plane != null)
            Plane.SetActive(false);
    }
}
