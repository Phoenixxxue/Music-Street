using UnityEngine;
using System.Collections;

public class AnimatedUVs : MonoBehaviour 
{
    public int materialIndex;
    public Vector2 uvAnimationRate;
    public string textureName;
	public float rate_x, rate_y;

    Vector2 uvOffset;
	
	void Awake() {
		materialIndex = 0;
		uvAnimationRate = new Vector2( rate_x, rate_y );
		textureName = "_MainTex";
		uvOffset = Vector2.zero;
	}
	
	public void SetRate(float rate_x, float rate_y) {
		uvAnimationRate = new Vector2( rate_x, rate_y );
	}

    void LateUpdate() 
    {
        uvOffset += ( uvAnimationRate * Time.deltaTime );
        if( GetComponent<Renderer>().enabled )
        {
            GetComponent<Renderer>().materials[ materialIndex ].SetTextureOffset( textureName, uvOffset );
        }
    }
}