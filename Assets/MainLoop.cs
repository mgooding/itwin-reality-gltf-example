/*---------------------------------------------------------------------------------------------
* Copyright (c) Bentley Systems, Incorporated. All rights reserved.
* See LICENSE.md in the project root for license terms and full copyright notice.
*--------------------------------------------------------------------------------------------*/

using System;
using Bentley.Coordinates;
using UnityEngine;

public class MainLoop : MonoBehaviour
{
    public UserInput UserInputObject;

    private async void Start()
    {
        string gltfPath = GetExampleFilePath();

        var gltfImport = new GLTFast.GltfImport();
        bool success = await gltfImport.Load(gltfPath);
        if (!success)
            throw new Exception("Failed to load GLTF");

        var gltfGameObject = new GameObject(System.IO.Path.GetFileName(gltfPath));
        gltfImport.InstantiateMainScene(gltfGameObject.transform);

        double[] gltfMatrix = GltfTransformReader.ReadEcefTransform(gltfPath);
        var ecefConverter = new EcefConverter(gltfMatrix);

        EcefPoint ecefOrigin = ecefConverter.UnityToEcef(Vector3.zero);
        LatLongConverter.TryEcefToLatLong(ecefOrigin, out LatLongPoint latLongOrigin);
        Debug.Log($"Origin ECEF: {ecefOrigin}");
        Debug.Log($"Origin LatLong: {latLongOrigin}");
    }

    public static string GetExampleFilePath()
    {
        // Find the sample data included in the repository. Will only work in the editor.
        var projectRootDirectory = System.IO.Directory.GetParent(Application.dataPath);
        var exampleDataDirectory = System.IO.Path.Combine(projectRootDirectory.FullName, "example-data");

        return System.IO.Path.Combine(exampleDataDirectory, "schoolhouse.gltf");
    }
}
