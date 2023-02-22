using UnityEngine;
using System.Collections;

public class SetRenderQueue : MonoBehaviour 
{
	public int Queue;
	void Start () 
	{
		
		Renderer rend = GetComponent<Renderer> ();
		Debug.Log (rend.material.renderQueue);
		rend.material.renderQueue = Queue;
	}
	
	void Update () 
	{
	
	}
}
