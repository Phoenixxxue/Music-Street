using System.Collections.Generic;
using UnityEngine;

public class FireventOnParticleCollision : MonoBehaviour
{

	public ParticleSystem mainParticleSystem;
	public ParticleSystem subParticleSystem;

	public List<ParticleCollisionEvent> collisions = new List<ParticleCollisionEvent> ( );

	public int amountOfCollisions;
	public int maximumCollisionsPerCallback = 32;
	public float maximumTriggeringAngle = 0f;
	public int maxCallbacksPerFrame = 1;
	private int currentCallbacksPerFrame = 0;

	private ParticleSystem.EmitParams emitterParameters = 
		new ParticleSystem.EmitParams ( );

	public void OnParticleTrigger ( )
	{
		
	}


	public void OnParticleCollision ( GameObject other )
	{
		if ( currentCallbacksPerFrame > maxCallbacksPerFrame ) return;

		mainParticleSystem.GetCollisionEvents ( other, collisions );
		amountOfCollisions = collisions.Count;
		var maxIterations = Mathf.Min ( maximumCollisionsPerCallback, amountOfCollisions );

		for ( int i = 0; i < maxIterations; i++ )
		{
			var validCollision = true;

			if ( maximumTriggeringAngle != 0 )
				validCollision = Vector3.Angle ( collisions [ i ].normal, Vector3.up ) < maximumTriggeringAngle;



			if ( validCollision )
			{
				emitterParameters.position = collisions [ i ].intersection;
				subParticleSystem.Emit ( emitterParameters, 1 );
			}
		}

		currentCallbacksPerFrame++;
	}


	private void LateUpdate ( )
	{
		currentCallbacksPerFrame = 0;
	}

#if UNITY_EDITOR
	private void OnValidate ( )
	{
		if ( mainParticleSystem == null )
			mainParticleSystem = GetComponent<ParticleSystem> ( );
	}
#endif
}
