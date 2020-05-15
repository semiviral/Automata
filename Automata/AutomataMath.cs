#region

using System;
using System.Collections.Generic;
using System.Numerics;
using Automata.Core.Components;
using Automata.Numerics;

#endregion

// ReSharper disable InconsistentNaming

namespace Automata
{
    public class AutomataMath
    {
        public static float UnLerp(float a, float b, float interpolant) => (interpolant - a) / (b - a);

        public static float ToRadians(float degrees) => degrees * ((float)Math.PI / 180f);
        public static Vector3 ToRadians(Vector3 degrees) => new Vector3(ToRadians(degrees.X), ToRadians(degrees.Y), ToRadians(degrees.Z));

        public static Matrix4x4 MatrixFromTranslationAndRotationWithScaleToView(float scale, Translation translation, Rotation rotation) =>
            Matrix4x4.Identity
            * Matrix4x4.CreateScale(scale)
            * Matrix4x4.CreateTranslation(Vector3d.AsVector3(translation.Value))
            * Matrix4x4.CreateFromQuaternion(rotation.Value);

        public static IEnumerable<float> UnrollMatrix4x4(Matrix4x4 matrix)
        {
            yield return matrix.M11;
            yield return matrix.M12;
            yield return matrix.M13;
            yield return matrix.M14;
            yield return matrix.M21;
            yield return matrix.M22;
            yield return matrix.M23;
            yield return matrix.M24;
            yield return matrix.M31;
            yield return matrix.M32;
            yield return matrix.M33;
            yield return matrix.M34;
            yield return matrix.M41;
            yield return matrix.M42;
            yield return matrix.M43;
            yield return matrix.M44;
        }

        public static IEnumerable<float> UnrollVector3(Vector3 vector3)
        {
            yield return vector3.X;
            yield return vector3.Y;
            yield return vector3.Z;
        }

        public static int Wrap(int v, int delta, int minVal, int maxVal)
        {
            int mod = (maxVal + 1) - minVal;
            v += delta - minVal;
            v += (1 - (v / mod)) * mod;
            return (v % mod) + minVal;
        }
    }
}
