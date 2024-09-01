using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulatedParenting : MonoBehaviour
{
	public Vector3 currentDelta { get; private set; }

	public List<Transform> TrackedObjects
	{
		get
		{
			return this.trackedObjects;
		}
	}

	private void Awake()
	{
		if (this.deltaReceiver == null)
		{
			this.deltaReceiver = base.transform;
		}
	}

	private void FixedUpdate()
	{
		this.currentDelta = Vector3.zero;
		if (this.playerTracker == null)
		{
			return;
		}
		Vector3 position = this.playerTracker.transform.position;
		float y = this.playerTracker.transform.eulerAngles.y;
		Vector3 vector = position - this.lastTrackedPos;
		this.lastTrackedPos = position;
		bool flag = true;
		/*if (MonoSingleton<NewMovement>.Instance && MonoSingleton<NewMovement>.Instance.groundProperties && MonoSingleton<NewMovement>.Instance.groundProperties.dontRotateCamera)
		{
			flag = false;
		}*/
		float num = y - this.lastAngle;
		this.lastAngle = y;
		float num2 = Mathf.Abs(num);
		if (num2 > 180f)
		{
			num2 = 360f - num2;
		}
		if (num2 > 5f)
		{
			this.DetachPlayer(null);
			return;
		}
		if (vector.magnitude > 2f)
		{
			this.DetachPlayer(null);
			return;
		}
		this.deltaReceiver.position += vector;
		this.playerTracker.transform.position = this.deltaReceiver.position;
		this.lastTrackedPos = this.playerTracker.transform.position;
		this.currentDelta = vector;
		if (flag)
		{
			transform.Rotate(new Vector3(0, num, 0));
		}
	}

	public bool IsPlayerTracking()
	{
		return this.playerTracker != null;
	}

	public bool IsObjectTracked(Transform other)
	{
		return this.trackedObjects.Contains(other);
	}

	public void AttachPlayer(Transform other)
	{
		if (this.lockParent)
		{
			return;
		}
		this.trackedObjects.Add(other);
		GameObject gameObject = new GameObject("Player Position Proxy")
		{
			transform =
			{
				parent = other,
				position = this.deltaReceiver.position,
				rotation = this.deltaReceiver.rotation
			}
		};
		this.lastTrackedPos = gameObject.transform.position;
		this.lastAngle = gameObject.transform.eulerAngles.y;
		if (this.playerTracker != null)
		{
			Object.Destroy(this.playerTracker.gameObject);
		}
		this.playerTracker = gameObject.transform;
		this.ClearNulls();
	}

	public void DetachPlayer([CanBeNull] Transform other = null)
	{
		if (this.lockParent)
		{
			return;
		}
		if (other == null)
		{
			this.trackedObjects.Clear();
		}
		else
		{
			this.trackedObjects.Remove(other);
		}
		if (this.trackedObjects.Count == 0)
		{
			Object.Destroy(this.playerTracker.gameObject);
			this.playerTracker = null;
		}
		else
		{
			this.playerTracker.SetParent(this.trackedObjects.First<Transform>());
		}
		this.ClearNulls();
	}

	private void ClearNulls()
	{
		for (int i = this.trackedObjects.Count - 1; i >= 0; i--)
		{
			if (this.trackedObjects[i] == null)
			{
				this.trackedObjects.RemoveAt(i);
			}
		}
	}

	public void LockMovementParent(bool fuck)
	{
		this.lockParent = fuck;
	}

	public void LockMovementParentTeleport(bool fuck)
	{
		if (this.playerTracker)
		{
			if (fuck)
			{
				this.teleportLockDelta = this.lastTrackedPos - this.playerTracker.position;
			}
			if (this.lockParent && !fuck)
			{
				this.lastTrackedPos = this.playerTracker.position - this.teleportLockDelta;
			}
		}
		else
		{
			this.teleportLockDelta = this.lastTrackedPos;
		}
		this.lockParent = fuck;
	}

	public Transform deltaReceiver;

	private Vector3 lastTrackedPos;

	private float lastAngle;

	private Transform playerTracker;

	[HideInInspector]
	public bool lockParent;

	private Vector3 teleportLockDelta;

	private List<Transform> trackedObjects = new List<Transform>();
}
