using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine // you might want to use your own namespace here
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/CharacterMindControl")]
    public class CharacterMindControl : CharacterAbility
    {
        [Header("Mind Control Settings")]
        public float MindControlDuration = 10f;
        public LayerMask EnemyLayer;
        private bool _isMindControlled = false;
        private float _mindControlTimer = 0f;
        private AIBrain _aiBrain;
        private CharacterHandleWeapon _handleWeapon;
        private LayerMask _originalDamageLayer;
        public LayerMask MindControlDamageLayer;
        private AIDecisionDetectTargetRadius2D _targetDetector;
        private LayerMask _originalTargetLayer;


        protected override void Initialization()
        {
            base.Initialization();
            _aiBrain = this.gameObject.GetComponent<AIBrain>();
            _targetDetector = this.GetComponentInChildren<AIDecisionDetectTargetRadius2D>();
            _handleWeapon = GetComponent<CharacterHandleWeapon>();
            if (_targetDetector != null)
            {
                _originalTargetLayer = _targetDetector.TargetLayer;
            }
            if (_handleWeapon != null && _handleWeapon.CurrentWeapon != null)
            {
                var meleeZone = _handleWeapon.CurrentWeapon.GetComponentInChildren<MeleeWeapon>();
                if (meleeZone != null)
                {
                    _originalDamageLayer = meleeZone.TargetLayerMask;
                    
                }
            }
        }

        public void ApplyMindControl()
        {
            if (_isMindControlled) return;

            _isMindControlled = true;
            _mindControlTimer = MindControlDuration;
            if (_originalDamageLayer == 0)
            {
                CacheOriginalDamageLayer();
            }

            // Ubah target AI menjadi musuh lainnya
            if (_targetDetector != null)
            {
                _targetDetector.TargetLayer = EnemyLayer;
            }
            UpdateWeaponDamageLayer(MindControlDamageLayer);
            UpdateBrainTarget(EnemyLayer);
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();

            if (_isMindControlled)
            {
                _mindControlTimer -= Time.deltaTime;
                if (_mindControlTimer <= 0f)
                {
                    _isMindControlled = false;
                    // Reset target AI ke default (misalnya, pemain)
                    // Ubah state AI jika diperlukan
                    if (_targetDetector != null)
                    {
                        _targetDetector.TargetLayer = _originalTargetLayer;
                    }
                    UpdateWeaponDamageLayer(_originalDamageLayer); 
                    UpdateBrainTarget(_originalTargetLayer);
                   
                    base.PlayAbilityStopFeedbacks();
                        
                }
            }
        }

        private void UpdateBrainTarget(LayerMask targetLayer)
        {
            // Cari target terdekat dalam radius 10
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 10f, targetLayer);
            if (hits.Length > 0)
            {
                Transform nearestTarget = hits[0].transform;
                float shortestDistance = Vector2.Distance(transform.position, nearestTarget.position);

                foreach (var hit in hits)
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    if (dist < shortestDistance)
                    {
                        shortestDistance = dist;
                        nearestTarget = hit.transform;
                    }
                }

                _aiBrain.Target = nearestTarget;
            }
            else
            {
                _aiBrain.Target = null;
            }
        }

        private void UpdateWeaponDamageLayer(LayerMask newLayer)
        {
            if (_handleWeapon != null && _handleWeapon.CurrentWeapon != null)
            {
                var melee = _handleWeapon.CurrentWeapon.GetComponentInChildren<MeleeWeapon>();
                if (melee != null)
                {
                    melee.TargetLayerMask = newLayer;
                    // Debug.Log($"[MindControl] Changed MeleeWeapon TargetLayerMask to: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(newLayer.value, 2)))}");
                }

                var damage = _handleWeapon.CurrentWeapon.GetComponentInChildren<DamageOnTouch>();
                if (damage != null)
                {
                    damage.TargetLayerMask = newLayer;
                    // Debug.Log($"[MindControl] Changed DamageOnTouch TargetLayerMask to: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(newLayer.value, 2)))}");
                }
            }
        }

        private void CacheOriginalDamageLayer()
        {
            if (_handleWeapon != null && _handleWeapon.CurrentWeapon != null)
            {
                var melee = _handleWeapon.CurrentWeapon.GetComponentInChildren<MeleeWeapon>();
                if (melee != null)
                {
                    _originalDamageLayer = melee.TargetLayerMask;
                }
            }
        }
    }
}