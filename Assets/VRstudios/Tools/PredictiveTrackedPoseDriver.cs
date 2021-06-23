using UnityEngine;
using UnityEngine.SpatialTracking;

namespace VRstudios.Tools
{
	public enum PredictionMode
	{
		OnUpdate,
		BeforeRender
	}

	public class PredictiveTrackedPoseDriver : TrackedPoseDriver
	{
		struct PrevTransform
		{
			public Vector3 pos, posPredicted;
			public Quaternion rot, rotPredicted;
		}

		private PrevTransform prevTransform;
		private bool canUpdate;

		public bool enablePrediction = true;
		public PredictionMode mode = PredictionMode.BeforeRender;
		public float linearVelClampMul = 10, linearVelMul = 1;
		public float angularVelClampMul = .1f, angularVelMul = 1;

		protected override void Awake()
		{
			// make sure prev rotations start as identity
			prevTransform.rot = Quaternion.identity;
			prevTransform.rotPredicted = Quaternion.identity;

			base.Awake();
		}

		protected override void Update()
		{
			canUpdate = mode == PredictionMode.OnUpdate;
			base.Update();
			canUpdate = !canUpdate;
		}

		protected override void SetLocalTransform(Vector3 newPosition, Quaternion newRotation, PoseDataFlags poseFlags)
		{
			// calculate predictions
			if (enablePrediction)
			{
				if (canUpdate)// only update before render
				{
					if ((poseFlags & PoseDataFlags.Position) != 0)
					{
						// capture vel & store this pos
						var linearVel = newPosition - prevTransform.pos;
						prevTransform.pos = newPosition;

						// scale vel
						linearVel *= linearVelMul;

						// apply vel offset strong the faster we're moving
						newPosition = Vector3.Lerp(newPosition, newPosition + linearVel, Mathf.Min(linearVel.magnitude * linearVelClampMul, 1.0f));
						prevTransform.posPredicted = newPosition;// capture predicted pos
					}

					if ((poseFlags & PoseDataFlags.Rotation) != 0)
					{
						// capture vel & store this rot
						var angularVel = Quaternion.Inverse(newRotation) * prevTransform.rot;
						prevTransform.rot = newRotation;

						// scale vel
						angularVel.ToAngleAxis(out float angle, out var axis);
						angularVel = Quaternion.AngleAxis(angle * angularVelMul, axis);

						// apply vel offset strong the faster we're moving
						newRotation = Quaternion.Lerp(newRotation, angularVel * newRotation, Mathf.Min(angle * angularVelClampMul, 1.0f));
						prevTransform.rotPredicted = newRotation;// capture predicted rot
					}
				}
				else if (mode == PredictionMode.OnUpdate)
				{
					// apply predicted transform to object before drawing
					newPosition = prevTransform.posPredicted;
					newRotation = prevTransform.rotPredicted;
				}
			}

			// set modified transform
			base.SetLocalTransform(newPosition, newRotation, poseFlags);
		}
	}
}