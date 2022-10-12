/*
 * Author: Cristion Dominguez
 * Date: ???
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHandler : MonoBehaviour
{
    [field: SerializeField] public Transform WeaponContainer { get; private set; }
    [field: SerializeField] public Transform ProjectileSpawn { get; protected set; }
    [SerializeField] int currentWeaponIndex;
    [SerializeField] Weapon[] weapons;

    Character wielder;
    
    Weapon currentWeapon;
    int maxWeapons = 0;
    int numOfWeapons = 0;

    private void Awake() => wielder = GetComponent<Character>();

    void Start()
    {
        maxWeapons = weapons.Length;
        for (int i = 0; i < maxWeapons; i++)
        {
            Weapon weapon = weapons[i];
            if (weapon != null)
            {
                WeaponEquipData data = new WeaponEquipData();
                data.container = WeaponContainer;
                data.projectileSpawn = ProjectileSpawn;
                data.target = wielder.Target;
                weapon.PrepareWeapon(data);
                weapon.gameObject.SetActive(false);
                numOfWeapons++;
            }
        }

        if (currentWeaponIndex >= maxWeapons)
            currentWeaponIndex = 0;
        SetCurrentWeapon(currentWeaponIndex);
    }

    public void Dev_OnEnable()
    {
        InputManager.PlayerActions.SwitchWeapon.performed += OnSwitchWeapon;

        InputManager.PlayerActions.Block.performed += OnBlock;
        InputManager.PlayerActions.Block.canceled += OnBlock;

        InputManager.PlayerActions.Reload.performed += OnReload;

        InputManager.PlayerActions.Shoot.performed += OnShoot;
        InputManager.PlayerActions.Shoot.canceled += OnShoot;

        InputManager.PlayerActions.Strike.performed += OnStrike;
    }
    public void Dev_OnDisable()
    {
        InputManager.PlayerActions.SwitchWeapon.performed -= OnSwitchWeapon;

        InputManager.PlayerActions.Block.performed -= OnBlock;
        InputManager.PlayerActions.Block.canceled -= OnBlock;

        InputManager.PlayerActions.Reload.performed -= OnReload;

        InputManager.PlayerActions.Shoot.performed -= OnShoot;
        InputManager.PlayerActions.Shoot.canceled -= OnShoot;

        InputManager.PlayerActions.Strike.performed -= OnStrike;
    }

    void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        if (numOfWeapons <= 1) return;

        currentWeapon.gameObject.SetActive(false);

        if (context.ReadValue<float>() > 0)
        {
            currentWeaponIndex = currentWeaponIndex - 1 >= 0 ? currentWeaponIndex - 1 : weapons.Length - 1;
        }
        else
        {
            currentWeaponIndex = currentWeaponIndex + 1 < weapons.Length ? currentWeaponIndex + 1 : 0;
        }

        SetCurrentWeapon(currentWeaponIndex);
    }

    void SetCurrentWeapon(int weaponIndex)
    {
        currentWeapon = weapons[weaponIndex];
        if (currentWeapon)
            currentWeapon.gameObject.SetActive(true);
    }

    void SetNewWeapon(Weapon newWeapon, int weaponIndex)
    {
        WeaponEquipData data = new WeaponEquipData();
        data.container = WeaponContainer;
        data.projectileSpawn = ProjectileSpawn;
        data.target = wielder.Target;
        newWeapon.PrepareWeapon(data);
        weapons[weaponIndex] = newWeapon;
    }

    void OnBlock(InputAction.CallbackContext context) { if (currentWeapon) currentWeapon.Block(context.phase == InputActionPhase.Performed); }
    void OnReload(InputAction.CallbackContext context) { if (currentWeapon) currentWeapon.Reload(); }
    void OnShoot(InputAction.CallbackContext context) { if (currentWeapon) currentWeapon.Shoot(context.phase == InputActionPhase.Performed); }
    void OnStrike(InputAction.CallbackContext context) { if (currentWeapon) currentWeapon.Strike(); }

    public void TakeNewWeapon(Weapon newWeapon)
    {
        if (numOfWeapons < maxWeapons)
        {
            // Add new weapon to empty slot
            if (weapons[numOfWeapons] == null)
            {
                SetNewWeapon(newWeapon, numOfWeapons);
                if (currentWeapon)
                    newWeapon.gameObject.SetActive(false);
                else
                    SetCurrentWeapon(numOfWeapons);
                numOfWeapons++;
            }
        } 
        else
        {
            // Replace current weapon with the new one
            WeaponUnequipData data = new WeaponUnequipData();
            data.dropPosition = newWeapon.transform.position;
            data.dropRotation = newWeapon.transform.rotation;
            currentWeapon.NeglectWeapon(data);
            SetNewWeapon(newWeapon, currentWeaponIndex);
            SetCurrentWeapon(currentWeaponIndex);
        }
    }
}
