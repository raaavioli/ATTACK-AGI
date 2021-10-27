using UnityEngine;

public class SwordAttack : Attack {
    private GameObject Sword;

    [SerializeField]
    private GameObject SwordPrefab;

    protected override int GetMaxTargets() {
        return 1;
    }

    protected override void InstantiateProjectile() {
        Sword = Instantiate(SwordPrefab, transform, true);
        Sword.SetActive(false);
    }

    protected override void StartProjectile() {
        Sword.transform.position = AttackSource.transform.position;
        Sword.SetActive(true);
    }

    protected override void StopProjectile() {
        Sword.SetActive(false);
    }

    protected override void UpdateProjectile() {
        if (Sword.activeSelf) {
            float t = (SimulationTime - ChargeTime) / MaxFireTime;
            if (t < 0)
                t = 0;
            Vector3 StartPosition = AttackSource.transform.position;
            Vector3 TargetPosition = TargetPositions[0];
            Sword.transform.position = StartPosition * (1 - t) + TargetPosition * t;
            Sword.transform.rotation = Quaternion.LookRotation(TargetPosition - StartPosition);
            Sword.transform.eulerAngles = new Vector3(90.0f, Sword.transform.eulerAngles.y, Sword.transform.eulerAngles.z);
        }
    }

    private void OnDisable() {
        if (Sword != null)
            Sword.SetActive(false);
    }
}
