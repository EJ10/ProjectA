using UnityEngine;
using System.Collections;

public class MiniMap : MonoBehaviour
{
    public GameObject _player;
	void Start ()
    {
	
	}
	
	void Update ()
    {
        transform.position = new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z);
	}
}
