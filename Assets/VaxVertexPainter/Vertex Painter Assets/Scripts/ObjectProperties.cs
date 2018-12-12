using UnityEngine;
using System.Collections;
using Vax.VertexPainter;

namespace Vax.VertexPainter{
	public class ObjectProperties : MonoBehaviour {
		public Material originalMaterial;
		public bool foldout = false;
		public bool showingVertex = false;
		public bool showingRedColor = true;
		public bool showingGreenColor = true;
		public bool showingBlueColor = true;
		public bool showingAlphaColor = true;
		public Color[] vertexColors = new Color[]{Color.red, Color.green, Color.blue, new Color(0.85f, 0.85f, 0.85f, 1f)};
		public bool showingWireframe = true;
		public bool instance = false;
		public bool meshCreated = false;
		public bool prefabCreated = false;
		public string meshName;
		public int instanceID;
	}
}