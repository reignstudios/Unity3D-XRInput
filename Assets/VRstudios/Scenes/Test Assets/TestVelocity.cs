using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRstudios
{
	public class TestVelocity : MonoBehaviour
	{
		public Transform rightHand;
		public float graphMulX = 1, graphMulY = 10;
		public float graphOffsetX = -5, graphOffsetY = 1;
		public LineRenderer lineCurrentAndLastPos, lineIMU;

		private bool imuVelocityValid;
		private int velocitySampleCount;
		private const int maxSampleLength = 2048;
		private Vector3[] velocitySamplesCurrentAndLastPos, velocitySamplesIMU;

		private void Start()
		{
			lastPos = rightHand.position;

			velocitySamplesCurrentAndLastPos = new Vector3[maxSampleLength];
			velocitySamplesIMU = new Vector3[maxSampleLength];

			lineCurrentAndLastPos.startWidth = .1f;
			lineCurrentAndLastPos.endWidth = .1f;
			lineCurrentAndLastPos.enabled = false;

			lineIMU.startWidth = .05f;
			lineIMU.endWidth = .05f;
			lineIMU.enabled = false;
		}

		private void Update()
		{
			// grab velocity
			GetCurrentAndLastPosVelocity(out var velocityCurrentAndLastPos);
			XRInput.Velocity(XRController.Right, out var imuLinearVel, out var imuAngularVel, out bool imuLinearVelValid, out bool imuAngularVelValid);
			imuVelocityValid = imuLinearVelValid && imuAngularVelValid;

			// reset velocity samples
			var button = XRInput.ButtonTrigger(XRController.Right);
			if (button.down)
			{
				velocitySampleCount = 0;
				lineCurrentAndLastPos.enabled = true;
				lineIMU.enabled = true;
			}

			// sample velocity length
			if (button.on)
			{
				velocitySamplesCurrentAndLastPos[velocitySampleCount] = new Vector3((velocitySampleCount * graphMulX) + graphOffsetX, (velocityCurrentAndLastPos.magnitude * graphMulY) + graphOffsetY, 5.8f);
				velocitySamplesIMU[velocitySampleCount] = new Vector3((velocitySampleCount * graphMulX) + graphOffsetX, (imuLinearVel.magnitude * graphMulY) + graphOffsetY, 5.9f);

				velocitySampleCount++;
				if (velocitySampleCount > maxSampleLength) velocitySampleCount = maxSampleLength;
			}

			// update line
			if (button.up)
			{
				lineCurrentAndLastPos.positionCount = velocitySampleCount;
				lineCurrentAndLastPos.SetPositions(velocitySamplesCurrentAndLastPos);

				lineIMU.positionCount = velocitySampleCount;
				lineIMU.SetPositions(velocitySamplesIMU);
			}

			// show errors
			if (!imuVelocityValid)
			{
				lineIMU.enabled = false;
				// TODO: display text
			}
			else
			{
				lineIMU.enabled = true;
			}
		}

		private Vector3 lastPos;
		private void GetCurrentAndLastPosVelocity(out Vector3 velocity)
		{
			velocity = (rightHand.position - lastPos) / Time.deltaTime;
			lastPos = rightHand.position;
		}
	}
}