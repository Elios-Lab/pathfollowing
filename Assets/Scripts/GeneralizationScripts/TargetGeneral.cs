using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGeneral : MonoBehaviour {
    public Vector3 Orientation => transform.forward;
    private Collider fullEndCollider;

    private void Awake() { fullEndCollider = GetComponent<Collider>(); }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("agent")) {
            if (fullEndCollider.bounds.Intersects(other.bounds)) {
                    other.gameObject.transform.parent.GetComponent<GeneralizationAgent>().GoalReward();
                    //Debug.Log("collision for reward");
            }
        }
    }
}
