using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Special : MonoBehaviour {

	public bool canExecute { get; protected set; } = true;

	public abstract void Execute(GameObject[] targets);
}
