using GoogleARCore;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARController : MonoBehaviour
{
	private List<DetectedPlane> m_NewTrackedPlanes = new List<DetectedPlane>();
	public GameObject GridPrefab;
	public GameObject Cylinder;

	void Start()
	{
		var location = UnityEngine.Input.location;
		location.Start();

		int maxWait = 20;
		while (location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			new WaitForSeconds(1);
			maxWait--;
		}

		if (maxWait < 1)
		{
			print("Timed out");
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Session.Status != SessionStatus.Tracking)
		{
			return;
		}

		Session.GetTrackables<DetectedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);

		var angle = DegreeBearing(50.36389, -4.15694, 42.35111, -71.04083);
		var distance = 0;

		var location = UnityEngine.Input.location;
		var lat = location.lastData.latitude;
		var longitude = location.lastData.longitude;

		GameObjectGPS obj = ReturnObjectToRender();
		obj.objeto.transform.rotation = Quaternion.Euler(0, (float)angle, 0);
		obj.objeto.transform.Translate(new Vector3(0, 0, distance));


		for (int i = 0; i < m_NewTrackedPlanes.Count; i++)
		{
			GameObject grid = Instantiate(GridPrefab, Vector3.zero, Quaternion.identity, transform);
			List<Vector3> bounds = new List<Vector3>();
			m_NewTrackedPlanes[i].GetBoundaryPolygon(bounds);

			grid.GetComponent<GridVisualiser>().Initialize(m_NewTrackedPlanes[i]);
		}

		Touch touch;
		if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
		{
			return;
		}

		TrackableHit hit;
		if (Frame.Raycast(touch.position.x, touch.position.y, TrackableHitFlags.PlaneWithinPolygon, out hit))
		{
			Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

			Vector3 position = hit.Pose.position;
			position.y = 0.1f;

			GameObject myCylinder = Instantiate(Cylinder, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

			myCylinder.transform.localScale = myCylinder.transform.localScale / 10;
			myCylinder.transform.position = position;
			myCylinder.transform.rotation = hit.Pose.rotation;
			myCylinder.transform.parent = anchor.transform;
			myCylinder.SetActive(true);
		}
	}

	private GameObjectGPS ReturnObjectToRender()
	{
		GameObject myCylinder = Instantiate(Cylinder, new Vector3(0, 0.1f, 0), Quaternion.identity) as GameObject;
		return new GameObjectGPS() { objeto = myCylinder, longitude = 0, latitude = 0 };
	}

	static double DegreeBearing(
	double lat1, double lon1,
	double lat2, double lon2)
	{
		var dLon = ToRad(lon2 - lon1);
		var dPhi = Math.Log(
			Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
		if (Math.Abs(dLon) > Math.PI)
			dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
		return ToBearing(Math.Atan2(dLon, dPhi));
	}

	public static double ToRad(double degrees)
	{
		return degrees * (Math.PI / 180);
	}

	public static double ToDegrees(double radians)
	{
		return radians * 180 / Math.PI;
	}

	public static double ToBearing(double radians)
	{
		// convert radians to degrees (as bearing: 0...360)
		return (ToDegrees(radians) + 360) % 360;
	}

	internal class GameObjectGPS
	{
		public GameObject objeto;
		public float longitude;
		public float latitude;
	}
}
