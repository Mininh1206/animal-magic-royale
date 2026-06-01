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
            string basePath = "Assets/_Project/Core/Data/Spells";
            
            if (!AssetDatabase.IsValidFolder("Assets/_Project")) AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Core")) AssetDatabase.CreateFolder("Assets/_Project", "Core");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Core/Data")) AssetDatabase.CreateFolder("Assets/_Project/Core", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Core/Data/Spells")) AssetDatabase.CreateFolder("Assets/_Project/Core/Data", "Spells");
            if (!AssetDatabase.IsValidFolder(basePath + "/Effects")) AssetDatabase.CreateFolder(basePath, "Effects");

            DamageEffect CreateDamage(string name, float dmg)
            {
                var effect = ScriptableObject.CreateInstance<DamageEffect>();
                effect.damageAmount = dmg;
                AssetDatabase.CreateAsset(effect, $"{basePath}/Effects/{name}_Damage.asset");
                return effect;
            }

            void CreateSpell(string spellName, SpellTier tier, float cooldown, float projSpeed, int count, params SpellEffect[] effects)
            {
                var spell = ScriptableObject.CreateInstance<SpellData>();
                spell.spellName = spellName;
                spell.tier = tier;
                spell.cooldown = cooldown;
                spell.projectileSpeed = projSpeed;
                spell.projectileCount = count;
                spell.effects = new List<SpellEffect>(effects);
                
                string safeName = spellName.Replace(" ", "").Replace(":", "").Replace(".", "");
                AssetDatabase.CreateAsset(spell, $"{basePath}/{tier}_{safeName}.asset");
            }

            // --- TIER BÁSICO ---
            CreateSpell("Palo de Madera", SpellTier.Basic, 0.5f, 20f, 1, CreateDamage("PaloDeMadera", 10f));

            // --- TIER HORMIGA ---
            var slowEffect = ScriptableObject.CreateInstance<SlowEffect>();
            slowEffect.slowPercent = 0.3f; slowEffect.duration = 3f;
            AssetDatabase.CreateAsset(slowEffect, $"{basePath}/Effects/MigasDePan_Slow.asset");
            CreateSpell("Migas de Pan", SpellTier.Hormiga, 3f, 15f, 1, CreateDamage("MigasDePan", 5f), slowEffect);
            
            CreateSpell("Picadura 1:00 AM", SpellTier.Hormiga, 4f, 25f, 1, CreateDamage("Picadura1AM", 15f));
            
            var knockback = ScriptableObject.CreateInstance<KnockbackEffect>(); knockback.force = 10f;
            AssetDatabase.CreateAsset(knockback, $"{basePath}/Effects/SprayLimpieza_KB.asset");
            CreateSpell("Spray Limpieza", SpellTier.Hormiga, 5f, 15f, 1, CreateDamage("SprayLimpieza", 10f), knockback);
            
            CreateSpell("Formación Fila", SpellTier.Hormiga, 6f, 18f, 3, CreateDamage("FormacionFila", 8f));
            
            var root = ScriptableObject.CreateInstance<StunEffect>(); root.duration = 1.5f;
            AssetDatabase.CreateAsset(root, $"{basePath}/Effects/AzucarGlass_Stun.asset");
            CreateSpell("Azúcar Glass", SpellTier.Hormiga, 7f, 12f, 1, root);
            
            CreateSpell("Hoja Afilada", SpellTier.Hormiga, 2f, 0f, 1, CreateDamage("HojaAfilada", 25f));
            
            CreateSpell("Boli Bic", SpellTier.Hormiga, 1f, 30f, 1, CreateDamage("BoliBic", 12f));
            
            var heal = ScriptableObject.CreateInstance<HealEffect>(); heal.healAmount = 20f;
            AssetDatabase.CreateAsset(heal, $"{basePath}/Effects/TicketComedor_Heal.asset");
            CreateSpell("Ticket Comedor", SpellTier.Hormiga, 15f, 0f, 1, heal);

            // Nuevos Hechizos Hormiga añadidos según CLAUDE.md
            CreateSpell("Antenas 5G", SpellTier.Hormiga, 8f, 15f, 1); // Invierte controles
            CreateSpell("Hormigón Armado", SpellTier.Hormiga, 10f, 0f, 1); // Cobertura
            CreateSpell("Canal Sur", SpellTier.Hormiga, 12f, 0f, 1); // Revela en radar
            CreateSpell("Ácido Fórmico", SpellTier.Hormiga, 5f, 10f, 1, CreateDamage("AcidoFormico", 12f)); // Charco DoT
            CreateSpell("Café Máquina", SpellTier.Hormiga, 10f, 0f, 1); // +25% cadencia
            CreateSpell("WiFi UHU", SpellTier.Hormiga, 8f, 20f, 1, CreateDamage("WiFiUHU", 5f)); // TP atrás
            CreateSpell("Grapadora", SpellTier.Hormiga, 3f, 25f, 1, CreateDamage("Grapadora", 18f)); // Reduce vel ataque
            CreateSpell("Clip de Papel", SpellTier.Hormiga, 0.8f, 20f, 1, CreateDamage("ClipPapel", 8f)); // Rebota
            CreateSpell("Post-it", SpellTier.Hormiga, 4f, 15f, 1, CreateDamage("Postit", 5f)); // Pega y explota
            CreateSpell("Pendrive Virus", SpellTier.Hormiga, 10f, 20f, 1, CreateDamage("Pendrive", 10f)); // Desactiva hab
            CreateSpell("Casio Científica", SpellTier.Hormiga, 4f, 18f, 1, CreateDamage("Casio", 20f)); // Homing
            CreateSpell("Rotulador Seco", SpellTier.Hormiga, 2f, 15f, 1, CreateDamage("Rotulador", 5f)); // Humo
            
            
            // --- TIER ORNITORRINCO ---
            var stun = ScriptableObject.CreateInstance<StunEffect>(); stun.duration = 1.5f;
            AssetDatabase.CreateAsset(stun, $"{basePath}/Effects/EspolonVeneno_Stun.asset");
            CreateSpell("Espolón Veneno", SpellTier.Ornitorrinco, 6f, 20f, 1, CreateDamage("EspolonVeneno", 20f), stun);
            
            CreateSpell("Pico-Metralla", SpellTier.Ornitorrinco, 5f, 15f, 5, CreateDamage("PicoMetralla", 5f));

            // Nuevos Hechizos Ornitorrinco añadidos según CLAUDE.md
            CreateSpell("Electro-localización", SpellTier.Ornitorrinco, 15f, 0f, 1); // Wallhack
            CreateSpell("Agente P", SpellTier.Ornitorrinco, 20f, 0f, 1); // Invisibilidad
            CreateSpell("Huevo Sorpresa", SpellTier.Ornitorrinco, 10f, 0f, 1, CreateDamage("HuevoSorpresa", 40f)); // Mina
            CreateSpell("PDF No Editable", SpellTier.Ornitorrinco, 12f, 15f, 1); // Bloquea magias
            CreateSpell("Carga Portátil", SpellTier.Ornitorrinco, 8f, 25f, 1, CreateDamage("Carga", 30f)); // Rayo eléctrico
            CreateSpell("Cable Ethernet", SpellTier.Ornitorrinco, 7f, 20f, 1, CreateDamage("Cable", 15f)); // Atrapa
            CreateSpell("Ratón de Bola", SpellTier.Ornitorrinco, 4f, 10f, 1, CreateDamage("Raton", 25f)); // Pesado
            CreateSpell("Pantallazo Azul", SpellTier.Ornitorrinco, 15f, 20f, 1); // Congela pantalla
            
            // --- TIER GOAT ---
            var megaSlow = ScriptableObject.CreateInstance<SlowEffect>(); megaSlow.slowPercent = 0.8f; megaSlow.duration = 6f;
            AssetDatabase.CreateAsset(megaSlow, $"{basePath}/Effects/MADEJA_Slow.asset");
            CreateSpell("MADEJA", SpellTier.GOAT, 20f, 10f, 1, CreateDamage("MADEJA", 10f), megaSlow);
            
            var tungTungKB = ScriptableObject.CreateInstance<KnockbackEffect>(); tungTungKB.force = 20f;
            AssetDatabase.CreateAsset(tungTungKB, $"{basePath}/Effects/TungTung_KB.asset");
            CreateSpell("Tung Tung Sahur", SpellTier.GOAT, 25f, 15f, 1, CreateDamage("TungTung", 50f), tungTungKB);
            
            var lifesteal = ScriptableObject.CreateInstance<HealEffect>(); lifesteal.healAmount = 35f;
            AssetDatabase.CreateAsset(lifesteal, $"{basePath}/Effects/Manifiesto_Heal.asset");
            CreateSpell("Manifiesto Comunista", SpellTier.GOAT, 15f, 20f, 1, CreateDamage("Manifiesto", 35f), lifesteal);

            // Nuevos Hechizos GOAT añadidos según CLAUDE.md
            var risitasStun = ScriptableObject.CreateInstance<StunEffect>(); risitasStun.duration = 1.5f;
            AssetDatabase.CreateAsset(risitasStun, $"{basePath}/Effects/ElRisitas_Stun.asset");
            CreateSpell("El Risitas", SpellTier.GOAT, 30f, 15f, 1, risitasStun); // Stun total + suelta arma
            
            CreateSpell("Beca Erasmus", SpellTier.GOAT, 25f, 20f, 1, CreateDamage("BecaErasmus", 20f)); // Intercambio + Confusión

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Spells successfully generated in Assets/_Project/Core/Data/Spells/");
        }
    }
}
