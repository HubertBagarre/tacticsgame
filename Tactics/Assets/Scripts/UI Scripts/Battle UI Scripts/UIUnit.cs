using System;
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
        [SerializeField] private Unit associatedUnit;

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

        [Header("Passives")]
        [SerializeField] private UIUnitPassiveIcon passiveIconPrefab;
        [SerializeField] private Transform passiveIconParent;
        private List<UIUnitPassiveIcon> unitPassiveIcons = new ();
        
        private void Start()
        {
            uiParentGo = uiParent.gameObject;
            cam = Camera.main;
            unitPassiveIcons.Clear();
            
            hpBarImage.color = associatedUnit.Team == 0 ? allyColor : enemyColor;
            
            UpdateUIRotation();
            
            associatedUnit.OnCurrentHealthChanged += UpdateHpBar;
            associatedUnit.OnPassiveAdded += AddPassiveIcon;
            associatedUnit.OnPassiveRemoved += RemovePassiveIcon;
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

        private void UpdateHpBar(int hp)
        {
            hpText.text = $"{hp}/{associatedUnit.Stats.MaxHp}";
            hpBarImage.fillAmount = hp / (float)associatedUnit.Stats.MaxHp;
            hpBarImageBack.DOFillAmount(hpBarImage.fillAmount, hpLerpTime);
        }

        private void AddPassiveIcon(UnitPassiveInstance passiveInstance)
        {
            var associatedPassive =
                unitPassiveIcons.FirstOrDefault(passiveIcon => passiveIcon.PassiveInstance == passiveInstance);

            if (associatedPassive != null) return;
            
            associatedPassive = Instantiate(passiveIconPrefab, passiveIconParent);
            unitPassiveIcons.Add(associatedPassive);
            
            associatedPassive.LinkToPassive(passiveInstance);
        }

        private void RemovePassiveIcon(UnitPassiveInstance passiveInstance)
        {
            var associatedPassive =
                unitPassiveIcons.FirstOrDefault(passiveIcon => passiveIcon.PassiveInstance == passiveInstance);
            if(associatedPassive == null) return;

            unitPassiveIcons.Remove(associatedPassive);
            Destroy(associatedPassive.gameObject);
        }

    }
}


