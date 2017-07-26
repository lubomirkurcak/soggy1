using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilities : MonoBehaviour {
	public Char c;
	public CameraScript script;
	public GameObject projectile;

	KeyCode[] Keybinds;
	int AbilityCount;
	Ability[] abilities;
	const float GlobalCooldown = 0.1f;
	float NextAction;

	public bool Casting;
	public Ability CastedAbility;
	public float CastingFinish;
	public float CastingStart;

	void DefaultKeybinds(){
		System.Array.Resize (ref Keybinds, 10);
		int at = 0;
		Keybinds [at++] = KeyCode.Mouse0;
		Keybinds [at++] = KeyCode.Mouse1;
		Keybinds [at++] = KeyCode.LeftShift;
		Keybinds [at++] = KeyCode.Q;
		Keybinds [at++] = KeyCode.E;
		Keybinds [at++] = KeyCode.R;
		Keybinds [at++] = KeyCode.F;
		Keybinds [at++] = KeyCode.Z;
		Keybinds [at++] = KeyCode.X;
		Keybinds [at++] = KeyCode.C;
	}

	void DefaultAbilities(){
		AbilityCount = 2;
		System.Array.Resize (ref abilities, AbilityCount);

		int at = 0;
		Ability a;
		a = new Ability ();
		a.Name = "Grenade";
		a.CastTime = 0.4f;
		a.Cooldown = 0.4f;
		a.RequiresGround = true;
		a.MoveWhileCasting = false;
		a.action = (Char c) => {
			next = TL.t + cooldown;
			Projectile proj = Pool.Request(projectilePoolID).GetComponent<Projectile> ();
			proj.Velocity = 20f;
			proj.Fire (c.player);

			script.target = proj.transform;
		};

		abilities [at++] = a;

		a = new Ability ();
		a.Name = "Grenade Weak Throw";
		a.CastTime = 0.4f;
		a.Cooldown = 0.4f;
		a.sharedCooldown = abilities [0].sharedCooldown;
		a.action = (Char c) => {
			next = TL.t + cooldown;
			Projectile proj = Pool.Request(projectilePoolID).GetComponent<Projectile> ();
			proj.Velocity = 7f;
			proj.Fire (c.player);

			script.target = proj.transform;
		};

		abilities [at++] = a;
		/*
		a = new Ability ();
		a.Name = "AWP";
		a.CastTime = 1f;
		a.Cooldown = 0.5f;
		a.action = (Char c) => {
			int LayerMask = -1;
			float Distance = 100f;

			Vector3 dir = c.player.q * Helper.v3forw;
			Vector3 startPoint = c.player.eye;
			Vector3 endPoint = startPoint + dir * Distance;

			// TODO(lubomir): Limit maximum number of pierces
			// TODO(lubomir): Make sure these lists are ordered based on distance
			// TODO(lubomir): Make sure to reverse the outs order (since its being cast from the end point)
			RaycastHit[] ins  = Physics.RaycastAll(startPoint, dir, Distance, LayerMask, QueryTriggerInteraction.Ignore);
			RaycastHit[] outs = Physics.RaycastAll(endPoint,  -dir, Distance, LayerMask, QueryTriggerInteraction.Ignore);

			for(int i=0; i<outs.Length; ++i){
				outs[i].distance = Distance - outs[i].distance;
			}

			float PiercePower = 100f;
			int InCount = ins.Length;
			int OutCount = outs.Length;
			int InAt = 0;
			int OutAt = 0;

			// Counts how many colliders are we inside of currently
			int Inside = 0;

			for(;;){
				if(ins[InAt].distance < outs[OutAt].distance){
					InAt++;
					Inside++;
				}else{
					OutAt--;
					Inside--;
				}
			}
		};
		abilities [at++] = a;*/
	}

	int projectilePoolID;
	float cooldown = 0.4f;
	float next;

	void Start(){
		projectilePoolID = Pool.Register (projectile);

		DefaultKeybinds ();
		DefaultAbilities ();
	}

	public void Interrupt(){
		Casting = false;
		NextAction = TL.t + GlobalCooldown;
	}

	public void update(){
		// TODO(lubomir): Undo
//		if (Keybinds.Interrupt.Value) {
//			Interrupt ();
//		}

		for (int i = 0; i < AbilityCount; i++) {
			if (CInput.GetKey (Keybinds [i])) {
				Ability a = abilities [i];
				if (a.IgnoreGlobalCooldown || TL.t >= NextAction) {
					if (TL.t >= a.sharedCooldown.NextUse) {
						if (Casting && abilities [i].PriorityCast == false) {
							continue;
						}

						if (abilities [i].RequiresGround && c.player.grounded == false) {
							continue;
						}

						Casting = true;
						CastedAbility = a;
						CastingStart = TL.t;
						CastingFinish = CastingStart + a.CastTime;
					}
				}
			}
		}

		if (Casting) {
			if (CastedAbility.RequiresGround && c.player.grounded == false) {
				Interrupt ();
			}

			if (TL.t >= CastingFinish) {
				NextAction = TL.t + GlobalCooldown;

				CastedAbility.sharedCooldown.NextUse = TL.t + CastedAbility.Cooldown;
				Casting = false;
				CastedAbility.action (c);
			}
		}
	}
}



public class Ability{
	public Ability(){
		sharedCooldown = new SharedCooldown ();

		MoveWhileCasting = true;
	}
	public Ability(string Name) : base(){
		this.Name = Name;
	}

	public string Name;
	public float CastTime;

	public class SharedCooldown{
		public float NextUse;
	}

	public SharedCooldown sharedCooldown;
	public float Cooldown;
	public bool IgnoreGlobalCooldown;
	public bool PriorityCast;
	public bool RequiresGround;
	public bool MoveWhileCasting;
	public delegate void Action(Char c);
	public Action action;
}