/*---------------------------------------------------------------------------------------------
* Copyright (c) Bentley Systems, Incorporated. All rights reserved.
* See LICENSE.md in the project root for license terms and full copyright notice.
*--------------------------------------------------------------------------------------------*/

using UnityEngine;

namespace Bentley.Coordinates
{
    // Maps to https://www.itwinjs.org/reference/imodeljs-common/imodels/eceflocation/
    public struct EcefPoint
    {
        public double x;
        public double y;
        public double z;

        public EcefPoint(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return $"({x:G15}, {y:G15}, {z:G15})";
        }

        public bool Equals(EcefPoint rhs)
        {
            return x == rhs.x && y == rhs.y && z == rhs.z;
        }
    }

    public class EcefConverter
    {
        private readonly double m00;
        private readonly double m10;
        private readonly double m20;
        private readonly double m01;
        private readonly double m11;
        private readonly double m21;
        private readonly double m02;
        private readonly double m12;
        private readonly double m22;
        private readonly double m03;
        private readonly double m13;
        private readonly double m23;

        private readonly double inverseM00;
        private readonly double inverseM10;
        private readonly double inverseM20;
        private readonly double inverseM01;
        private readonly double inverseM11;
        private readonly double inverseM21;
        private readonly double inverseM02;
        private readonly double inverseM12;
        private readonly double inverseM22;

        public bool IsValid { get; }

        public EcefConverter(double[] gltfMatrix)
        {
            if (gltfMatrix == null)
            {
                Debug.LogWarning("EcefConverter initialized with null gltfMatrix");
                IsValid = false;
                return;
            }

            if (gltfMatrix.Length != 16)
            {
                Debug.LogWarning("EcefConverter initialized with non 4x4 gltfMatrix");
                IsValid = false;
                return;
            }

            if (IsIdentityGltfMatrix(gltfMatrix))
            {
                Debug.Log("EcefConverter initialized with identity matrix - source model was not geolocated");
                IsValid = false;
                return;
            }

            // Negate x-axis to convert left-handed Unity points to right-handed.
            // Full transform is then: Unity LHS, Y-up => GLTF RHS, Y-up => ECEF RHS, Z-up
            //
            // NOTE - this is correct for how glTFast converts from RHS to LHS, but other glTF importers
            // like Azure Remote Rendering might negate the Z-axis instead. This transformation must match that conversion.
            m00 = -gltfMatrix[0];
            m10 = -gltfMatrix[1];
            m20 = -gltfMatrix[2];
            m01 = gltfMatrix[4];
            m11 = gltfMatrix[5];
            m21 = gltfMatrix[6];
            m02 = gltfMatrix[8];
            m12 = gltfMatrix[9];
            m22 = gltfMatrix[10];
            m03 = gltfMatrix[12];
            m13 = gltfMatrix[13];
            m23 = gltfMatrix[14];

            // Hacky way of calculating inverse ecefTransform using EcefPoints as Vector3 with doubles.
            // Can't just use Matrix4x4.inverse because double precision is important here.
            var rowX = new EcefPoint(m00, m01, m02);
            var rowY = new EcefPoint(m10, m11, m12);
            var rowZ = new EcefPoint(m20, m21, m22);

            var crossXY = CrossProduct(rowX, rowY);
            var crossYZ = CrossProduct(rowY, rowZ);
            var crossZX = CrossProduct(rowZ, rowX);

            var determinant = rowX.x * crossYZ.x + rowX.y * crossYZ.y + rowX.z * crossYZ.z;

            inverseM00 = crossYZ.x / determinant;
            inverseM01 = crossZX.x / determinant;
            inverseM02 = crossXY.x / determinant;

            inverseM10 = crossYZ.y / determinant;
            inverseM11 = crossZX.y / determinant;
            inverseM12 = crossXY.y / determinant;

            inverseM20 = crossYZ.z / determinant;
            inverseM21 = crossZX.z / determinant;
            inverseM22 = crossXY.z / determinant;

            IsValid = true;
        }

        public EcefPoint UnityToEcef(Vector3 point)
        {
            return new EcefPoint
            {
                x = m00 * point.x + m01 * point.y + m02 * point.z + m03,
                y = m10 * point.x + m11 * point.y + m12 * point.z + m13,
                z = m20 * point.x + m21 * point.y + m22 * point.z + m23,
            };
        }

        public Vector3 EcefToUnity(EcefPoint point)
        {
            double pX = point.x - m03;
            double pY = point.y - m13;
            double pZ = point.z - m23;

            return new Vector3((float)(inverseM00 * pX + inverseM01 * pY + inverseM02 * pZ),
                               (float)(inverseM10 * pX + inverseM11 * pY + inverseM12 * pZ),
                               (float)(inverseM20 * pX + inverseM21 * pY + inverseM22 * pZ));
        }

        private static bool IsIdentityGltfMatrix(double[] gltfMatrix)
        {
            double[] identityMatrix = { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0 };
            // matching length already validated in earlier check
            for (int i = 0; i < gltfMatrix.Length; ++i)
            {
                // Exact comparison is fine. Identity matrix is a "magic" value set for holograms
                // where we attempted to calculate ecefTransform but found the model wasn't geocoordinated.
                if (gltfMatrix[i] != identityMatrix[i])
                    return false;
            }
            return true;
        }

        // not really EcefPoints, just reusing for a convenient Vector3 struct with doubles
        private static EcefPoint CrossProduct(EcefPoint a, EcefPoint b)
        {
            return new EcefPoint(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }
    }
}