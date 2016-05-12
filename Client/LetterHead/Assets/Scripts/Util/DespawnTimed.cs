using UnityEngine;
using System.Collections;
using PathologicalGames;

public class DespawnTimed : MonoBehaviour
{
    public float delay;
    public string poolName;

	// Use this for initialization
	IEnumerator Start () {
	    yield return new WaitForSeconds(delay);
        PoolManager.Pools[poolName].Despawn(transform);
	}
}
