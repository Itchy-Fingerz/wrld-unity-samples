using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class ARMapPositioner : MonoBehaviour 
{

	private Transform m_mapMask;
	private TrackedPlane m_AttachedPlane;
	private float m_planeYOffset;

	public void Update()
	{

		while (m_AttachedPlane.SubsumedBy != null)
		{
			m_AttachedPlane = m_AttachedPlane.SubsumedBy;
		}

		if (!m_AttachedPlane.IsValid)
		{
			m_mapMask.localScale = Vector3.zero;
		}
		else if (m_AttachedPlane.IsValid)
		{
		}

		transform.position = new Vector3(transform.position.x, m_AttachedPlane.Position.y + m_planeYOffset,
			transform.position.z);
	}

	public void Attach(TrackedPlane plane)
	{
		m_AttachedPlane = plane;
		m_planeYOffset = transform.position.y - plane.Position.y;
	}
}
