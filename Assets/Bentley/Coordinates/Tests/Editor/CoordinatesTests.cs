/*---------------------------------------------------------------------------------------------
* Copyright (c) Bentley Systems, Incorporated. All rights reserved.
* See LICENSE.md in the project root for license terms and full copyright notice.
*--------------------------------------------------------------------------------------------*/

using NUnit.Framework;
using UnityEngine;

namespace Bentley.Coordinates.Tests
{
    public class CoordinatesTests
    {
        [Test]
        public void EcefConverterIsInvalidOnBadEcefTransform()
        {
            var nullMatrixConverter = new EcefConverter(null);
            Assert.IsFalse(nullMatrixConverter.IsValid, "EcefConverter with null gltfMatrix shouldn't be valid");

            var shortMatrixConverter = new EcefConverter(new [] { 1.0 });
            Assert.IsFalse(shortMatrixConverter.IsValid, "EcefConverter with short gltfMatrix shouldn't be valid");

            double[] identityMatrix = { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0 };
            var identityMatrixConverter = new EcefConverter(identityMatrix);
            Assert.IsFalse(identityMatrixConverter.IsValid, "EcefConverter with identity gltfMatrix shouldn't be valid");
        }

        // Epsilon matches are expected given a small amount of precision loss due to extra transformations.
        const double EcefDelta = 0.01;
        const double LatLongDelta = 1.0E-9;

        [Test]
        public void LatLongToEcefMatchesExpectedOutputs()
        {
            var extonCampusLLPoint = new LatLongPoint { Latitude = 40.065422212104785, Longitude = -75.68705576058757, Height = 129.74222580583697 };
            var extonCampusEcefPointExpected = new EcefPoint(1208434.44796148, -4736406.23988482, 4083631.09423245);
            EcefPoint extonCampusEcefPointActual = LatLongConverter.LatLongToEcef(extonCampusLLPoint);
            Assert.AreEqual(extonCampusEcefPointExpected.x, extonCampusEcefPointActual.x, EcefDelta, "extonCampus EcefPoint.x does not match");
            Assert.AreEqual(extonCampusEcefPointExpected.y, extonCampusEcefPointActual.y, EcefDelta, "extonCampus EcefPoint.y does not match");
            Assert.AreEqual(extonCampusEcefPointExpected.z, extonCampusEcefPointActual.z, EcefDelta, "extonCampus EcefPoint.z does not match");

            var extonEcefTransform = new[] { 0.9689597103374167, 0.24721868809382036, 0, 0, 0.18919880602789899, -0.741554053613476, 0.6436624871522647, 0, 0.1591253956489884, -0.6236830171061196, -0.7653094816039853, 0, 1208434.4479614785, -4736406.239884817, 4083631.094232447, 1 };
            var extonCampusEcefConverter = new EcefConverter(extonEcefTransform);
            Vector3 extonCampusUnityPointActual = extonCampusEcefConverter.EcefToUnity(extonCampusEcefPointActual);
            Assert.AreEqual(0.0f, extonCampusUnityPointActual.x, 1.0E-6, "extonCampus UnityPoint.x does not match");
            Assert.AreEqual(0.0f, extonCampusUnityPointActual.y, 1.0E-6, "extonCampus UnityPoint.y does not match");
            Assert.AreEqual(0.0f, extonCampusUnityPointActual.z, 1.0E-6, "extonCampus UnityPoint.z does not match");

            var boulieuLLPoint = new LatLongPoint { Latitude = 45.26511332264151, Longitude = 4.66876105765112, Height = 425.2623529129733 };
            var boulieuEcefPointExpected = new EcefPoint(4482086.969785204, 366034.48463337176, 4508435.856557773);
            EcefPoint boulieuEcefPointActual = LatLongConverter.LatLongToEcef(boulieuLLPoint);
            Assert.AreEqual(boulieuEcefPointExpected.x, boulieuEcefPointActual.x, EcefDelta, "boulieu EcefPoint.x does not match");
            Assert.AreEqual(boulieuEcefPointExpected.y, boulieuEcefPointActual.y, EcefDelta, "boulieu EcefPoint.y does not match");
            Assert.AreEqual(boulieuEcefPointExpected.z, boulieuEcefPointActual.z, EcefDelta, "boulieu EcefPoint.z does not match");

            var boulieuEcefTransform = new[] { -0.08139589142428175, 0.9966818493678143, 0, 0, 0.701491407076031, 0.057288610645056165, 0.7103716076032667, 0, 0.7080144876044111, 0.057821330243367976, -0.7038268104520817, 0, 4482086.9714727765, 366034.4847711891, 4508435.858255264, 1 };
            var boulieuEcefConverter = new EcefConverter(boulieuEcefTransform);
            Vector3 boulieuUnityPointActual = boulieuEcefConverter.EcefToUnity(boulieuEcefPointActual);
            Assert.AreEqual(0.0f, boulieuUnityPointActual.x, 1.0E-6, "boulieu UnityPoint.x does not match");
            Assert.AreEqual(0.0f, boulieuUnityPointActual.y, 1.0E-6, "boulieu UnityPoint.y does not match");
            Assert.AreEqual(0.0f, boulieuUnityPointActual.z, 1.0E-6, "boulieu UnityPoint.z does not match");

            var tokyoLLPoint = new LatLongPoint { Latitude = 32.6820226055114, Longitude = 133.43776232199872, Height = 201.39803754192533 };
            var tokyoEcefPointExpected = new EcefPoint(-3694821.6805815096, 3902009.683775527, 3424439.1912230984);
            EcefPoint tokyoEcefPointActual = LatLongConverter.LatLongToEcef(tokyoLLPoint);
            Assert.AreEqual(tokyoEcefPointExpected.x, tokyoEcefPointActual.x, EcefDelta, "tokyo EcefPoint.x does not match");
            Assert.AreEqual(tokyoEcefPointExpected.y, tokyoEcefPointActual.y, EcefDelta, "tokyo EcefPoint.y does not match");
            Assert.AreEqual(tokyoEcefPointExpected.z, tokyoEcefPointActual.z, EcefDelta, "tokyo EcefPoint.z does not match");

            var tokyoEcefTransform = new[] { -0.7261211289374035, -0.6875668011987421, 5.5084127901019255e-11, 0, -0.5787111043584399, 0.6111615041949102, 0.5399769193979032, 0, -0.37127020322523263, 0.39208865028146905, -0.8416798242310138, 0, -3694821.681124058, 3902009.684348498, 3424439.191725944, 1 };
            var tokyoEcefConverter = new EcefConverter(tokyoEcefTransform);
            Vector3 tokyoUnityPointActual = tokyoEcefConverter.EcefToUnity(tokyoEcefPointActual);
            Assert.AreEqual(0.0f, tokyoUnityPointActual.x, 1.0E-6, "tokyo UnityPoint.x does not match");
            Assert.AreEqual(0.0f, tokyoUnityPointActual.y, 1.0E-6, "tokyo UnityPoint.y does not match");
            Assert.AreEqual(0.0f, tokyoUnityPointActual.z, 1.0E-6, "tokyo UnityPoint.z does not match");
        }

        [Test]
        public void UnityToEcefToLatLongMatchesExpectedOutputs()
        {
            // These tests validate the combined EcefConverter and LatLongConverter logic with known inputs and outputs from iTwin.js.

            // EXTON
            var extonEcefTransform = new[] { 0.9689597103374167, 0.24721868809382036, 0, 0, 0.18919880602789899, -0.741554053613476, 0.6436624871522647, 0, 0.1591253956489884, -0.6236830171061196, -0.7653094816039853, 0, 1208434.4479614785, -4736406.239884817, 4083631.094232447, 1 };
            var extonCampusEcefConverter = new EcefConverter(extonEcefTransform);
            Assert.IsTrue(extonCampusEcefConverter.IsValid, "extonCampusEcefConverter is not valid");

            var extonCampusEcefPointExpected = new EcefPoint(1208434.44796148, -4736406.23988482, 4083631.09423245);
            EcefPoint extonCampusEcefPointActual = extonCampusEcefConverter.UnityToEcef(Vector3.zero);
            Assert.AreEqual(extonCampusEcefPointExpected.x, extonCampusEcefPointActual.x, EcefDelta, "extonCampus EcefPoint does not match");
            Assert.AreEqual(extonCampusEcefPointExpected.y, extonCampusEcefPointActual.y, EcefDelta, "extonCampus EcefPoint does not match");
            Assert.AreEqual(extonCampusEcefPointExpected.z, extonCampusEcefPointActual.z, EcefDelta, "extonCampus EcefPoint does not match");

            var extonCampusLLPointExpected = new LatLongPoint { Latitude = 40.065422212104785, Longitude = -75.68705576058757, Height = 129.74222580583697 };
            bool extonCampusConvertSuccess = LatLongConverter.TryEcefToLatLong(extonCampusEcefPointActual, out LatLongPoint extonCampusLLPointActual);
            Assert.IsTrue(extonCampusConvertSuccess, "extonCampus LatLong conversion failed");
            Assert.AreEqual(extonCampusLLPointExpected.Latitude, extonCampusLLPointActual.Latitude, LatLongDelta, "extonCampus LatLongPoint does not match");
            Assert.AreEqual(extonCampusLLPointExpected.Longitude, extonCampusLLPointActual.Longitude, LatLongDelta, "extonCampus LatLongPoint does not match");
            Assert.AreEqual(extonCampusLLPointExpected.Height, extonCampusLLPointActual.Height, EcefDelta, "extonCampus LatLongPoint does not match");

            // BOULIEU
            var boulieuEcefTransform = new[] { -0.08139589142428175, 0.9966818493678143, 0, 0, 0.701491407076031, 0.057288610645056165, 0.7103716076032667, 0, 0.7080144876044111, 0.057821330243367976, -0.7038268104520817, 0, 4482086.9714727765, 366034.4847711891, 4508435.858255264, 1 };
            var boulieuEcefConverter = new EcefConverter(boulieuEcefTransform);
            Assert.IsTrue(boulieuEcefConverter.IsValid, "boulieuEcefConverter is not valid");

            var boulieuEcefPointExpected = new EcefPoint(4482086.969785204, 366034.48463337176, 4508435.856557773);
            EcefPoint boulieuEcefPointActual = boulieuEcefConverter.UnityToEcef(Vector3.zero);
            Assert.AreEqual(boulieuEcefPointExpected.x, boulieuEcefPointActual.x, EcefDelta, "boulieu EcefPoint does not match");
            Assert.AreEqual(boulieuEcefPointExpected.y, boulieuEcefPointActual.y, EcefDelta, "boulieu EcefPoint does not match");
            Assert.AreEqual(boulieuEcefPointExpected.z, boulieuEcefPointActual.z, EcefDelta, "boulieu EcefPoint does not match");

            var boulieuLLPointExpected = new LatLongPoint { Latitude = 45.26511332264151, Longitude = 4.66876105765112, Height = 425.2623529129733 };
            bool boulieuConvertSuccess = LatLongConverter.TryEcefToLatLong(boulieuEcefPointActual, out LatLongPoint boulieuLLPointActual);
            Assert.IsTrue(boulieuConvertSuccess, "boulieu LatLong conversion failed");
            Assert.AreEqual(boulieuLLPointExpected.Latitude, boulieuLLPointActual.Latitude, LatLongDelta, "boulieu LatLongPoint does not match");
            Assert.AreEqual(boulieuLLPointExpected.Longitude, boulieuLLPointActual.Longitude, LatLongDelta, "boulieu LatLongPoint does not match");
            Assert.AreEqual(boulieuLLPointExpected.Height, boulieuLLPointActual.Height, EcefDelta, "boulieu LatLongPoint does not match");

            // TOKYO
            var tokyoEcefTransform = new[] { -0.7261211289374035, -0.6875668011987421, 5.5084127901019255e-11, 0, -0.5787111043584399, 0.6111615041949102, 0.5399769193979032, 0, -0.37127020322523263, 0.39208865028146905, -0.8416798242310138, 0, -3694821.681124058, 3902009.684348498, 3424439.191725944, 1 };
            var tokyoEcefConverter = new EcefConverter(tokyoEcefTransform);
            Assert.IsTrue(tokyoEcefConverter.IsValid, "tokyoEcefConverter is not valid");

            var tokyoEcefPointExpected = new EcefPoint(-3694821.6805815096, 3902009.683775527, 3424439.1912230984);
            EcefPoint tokyoEcefPointActual = tokyoEcefConverter.UnityToEcef(Vector3.zero);
            Assert.AreEqual(tokyoEcefPointExpected.x, tokyoEcefPointActual.x, EcefDelta, "tokyo EcefPoint does not match");
            Assert.AreEqual(tokyoEcefPointExpected.y, tokyoEcefPointActual.y, EcefDelta, "tokyo EcefPoint does not match");
            Assert.AreEqual(tokyoEcefPointExpected.z, tokyoEcefPointActual.z, EcefDelta, "tokyo EcefPoint does not match");

            var tokyoLLPointExpected = new LatLongPoint { Latitude = 32.6820226055114, Longitude = 133.43776232199872, Height = 201.39803754192533 };
            bool tokyoConvertSuccess = LatLongConverter.TryEcefToLatLong(tokyoEcefPointActual, out LatLongPoint tokyoLLPointActual);
            Assert.IsTrue(tokyoConvertSuccess, "tokyo LatLong conversion failed");
            Assert.AreEqual(tokyoLLPointExpected.Latitude, tokyoLLPointActual.Latitude, LatLongDelta, "tokyo LatLongPoint does not match");
            Assert.AreEqual(tokyoLLPointExpected.Longitude, tokyoLLPointActual.Longitude, LatLongDelta, "tokyo LatLongPoint does not match");
            Assert.AreEqual(tokyoLLPointExpected.Height, tokyoLLPointActual.Height, EcefDelta, "tokyo LatLongPoint does not match");
        }
    }
}
