using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Un controller por arma colocada en la grilla. Ataca solo según su Formation (lugar en la grilla)
    // y dispara su habilidad especial según el trigger configurado. Todo automático.
    public class WeaponController : MonoBehaviour, IKillListener
    {
        RuntimeItem item;
        WeaponDefinition def;
        AbilityTrigger trigger;
        WeaponManager manager;
        PlayerInventory inventory;
        PlayerStats stats;
        PlayerHealth health;
        Transform owner;
        LayerMask enemyMask;
        CoverageWedgeView wedge;

        float attackTimer;
        float specialTimer;
        float lowHpTimer;
        int hits;
        int kills;
        Formation formation;
        Vector2Int cachedAnchor = new Vector2Int(int.MinValue, int.MinValue);
        readonly List<IDamageable> targets = new List<IDamageable>();

        public RuntimeItem Item => item;
        public bool HasSpecial => def != null && def.special != null;

        public bool UsesRadial => trigger.type == TriggerType.Cooldown
                               || trigger.type == TriggerType.InventoryCondition
                               || trigger.type == TriggerType.LowHealth;

        // 0..1: cuánto falta para que la habilidad esté lista.
        public float Progress01
        {
            get
            {
                switch (trigger.type)
                {
                    case TriggerType.Cooldown:
                    case TriggerType.InventoryCondition: return Mathf.Clamp01(specialTimer / trigger.cooldownSeconds);
                    case TriggerType.LowHealth:          return Mathf.Clamp01(lowHpTimer / trigger.cooldownSeconds);
                    case TriggerType.EveryNHits:         return Mathf.Clamp01((float)hits / trigger.everyN);
                    default:                             return Mathf.Clamp01((float)kills / trigger.everyN);
                }
            }
        }

        public string ProgressLabel
            => trigger.type == TriggerType.EveryNHits ? Mathf.Min(hits, trigger.everyN) + "/" + trigger.everyN
             : trigger.type == TriggerType.EveryNKills ? Mathf.Min(kills, trigger.everyN) + "/" + trigger.everyN
             : "";

        float Yaw => inventory.Config.compassMode == CompassMode.RelativeToPlayer ? owner.eulerAngles.y : 0f;
        float Damage => def.damage * stats.Get(StatType.Damage);

        public void Initialize(RuntimeItem it, WeaponManager mgr, PlayerInventory inv, PlayerStats st,
                               PlayerHealth hp, LayerMask mask)
        {
            item = it;
            def = (WeaponDefinition)it.def;
            trigger = def.specialTrigger ?? new AbilityTrigger();
            manager = mgr;
            inventory = inv;
            stats = st;
            health = hp;
            owner = hp.transform;
            enemyMask = mask;
            lowHpTimer = trigger.cooldownSeconds;   // lista desde el principio

            GameObject go = new GameObject("Coverage_" + def.DisplayName);
            go.transform.SetParent(transform, false);
            wedge = go.AddComponent<CoverageWedgeView>();
            wedge.Setup(def.color, mgr);
            RefreshFormation(true);
        }

        public void OnKill() => kills++;

        void RefreshFormation(bool force)
        {
            bool relative = inventory.Config.compassMode == CompassMode.RelativeToPlayer;
            if (!force && !relative && item.anchor == cachedAnchor) return;
            cachedAnchor = item.anchor;
            formation = inventory.Config.ComputeFormation(def.Cells, item.anchor);
            wedge.SetFormation(formation, Yaw);
        }

        void Update()
        {
            if (item == null || !GameState.IsPlaying || health.IsDead) return;
            RefreshFormation(false);
            float dt = Time.deltaTime;
            TickAttack(dt);
            TickSpecial(dt);
        }

        void TickAttack(float dt)
        {
            attackTimer -= dt;
            if (attackTimer > 0f) return;

            TargetQuery.Gather(owner.position, formation, enemyMask, Mathf.Max(1, def.maxTargetsPerAttack),
                               targets, 0.3f, Yaw);
            if (targets.Count == 0) return;   // el timer queda en 0: dispara apenas entre un enemigo

            attackTimer = def.attackInterval;
            float damage = Damage;

            if (def.isRanged)
            {
                Component c = targets[0] as Component;
                if (c != null)
                {
                    Vector3 dir = c.transform.position - owner.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.0001f)
                    {
                        dir.Normalize();
                        Projectile.Spawn(def, owner.position + dir * 0.8f, dir, damage, enemyMask, this);
                    }
                }
            }
            else
            {
                for (int i = 0; i < targets.Count; i++) targets[i].TakeDamage(damage, this);
            }

            hits++;
            wedge.Flash();
        }

        void TickSpecial(float dt)
        {
            if (!HasSpecial) return;

            switch (trigger.type)
            {
                case TriggerType.Cooldown:
                    specialTimer += dt;
                    if (specialTimer >= trigger.cooldownSeconds) { specialTimer = 0f; FireSpecial(); }
                    break;

                case TriggerType.InventoryCondition:
                    specialTimer = Mathf.Min(specialTimer + dt, trigger.cooldownSeconds);
                    if (specialTimer >= trigger.cooldownSeconds && inventory.Grid.CountOf(def) >= trigger.requiredCopies)
                    {
                        specialTimer = 0f;
                        FireSpecial();
                    }
                    break;

                case TriggerType.EveryNHits:
                    if (hits >= trigger.everyN) { hits = 0; FireSpecial(); }
                    break;

                case TriggerType.EveryNKills:
                    if (kills >= trigger.everyN) { kills = 0; FireSpecial(); }
                    break;

                case TriggerType.LowHealth:
                    lowHpTimer = Mathf.Min(lowHpTimer + dt, trigger.cooldownSeconds);
                    if (lowHpTimer >= trigger.cooldownSeconds && health.Fraction < trigger.healthFraction)
                    {
                        lowHpTimer = 0f;
                        FireSpecial();
                    }
                    break;
            }
        }

        void FireSpecial()
        {
            AbilityContext ctx = new AbilityContext
            {
                owner = owner,
                origin = owner.position,
                baseDamage = Damage,
                enemyMask = enemyMask,
                formation = formation,
                yawOffsetDeg = Yaw,
                source = this
            };
            def.special.Execute(ctx);

            // Muestra en el suelo la zona que alcanzó la habilidad.
            AreaFlash.Spawn(owner.position, def.special.GetArea(ctx), ctx.yawOffsetDeg, def.color);
            wedge.Flash();
        }
    }
}
