// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

public class Pvr_UnitySDKPose
{
    /************************************    left hand to right hand  *************************/
    protected static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));
    public Matrix4x4 RightHandedMatrix
    {
        get
        {
            return flipZ * Matrix * flipZ;
        }
    }

    /************************************    Properties  *************************************/
    #region Properties
    public Vector3 Position { get; protected set; }
    public Quaternion Orientation { get; protected set; }
    public Matrix4x4 Matrix { get; protected set; }
    #endregion

    /************************************  Pose Interfaces **********************************/
    #region Interfaces
    public Pvr_UnitySDKPose(Matrix4x4 matrix)
    {
        Set(matrix);
    }

    public Pvr_UnitySDKPose(Vector3 position, Quaternion orientation)
    {
        Set(position, orientation);
    }

    public void Set(Vector3 position, Quaternion orientation)
    {
        Position = position;
        Orientation = orientation;
        Matrix = Matrix4x4.TRS(position, orientation, Vector3.one);
    }

    protected void Set(Matrix4x4 matrix)
    {
        Matrix = matrix;
        Position = matrix.GetColumn(3);
        Orientation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
    }

    private Quaternion NormalizeQuaternion(ref Quaternion q)
    {
        float sum = 0;
        for (int i = 0; i < 4; ++i)
        {
            sum += q[i] * q[i];
        }

        float magnitudeInverse = 1 / Mathf.Sqrt(sum);
        for (int i = 0; i < 4; ++i)
        {
            q[i] *= magnitudeInverse;
        }
        return q;
    }

    #endregion  
}
