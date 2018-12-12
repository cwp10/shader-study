using UnityEngine;
using UnityEditor;
using Vax.VertexPainter;

[CustomEditor (typeof(ObjectProperties))]
public class ObjectPropertiesEditor : Editor {

	ObjectProperties castedTarget;

	void OnEnable()
	{
		castedTarget = target as ObjectProperties;
		castedTarget.GetComponent<ObjectProperties>().hideFlags = HideFlags.HideInInspector;
	}
	
	static public void UninstallIt(){
		
		ObjectProperties[] objects = FindObjectsOfType(typeof(ObjectProperties)) as ObjectProperties[];
		foreach (ObjectProperties op in objects) {
			DestroyImmediate(op);
		}
		
	}
}