using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightProbeGroup))]
public class LightProbeBaker : Editor
{
    private const float minProbeSpacing = 2f;
    private const int maxProbes = 10000;

    [MenuItem("CONTEXT/LightProbeGroup/Bake to Reflection Probe Bounds")]
    private static void BakeProbesToReflectionProbeBounds(MenuCommand command)
    {
        LightProbeGroup lightProbeGroup = (LightProbeGroup)command.context;
        ReflectionProbe reflectionProbe = lightProbeGroup.GetComponent<ReflectionProbe>();

        if (reflectionProbe == null)
        {
            Debug.LogWarning("No Reflection Probe found on the object. Please attach a Reflection Probe to use this tool.");
            return;
        }

        // Get the bounds and position of the reflection probe
        Vector3 probeSize = reflectionProbe.size;
        Vector3 probeCenter = reflectionProbe.transform.position + reflectionProbe.center;

        // Calculate the grid resolution based on the minimum spacing
        int gridResolutionX = Mathf.Max(2, Mathf.FloorToInt(probeSize.x / minProbeSpacing) + 1);
        int gridResolutionY = Mathf.Max(2, Mathf.FloorToInt(probeSize.y / minProbeSpacing) + 1);
        int gridResolutionZ = Mathf.Max(2, Mathf.FloorToInt(probeSize.z / minProbeSpacing) + 1);

        // Calculate total number of probes
        int totalProbes = gridResolutionX * gridResolutionY * gridResolutionZ;

        // Adjust grid resolution if exceeding max probe count
        if (totalProbes > maxProbes)
        {
            float scaleFactor = Mathf.Pow((float)maxProbes / totalProbes, 1f / 3f);
            gridResolutionX = Mathf.FloorToInt(gridResolutionX * scaleFactor);
            gridResolutionY = Mathf.FloorToInt(gridResolutionY * scaleFactor);
            gridResolutionZ = Mathf.FloorToInt(gridResolutionZ * scaleFactor);
        }

        // Recalculate the spacing based on the adjusted resolution
        float spacingX = probeSize.x / (gridResolutionX - 1);
        float spacingY = probeSize.y / (gridResolutionY - 1);
        float spacingZ = probeSize.z / (gridResolutionZ - 1);

        // Start point for the probes (bottom-front-left corner of the reflection probe box)
        Vector3 origin = probeCenter - (probeSize / 2);

        // Generate probe positions within the reflection probe bounds
        Vector3[] probePositions = new Vector3[gridResolutionX * gridResolutionY * gridResolutionZ];
        int index = 0;

        for (int x = 0; x < gridResolutionX; x++)
        {
            for (int y = 0; y < gridResolutionY; y++)
            {
                for (int z = 0; z < gridResolutionZ; z++)
                {
                    Vector3 position = origin + new Vector3(x * spacingX, y * spacingY, z * spacingZ);
                    probePositions[index] = position;
                    index++;
                }
            }
        }

        // Assign the positions to the Light Probe Group
        lightProbeGroup.probePositions = probePositions;

        // Mark the scene as dirty to save changes
        EditorUtility.SetDirty(lightProbeGroup);
        Debug.Log($"Light Probe Group baked to match the Reflection Probe bounds with spacing {minProbeSpacing} meters. Total probes: {index}");
    }
}
