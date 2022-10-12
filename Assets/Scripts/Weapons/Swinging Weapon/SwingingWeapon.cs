using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingWeapon : MeleeWeapon
{
    public Vector3 strikeOffsetPosition;
    public Vector3 strikeOffsetRotation;

    public CharacterType targetType;

    private new Collider collider;
    private Vector3 oldPosition;
    private Vector3 oldRotation;
    private bool isStriking;

    private void Start()
    {
        collider = GetComponent<Collider>();
        isStriking = false;
    }

    public override void Block(bool isStarting) { }

    public override void Strike()
    {
        if (!isStriking) {
            print("Testing strike...");
            StartCoroutine(StrikeEnemy());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        print(targetType.GetLayer());
        print(other.gameObject.layer);
        if (other.gameObject.layer.ToString().Equals("Enemy"))
        {
            other.transform.root.GetComponent<Character>().ReceiveDamage(damage);
            collider.isTrigger = false;
            collider.enabled = false;
        }
    }

    private IEnumerator StrikeEnemy()
    {
        isStriking = true;
        collider.enabled = true;
        collider.isTrigger = true;
        oldPosition = transform.localPosition;
        oldRotation = transform.localEulerAngles;
        transform.localPosition += strikeOffsetPosition;
        transform.Rotate(strikeOffsetRotation);

        yield return new WaitForSeconds(strikeSpeed);

        transform.localPosition = oldPosition;
        transform.localRotation = Quaternion.Euler(oldRotation);
        collider.isTrigger = false;
        collider.enabled = false;
        isStriking = false;
    }
}
