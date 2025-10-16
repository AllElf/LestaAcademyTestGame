using UnityEngine;
using System;
using System.Collections;

public class BattleSystem : MonoBehaviour
{
    [Header("Логи и темп боя")]
    public bool showLogs = true;
    public Action<string> OnBattleLog;
    [Tooltip("Пауза после каждого ключевого лога, сек.")]
    public float logPause = 0.35f;

    [Header("Анимация удара")]
    [Tooltip("Сколько секунд юнит идёт к цели")]
    public float approachTime = 0.35f;
    [Tooltip("Сколько секунд юнит возвращается назад")]
    public float returnTime = 0.30f;
    [Tooltip("На каком расстоянии от цели останавливаемся")]
    public float stopDistance = 0.75f;
    [Tooltip("Короткая пауза перед самим ударом")]
    public float preHitPause = 0.15f;

    private int turnCounter = 0;                // для эффектов: Порыв/Ярость/Яд/Дыхание
    public bool weaponChoiceMade = false;       // для окна выбора трофея

    // =====================================================
    // 🔹 ГЛАВНЫЙ БОЕВОЙ ЦИКЛ ДЛЯ АТАКИ
    // =====================================================
    public IEnumerator ProcessAttackRoutine(UnitBase attacker, UnitBase defender)
    {
        if (attacker == null || defender == null || attacker.isDead || defender.isDead)
            yield break;

        turnCounter++;

        // 0) Подход к цели — анимация и движение параллельно
        if (attacker.AnimController != null)
            attacker.AnimController.PlayMove();

        yield return attacker.MoveToPoint(defender.transform.position, stopDistance, approachTime);

        if (attacker.AnimController != null)
            attacker.AnimController.PlayIdle();

        if (preHitPause > 0f)
            yield return new WaitForSeconds(preHitPause);

        // 1) Атака всегда проигрывается
        if (attacker.AnimController != null)
            yield return attacker.AnimController.PlayAttack();

        // 2) Проверка попадания (после замаха)
        bool hit = TryHit(attacker, defender);
        yield return Pause();

        if (!hit)
        {
            // 🔊 Промах
            if (attacker.AnimController != null)
                attacker.AnimController.PlayMiss();

            Log($"❌ {attacker.unitName} промахнулся по {defender.unitName}!");
            yield return Pause();

            yield return attacker.ReturnToStart(returnTime);
            yield break;
        }

        // 3) Попадание — расчёт урона
        float baseDamage = CalculateDamage(attacker, defender);
        float finalDamage = ApplyModifiers(attacker, defender, baseDamage);
        yield return Pause();

        // 🔊 звук попадания (атакующий) и 🩸 кровь (защитник) — СОБЫТИЕ УРОНА
        if (attacker.AnimController != null)
            attacker.AnimController.PlayImpact();   // звук удара

        if (defender.AnimController != null)
            defender.AnimController.PlayHitEffect(); // кровь у получателя

        // Применяем урон
        defender.TakeDamage(finalDamage);
        Log($"⚔️ {attacker.unitName} нанёс {finalDamage:0.##} урона по {defender.unitName}. Осталось HP: {defender.currentHealth:0.##}");
        yield return Pause();

        // 4) Смерть цели
        if (defender.isDead)
        {
            if (defender.AnimController != null)
                yield return defender.AnimController.PlayDeath();

            string skull = (defender is EnemyUnit) ? "💀" : "☠️";
            Log($"{skull} {defender.unitName} погиб.");
            
            yield return Pause();
        }

        // 5) Возврат атакующего
        yield return attacker.ReturnToStart(returnTime);

        // 6) Возврат в Idle
        if (attacker.AnimController != null)
            attacker.AnimController.PlayIdle();
        if (defender.AnimController != null && !defender.isDead)
            defender.AnimController.PlayIdle();
    }


    // =====================================================
    // Старый вызов (для совместимости)
    // =====================================================
    public void ProcessAttack(UnitBase attacker, UnitBase defender)
    {
        StartCoroutine(ProcessAttackRoutine(attacker, defender));
    }

    // =====================================================
    // 🎯 Проверка попадания
    // =====================================================
    private bool TryHit(UnitBase attacker, UnitBase defender)
    {
        float atkDex = (attacker is PlayerUnit ap) ? ap.TotalDexterity : attacker.dexterity;
        float defDex = (defender is PlayerUnit dp) ? dp.TotalDexterity : defender.dexterity;

        int total = Mathf.RoundToInt(atkDex + defDex);
        int roll = UnityEngine.Random.Range(1, total + 1);

        bool hit = roll > defDex;
        Log($"🎯 Бросок точности: {roll}/{total} — {(hit ? "попадание!" : "промах!")}");
        return hit;
    }

    // =====================================================
    // 💥 Базовый урон
    // =====================================================
    private float CalculateDamage(UnitBase attacker, UnitBase defender)
    {
        float str = (attacker is PlayerUnit ap2) ? ap2.TotalStrength : attacker.strength;
        return Mathf.Max(0, attacker.weaponDamage + str);
    }

    // =====================================================
    // 🔮 Модификаторы классов и особенностей врагов
    // =====================================================
    private float ApplyModifiers(UnitBase attacker, UnitBase defender, float damage)
    {
        float modifiedDamage = damage;

        // --- эффекты атакующего (игрок)
        if (attacker is PlayerUnit atkPlayer && atkPlayer.classData != null)
        {
            string cls = atkPlayer.classData.className.ToLower();
            int lvl = atkPlayer.level;

            if (cls.Contains("разбой"))
            {
                if (lvl >= 1)
                {
                    float aDex = atkPlayer.TotalDexterity;
                    float dDex = (defender is PlayerUnit pd) ? pd.TotalDexterity : defender.dexterity;
                    if (aDex > dDex)
                    {
                        modifiedDamage += 1f;
                        Log($"🗡️ {atkPlayer.unitName} провёл скрытую атаку (+1).");
                    }
                }
                if (lvl >= 3 && turnCounter >= 2)
                {
                    int poison = turnCounter - 1;
                    modifiedDamage += poison;
                    Log($"☠️ Яд усиливает атаку (+{poison}).");
                }
            }

            if (cls.Contains("воин"))
            {
                if (lvl >= 1 && turnCounter == 1)
                {
                    modifiedDamage += attacker.weaponDamage;
                    Log($"⚔️ Порыв: дополнительный урон оружием (+{attacker.weaponDamage}).");
                }
            }

            if (cls.Contains("варвар"))
            {
                if (lvl >= 1)
                {
                    if (turnCounter <= 3)
                    {
                        modifiedDamage += 2f;
                        Log($"🔥 Ярость (+2).");
                    }
                    else
                    {
                        modifiedDamage = Mathf.Max(0, modifiedDamage - 1f);
                        Log($"😤 Усталость (−1).");
                    }
                }
            }
        }

        // --- защитные эффекты цели (игрок)
        if (defender is PlayerUnit defPlayer && defPlayer.classData != null)
        {
            string clsDef = defPlayer.classData.className.ToLower();
            int lvlDef = defPlayer.level;

            if (clsDef.Contains("воин") && lvlDef >= 2)
            {
                float defStr = defPlayer.TotalStrength;
                float atkStr = (attacker is PlayerUnit ap3) ? ap3.TotalStrength : attacker.strength;
                if (defStr > atkStr)
                {
                    modifiedDamage = Mathf.Max(0, modifiedDamage - 3f);
                    Log($"🛡️ Щит: −3 к входящему урону.");
                }
            }

            if (clsDef.Contains("варвар") && lvlDef >= 2)
            {
                float reduce = defPlayer.TotalEndurance;
                modifiedDamage = Mathf.Max(0, modifiedDamage - reduce);
                Log($"🪨 Каменная кожа: −{reduce} к входящему урону.");
            }
        }

        // --- особенности врага
        if (defender is EnemyUnit enemy)
        {
            string name = enemy.enemyData.enemyName.ToLower();

            WeaponData.DamageType wType = WeaponData.DamageType.Slashing;
            if (attacker is PlayerUnit p && p.currentWeapon != null)
                wType = p.currentWeapon.damageType;

            if (name.Contains("скелет") && wType == WeaponData.DamageType.Blunt)
            {
                modifiedDamage *= 2f;
                Log($"💀 Скелет получает двойной урон от дробящего!");
            }

            if (name.Contains("слайм") && wType == WeaponData.DamageType.Slashing)
            {
                modifiedDamage = Mathf.Max(0, modifiedDamage - attacker.weaponDamage);
                Log($"🧪 Слайм игнорирует рубящий урон оружия.");
            }

            if (name.Contains("голем"))
            {
                modifiedDamage = Mathf.Max(0, modifiedDamage - enemy.endurance);
                Log($"🪨 Каменная кожа Голема: −{enemy.endurance}.");
            }

            if (name.Contains("дракон") && turnCounter % 3 == 0)
            {
                modifiedDamage += 3;
                Log($"🔥 Дракон дышит огнём (+3).");
            }
        }

        // --- особенности врага (если он атакует)
        if (attacker is EnemyUnit attEnemy)
        {
            string aname = attEnemy.enemyData.enemyName.ToLower();
            if (aname.Contains("призрак"))
            {
                float aDex = attEnemy.dexterity;
                float dDex = (defender is PlayerUnit pd3) ? pd3.TotalDexterity : defender.dexterity;
                if (aDex > dDex)
                {
                    modifiedDamage += 1f;
                    Log($"👻 Призрак наносит скрытую атаку (+1).");
                }
            }
        }

        return Mathf.Max(0, modifiedDamage);
    }

    // =====================================================
    // 🎁 Выбор трофея
    // =====================================================
    public void OfferWeaponChange(PlayerUnit player, WeaponData droppedWeapon, UIManager uiManager)
    {
        if (player == null || droppedWeapon == null || uiManager == null)
            return;

        weaponChoiceMade = false;
        Log($"🎁 Выпало оружие: {droppedWeapon.weaponName} (урон {droppedWeapon.baseDamage})");

        if (player.currentWeapon != null)
        {
            uiManager.ShowWeaponChoice(player.currentWeapon, droppedWeapon, (bool takeNew) =>
            {
                if (takeNew)
                {
                    player.EquipWeapon(droppedWeapon);
                    Log($"🗡️ {player.unitName} экипировал новое оружие: {droppedWeapon.weaponName}");
                }
                else
                {
                    Log($"↩️ Оставлено текущее оружие: {player.currentWeapon.weaponName}");
                }

                weaponChoiceMade = true;
            });
        }
        else
        {
            player.EquipWeapon(droppedWeapon);
            Log($"⚙️ {player.unitName} получил оружие: {droppedWeapon.weaponName}");
            weaponChoiceMade = true;
        }
    }

    // =====================================================
    // 🔊 Лог и пауза
    // =====================================================
    private void Log(string msg)
    {
        if (showLogs) Debug.Log(msg);
        OnBattleLog?.Invoke(msg);
    }

    private WaitForSeconds Pause()
    {
        return logPause > 0f ? new WaitForSeconds(logPause) : null;
    }
}
