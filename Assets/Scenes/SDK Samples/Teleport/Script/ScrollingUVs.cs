using UnityEngine;
using System.Collections;

public class ScrollingUVs : MonoBehaviour 
{
	public Vector2 MainUVOffset = new Vector2( 0.0f, 0.0f );
	public Vector2 DetailUVOffset = new Vector2(0.0f, 0.0f);
	public string Mainname = "_MainTex";
	public string DetailMap = "_DetailAlbedoMap";
	public string CustomMap =  "_BumpMap";
	public float waitTime = 0.1f;
	bool isLateUpdate= false;

	Vector2 uvOffset = Vector2.zero;
	Vector2 uvOffset2 = Vector2.zero;

	void Awake ()
	{
		StartCoroutine( wait() );
	}

	void LateUpdate()
	{
		if (isLateUpdate)
		{   
			uvOffset += (MainUVOffset * Time.deltaTime);
			uvOffset2 += (DetailUVOffset * Time.deltaTime);
			gameObject.GetComponent<Renderer> ().material.SetTextureOffset (Mainname, uvOffset);
			gameObject.GetComponent<Renderer> ().material.SetTextureOffset (DetailMap, uvOffset2);
			gameObject.GetComponent<Renderer> ().material.SetTextureOffset (CustomMap, uvOffset);
		}
	}

	IEnumerator wait()
	{
		yield return new WaitForSeconds(waitTime);
		isLateUpdate = true;
	}
}
