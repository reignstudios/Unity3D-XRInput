using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reign.XR.Example
{
	public class TestVelocity : MonoBehaviour
	{
		public Rigidbody ballVel1, ballVel2, ballVel3;
		public Vector3 ballGrabOffset;

		public Transform rightHand, rightHandGrabPoint;
		public float graphMulX = 1, graphMulY = 10;
		public float graphOffsetX = -5, graphOffsetY = 1;
		public LineRenderer velLine1, velLine2, velLine3;
		public Transform throwVel1, throwVel2, throwVel3;

		private bool imuVelocityValid;
		private int velocitySampleCount;
		private const int maxSampleLength = 2048;
		private Vector3[] velocitySamples1, velocitySamples2, velocitySamples3;

		private void Start()
		{
			lastGrabBallPos = rightHand.position;
			lastGrabBallRotation = rightHand.rotation;

			Physics.IgnoreCollision(ballVel1.GetComponent<SphereCollider>(), ballVel2.GetComponent<SphereCollider>());
			Physics.IgnoreCollision(ballVel1.GetComponent<SphereCollider>(), ballVel3.GetComponent<SphereCollider>());
			Physics.IgnoreCollision(ballVel2.GetComponent<SphereCollider>(), ballVel3.GetComponent<SphereCollider>());

			velocitySamples1 = new Vector3[maxSampleLength];
			velocitySamples2 = new Vector3[maxSampleLength];
			velocitySamples3 = new Vector3[maxSampleLength];

			velLine1.startWidth = .05f;
			velLine1.endWidth = .05f;
			velLine1.enabled = false;

			velLine2.startWidth = .05f;
			velLine2.endWidth = .05f;
			velLine2.enabled = false;

			velLine3.startWidth = .05f;
			velLine3.endWidth = .05f;
			velLine3.enabled = false;

			throwVel1.gameObject.SetActive(false);
			throwVel2.gameObject.SetActive(false);
			throwVel3.gameObject.SetActive(false);
		}

		private void Update()
		{
			// grab velocity
			XRInput.Velocity(XRController.Right, out var imuLinearVel, out var imuAngularVel, out bool imuLinearVelValid, out bool imuAngularVelValid);
			imuVelocityValid = imuLinearVelValid && imuAngularVelValid;

			var vel1 = XRInput.GetVelocityAtOffset(imuLinearVel, imuAngularVel, rightHand.position, rightHandGrabPoint.position);
			var vel2 = XRInput.GetVelocityAtOffset2(imuLinearVel, imuAngularVel, rightHand.position, rightHandGrabPoint.position);
			GetCurrentAndLastPosVelocity(out var vel3, out var angularVel3);

			// reset velocity samples
			var button = XRInput.ButtonTrigger(XRController.Right);
			if (button.down)
			{
				velocitySampleCount = 0;
				velLine1.enabled = true;
				velLine2.enabled = true;
				velLine3.enabled = true;
			}

			// sample velocity length
			if (button.on || button.up)
			{
				velocitySamples1[velocitySampleCount] = new Vector3((velocitySampleCount * graphMulX) + graphOffsetX, (vel1.magnitude * graphMulY) + graphOffsetY, 5.9f);
				velocitySamples2[velocitySampleCount] = new Vector3((velocitySampleCount * graphMulX) + graphOffsetX, (vel2.magnitude * graphMulY) + graphOffsetY, 5.8f);
				velocitySamples3[velocitySampleCount] = new Vector3((velocitySampleCount * graphMulX) + graphOffsetX, (vel3.magnitude * graphMulY) + graphOffsetY, 5.7f);

				velocitySampleCount++;
				if (velocitySampleCount > maxSampleLength) velocitySampleCount = maxSampleLength;

				// snap balls to grab point
				SnapBallTransform();
				ballVel1.velocity = Vector3.zero;
				ballVel1.angularVelocity = Vector3.zero;
				ballVel2.velocity = Vector3.zero;
				ballVel2.angularVelocity = Vector3.zero;
				ballVel3.velocity = Vector3.zero;
				ballVel3.angularVelocity = Vector3.zero;
			}

			// update line
			if (button.up)
			{
				velLine1.positionCount = velocitySampleCount;
				velLine1.SetPositions(velocitySamples1);

				velLine2.positionCount = velocitySampleCount;
				velLine2.SetPositions(velocitySamples2);

				velLine3.positionCount = velocitySampleCount;
				velLine3.SetPositions(velocitySamples3);

				// throw balls
				SnapBallTransform();

				ballVel1.velocity = vel1;
				ballVel1.angularVelocity = imuAngularVel;

				ballVel2.velocity = vel2;
				ballVel2.angularVelocity = imuAngularVel;

				ballVel3.velocity = vel3;
				ballVel3.angularVelocity = angularVel3;

				// set velocity vector visuals
				throwVel1.gameObject.SetActive(true);
				throwVel2.gameObject.SetActive(true);
				throwVel3.gameObject.SetActive(true);
				throwVel1.localScale = new Vector3(1, 1, vel1.magnitude * 0.1f);
				throwVel2.localScale = new Vector3(1, 1, vel2.magnitude * 0.1f);
				throwVel3.localScale = new Vector3(1, 1, vel3.magnitude * 0.1f);
				throwVel1.position = rightHandGrabPoint.position;
				throwVel2.position = rightHandGrabPoint.position;
				throwVel3.position = rightHandGrabPoint.position;
				throwVel1.rotation = Quaternion.LookRotation(vel1);
				throwVel2.rotation = Quaternion.LookRotation(vel2);
				throwVel3.rotation = Quaternion.LookRotation(vel3);
			}

			// show errors
			if (!imuVelocityValid)
			{
				velLine1.enabled = false;
				velLine2.enabled = true;
				// TODO: display text
			}
			else
			{
				velLine1.enabled = true;
				velLine2.enabled = true;
			}
		}

		private void SnapBallTransform()
        {
			ballVel1.position = rightHandGrabPoint.position;
			ballVel1.rotation = rightHandGrabPoint.rotation;
			ballVel1.transform.position = rightHandGrabPoint.position;
			ballVel1.transform.rotation = rightHandGrabPoint.rotation;

			ballVel2.position = rightHandGrabPoint.position;
			ballVel2.rotation = rightHandGrabPoint.rotation;
			ballVel2.transform.position = rightHandGrabPoint.position;
			ballVel2.transform.rotation = rightHandGrabPoint.rotation;

			ballVel3.position = rightHandGrabPoint.position;
			ballVel3.rotation = rightHandGrabPoint.rotation;
			ballVel3.transform.position = rightHandGrabPoint.position;
			ballVel3.transform.rotation = rightHandGrabPoint.rotation;
		}

		private Vector3 lastGrabBallPos;
		private Quaternion lastGrabBallRotation;
		private void GetCurrentAndLastPosVelocity(out Vector3 linearVelocity, out Vector3 angularVelocity)
		{
			// linear
			linearVelocity = (rightHandGrabPoint.position - lastGrabBallPos) / Time.deltaTime;
			lastGrabBallPos = rightHandGrabPoint.position;

			// angular
			(Quaternion.Inverse(lastGrabBallRotation) * rightHandGrabPoint.rotation).ToAngleAxis(out float angle, out angularVelocity);
			angularVelocity *= angle / Time.deltaTime;
			lastGrabBallRotation = rightHandGrabPoint.rotation;
		}
	}
}