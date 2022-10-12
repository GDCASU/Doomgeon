/*
 * Author: Cristion Dominguez
 * Date: ???
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public abstract class Weapon: MonoBehaviour
{
    [Header("Offset")]
    [SerializeField] protected Vector3 offsetPosition;
    [SerializeField] protected Vector3 offsetRotation;

    public CharacterType Target { get; protected set; }

    protected Animator animator;
    protected Collider attachedCollider;
    protected EquipableEntity equipableEntity; 

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        attachedCollider = GetComponent<Collider>();
        equipableEntity = GetComponent<EquipableEntity>();

        animator.keepAnimatorControllerStateOnDisable = true;
    }

    public abstract void Block(bool isStarting);
    public abstract void Reload();
    public abstract void Shoot(bool isStarting);
    public abstract void Strike();
    public abstract void PrepareWeapon(WeaponEquipData data);
    public abstract void NeglectWeapon(WeaponUnequipData data);
}

public abstract class MeleeWeapon : Weapon
{
    [Header("Basic Properties")]
    [SerializeField] protected float damage;
    [SerializeField] protected float strikeSpeed;
    [SerializeField] protected float damageBlockedPercentage;

    public override void Reload() { }
    public override void Shoot(bool isStarting) { }
    
    public override void PrepareWeapon(WeaponEquipData data) 
    {
        attachedCollider.enabled = false;
        equipableEntity.Equip();
        transform.SetParent(data.container);
        transform.localPosition = offsetPosition;
        transform.localRotation = Quaternion.Euler(offsetRotation);
        Target = data.target;
    }

    public override void NeglectWeapon(WeaponUnequipData data) 
    {
        attachedCollider.enabled = true;
        equipableEntity.Unequip();
        transform.SetParent(null);
        transform.position = data.dropPosition;
        transform.rotation = data.dropRotation;
    }
}

public abstract class RangedWeapon: Weapon
{
    [Header("Basic Properties")]
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected int fireRate;
    [SerializeField] protected float reloadDuration;
    [SerializeField] protected float launchSpeed;
    [SerializeField] protected Projectile projectile;
    [SerializeField] protected Transform visualProjectileSpawn;

    [Header("Read-Only Properties")]
    [SerializeField] protected int currentAmmo;

    protected Transform projectileSpawn;
    protected ObjectPool<Projectile> projectilePool;
    protected Action<Action<Projectile>> notifyBulletToDestoySelf;
    protected Coroutine neglectRoutine;
    protected WaitForSeconds neglectWait = new WaitForSeconds(3f);
    protected bool canFire = true;
    protected bool isReloading = false;

    public override void Block(bool isStarting) { }

    public override void PrepareWeapon(WeaponEquipData data)
    {
        attachedCollider.enabled = false;
        equipableEntity.Equip();
        transform.SetParent(data.container);
        transform.localPosition = offsetPosition;
        transform.localRotation = Quaternion.Euler(offsetRotation);
        projectileSpawn = data.projectileSpawn;
        Target = data.target;

        if (neglectRoutine != null)
        {
            StopCoroutine(neglectRoutine);
            neglectRoutine = null;
            return;
        }

        int capacity = projectile.Lifespan > 0f ? Mathf.CeilToInt(projectile.Lifespan * fireRate) : fireRate;
        projectilePool = new ObjectPool<Projectile>
        (
            () =>
            {
                Projectile newProjectile = Instantiate(projectile);
                newProjectile.ReturnSelfTo((p) => projectilePool.Release(p));
                notifyBulletToDestoySelf += newProjectile.ReturnSelfTo;
                return newProjectile;
            },
            (p) => p.gameObject.SetActive(true),
            (p) => p.gameObject.SetActive(false),
            (p) =>
            {
                if (p)
                {
                    notifyBulletToDestoySelf -= p.ReturnSelfTo;
                    Destroy(p.gameObject);
                }
            },
            false,
            capacity,
            capacity + 1
        );
    }

    public override void NeglectWeapon(WeaponUnequipData data)
    {
        neglectRoutine = StartCoroutine(WaitToNeglect());
        attachedCollider.enabled = true;
        equipableEntity.Unequip();
        transform.SetParent(null);
        transform.position = data.dropPosition;
        transform.rotation = data.dropRotation;
    }

    protected virtual IEnumerator WaitToNeglect()
    {
        yield return neglectWait;
        projectilePool.Clear();
        notifyBulletToDestoySelf?.Invoke(null);
        projectileSpawn = null;
        neglectRoutine = null;
    }
}