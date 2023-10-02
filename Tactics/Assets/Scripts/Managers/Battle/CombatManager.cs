using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class CombatManager : MonoBehaviour
    {
        public List<IEnumerator> routines;

        private void Start()
        {   
            StartCoroutine(Routine1());
        }

        IEnumerator Routine1()
        {
            Debug.Log("1");
            yield return StartCoroutine(Routine2());
            Debug.Log("4");  
        }

        IEnumerator Routine2()
        {
            Debug.Log("2");
            yield return new WaitForSeconds(1f);
            Debug.Log("3"); 
        }
        IEnumerator Routine3()
        {
            Debug.Log("R3");
            yield return new WaitForSeconds(2f);
        }
        
        IEnumerator Routine4()
        {
            Debug.Log("R4 1");
            yield return new WaitForSeconds(1f);
            Debug.Log("R4 2"); 
        }
        
    }
}


