using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UIComponent
{
    public class UIUnit : MonoBehaviour
    {
        private Unit associatedUnit;
        
        [SerializeField] private Transform uiParent;
        private GameObject uiParentGo;
        private Quaternion originalRotation;
        private Camera cam;
        
        [Header("Hp")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image hpBarImage;
        [SerializeField] private Image hpBarImageBack;
        [SerializeField] private float hpLerpTime = 0.5f;
        [SerializeField] private Color allyColor = Color.cyan;
        [SerializeField] private Color enemyColor = Color.red;
        
        [Header("Shield")]
        [SerializeField] private Image shieldBarImage;

        [Header("Passives")]
        [SerializeField] private UIUnitPassiveIcon passiveIconPrefab;
        [SerializeField] private Transform passiveIconParent;
        private List<UIUnitPassiveIcon> unitPassiveIcons = new ();
        
        private void Awake()
        {
            uiParentGo = uiParent.gameObject;
            cam = Camera.main;
            unitPassiveIcons.Clear();
        }

        public void LinkToUnit(Unit unit)
        {
            associatedUnit = unit;
            
            hpBarImage.color = associatedUnit.Team == 0 ? allyColor : enemyColor;
            
            UpdateUIRotation();
            
            // TODO - setup after unit gets initialized
            associatedUnit.Stats.OnMaxHpModified += UpdateHpBar;
            associatedUnit.Stats.OnCurrentHpModified += UpdateHpBar;
            
            associatedUnit.Stats.OnMaxShieldModified += UpdateShieldBar;
            associatedUnit.Stats.OnCurrentShieldModified += UpdateShieldBar;
            
            associatedUnit.AddOnPassiveAddedCallback(AddPassiveIcon);
            associatedUnit.AddOnPassiveRemovedCallback(RemovePassiveIcon);
        }

        private void Update()
        {
            UpdateUIRotation();
        }

        private void UpdateUIRotation()
        {
            var camRot = cam.transform.rotation;
            uiParent.LookAt(uiParent.position + camRot * Vector3.forward,camRot * Vector3.up);
        }

        private void UpdateHpBar(UnitStatsInstance statsInstance)
        {
            var current = statsInstance.CurrentHp;
            var max = statsInstance.MaxHp;
            
            hpText.text = $"{current}/{max}";
            hpBarImage.fillAmount = current / (float)max;
            hpBarImageBack.DOFillAmount(hpBarImage.fillAmount, hpLerpTime);
        }
        
        private void UpdateShieldBar(UnitStatsInstance statsInstance)
        {
            var current = statsInstance.CurrentShield;
            var max = statsInstance.MaxShield;
            
            shieldBarImage.fillAmount = current / (float)max;
        }
        
        private IEnumerator AddPassiveIcon(PassiveInstance<Unit> passiveInstance)
        {
            Debug.Log("Here");
            
            var associatedPassive =
                unitPassiveIcons.FirstOrDefault(passiveIcon => passiveIcon.PassiveInstance == passiveInstance);

            if (associatedPassive != null) yield break;
            
            associatedPassive = Instantiate(passiveIconPrefab, passiveIconParent);
            unitPassiveIcons.Add(associatedPassive);
            
            associatedPassive.LinkToPassive(passiveInstance);
        }

        private IEnumerator RemovePassiveIcon(PassiveInstance<Unit> passiveInstance)
        {
            var associatedPassive =
                unitPassiveIcons.FirstOrDefault(passiveIcon => passiveIcon.PassiveInstance == passiveInstance);
            if(associatedPassive == null) yield break;

            unitPassiveIcons.Remove(associatedPassive);
            Destroy(associatedPassive.gameObject);
        }
    }
}


