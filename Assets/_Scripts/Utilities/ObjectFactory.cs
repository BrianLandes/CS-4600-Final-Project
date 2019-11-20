using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFactory {

	public static GameObject Folder(string folderName, Transform parentFolder) {
		GameObject emptyGameObject = new GameObject(folderName);
		emptyGameObject.transform.SetParent(parentFolder);
		emptyGameObject.transform.localPosition = Vector2.zero;
		return emptyGameObject;
	}
}
