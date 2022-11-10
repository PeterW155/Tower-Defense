using UnityEngine;
using System.Collections;
using Tests;

public class AStarRunner : MonoBehaviour {

	public Transform prefab;
	
	private LineRenderer lineRenderer;
	private Texture2D tex;
	private Vector3[] arr;
	private float t;
	private int p = 0;
	private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 endPosition;	
	
	
	// Use this for initialization
	void Start () {
		 lineRenderer = GetComponent<LineRenderer>();
		

		tex = new Texture2D(10, 10);
		GetComponent<Renderer>().material.mainTexture = tex;
		GetComponent<Renderer>().material.mainTexture.filterMode = FilterMode.Point;
		
		arr = Program.Main(tex,prefab,lineRenderer);
		
		// fly through points
		//StartCoroutine(fly());
		transform.position = arr[0];
		flystart();
		
	}
	
	private void flystart()
	{
		StartCoroutine(fly());
	}
	
	private IEnumerator fly()
    {
		startPosition = transform.position;
		endPosition = arr[p];
		isMoving = true;
		t = 0;
		
		//transform.LookAt(arr[p+1]);
		SmoothLookAt smooth = GetComponent<SmoothLookAt>();
//		smooth.target = arr[p+1];

		
		while (t < 1.0f)
		{
			t += Time.deltaTime*0.5f;
			
			//transform.LookAt(Vector3.Lerp(arr[p], arr[p+1], t));
			//transform.LookAt(Vector3.Lerp(transform.position, arr[p+1], t));
			
			transform.position = Vector3.Lerp(startPosition, endPosition, t);
			
			//var rotation = Quaternion.LookRotation(arr[p+1] - arr[p]);
			//var rotation = Quaternion.LookRotation(Vector3.Lerp(arr[p], arr[p+1], t) - transform.position,  Vector3.up);
			//transform.rotation = Quaternion.Slerp(transform.rotation, rotation, t);
			
			smooth.target = Vector3.Lerp(arr[p+1], arr[p+2], t);
			
			yield return null;
		}
		
		// TODO: last point is over the array..?
		
		// Assets/FastAStar/AStarRunner.cs(59,21): error CS0019: Operator `<' cannot be applied to operands of type `int' and `method group'
		//if (p<arr.length)
		if (p<arr.Length-1)
		{
			p++;
			Invoke("flystart", 0);
		}
		
		isMoving = false;
		yield return 0;
	}

}
