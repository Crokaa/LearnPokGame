using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{

     [SerializeField] GameObject health;
     public bool IsUpdating { get; set; }

     public void SetHP(float hpNormalized)
     {

          health.transform.localScale = new Vector3(hpNormalized, 1f);
     }

     public IEnumerator SetHPSmooth(float newHP)
     {

          IsUpdating = true;

          float currHP = health.transform.localScale.x;
          float change = currHP - newHP;

          bool damaged = newHP < currHP;

          while (damaged ? currHP - newHP > Mathf.Epsilon : newHP - currHP > Mathf.Epsilon)
          {
               currHP -= change * Time.deltaTime;
               health.transform.localScale = new Vector3(currHP, 1f);
               yield return null;
          }

          health.transform.localScale = new Vector3(newHP, 1f);

          IsUpdating = false;
     }
}
