using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shielding : Special {

	[SerializeField]
	private GameObject shieldPrefab;
	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float shielding = 0.5f;

	private const float SHIELD_OFFSET_FORWARD = 5f;
	private const float SHIELD_OFFSET_FORWARD_EPSILON = 1.5f;
	private const float SHIELD_OFFSET_UP = 2.0f;

	private GameObject shield;

	private CharacterCommon lastCharacter;
	private CharacterCommon ownCharacter;

	private Vector3 currentVelocity;
	private Vector3 currentTargetPosition;

	private void Awake() {
		ownCharacter = GetComponent<CharacterCommon>();

		shield = Instantiate(shieldPrefab);
		shield.transform.position = CalculateTargetPosition(transform);
		shield.SetActive(false);
	}

	private void Update() {
		// Smoothly move the shield to the target position.
		shield.transform.position = Vector3.SmoothDamp(shield.transform.position, currentTargetPosition, ref currentVelocity, 0.1f);
	}

	public override void Execute(GameObject[] targets) {
		shield.SetActive(true);

		// Remove damage modifier from last targeted character.
		if (lastCharacter != null) {
			lastCharacter.damageModifier /= shielding;
		}

		// Filter out null characters.
		List<CharacterCommon> nonNullTargets = new List<CharacterCommon>();
		foreach (GameObject obj in targets) {
			if (obj != null) {
				nonNullTargets.Add(obj.GetComponent<CharacterCommon>());
			}
		}

		// Filter out offensive characters.
		List<CharacterCommon> defensiveTargets = new List<CharacterCommon>();
		foreach (CharacterCommon obj in nonNullTargets) {
			if (obj.Mode == CharacterMode.Defensive) {
				defensiveTargets.Add(obj);
			}
		}

		// Get team and set rotation, this does not have to be done every time, but the alternative is annoying to do.
		Team team = ownCharacter.GetTeam();
		shield.transform.eulerAngles = new Vector3(0.0f, team == 0 ? 90.0f : -90.0f, 0.0f);

		// Get the target character if there are any non-null.
		if (nonNullTargets.Count < 1) {
			return;
		}

		// Prefer defensive characters.
		CharacterCommon target;
		if (defensiveTargets.Count > 0) {
			target = defensiveTargets[Random.Range(0, defensiveTargets.Count)];
		} else {
			target = nonNullTargets[Random.Range(0, nonNullTargets.Count)];

		}

		// Set the new target position.
		Transform targetTransform = target.transform;
		currentTargetPosition = CalculateTargetPosition(targetTransform);

		// Set the damage modifier and last target.
		lastCharacter = target;
		target.damageModifier *= shielding;
	}

	private Vector3 CalculateTargetPosition(Transform targetTransform) {
		float forwardOffset = Random.Range(SHIELD_OFFSET_FORWARD - SHIELD_OFFSET_FORWARD_EPSILON, SHIELD_OFFSET_FORWARD + SHIELD_OFFSET_FORWARD_EPSILON);
		return targetTransform.position + shield.transform.forward * forwardOffset + shield.transform.up * SHIELD_OFFSET_UP;
	}

	private void OnDestroy() {
		if (lastCharacter != null) {
			lastCharacter.damageModifier /= shielding;
		}
		Destroy(shield);
	}
}
