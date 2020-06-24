﻿using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using Net3dBool;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Transform Mesh")]
	public class TransformMesh : MixtureNode
	{
		[Input("Mesh")]
		public MixtureMesh inputMesh;
		[Input("Attribute")]
		public MixtureAttribute inputAttrib;

        public Vector3 pos;
        public Vector3 eulerAngles;
        public Vector3 scale = Vector3.one;
        
        public bool bakeIntoMesh = false;

        [Output("Output")]
        public MixtureMesh output;

		public override string	name => "Transform Mesh";

		public override bool    hasPreview => false;
		public override bool    showDefaultInspector => true;

		protected override void Enable()
		{
		}

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		// [CustomPortBehavior(nameof(inputMeshes))]
		// public IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		// {
        //     yield return new PortData
        //     {
        //         identifier = nameof(inputMeshes),
        //         displayName = "Input Meshes",
        //         allowMultiple = true,
        //         displayType()
        //     };
		// }

		// [CustomPortInput(nameof(inputMeshes), typeof(MixtureMesh))]
		// protected void GetMaterialInputs(List< SerializableEdge > edges)
		// {
        //     if (inputMeshes == null)
        //         inputMeshes = new List<MixtureMesh>();
        //     inputMeshes.Clear();
		// 	foreach (var edge in edges)
        //     {
        //         if (edge.passThroughBuffer is MixtureMesh m)
        //             inputMeshes.Add(m);
        //     }
		// }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (inputMesh == null || inputMesh.mesh == null)
                return false;

			output = inputMesh.Clone();

            if (inputAttrib != null)
            {
                // Try to get values from attribute in param:
                inputAttrib.TryGetValue("position", out var position);
                inputAttrib.TryGetValue("rotation", out var rotation);
                inputAttrib.TryGetValue("scale", out var scale);

                if (position == null)
                    position = Vector3.zero;
                if (rotation == null)
                    rotation = Quaternion.identity;
                if (scale == null)
                    scale = Vector3.one;

                output.localToWorld *= Matrix4x4.TRS((Vector3)position, (Quaternion)rotation, (Vector3)scale);

                // Is this needed ?
                // Solid s = new Solid(output.mesh.vertices, output.mesh.triangles, output.mesh.normals);
                // s.ApplyMatrix(output.localToWorld);
            }
            else
            {
                output.localToWorld = Matrix4x4.TRS(pos, Quaternion.Euler(eulerAngles), scale);
            }

            if (bakeIntoMesh)
            {
                var combine = new CombineInstance[1];
                combine[0].mesh = output.mesh;
                combine[0].transform = output.localToWorld;

                output.mesh = new Mesh();
                output.localToWorld = Matrix4x4.identity;
                output.mesh.CombineMeshes(combine);
            }

			return true;
		}
    }
}