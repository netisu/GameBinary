using System;
using System.Collections.Generic;
using Godot;
using Netisu.Datamodels;

namespace Netisu
{
    public sealed class MultiMeshInstance
    {
        public MultiMeshInstance3D Instance { get; init; } = null!;
        public int TotalInstances;
    }

    public partial class BatchRenderPart : Node
    {
        public static BatchRenderPart Instance { get; private set; } = null!;

        public Dictionary<string, MultiMeshInstance> MultiMeshInstances = null!;

        string map = "";

        public override void _EnterTree()
        {
            Instance = this;
            MultiMeshInstances = new()
            {
                { "Cube", new MultiMeshInstance { Instance = GetNodeOrNull<MultiMeshInstance3D>("CubeMultiMesh"), TotalInstances = 0 } },
                { "Sphere", new MultiMeshInstance { Instance = GetNodeOrNull<MultiMeshInstance3D>("SphereMultiMesh"), TotalInstances = 0 } },
                { "Cylinder", new MultiMeshInstance { Instance = GetNodeOrNull<MultiMeshInstance3D>("CylinderMultiMesh"), TotalInstances = 0 } },
                { "Wedge", new MultiMeshInstance { Instance = GetNodeOrNull<MultiMeshInstance3D>("WedgeMultiMesh"), TotalInstances = 0 } },
                { "Cone", new MultiMeshInstance { Instance = GetNodeOrNull<MultiMeshInstance3D>("ConeMultiMesh"), TotalInstances = 0 } },
            };
        }

        public int BuildPart(Part part)
        {
            if (!MultiMeshInstances.TryGetValue(part.Shape, out var mmInst))
                return -1;

            var mm = mmInst.Instance.Multimesh;

            if (mmInst.TotalInstances >= mm.InstanceCount)
                mm.InstanceCount = Math.Max(1, mm.InstanceCount * 2);

            int idx = mmInst.TotalInstances;
            mmInst.TotalInstances++;

            mm.VisibleInstanceCount = mmInst.TotalInstances;

            mm.SetInstanceTransform(idx, part.Container.GlobalTransform);
            mm.SetInstanceColor(idx, new Color(part.Color.r, part.Color.g,
                                               part.Color.b, part.Transparency));

            return idx;
        }


        public void RemovePart(int idx, string partShape)
        {

            if (!MultiMeshInstances.TryGetValue(partShape, out MultiMeshInstance multiMeshInstance))
                return;

            multiMeshInstance.Instance.Multimesh.SetInstanceTransform(idx, new());
            multiMeshInstance.TotalInstances--;


        }

        public void UpdateTransform(int idx, Transform3D transform3D, string shape)
        {
            if (!MultiMeshInstances.TryGetValue(shape, out MultiMeshInstance multiMeshInstance))
            {
                return;
            }

            multiMeshInstance.Instance.Multimesh.SetInstanceTransform(idx, transform3D);
        }

        public void UpdateColor(int idx, Color color, string shape)
        {
            if (!MultiMeshInstances.TryGetValue(shape, out MultiMeshInstance multiMeshInstance))
            {
                return;
            }

            if (idx > multiMeshInstance.TotalInstances || idx < 0)
                return;

            multiMeshInstance.Instance.Multimesh.SetInstanceColor(idx, color);
        }
    }
}