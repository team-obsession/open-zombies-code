﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerWeaponHandler : PlayerRelatedScript
{
	public PlayerLoadout loadout;
	public Transform weaponPoint;

	private LocalPlayer player;
	private PlayerInput input;

	WeaponInstance[] weaponInstances = new WeaponInstance[2];

	int currentWeapon; //TODO: dual-wield functionality?
	public WeaponInstance CurrentWeapon
	{
		get
		{
			return weaponInstances [currentWeapon];
		}
	}

	void OnInputSwitch (float timeHeld)
	{
		if (timeHeld != 0f)	{	return;	}
		SwitchWeapon ();
	}

	/// <summary>
	/// Gets the weapon instance.
	/// </summary>
	/// <returns>The weapon instance.</returns>
	/// <param name="weap">Weap.</param>
	public WeaponInstance GetWeaponInstance (Weapon weap)
	{
		WeaponInstance result = null;
		foreach (WeaponInstance weapInst in weaponInstances)
		{
			result = weapInst != null ? (weapInst.weapon == weap ? weapInst : result) : null;
		}
		return result;
	}

	public WeaponInstance SwitchWeapon ()
	{
		int otherWeapon = currentWeapon == 0 ? 1 : 0;
		if (weaponInstances[otherWeapon] == null)	{	return CurrentWeapon;	}
		return SwitchWeapon (otherWeapon);
	}

	public WeaponInstance SwitchWeapon (int weaponIndex)
	{
		if (weaponIndex < 0 || weaponIndex >= weaponInstances.Length) {		return null;	}

		if (CurrentWeapon != null)
		{
			CurrentWeapon.gameObject.SetActive (false);
		}
		currentWeapon = weaponIndex;
		CurrentWeapon.gameObject.SetActive (true);

		OnWeaponChange ();

		return CurrentWeapon;
	}

	public WeaponInstance pickupWeapon (Weapon weapon)
	{
		//We don't want to pick up a weapon if we already have it or if we somehow get passed a null weapon
		if (weapon == null || GetWeaponInstance (weapon) != null)	{	return null;	}

		/* Find a good weapon 'slot' to add the weapon to.
		*  If the player has an empty slot , add it there;
		*  otherwise replace the current weapon.
		*/
		int index = currentWeapon;
		for (int i = 0; i < weaponInstances.Length; i++)
		{
			if (weaponInstances[i] == null)
			{
				index = i;
				break;
			}
		}
		//Destroy our current weapon if we are replacing it
		if (weaponInstances[index] != null)
		{
			Destroy (weaponInstances[index].gameObject);
		}

		weaponInstances[index] = weapon.Spawn (weaponPoint).GetComponent<WeaponInstance>();
		InitializeWeapon (index);
		SwitchWeapon (index);
		OnWeaponChange ();
		return weaponInstances[index];
	}

	private void InitializeWeapon (int weaponIndex)
	{
		weaponInstances[weaponIndex].player = player;
		weaponInstances[weaponIndex].input = input;
		weaponInstances[weaponIndex].weapHandler = this;
		weaponInstances[weaponIndex].Initialize ();
	}

	protected override void OnInitialize()
	{
		player = GetComponent<LocalPlayer>();
		if(input == null && ((input = player.GetComponent<PlayerInput>()) == null))
		{
			Debug.LogError (gameObject.name + " couldn't find an input from its player");
		}
		input.RegisterInputSwitch (OnInputSwitch);

		weaponInstances[0] = loadout.weapon1 != null ? pickupWeapon (loadout.weapon1) : null;
		weaponInstances[1] = loadout.weapon2 != null ? pickupWeapon (loadout.weapon2) : null;

	}

	protected override void OnTerminate()
	{
		if (input == null) {	return;		}
		input.UnregisterInputAim (OnInputSwitch);
	}

/*	
	=====================================
	|			CALLBACKS				|
	=====================================
*/

	Action<WeaponInstance> cbWeaponChange;

	void OnWeaponChange ()
	{
		if (cbWeaponChange != null)
		{
			cbWeaponChange (CurrentWeapon);
		}
	}

	public void RegisterWeaponChange (Action<WeaponInstance> callbackFunc)
	{
		cbWeaponChange += callbackFunc;
	}

	public void UnregisterWeaponChange (Action<WeaponInstance> callbackFunc)
	{
		cbWeaponChange -= callbackFunc;
	}




}





