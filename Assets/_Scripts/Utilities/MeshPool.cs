using UnityEngine;
using System.Collections.Generic;

public static class MeshPool {

	static Stack<Mesh> stack = new Stack<Mesh>();

	public static Mesh Get() {
		if (stack.Count > 0) {
			return stack.Pop();
		}
		return new Mesh();
	}

	public static void Add(Mesh mesh) {
		mesh.Clear();
		stack.Push(mesh);
	}
}