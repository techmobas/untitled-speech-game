using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField] LayerMask targetLayer;

	private void OnTriggerEnter2D(Collider2D collision) {
		if(targetLayer == (targetLayer | 1 << collision.gameObject.layer)) {
			Destroy(gameObject);
		}
	}
}
