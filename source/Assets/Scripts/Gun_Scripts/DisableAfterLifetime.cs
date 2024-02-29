using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterLifetime : MonoBehaviour {

	public float lifeTime = 5.0f;

	private IEnumerator FuncDisableAfterLifetime()
	{
		yield return new WaitForSeconds(lifeTime);
		gameObject.SetActive(false);
	}

    private void OnEnable()
    {
		StartCoroutine(FuncDisableAfterLifetime());
    }
}
