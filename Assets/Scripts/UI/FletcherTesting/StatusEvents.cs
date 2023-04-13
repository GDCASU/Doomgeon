using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class handles all outside interactions that need to alter
 * anything contained on PlayerStats and the UI.
 * Use StatusEvents.player.method to access anything here from within
 * other scripts around the game.
*/


public class StatusEvents : MonoBehaviour
{
    public static StatusEvents player;
    public GameObject playerStatsHolder; //Assign to object that holds playerStats
    public GameObject UIManagerHolder; //Assign to object that holds the UI Manager
    private CrosshairRecoil crosshairRecoil;
    private PlayerStats playerStats;

    //Testing
    public bool spend3Battery = false; //Bool to test out battery regen without buttons

    private void Awake()
    {
        this.playerStats = playerStatsHolder.GetComponent<PlayerStats>();
        this.crosshairRecoil = UIManagerHolder.GetComponent<CrosshairRecoil>();
        player = this;
    }

    private void Update()
    {
        if (spend3Battery)
        {
            useBattery(3f);
            spend3Battery = false;
        }
        this.playerStats.regenBattery();
    }

    /*
    TODO: ADD BATTERY RECHARGING
    */
    
    private void deathEvent()
    {
        //TODO: Will trigger with health check
    }
    
    public void setRecoil(float recoil)
    {
        this.crosshairRecoil.recoilChange(recoil);
    }

    public void sendDamage(float damage)
    {
        bool result = playerStats.recieveDamage(damage);
        if (!result) {
            this.deathEvent(); //Calls death event when health was not enough to tank damage
        }
    }

    public void sendHealth(float heal)
    {
        bool result = playerStats.recieveHealth(heal);
        if (result)
        {
            //Player Healed! Leaving this here in case its useful for interacting with something else
        }
        else
        {
            //Player at Max Health
        }
    }

    public void useBattery(float cost)
    {
        bool result = playerStats.spendBattery(cost);
        if (result)
        {
            //Battery was enough to take the cost
        }
        else
        {
            //Battery charge wasnt enough for cost
        }
    }

    public void sendBattery(float charge)
    {
        bool result = playerStats.recieveBattery(charge);
        if (result)
        {
            //Player gained battery
        }
        else
        {
            //Player at max battery
        }
    }

    public void upgradeMaxBattery(float batUpgrade)
    {
        bool result = playerStats.raiseMaxBattery(batUpgrade);
        if (result)
        {
            //Battery Max Upgraded
        }
        else
        {
            //Battery Upgrade Limit reached
        }
    }

    public void upgradeMaxHealth(float hpUpgrade)
    {
        bool result = playerStats.raiseMaxHealth(hpUpgrade);
        if (result)
        {
            //Max Health Upgraded
        }
        else
        {
            //Health Upgrade Limit reached
        }
    }
    
    //Methods neccesary for status bars to work
    public float getHealth()
    {
        return playerStats.health;
    }

    public float getBattery()
    {
        return playerStats.battery;
    }

    public float getMaxHealth()
    {
        return playerStats.maxHealth;
    }

    public float getMaxBattery()
    {
        return playerStats.maxBattery;
    }

}
