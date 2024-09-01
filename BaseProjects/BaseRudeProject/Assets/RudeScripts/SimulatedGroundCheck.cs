using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedGroundCheck : MonoBehaviour
{
	public static SimulatedGroundCheck instance;
	
	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		this.pmov = base.transform.parent.GetComponent<SimulatedParenting>();
		this.waterMask = LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies);
		this.waterMask |= 4;
	}

	private void OnEnable()
	{
		base.transform.parent.parent = null;
	}

	private void OnDisable()
	{
		this.touchingGround = false;
		this.cols.Clear();
		this.canJump = false;
	}

	private void Update()
	{
		if (this.forcedOff > 0)
		{
			this.onGround = false;
		}
		else if (this.onGround != this.touchingGround)
		{
			this.onGround = this.touchingGround;
		}
		if (this.onGround)
		{
			this.sinceLastGrounded = 0f;
		}
		if (this.superJumpChance > 0f)
		{
			this.superJumpChance = Mathf.MoveTowards(this.superJumpChance, 0f, Time.deltaTime);
		}
		if (this.extraJumpChance > 0f)
		{
			this.extraJumpChance = Mathf.MoveTowards(this.extraJumpChance, 0f, Time.deltaTime);
		}
		if (this.cols.Count > 0)
		{
			for (int i = this.cols.Count - 1; i >= 0; i--)
			{
				if (!this.ColliderIsStillUsable(this.cols[i]))
				{
					this.cols.RemoveAt(i);
				}
			}
		}
		if (this.cols.Count == 0)
		{
			this.touchingGround = false;
			// MonoSingleton<NewMovement>.Instance.groundProperties = null;
		}
		if (this.canJump && (this.currentEnemyCol == null || !this.currentEnemyCol.gameObject.activeInHierarchy || Vector3.Distance(base.transform.position, this.currentEnemyCol.transform.position) > 40f))
		{
			this.canJump = false;
		}
	}

	private void FixedUpdate()
	{
		/*if (this.slopeCheck || MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y >= 0f || (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS && !MonoSingleton<NewMovement>.Instance.sliding) || (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && !MonoSingleton<PlatformerMovement>.Instance.sliding))
		{
			return;
		}*/
		/*RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, Mathf.Abs(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y), this.waterMask, QueryTriggerInteraction.Collide) && raycastHit.transform.gameObject.layer == 4)
		{
			this.BounceOnWater(raycastHit.collider);
		}*/
	}

	private void OnTriggerExit(Collider other)
	{
		if (this.ColliderIsCheckable(other) && this.cols.Contains(other))
		{
			if (this.cols.IndexOf(other) == this.cols.Count - 1)
			{
				this.cols.Remove(other);
				if (this.cols.Count > 0)
				{
					for (int i = this.cols.Count - 1; i >= 0; i--)
					{
						if (this.ColliderIsStillUsable(this.cols[i]))
						{
							//MonoSingleton<NewMovement>.Instance.groundProperties = this.cols[i].GetComponent<CustomGroundProperties>();
							break;
						}
						this.cols.RemoveAt(i);
					}
				}
			}
			else
			{
				this.cols.Remove(other);
			}
			if (this.cols.Count == 0)
			{
				this.touchingGround = false;
				//MonoSingleton<NewMovement>.Instance.groundProperties = null;
			}
			if (!this.slopeCheck && (other.gameObject.CompareTag("Moving") || other.gameObject.layer == 11 || other.gameObject.layer == 26) && this.pmov.IsObjectTracked(other.transform))
			{
				this.pmov.DetachPlayer(other.transform);
				return;
			}
		}
		else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == 12)
		{
			this.canJump = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.LogWarning(other.name);

		if (this.ColliderIsCheckable(other) && !this.cols.Contains(other))
		{
			this.cols.Add(other);
			this.touchingGround = true;
			CustomGroundProperties groundProperties;
			if (other.TryGetComponent<CustomGroundProperties>(out groundProperties))
			{
				//MonoSingleton<NewMovement>.Instance.groundProperties = groundProperties;
			}
			else
			{
				//MonoSingleton<NewMovement>.Instance.groundProperties = null;
			}
			if (!this.slopeCheck && (other.gameObject.CompareTag("Moving") || other.gameObject.layer == 11 || other.gameObject.layer == 26) && other.attachedRigidbody != null && !this.pmov.IsObjectTracked(other.transform))
			{
				this.pmov.AttachPlayer(other.transform);
			}
		}
		else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == 12)
		{
			this.currentEnemyCol = other;
			this.canJump = true;
		}
		if (this.heavyFall)
		{
			if (other.gameObject.layer == 10 || other.gameObject.layer == 11)
			{
				EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
				if (component && component.eid)
				{
					component.eid.hitter = "ground slam";
					component.eid.DeliverDamage(other.gameObject, (base.transform.position - other.transform.position) * 5000f, other.transform.position, 2f, true, 0f, null, false);
					if (!component.eid.exploded)
					{
						this.heavyFall = false;
					}
				}
			}
			else if (!other.gameObject.CompareTag("Slippery") && (other.gameObject.layer == 8 || other.gameObject.layer == 24))
			{
				Breakable component2 = other.gameObject.GetComponent<Breakable>();
				if (component2 != null && (component2.weak || component2.forceGroundSlammable))
				{
					component2.Break();
				}
				else
				{
					this.heavyFall = false;
				}
				Bleeder bleeder;
				if (other.gameObject.TryGetComponent<Bleeder>(out bleeder))
				{
					bleeder.GetHit(other.transform.position, GoreType.Body);
				}
				Idol idol;
				if (other.transform.TryGetComponent<Idol>(out idol))
				{
					idol.Death();
				}
				this.superJumpChance = 0.075f;
			}
		}
	}

	private void BounceOnWater(Collider other)
	{
	}

	public void ForceOff()
	{
		this.onGround = false;
		this.forcedOff++;
	}

	public void StopForceOff()
	{
		this.forcedOff--;
		if (this.forcedOff <= 0)
		{
			this.onGround = this.touchingGround;
		}
	}

	public bool ColliderIsCheckable(Collider col)
	{
		return !col.gameObject.CompareTag("Slippery") && (col.gameObject.layer == 8 || col.gameObject.layer == 24 || col.gameObject.layer == 11 || col.gameObject.layer == 26);
	}

	public bool ColliderIsStillUsable(Collider col)
	{
		return !(col == null) && col.enabled && col.gameObject.activeInHierarchy && col.gameObject.layer != 17 && col.gameObject.layer != 10;
	}

	public bool slopeCheck;

	public bool onGround;

	public bool touchingGround;

	public bool canJump;

	public bool heavyFall;

	public GameObject shockwave;

	public float superJumpChance;

	public float extraJumpChance;

	public TimeSince sinceLastGrounded;

	private SimulatedParenting pmov;

	private Collider currentEnemyCol;

	public int forcedOff;

	private LayerMask waterMask;

	public List<Collider> cols = new List<Collider>();
}
