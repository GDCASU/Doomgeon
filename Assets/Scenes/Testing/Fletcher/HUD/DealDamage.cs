﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour 
{
	public void sendDamage (int dmg)
	{
		PlayerStats targetPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
		targetPlayer.takeDamage(dmg);
	}
}