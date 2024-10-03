using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;


/*
*   Many have found that nesting gameobjects massively impacts performance as opposed to keeping them at the root. 
*   However, hierarchical objects are very helpful for organization.
*   This script de-parents objects at build if their parent object has no components beyond the transform,
*   and the parent trasnform has default scale.
*   some additional customizable parameters have been added as well, such as tags and active state
*/
public class AtBuild_DeparentGameObjects : IProcessSceneWithReport
{
	//User-customizable variables:
	
	// stop at digging at GameObjects which are inactive by default.
	bool ignoreInactive = true; 
		
	// stop digging at objects which have one of the below tags
	List<string> UnsafeTags = new List<string> {
		}; 
	
	// stop digging at objects which DO NOT have one of the below tags
	List<string> SafeTags = new List<string> {
		"Untagged"
		}; 
	
	
    // set callbackOrder so that de-parenting happens safely before the build
    public int callbackOrder => 0;

    // This method is called during the build process for each scene
    public void OnProcessScene(Scene scene, BuildReport report)
    {
        // Get all root GameObjects in the scene
        GameObject[] rootObjects = scene.GetRootGameObjects();
        List<GameObject> toDeparent = new List<GameObject>();

        // use for loops to preserve orderliness from GetRootGameObjects, if any
        for (int i = 0; i < rootObjects.Length; i++)
        {
            RecursiveCheck(rootObjects[i].transform, toDeparent);
        }

        // De-parent the collected GameObjects
        for (int i = 0; i < toDeparent.Count; i++)
        {
            GameObject obj = toDeparent[i];
			// default value for worldPositionStays in SetParent (see Unity docs) is true, which means that the child transform will inherit the position offset of the parent
            obj.transform.SetParent(null);
            Debug.Log($"De-parented {obj.name} during build process.");
        }
    }

    // recursive method to check each child GameObject
    private void RecursiveCheck(Transform parent, List<GameObject> toDeparent)
    { 
        //If we reach here, this object ("parent") will be placed at the root of the scene.
		//Before we do so, check whether we should also first do the same for its children 
		bool deparentChildren = true;
		
		//does this object have extra components? (which might access its own hierarchy)
		if (!HasNoAdditionalComponents(parent)) 
			deparentChildren = false;
		
		//does this object have non-default scale? (which can't be preserved on de-parenting)
		if (deparentChildren && !HasDefaultScale(parent)) 
			deparentChildren = false;
		
		//user-variable checks:
		//...active?
        if (deparentChildren && ignoreInactive && !parent.gameObject.activeSelf) 
			deparentChildren = false;
		
		//has a safe tag?
		// ignore this step if the dev doesn't want to check safe tags
		if (SafeTags.Count > 0) 
		{
			bool tagSafe = false;
			foreach (string tag in SafeTags)
				if (parent.tag == tag)
					tagSafe = true;
			if (!tagSafe)
				deparentChildren = false;
		}
		//has an unsafe tag?
		foreach (string tag in UnsafeTags)
			if (parent.tag == tag)
				deparentChildren = false;
		
        //if this object is clean, recursively check all of the children
		if (deparentChildren)
		{
			foreach (Transform child in parent)
			{
				// Recursively check this child's children
				RecursiveCheck(child, toDeparent);
				//Add to de-parent list last, so that we aren't deparenting parents that still have children and doing a bunch of redundant work
				toDeparent.Add(child.gameObject);
			}
		}
    }

    // check if it has only the Transform component
    private bool HasNoAdditionalComponents(Transform transformToCheck)
    {
        // get all components attached to this GameObject
        Component[] components = transformToCheck.gameObject.GetComponents<Component>();

        // return true if the only component is the Transform
        return components.Length == 1 && components[0] is Transform;
    }

    // check if a transform has default scale
    private bool HasDefaultScale(Transform transformToCheck)
    {
        if (transformToCheck.localScale != Vector3.one)
        {
            //Log it so we know we had to stop here even though it was just an empty organizational gameobject. It's much more likely that the dev will want to remedy this than the other escape conditions
            Debug.LogFormat("gameObject {0} has non-default scale", transformToCheck.gameObject.name);
            return false;
        }
        else
            return true;

    }
}
