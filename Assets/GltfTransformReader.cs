/*---------------------------------------------------------------------------------------------
* Copyright (c) Bentley Systems, Incorporated. All rights reserved.
* See LICENSE.md in the project root for license terms and full copyright notice.
*--------------------------------------------------------------------------------------------*/

using UnityEngine;

[System.Serializable]
public class GltfJson
{
    public GltfJsonExtras extras;
}

[System.Serializable]
public class GltfJsonExtras
{
    public double[] ecefTransform;
}

public static class GltfTransformReader
{
    public static double[] ReadEcefTransform(string gltfFilePath)
    {
        string gltfJsonText = System.IO.File.ReadAllText(gltfFilePath);
        var gltfJson = JsonUtility.FromJson<GltfJson>(gltfJsonText);
        return gltfJson.extras.ecefTransform;
    }
}
