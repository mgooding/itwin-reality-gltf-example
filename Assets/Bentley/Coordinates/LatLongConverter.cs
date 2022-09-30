/*---------------------------------------------------------------------------------------------
* Copyright (c) Bentley Systems, Incorporated. All rights reserved.
* See LICENSE.md in the project root for license terms and full copyright notice.
*--------------------------------------------------------------------------------------------*/

namespace Bentley.Coordinates
{
    // Maps to https://www.itwinjs.org/reference/imodeljs-common/geometry/cartographic/
    // But stored in degrees instead of radians
    public struct LatLongPoint
    {
        public double Latitude;
        public double Longitude;
        public double Height;

        public override string ToString()
        {
            return $"Latitude: {Latitude:G15} Longitude: {Longitude:G15} Height: {Height:G15})";
        }

        public bool Equals(LatLongPoint rhs)
        {
            return Latitude == rhs.Latitude && Longitude == rhs.Longitude && Height == rhs.Height;
        }
    }

    public static class LatLongConverter
    {
        // Port of Cartographic.fromEcef from iTwin.js
        // https://github.com/imodeljs/imodeljs/blob/master/core/common/src/geometry/Cartographic.ts
        public static bool TryEcefToLatLong(EcefPoint ecefPoint, out LatLongPoint latLongPoint)
        {
            latLongPoint = new LatLongPoint { Latitude = 0, Longitude = 0, Height = 0 };

            // Non-normalized vector between ECEF point and cartographic point
            double surfX, surfY, surfZ;

            if (!TryScaleEcefToGeodeticSurface(ecefPoint, out surfX, out surfY, out surfZ))
                return false;

            // Normalized vector between ECEF point and cartographic point
            double surfNormX = surfX * Wgs84OneOverEquatorRadiusSquared;
            double surfNormY = surfY * Wgs84OneOverEquatorRadiusSquared;
            double surfNormZ = surfZ * Wgs84OneOverPolarRadiusSquared;

            double surfNormMag = System.Math.Sqrt(surfNormX * surfNormX + surfNormY * surfNormY + surfNormZ * surfNormZ);
            surfNormX /= surfNormMag;
            surfNormY /= surfNormMag;
            surfNormZ /= surfNormMag;

            latLongPoint.Longitude = RadiansToDegrees(System.Math.Atan2(surfNormY, surfNormX));
            latLongPoint.Latitude = RadiansToDegrees(System.Math.Asin(surfNormZ));

            double hX = ecefPoint.x - surfX;
            double hY = ecefPoint.y - surfY;
            double hZ = ecefPoint.z - surfZ;
            double aboveSurfaceSign = System.Math.Sign(hX * ecefPoint.x + hY * ecefPoint.y + hZ * ecefPoint.z);

            latLongPoint.Height = aboveSurfaceSign * System.Math.Sqrt(hX * hX + hY * hY + hZ * hZ);

            return true;
        }

        // Port of Cartographic.toEcef from iTwin.js
        // https://github.com/imodeljs/imodeljs/blob/master/core/common/src/geometry/Cartographic.ts
        public static EcefPoint LatLongToEcef(LatLongPoint latLongPoint)
        {
            double latRadians = DegreesToRadians(latLongPoint.Latitude);
            double longRadians = DegreesToRadians(latLongPoint.Longitude);

            double cosLatitude = System.Math.Cos(latRadians);
            double nX = cosLatitude * System.Math.Cos(longRadians);
            double nY = cosLatitude * System.Math.Sin(longRadians);
            double nZ = System.Math.Sin(latRadians);

            double nMag = System.Math.Sqrt(nX * nX + nY * nY + nZ * nZ);
            nX /= nMag;
            nY /= nMag;
            nZ /= nMag;

            double kX = nX * Wgs84EquatorRadiusSquared;
            double kY = nY * Wgs84EquatorRadiusSquared;
            double kZ = nZ * Wgs84PolarRadiusSquared;

            double gamma = System.Math.Sqrt(nX * kX + nY * kY + nZ * kZ); 
            kX /= gamma;
            kY /= gamma;
            kZ /= gamma;

            nX *= latLongPoint.Height;
            nY *= latLongPoint.Height;
            nZ *= latLongPoint.Height;

            return new EcefPoint(kX + nX, kY + nY, kZ + nZ);
        }

        // Port from Cartographic.ts in iTwin.js
        private const double Wgs84EquatorRadius = 6378137.0;
        private const double Wgs84PolarRadius = 6356752.3142451793;
        private const double Wgs84EquatorRadiusSquared = Wgs84EquatorRadius * Wgs84EquatorRadius;
        private const double Wgs84PolarRadiusSquared = Wgs84PolarRadius * Wgs84PolarRadius;
        private const double Wgs84OneOverEquatorRadiusSquared = 1.0 / Wgs84EquatorRadiusSquared;
        private const double Wgs84OneOverPolarRadiusSquared = 1.0 / Wgs84PolarRadiusSquared;
        private const double Wgs84CenterToleranceSquared = 0.1;

        // Port of Cartographic._scaleToGeodeticSurface from iTwin.js
        private static bool TryScaleEcefToGeodeticSurface(EcefPoint ecefPoint, out double surfaceX, out double surfaceY, out double surfaceZ)
        {
            double x2 = ecefPoint.x * ecefPoint.x * Wgs84OneOverEquatorRadiusSquared;
            double y2 = ecefPoint.y * ecefPoint.y * Wgs84OneOverEquatorRadiusSquared;
            double z2 = ecefPoint.z * ecefPoint.z * Wgs84OneOverPolarRadiusSquared;

            // Compute the squared ellipsoid norm.
            double squaredNorm = x2 + y2 + z2;
            double ratio = System.Math.Sqrt(1.0 / squaredNorm);

            // As an initial approximation, assume that the radial intersection is the projection point.
            double intersectionX = ecefPoint.x * ratio;
            double intersectionY = ecefPoint.y * ratio;
            double intersectionZ = ecefPoint.z * ratio;

            // If the position is near the center, the iteration will not converge.
            if (squaredNorm < Wgs84CenterToleranceSquared)
            {
                if (double.IsInfinity(ratio) || double.IsNaN(ratio))
                {
                    surfaceX = surfaceY = surfaceZ = 0.0;
                    return false;
                }

                surfaceX = intersectionX;
                surfaceY = intersectionY;
                surfaceZ = intersectionZ;
                return true;
            }

            // Use the gradient at the intersection point in place of the true unit normal.
            // The difference in magnitude will be absorbed in the multiplier.
            double gradientX = intersectionX * Wgs84OneOverEquatorRadiusSquared * 2.0;
            double gradientY = intersectionY * Wgs84OneOverEquatorRadiusSquared * 2.0;
            double gradientZ = intersectionZ * Wgs84OneOverPolarRadiusSquared * 2.0;

            double ecefMag = System.Math.Sqrt(ecefPoint.x * ecefPoint.x + ecefPoint.y * ecefPoint.y + ecefPoint.z * ecefPoint.z);
            double gradientMag = System.Math.Sqrt(gradientX * gradientX + gradientY * gradientY + gradientZ * gradientZ);

            // Compute the initial guess at the normal vector multiplier, lambda.
            double lambda = (1.0 - ratio) * ecefMag / (0.5 * gradientMag);
            double correction = 0.0;
            double func, xMultiplier, yMultiplier, zMultiplier;

            do
            {
                lambda -= correction;

                xMultiplier = 1.0 / (1.0 + lambda * Wgs84OneOverEquatorRadiusSquared);
                yMultiplier = 1.0 / (1.0 + lambda * Wgs84OneOverEquatorRadiusSquared);
                zMultiplier = 1.0 / (1.0 + lambda * Wgs84OneOverPolarRadiusSquared);

                double xMultiplier2 = xMultiplier * xMultiplier;
                double yMultiplier2 = yMultiplier * yMultiplier;
                double zMultiplier2 = zMultiplier * zMultiplier;

                double xMultiplier3 = xMultiplier2 * xMultiplier;
                double yMultiplier3 = yMultiplier2 * yMultiplier;
                double zMultiplier3 = zMultiplier2 * zMultiplier;

                func = x2 * xMultiplier2 + y2 * yMultiplier2 + z2 * zMultiplier2 - 1.0;

                // "denominator" here refers to the use of this expression in the velocity and acceleration
                // computations in the sections to follow.
                double denominator = x2 * xMultiplier3 * Wgs84OneOverEquatorRadiusSquared +
                                     y2 * yMultiplier3 * Wgs84OneOverEquatorRadiusSquared +
                                     z2 * zMultiplier3 * Wgs84OneOverPolarRadiusSquared;

                double derivative = -2.0 * denominator;
                correction = func / derivative;
            }
            while (System.Math.Abs(func) > 0.01);

            surfaceX = ecefPoint.x * xMultiplier;
            surfaceY = ecefPoint.y * yMultiplier;
            surfaceZ = ecefPoint.z * zMultiplier;

            return true;
        }

        public static double DegreesToRadians(double degrees) { return degrees * System.Math.PI / 180.0; }

        // Port Angle.radiansToDegrees from iTwin.js
        // https://github.com/imodeljs/imodeljs/blob/master/core/geometry/src/geometry3d/Angle.ts
        private static double RadiansToDegrees(double radians)
        {
            if (radians < 0.0)
                return -RadiansToDegrees(-radians);
            // Now radians is positive ...
            const double pi = System.Math.PI;
            if (radians <= 0.25 * pi)
                return (180.0 / pi) * radians;
            if (radians < 0.75 * pi)
                return 90.0 + 180 * ((radians - 0.5 * pi) / pi);
            if (radians <= 1.25 * pi)
                return 180.0 + 180 * ((radians - pi) / pi);
            if (radians <= 1.75 * pi)
                return 270.0 + 180 * ((radians - 1.5 * pi) / pi);
            // all larger radians reference from 360 degrees (2PI)
            return 360.0 + 180 * ((radians - 2.0 * pi) / pi);
        }
    }

}
