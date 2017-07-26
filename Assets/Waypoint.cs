using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Waypoint : MonoBehaviour {
	public RectTransform rect;
	public RectTransform imageRect;
	public Image image;
	public Text text;
	public Camera cam;
	public Player player;

	Transform target;

	public Transform DEBUG_WAYPOINT;

	int lastMeters;

	public void SetTarget(Transform t){
		target = t;

		bool enable = target != null;

		image.enabled = enable;
		text.enabled = enable;
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.T)) {
			SetTarget (null);
		}

		Vector3 pos = cam.transform.position;
		Vector3 fwd = cam.transform.forward;

		if (Input.GetKeyDown (KeyCode.F)) {
			RaycastHit hit;
			if (Physics.Raycast (pos, fwd, out hit, 500f, -1, QueryTriggerInteraction.Ignore)) {
				Transform t = hit.transform;

				if (t.GetComponent<Rigidbody>() != null) {
					SetTarget(t);
					goto done;
				}

				DEBUG_WAYPOINT.localPosition = hit.point;
				SetTarget(DEBUG_WAYPOINT);
			}
		}
		done:

		if (target == null) {
			return;
		}

		Vector3 delta = target.transform.position - pos;
		float deltaMagn = delta.magnitude;

		#if TRUTH
		int meters = (int)deltaMagn;

		if (lastMeters != meters) {
			lastMeters = meters;
			text.text = meters + " m";
		}

		text.text = Helper.FormatFloat(deltaMagn,0,1) + "m";
		#else
		{
			const float truth_threshold = 20f;
			float distance;

			if(deltaMagn < truth_threshold){
				distance = deltaMagn;
				goto done2;
			}

			distance = Mathf.Pow(deltaMagn - truth_threshold, 1.3f) + truth_threshold;
//			distance = Mathf.Pow(deltaMagn - truth_threshold, 2f) + truth_threshold;
			goto done2;

			done2:
			text.text = Helper.FormatFloat(distance) + "m";
		}
		#endif

		Vector2 screenPos = cam.WorldToViewportPoint (target.localPosition);

		bool facing = Helper.Inner (fwd, delta) > 0f;

		if (facing && screenPos.x >= 0f && screenPos.x <= 1f && screenPos.y >= 0f && screenPos.y <= 1f) {
			imageRect.localRotation = Helper.Euler (Helper.quarterpi);
		} else {
			screenPos *= 2f;
			screenPos.x -= 1f;
			screenPos.y -= 1f;

			if (facing == false) {
				screenPos.x = -screenPos.x;
				screenPos.y = -screenPos.y;
			}

			float max = Mathf.Max (Mathf.Abs (screenPos.x), Mathf.Abs (screenPos.y));
			screenPos.x /= max;
			screenPos.y /= max;

			imageRect.localRotation = Helper.Euler (Mathf.Atan2 (screenPos.y, screenPos.x) - Helper.quarterpi + Helper.pi);

			screenPos.x += 1f;
			screenPos.y += 1f;
			screenPos *= 0.5f;
		}

		rect.anchorMin = screenPos;
		rect.anchorMax = screenPos;

		if (Input.GetKey (KeyCode.V)) {
			delta /= deltaMagn;
			player.pitch = -Mathf.Asin (delta.y);
			player.yaw = Mathf.Atan2 (delta.x, delta.z);
		}
	}


}
