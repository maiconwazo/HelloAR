using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARController : MonoBehaviour
{
	private List<DetectedPlane> m_NewTrackedPlanes = new List<DetectedPlane>();
	public GameObject GridPrefab;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (Session.Status != SessionStatus.Tracking)
		{
			return;
		}

		Session.GetTrackables<DetectedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);

		for (int i = 0; i <m_NewTrackedPlanes.Count; i++)
		{
			GameObject grid = Instantiate(GridPrefab, Vector3.zero, Quaternion.identity, transform);

			grid.GetComponent<GridVisualiser>().Initialize(m_NewTrackedPlanes[i]);
		}
    }
}
