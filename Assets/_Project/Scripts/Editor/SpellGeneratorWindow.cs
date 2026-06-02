using UnityEngine;
using UnityEditor;
using AnimalMagicRoyale.Spells;
using AnimalMagicRoyale.Spells.Effects;
using System.Collections.Generic;

namespace AnimalMagicRoyale.EditorScripts
{
    public class SpellGeneratorWindow : EditorWindow
    {
        [MenuItem("Animal Magic Royale/Generate Spells")]
        public static void GenerateSpells()
        {
            string basePath = "Assets/_Project/Core/Data/Resources/Spells";
            
            if (!AssetDatabase.IsValidFolder("Assets/_Project")) AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Core")) AssetDatabase.CreateFolder("Assets/_Project", "Core");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Core/Data")) AssetDatabase.CreateFolder("Assets/_Project/Core", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Core/Data/Resources")) AssetDatabase.CreateFolder("Assets/_Project/Core/Data", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Core/Data/Resources/Spells")) AssetDatabase.CreateFolder("Assets/_Project/Core/Data/Resources", "Spells");
            if (!AssetDatabase.IsValidFolder(basePath + "/Effects")) AssetDatabase.CreateFolder(basePath, "Effects");

            GameObject testProjectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Core/Prefabs/TestProjectile.prefab");

            // Cargar sprites del tilemap
            Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Core/Art/UI/Icons/tilemap hechizos.png");
            List<Sprite> icons = new List<Sprite>();
            foreach (var obj in allSprites)
            {
                if (obj is Sprite sprite)
                {
                    icons.Add(sprite);
                }
            }
            // Asegurarnos de tener al menos 13 iconos, de lo contrario llenamos con nulos
            while (icons.Count < 13) icons.Add(null);

            DamageEffect CreateDamage(string name, float dmg)
            {
                var effect = ScriptableObject.CreateInstance<DamageEffect>();
                effect.damageAmount = dmg;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Damage.asset");
                return effect;
            }

            SlowEffect CreateSlow(string name, float percent, float duration)
            {
                var effect = ScriptableObject.CreateInstance<SlowEffect>();
                effect.slowPercent = percent;
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Slow.asset");
                return effect;
            }

            DoTEffect CreateDoT(string name, float dps, float duration)
            {
                var effect = ScriptableObject.CreateInstance<DoTEffect>();
                effect.dps = dps;
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_DoT.asset");
                return effect;
            }

            KnockbackEffect CreateKnockback(string name, float force)
            {
                var effect = ScriptableObject.CreateInstance<KnockbackEffect>();
                effect.force = force;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_KB.asset");
                return effect;
            }

            StunEffect CreateStun(string name, float duration)
            {
                var effect = ScriptableObject.CreateInstance<StunEffect>();
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Stun.asset");
                return effect;
            }

            HealEffect CreateHeal(string name, float amount, bool healCasterInstead = false)
            {
                var effect = ScriptableObject.CreateInstance<HealEffect>();
                effect.healAmount = amount;
                effect.healCasterInstead = healCasterInstead;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Heal.asset");
                return effect;
            }

            InvertControlsEffect CreateInvertControls(string name, float duration)
            {
                var effect = ScriptableObject.CreateInstance<InvertControlsEffect>();
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Invert.asset");
                return effect;
            }

            ShieldEffect CreateShield(string name, float amount)
            {
                var effect = ScriptableObject.CreateInstance<ShieldEffect>();
                effect.shieldAmount = amount;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Shield.asset");
                return effect;
            }

            RevealOnRadarEffect CreateRadar(string name, float duration)
            {
                var effect = ScriptableObject.CreateInstance<RevealOnRadarEffect>();
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Radar.asset");
                return effect;
            }

            FireRateModifierEffect CreateFireRateMod(string name, float multiplier, float duration)
            {
                var effect = ScriptableObject.CreateInstance<FireRateModifierEffect>();
                effect.multiplier = multiplier;
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_FireRate.asset");
                return effect;
            }

            RandomDebuffEffect CreateRandomDebuff(string name)
            {
                var effect = ScriptableObject.CreateInstance<RandomDebuffEffect>();
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_RandomDebuff.asset");
                return effect;
            }

            StickyBombEffect CreateStickyBomb(string name, float delay, float radius, float damage)
            {
                var effect = ScriptableObject.CreateInstance<StickyBombEffect>();
                effect.delay = delay;
                effect.radius = radius;
                effect.damage = damage;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Sticky.asset");
                return effect;
            }

            SilenceEffect CreateSilence(string name, float duration, SilenceType type)
            {
                var effect = ScriptableObject.CreateInstance<SilenceEffect>();
                effect.duration = duration;
                effect.silenceType = type;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Silence.asset");
                return effect;
            }

            BlindEffect CreateBlind(string name, float duration)
            {
                var effect = ScriptableObject.CreateInstance<BlindEffect>();
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Blind.asset");
                return effect;
            }

            HardStunEffect CreateHardStun(string name, float duration)
            {
                var effect = ScriptableObject.CreateInstance<HardStunEffect>();
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_HardStun.asset");
                return effect;
            }

            WallhackEffect CreateWallhack(string name, float duration)
            {
                var effect = ScriptableObject.CreateInstance<WallhackEffect>();
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Wallhack.asset");
                return effect;
            }

            InvisibilityEffect CreateInvis(string name, float duration)
            {
                var effect = ScriptableObject.CreateInstance<InvisibilityEffect>();
                effect.duration = duration;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Invis.asset");
                return effect;
            }

            ChainLightningEffect CreateChainLightning(string name, float jumpRadius, int maxJumps, float damageAmount)
            {
                var effect = ScriptableObject.CreateInstance<ChainLightningEffect>();
                effect.jumpRadius = jumpRadius;
                effect.maxJumps = maxJumps;
                effect.damageAmount = damageAmount;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_ChainLight.asset");
                return effect;
            }

            PullEffect CreatePull(string name)
            {
                var effect = ScriptableObject.CreateInstance<PullEffect>();
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Pull.asset");
                return effect;
            }

            DisarmEffect CreateDisarm(string name)
            {
                var effect = ScriptableObject.CreateInstance<DisarmEffect>();
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Disarm.asset");
                return effect;
            }

            SwapPositionEffect CreateSwap(string name)
            {
                var effect = ScriptableObject.CreateInstance<SwapPositionEffect>();
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Swap.asset");
                return effect;
            }

            SpellData CreateSpell(string spellName, string description, SpellTier tier, float cooldown, float projSpeed, int count, Sprite icon, TargetType targetType, bool isHoming, int maxBounces, params SpellEffect[] effects)
            {
                var spell = ScriptableObject.CreateInstance<SpellData>();
                spell.spellName = spellName;
                spell.description = description;
                spell.tier = tier;
                spell.cooldown = cooldown;
                spell.projectileSpeed = projSpeed;
                spell.projectileCount = count;
                spell.projectilePrefab = testProjectilePrefab;
                spell.icon = icon;
                spell.targetType = targetType;
                spell.isHoming = isHoming;
                spell.maxBounces = maxBounces;
                spell.effects = new List<SpellEffect>(effects);
                
                switch (tier)
                {
                    case SpellTier.Basic: spell.spellColor = Color.white; break;
                    case SpellTier.Hormiga: spell.spellColor = Color.green; break;
                    case SpellTier.Ornitorrinco: spell.spellColor = Color.cyan; break;
                    case SpellTier.GOAT: spell.spellColor = new Color(0.8f, 0f, 1f); break; // Purple
                }

                string safeName = spellName.Replace(" ", "").Replace(":", "").Replace(".", "");
                AssetDatabase.CreateAsset(spell, $"{basePath}/{tier}_{safeName}.asset");
                return spell;
            }

            // --- TIER BÁSICO - Básico ---
            CreateSpell("Palo de Madera", "Proyectil rápido, sin efecto especial", SpellTier.Basic, 0.5f, 40f, 1, icons[0], TargetType.Enemy, false, 0, CreateDamage("PaloDeMadera", 10f));

            // --- TIER HORMIGA - Común ---
            CreateSpell("Migas de Pan", "Ralentiza 30% (3s)", SpellTier.Hormiga, 3f, 30f, 1, icons[1], TargetType.Enemy, false, 0, CreateDamage("MigasDePan", 5f), CreateSlow("MigasDePan", 0.3f, 3f));
            CreateSpell("Picadura 1:00 AM", "Veneno: 2 daño/s (5s)", SpellTier.Hormiga, 4f, 45f, 1, icons[2], TargetType.Enemy, false, 0, CreateDamage("Picadura1AM", 15f), CreateDoT("Picadura1AM", 2f, 5f));
            CreateSpell("Spray Limpieza", "Knockback fuerte", SpellTier.Hormiga, 5f, 30f, 1, icons[3], TargetType.Enemy, false, 0, CreateDamage("SprayLimpieza", 10f), CreateKnockback("SprayLimpieza", 10f));
            CreateSpell("Formación Fila", "Triple proyectil", SpellTier.Hormiga, 6f, 35f, 3, icons[4], TargetType.Enemy, false, 0, CreateDamage("FormacionFila", 8f));
            CreateSpell("Azúcar Glass", "Inmoviliza 1.5s", SpellTier.Hormiga, 7f, 25f, 1, icons[5], TargetType.Enemy, false, 0, CreateStun("AzucarGlass", 1.5f));
            CreateSpell("Hoja Afilada", "Melee (rango corto)", SpellTier.Hormiga, 2f, 0f, 1, icons[6], TargetType.Enemy, false, 0, CreateDamage("HojaAfilada", 25f));
            CreateSpell("Boli Bic", "Proyectil preciso y rápido", SpellTier.Hormiga, 1f, 50f, 1, icons[7], TargetType.Enemy, false, 0, CreateDamage("BoliBic", 12f));
            CreateSpell("Ticket Comedor", "Cura 20 HP", SpellTier.Hormiga, 15f, 0f, 1, icons[8], TargetType.Self, false, 0, CreateHeal("TicketComedor", 20f));
            CreateSpell("Antenas 5G", "Invierte controles (3s)", SpellTier.Hormiga, 8f, 30f, 1, icons[9], TargetType.Enemy, false, 0, CreateInvertControls("Antenas5G", 3f));
            CreateSpell("Hormigón Armado", "Crea escudo personal de 50 HP", SpellTier.Hormiga, 10f, 0f, 1, icons[10], TargetType.Self, false, 0, CreateShield("HormigonArmado", 50f));
            CreateSpell("Canal Sur", "Revela enemigos en radar (10s)", SpellTier.Hormiga, 12f, 0f, 1, icons[11], TargetType.Self, false, 0, CreateRadar("CanalSur", 10f));
            CreateSpell("Ácido Fórmico", "Veneno fuerte DoT (4s)", SpellTier.Hormiga, 5f, 20f, 1, icons[12], TargetType.Enemy, false, 0, CreateDamage("AcidoFormico", 12f), CreateDoT("AcidoFormico", 3f, 4f));
            CreateSpell("Café Máquina", "+25% cadencia de disparo (5s)", SpellTier.Hormiga, 10f, 0f, 1, icons[0], TargetType.Self, false, 0, CreateFireRateMod("CafeMaquina", 1.25f, 5f));
            CreateSpell("WiFi UHU", "Aplica un debuff aleatorio al golpear", SpellTier.Hormiga, 8f, 40f, 1, icons[1], TargetType.Enemy, false, 0, CreateDamage("WiFiUHU", 5f), CreateRandomDebuff("WiFiUHU"));
            CreateSpell("Grapadora", "Reduce cadencia al 50% (4s)", SpellTier.Hormiga, 3f, 45f, 1, icons[2], TargetType.Enemy, false, 0, CreateDamage("Grapadora", 18f), CreateFireRateMod("Grapadora", 0.5f, 4f));
            CreateSpell("Clip de Papel", "Rebota en paredes (x2)", SpellTier.Hormiga, 0.8f, 40f, 1, icons[3], TargetType.Enemy, false, 2, CreateDamage("ClipPapel", 8f));
            CreateSpell("Post-it", "Se pega y explota a los 3s", SpellTier.Hormiga, 4f, 30f, 1, icons[4], TargetType.Enemy, false, 0, CreateDamage("Postit", 5f), CreateStickyBomb("Postit", 3f, 3f, 25f));
            CreateSpell("Pendrive Virus", "Silencia habilidad especial (5s)", SpellTier.Hormiga, 10f, 40f, 1, icons[5], TargetType.Enemy, false, 0, CreateDamage("Pendrive", 10f), CreateSilence("Pendrive", 5f, SilenceType.SpecialAbility));
            CreateSpell("Casio Científica", "Proyectil homing", SpellTier.Hormiga, 4f, 35f, 1, icons[6], TargetType.Enemy, true, 0, CreateDamage("Casio", 20f));
            CreateSpell("Rotulador Seco", "Ciega al objetivo (3s)", SpellTier.Hormiga, 2f, 30f, 1, icons[7], TargetType.Enemy, false, 0, CreateDamage("Rotulador", 5f), CreateBlind("Rotulador", 3f));

            // --- TIER ORNITORRINCO - Raro ---
            CreateSpell("Espolón Veneno", "Parálisis total (1.5s)", SpellTier.Ornitorrinco, 6f, 40f, 1, icons[8], TargetType.Enemy, false, 0, CreateDamage("EspolonVeneno", 20f), CreateHardStun("EspolonVeneno", 1.5f));
            CreateSpell("Pico-Metralla", "Escopeta (cono)", SpellTier.Ornitorrinco, 4f, 35f, 5, icons[9], TargetType.Enemy, false, 0, CreateDamage("PicoMetralla", 5f));
            CreateSpell("Electro-localización", "Wallhack: ver tras muros (6s)", SpellTier.Ornitorrinco, 12f, 0f, 1, icons[10], TargetType.Self, false, 0, CreateWallhack("ElectroLoc", 6f));
            CreateSpell("Agente P", "Invisibilidad (8s / hasta atacar)", SpellTier.Ornitorrinco, 15f, 0f, 1, icons[11], TargetType.Self, false, 0, CreateInvis("AgenteP", 8f));
            CreateSpell("Huevo Sorpresa", "Mina terrestre (radio 4m)", SpellTier.Ornitorrinco, 8f, 0f, 1, icons[12], TargetType.Enemy, false, 0, CreateDamage("HuevoSorpresa", 40f), CreateStickyBomb("HuevoSorpresa", 30f, 4f, 40f));
            CreateSpell("PDF No Editable", "Bloquea magias al rival (4s)", SpellTier.Ornitorrinco, 10f, 35f, 1, icons[0], TargetType.Enemy, false, 0, CreateDamage("PDF", 15f), CreateSilence("PDF", 4f, SilenceType.SpellInventory));
            CreateSpell("Carga Portátil", "Rayo eléctrico encadenado", SpellTier.Ornitorrinco, 6f, 45f, 1, icons[1], TargetType.Enemy, false, 0, CreateDamage("Carga", 30f), CreateChainLightning("Carga", 5f, 3, 20f));
            CreateSpell("Cable Ethernet", "Atrapa enemigo y lo acerca", SpellTier.Ornitorrinco, 6f, 40f, 1, icons[2], TargetType.Enemy, false, 0, CreateDamage("Cable", 15f), CreatePull("CableEthernet"));
            CreateSpell("Ratón de Bola", "Proyectil pesado (rueda)", SpellTier.Ornitorrinco, 3f, 25f, 1, icons[3], TargetType.Enemy, false, 0, CreateDamage("Raton", 25f), CreateKnockback("RatonBola", 15f));
            CreateSpell("Pantallazo Azul", "Congela y ciega al rival (1.5s)", SpellTier.Ornitorrinco, 12f, 40f, 1, icons[4], TargetType.Enemy, false, 0, CreateDamage("Pantallazo", 10f), CreateHardStun("Pantallazo", 1f), CreateBlind("Pantallazo", 1.5f));

            // --- TIER GOAT - Legendario ---
            CreateSpell("MADEJA", "Ralentiza 80% (6s)", SpellTier.GOAT, 15f, 30f, 1, icons[5], TargetType.Enemy, false, 0, CreateDamage("MADEJA", 20f), CreateSlow("MADEJA", 0.8f, 6f));
            CreateSpell("Tung Tung Sahur", "Explosión masiva + Knockback", SpellTier.GOAT, 20f, 35f, 1, icons[6], TargetType.Enemy, false, 0, CreateDamage("TungTung", 50f), CreateKnockback("TungTung", 20f));
            CreateSpell("Manifiesto Comunista", "Roba 35 HP (Daño = Curación)", SpellTier.GOAT, 12f, 40f, 1, icons[7], TargetType.Enemy, false, 0, CreateDamage("Manifiesto", 35f), CreateHeal("Manifiesto", 35f, true));
            CreateSpell("El Risitas", "Stun total (1.5s) + suelta arma", SpellTier.GOAT, 20f, 35f, 1, icons[8], TargetType.Enemy, false, 0, CreateDamage("Risitas", 10f), CreateHardStun("ElRisitas", 1.5f), CreateDisarm("ElRisitas"));
            CreateSpell("Beca Erasmus", "Intercambio + Confusión (3s)", SpellTier.GOAT, 18f, 40f, 1, icons[9], TargetType.Enemy, false, 0, CreateDamage("BecaErasmus", 20f), CreateSwap("BecaErasmus"), CreateInvertControls("BecaErasmus", 3f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Spells successfully generated in Assets/_Project/Core/Data/Spells/");
        }
    }
}
