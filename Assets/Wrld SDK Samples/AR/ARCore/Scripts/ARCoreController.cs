//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.HelloAR
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Rendering;
	using GoogleARCore;

	/// <summary>
	/// Controlls the HelloAR example.
	/// </summary>
	public class ARCoreController : MonoBehaviour
	{
		public Camera m_firstPersonCamera;
		//		public GameObject m_trackedPlanePrefab;
		public GameObject m_wrldMap;
		public GameObject m_wrldMapMask;
		public GameObject m_searchingForPlaneUI;

		private List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();
		private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

		private TrackedPlane currentPlane;
		private Vector3 position;
		private Quaternion quaternion;

		public void Update ()
		{
			_QuitOnConnectionErrors();

				if (Frame.TrackingState != FrameTrackingState.Tracking)
			{
				const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
				Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
				return;
			}


			Debug.Log ("current plane: "+currentPlane);

			if (currentPlane == null) 
			{
				Frame.GetNewPlanes(ref m_newPlanes);
				for (int i = 0; i < m_newPlanes.Count; i++)
				{
					currentPlane = m_newPlanes [i];
					position = currentPlane.Position;
					position = currentPlane.Position;
					quaternion = currentPlane.Rotation;
					break;
				}
			}

			// Disable the snackbar UI when no planes are valid.
			bool doWeHaveAValidPlane = false;
			Frame.GetAllPlanes(ref m_allPlanes);
			for (int i = 0; i < m_allPlanes.Count; i++) {
				if (m_allPlanes [i].IsValid) {
					doWeHaveAValidPlane = true;
					break;
				}
			}

			if (!doWeHaveAValidPlane) 
			{
				currentPlane = null;
				m_wrldMapMask.transform.localScale = Vector3.zero;
				m_searchingForPlaneUI.SetActive(true);
			}
			else if(currentPlane!=null)
			{
				m_wrldMap.transform.position = position;
				m_wrldMap.transform.rotation = quaternion;
				m_wrldMapMask.transform.localScale = new Vector3(currentPlane.Bounds.x, currentPlane.Bounds.y, 1f);
			}

		}

		/// <summary>
		/// Quit the application if there was a connection error for the ARCore session.
		/// </summary>
		private void _QuitOnConnectionErrors()
		{
			// Do not update if ARCore is not tracking.
			if (Session.ConnectionState == SessionConnectionState.DeviceNotSupported)
			{
				_ShowAndroidToastMessage("This device does not support ARCore.");
				Application.Quit();
			}
			else if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission)
			{
				_ShowAndroidToastMessage("Camera permission is needed to run this application.");
				Application.Quit();
			}
			else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed)
			{
				_ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
				Application.Quit();
			}
		}

		/// <summary>
		/// Show an Android toast message.
		/// </summary>
		/// <param name="message">Message string to show in the toast.</param>
		/// <param name="length">Toast message time length.</param>
		private static void _ShowAndroidToastMessage(string message)
		{
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

			if (unityActivity != null)
			{
				AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
				unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
					{
						AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
							message, 0);
						toastObject.Call("show");
					}));
			}
		}
	}
}
