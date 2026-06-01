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

            SpellData CreateSpell(string spellName, string description, SpellTier tier, float cooldown, float projSpeed, int count, Sprite icon, params SpellEffect[] effects)
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
            CreateSpell("Palo de Madera", "Proyectil rápido, sin efecto especial", SpellTier.Basic, 0.5f, 40f, 1, icons[0], CreateDamage("PaloDeMadera", 10f));

            // --- TIER HORMIGA - Común ---
            var slowEffect = ScriptableObject.CreateInstance<SlowEffect>();
            slowEffect.slowPercent = 0.3f; slowEffect.duration = 3f;
            AssetDatabase.CreateAsset(slowEffect, $"{basePath}/Effects/MigasDePan_Slow.asset");
            CreateSpell("Migas de Pan", "Ralentiza 30% (3s)", SpellTier.Hormiga, 3f, 30f, 1, icons[1], CreateDamage("MigasDePan", 5f), slowEffect);
                      var dotAcido = ScriptableObject.CreateInstance<DoTEffect>(); dotAcido.dps = 2f; dotAcido.duration = 5f;
            AssetDatabase.CreateAsset(dotAcido, $"{basePath}/Effects/Picadura1AM_DoT.asset");
            CreateSpell("Picadura 1:00 AM", "Veneno: 2 daño/s (5s)", SpellTier.Hormiga, 4f, 45f, 1, icons[2], CreateDamage("Picadura1AM", 15f), dotAcido);
            
            var knockback = ScriptableObject.CreateInstance<KnockbackEffect>(); knockback.force = 10f;
            AssetDatabase.CreateAsset(knockback, $"{basePath}/Effects/SprayLimpieza_KB.asset");
            CreateSpell("Spray Limpieza", "Knockback fuerte", SpellTier.Hormiga, 5f, 30f, 1, icons[3], CreateDamage("SprayLimpieza", 10f), knockback);
            
            CreateSpell("Formación Fila", "Triple proyectil", SpellTier.Hormiga, 6f, 35f, 3, icons[4], CreateDamage("FormacionFila", 8f));
            
            var root = ScriptableObject.CreateInstance<StunEffect>(); root.duration = 1.5f;
            AssetDatabase.CreateAsset(root, $"{basePath}/Effects/AzucarGlass_Stun.asset");
            CreateSpell("Azúcar Glass", "Inmoviliza 1.5s", SpellTier.Hormiga, 7f, 25f, 1, icons[5], root);
            
            CreateSpell("Hoja Afilada", "Melee (rango corto)", SpellTier.Hormiga, 2f, 0f, 1, icons[6], CreateDamage("HojaAfilada", 25f));
            
            CreateSpell("Boli Bic", "Proyectil preciso y rápido", SpellTier.Hormiga, 1f, 50f, 1, icons[7], CreateDamage("BoliBic", 12f));
            
            var heal = ScriptableObject.CreateInstance<HealEffect>(); heal.healAmount = 20f;
            AssetDatabase.CreateAsset(heal, $"{basePath}/Effects/TicketComedor_Heal.asset");
            CreateSpell("Ticket Comedor", "Cura 20 HP", SpellTier.Hormiga, 15f, 0f, 1, icons[8], heal);

            var invertControls = ScriptableObject.CreateInstance<InvertControlsEffect>(); invertControls.duration = 3f;
            AssetDatabase.CreateAsset(invertControls, $"{basePath}/Effects/Antenas5G_Invert.asset");
            CreateSpell("Antenas 5G", "Invierte controles (3s)", SpellTier.Hormiga, 8f, 30f, 1, icons[9], invertControls);
            
            var shield = ScriptableObject.CreateInstance<ShieldEffect>(); shield.shieldAmount = 50f;
            AssetDatabase.CreateAsset(shield, $"{basePath}/Effects/HormigonArmado_Shield.asset");
            var hormigon = CreateSpell("Hormigón Armado", "Crea escudo personal de 50 HP", SpellTier.Hormiga, 10f, 0f, 1, icons[10], shield);
            hormigon.targetType = TargetType.Self;

            var radar = ScriptableObject.CreateInstance<RevealOnRadarEffect>(); radar.duration = 10f;
            AssetDatabase.CreateAsset(radar, $"{basePath}/Effects/CanalSur_Radar.asset");
            var canalSur = CreateSpell("Canal Sur", "Revela enemigos en radar (10s)", SpellTier.Hormiga, 12f, 0f, 1, icons[11], radar);
            canalSur.targetType = TargetType.Self;

            var dotArea = ScriptableObject.CreateInstance<DoTEffect>(); dotArea.dps = 3f; dotArea.duration = 4f;
            AssetDatabase.CreateAsset(dotArea, $"{basePath}/Effects/AcidoFormico_DoT.asset");
            CreateSpell("Ácido Fórmico", "Veneno fuerte DoT (4s)", SpellTier.Hormiga, 5f, 20f, 1, icons[12], CreateDamage("AcidoFormico", 12f), dotArea);
            
            var fireRate = ScriptableObject.CreateInstance<FireRateModifierEffect>(); fireRate.multiplier = 1.25f; fireRate.duration = 5f;
            AssetDatabase.CreateAsset(fireRate, $"{basePath}/Effects/CafeMaquina_FireRate.asset");
            var cafe = CreateSpell("Café Máquina", "+25% cadencia de disparo (5s)", SpellTier.Hormiga, 10f, 0f, 1, icons[0], fireRate);
            cafe.targetType = TargetType.Self;

            var randomDebuff = ScriptableObject.CreateInstance<RandomDebuffEffect>();
            AssetDatabase.CreateAsset(randomDebuff, $"{basePath}/Effects/WiFiUHU_RandomDebuff.asset");
            CreateSpell("WiFi UHU", "Aplica un debuff aleatorio al golpear", SpellTier.Hormiga, 8f, 40f, 1, icons[1], CreateDamage("WiFiUHU", 5f), randomDebuff);
            
            var fireRateDebuff = ScriptableObject.CreateInstance<FireRateModifierEffect>(); fireRateDebuff.multiplier = 0.5f; fireRateDebuff.duration = 4f;
            AssetDatabase.CreateAsset(fireRateDebuff, $"{basePath}/Effects/Grapadora_FireRateDown.asset");
            CreateSpell("Grapadora", "Reduce cadencia al 50% (4s)", SpellTier.Hormiga, 3f, 45f, 1, icons[2], CreateDamage("Grapadora", 18f), fireRateDebuff);
            
            var clipPapel = CreateSpell("Clip de Papel", "Rebota en paredes (x2)", SpellTier.Hormiga, 0.8f, 40f, 1, icons[3], CreateDamage("ClipPapel", 8f));
            clipPapel.maxBounces = 2;

            var stickyBomb = ScriptableObject.CreateInstance<StickyBombEffect>(); stickyBomb.delay = 3f; stickyBomb.radius = 3f; stickyBomb.damage = 25f;
            AssetDatabase.CreateAsset(stickyBomb, $"{basePath}/Effects/Postit_Sticky.asset");
            CreateSpell("Post-it", "Se pega y explota a los 3s", SpellTier.Hormiga, 4f, 30f, 1, icons[4], CreateDamage("Postit", 5f), stickyBomb);
            
            var silenceSpecial = ScriptableObject.CreateInstance<SilenceEffect>(); silenceSpecial.duration = 5f; silenceSpecial.silenceType = SilenceType.SpecialAbility;
            AssetDatabase.CreateAsset(silenceSpecial, $"{basePath}/Effects/Pendrive_Silence.asset");
            CreateSpell("Pendrive Virus", "Silencia habilidad especial (5s)", SpellTier.Hormiga, 10f, 40f, 1, icons[5], CreateDamage("Pendrive", 10f), silenceSpecial);
            
            var casio = CreateSpell("Casio Científica", "Proyectil homing", SpellTier.Hormiga, 4f, 35f, 1, icons[6], CreateDamage("Casio", 20f));
            casio.isHoming = true;

            var blind = ScriptableObject.CreateInstance<BlindEffect>(); blind.duration = 3f;
            AssetDatabase.CreateAsset(blind, $"{basePath}/Effects/Rotulador_Blind.asset");
            CreateSpell("Rotulador Seco", "Ciega al objetivo (3s)", SpellTier.Hormiga, 2f, 30f, 1, icons[7], CreateDamage("Rotulador", 5f), blind);
            
            
            // --- TIER ORNITORRINCO - Raro ---
            var hardStun = ScriptableObject.CreateInstance<HardStunEffect>(); hardStun.duration = 1.5f;
            AssetDatabase.CreateAsset(hardStun, $"{basePath}/Effects/EspolonVeneno_HardStun.asset");
            CreateSpell("Espolón Veneno", "Parálisis total (1.5s)", SpellTier.Ornitorrinco, 6f, 40f, 1, icons[8], CreateDamage("EspolonVeneno", 20f), hardStun);
            
            CreateSpell("Pico-Metralla", "Escopeta (cono)", SpellTier.Ornitorrinco, 4f, 35f, 5, icons[9], CreateDamage("PicoMetralla", 5f));

            var wallhack = ScriptableObject.CreateInstance<WallhackEffect>(); wallhack.duration = 6f;
            AssetDatabase.CreateAsset(wallhack, $"{basePath}/Effects/ElectroLoc_Wallhack.asset");
            var electroloc = CreateSpell("Electro-localización", "Wallhack: ver tras muros (6s)", SpellTier.Ornitorrinco, 12f, 0f, 1, icons[10], wallhack);
            electroloc.targetType = TargetType.Self;

            var invis = ScriptableObject.CreateInstance<InvisibilityEffect>(); invis.duration = 8f;
            AssetDatabase.CreateAsset(invis, $"{basePath}/Effects/AgenteP_Invis.asset");
            var agenteP = CreateSpell("Agente P", "Invisibilidad (8s / hasta atacar)", SpellTier.Ornitorrinco, 15f, 0f, 1, icons[11], invis);
            agenteP.targetType = TargetType.Self;

            var mina = ScriptableObject.CreateInstance<StickyBombEffect>(); mina.delay = 30f; mina.radius = 4f; mina.damage = 40f;
            AssetDatabase.CreateAsset(mina, $"{basePath}/Effects/HuevoSorpresa_Mina.asset");
            CreateSpell("Huevo Sorpresa", "Mina terrestre (radio 4m)", SpellTier.Ornitorrinco, 8f, 0f, 1, icons[12], CreateDamage("HuevoSorpresa", 40f), mina);
            
            var silenceSpells = ScriptableObject.CreateInstance<SilenceEffect>(); silenceSpells.duration = 4f; silenceSpells.silenceType = SilenceType.SpellInventory;
            AssetDatabase.CreateAsset(silenceSpells, $"{basePath}/Effects/PDF_Silence.asset");
            CreateSpell("PDF No Editable", "Bloquea magias al rival (4s)", SpellTier.Ornitorrinco, 10f, 35f, 1, icons[0], CreateDamage("PDF", 15f), silenceSpells);
            
            var chainLightning = ScriptableObject.CreateInstance<ChainLightningEffect>(); chainLightning.jumpRadius = 5f; chainLightning.maxJumps = 3; chainLightning.damageAmount = 20f;
            AssetDatabase.CreateAsset(chainLightning, $"{basePath}/Effects/Carga_ChainLight.asset");
            CreateSpell("Carga Portátil", "Rayo eléctrico encadenado", SpellTier.Ornitorrinco, 6f, 45f, 1, icons[1], CreateDamage("Carga", 30f), chainLightning);
            
            var pull = ScriptableObject.CreateInstance<PullEffect>();
            AssetDatabase.CreateAsset(pull, $"{basePath}/Effects/CableEthernet_Pull.asset");
            CreateSpell("Cable Ethernet", "Atrapa enemigo y lo acerca", SpellTier.Ornitorrinco, 6f, 40f, 1, icons[2], CreateDamage("Cable", 15f), pull);
            
            // Raton de bola: projectile that hits hard and knocks back?
            var heavyKb = ScriptableObject.CreateInstance<KnockbackEffect>(); heavyKb.force = 15f;
            AssetDatabase.CreateAsset(heavyKb, $"{basePath}/Effects/RatonBola_KB.asset");
            CreateSpell("Ratón de Bola", "Proyectil pesado (rueda)", SpellTier.Ornitorrinco, 3f, 25f, 1, icons[3], CreateDamage("Raton", 25f), heavyKb);
            
            var freeze = ScriptableObject.CreateInstance<HardStunEffect>(); freeze.duration = 1f;
            AssetDatabase.CreateAsset(freeze, $"{basePath}/Effects/Pantallazo_Freeze.asset");
            var blindPantallazo = ScriptableObject.CreateInstance<BlindEffect>(); blindPantallazo.duration = 1.5f;
            AssetDatabase.CreateAsset(blindPantallazo, $"{basePath}/Effects/Pantallazo_Blind.asset");
            CreateSpell("Pantallazo Azul", "Congela y ciega al rival (1.5s)", SpellTier.Ornitorrinco, 12f, 40f, 1, icons[4], CreateDamage("Pantallazo", 10f), freeze, blindPantallazo);
            
            // --- TIER GOAT - Legendario ---
            var megaSlow = ScriptableObject.CreateInstance<SlowEffect>(); megaSlow.slowPercent = 0.8f; megaSlow.duration = 6f;
            AssetDatabase.CreateAsset(megaSlow, $"{basePath}/Effects/MADEJA_Slow.asset");
            CreateSpell("MADEJA", "Ralentiza 80% (6s)", SpellTier.GOAT, 15f, 30f, 1, icons[5], CreateDamage("MADEJA", 20f), megaSlow);
            
            var tungTungKB = ScriptableObject.CreateInstance<KnockbackEffect>(); tungTungKB.force = 20f;
            AssetDatabase.CreateAsset(tungTungKB, $"{basePath}/Effects/TungTung_KB.asset");
            CreateSpell("Tung Tung Sahur", "Explosión masiva + Knockback", SpellTier.GOAT, 20f, 35f, 1, icons[6], CreateDamage("TungTung", 50f), tungTungKB);
            
            var lifesteal = ScriptableObject.CreateInstance<HealEffect>(); lifesteal.healAmount = 35f;
            AssetDatabase.CreateAsset(lifesteal, $"{basePath}/Effects/Manifiesto_Heal.asset");
            CreateSpell("Manifiesto Comunista", "Roba 35 HP (Daño = Curación)", SpellTier.GOAT, 12f, 40f, 1, icons[7], CreateDamage("Manifiesto", 35f), lifesteal);

            var risitasStun = ScriptableObject.CreateInstance<HardStunEffect>(); risitasStun.duration = 1.5f;
            AssetDatabase.CreateAsset(risitasStun, $"{basePath}/Effects/ElRisitas_Stun.asset");
            var disarm = ScriptableObject.CreateInstance<DisarmEffect>();
            AssetDatabase.CreateAsset(disarm, $"{basePath}/Effects/ElRisitas_Disarm.asset");
            CreateSpell("El Risitas", "Stun total (1.5s) + suelta arma", SpellTier.GOAT, 20f, 35f, 1, icons[8], CreateDamage("Risitas", 10f), risitasStun, disarm);
            
            var swap = ScriptableObject.CreateInstance<SwapPositionEffect>();
            AssetDatabase.CreateAsset(swap, $"{basePath}/Effects/BecaErasmus_Swap.asset");
            var confusion = ScriptableObject.CreateInstance<InvertControlsEffect>(); confusion.duration = 3f;
            AssetDatabase.CreateAsset(confusion, $"{basePath}/Effects/BecaErasmus_Invert.asset");
            CreateSpell("Beca Erasmus", "Intercambio + Confusión (3s)", SpellTier.GOAT, 18f, 40f, 1, icons[9], CreateDamage("BecaErasmus", 20f), swap, confusion);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Spells successfully generated in Assets/_Project/Core/Data/Spells/");
        }
    }
}
