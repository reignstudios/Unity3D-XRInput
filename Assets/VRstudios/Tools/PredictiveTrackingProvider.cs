using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;

public class PredictiveTrackingProvider : BasePoseProvider
{
	public override PoseDataFlags GetPoseFromProvider(out Pose output)
	{
		return base.GetPoseFromProvider(out output);
	}

	public override bool TryGetPoseFromProvider(out Pose output)
	{
		return GetPoseFromProvider(out output) != PoseDataFlags.NoData;
	}
}
