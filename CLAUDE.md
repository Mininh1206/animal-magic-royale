# CLAUDE.md — Animal Magic Royale

> Archivo de contexto del proyecto. Proporciona información completa sobre la arquitectura, requisitos y estado de desarrollo para guiar a cualquier asistente de IA en futuras sesiones.

---

## 1. Visión General del Proyecto

**Animal Magic Royale** es un videojuego de acción **3D** tipo **Battle Royale** desarrollado en **Unity** (C#). Animales Low-Poly compiten en una arena que se cierra progresivamente. El humor del juego se basa en proyectiles y hechizos inspirados en cultura popular, memes y referencias académicas/locales (MADEJA, Tung Tung Sahur, Manifiesto Comunista, WiFi UHU, etc.).

- **Motor:** Unity (URP — Universal Render Pipeline 17.4.0)
- **Lenguaje:** C#
- **Template base:** Multiplayer Third-Person Gameplay (adaptado)
- **Plataforma objetivo:** PC (Standalone)

---

## 2. Stack Tecnológico y Paquetes

| Paquete | Versión | Propósito |
|---|---|---|
| `com.unity.cinemachine` | 3.1.5 | Cámara de seguimiento del jugador |
| `com.unity.inputsystem` | 1.19.0 | Sistema de inputs moderno (WASD/Gamepad) |
| `com.unity.modules.ai` | 1.0.0 | NavMesh para navegación de bots |
| `com.unity.render-pipelines.universal` | 17.4.0 | Pipeline gráfico (URP) |
| `com.unity.postprocessing` | 3.5.4 | Efectos de post-procesado |
| `com.unity.visualeffectgraph` | 17.4.0 | VFX para partículas de impacto |
| `com.unity.netcode.gameobjects` | 2.11.2 | Netcode (incluido por template) |
| `com.unity.animation.rigging` | 1.4.1 | Rigging de animaciones |
| `com.unity.timeline` | 1.8.12 | Secuencias cinemáticas |
| `com.unity.ugui` | 2.0.0 | Sistema UI |

---

## 3. Estructura de Carpetas

```
animal-magic-royale/
├── Assets/
│   ├── Core/
│   │   ├── Art/              # Modelos, materiales, texturas
│   │   ├── Audio/            # SFX y música
│   │   ├── Data/             # ScriptableObjects (SpellData, etc.)
│   │   ├── GameEvents/       # Eventos del Event Bus
│   │   ├── Prefabs/          # Prefabs reutilizables
│   │   ├── Scripts/
│   │   │   ├── Editor/       # Scripts de editor
│   │   │   └── Runtime/      # Scripts de gameplay
│   │   │       ├── Components/   # HealthComponent, SpellInventory, etc.
│   │   │       ├── Framework/    # FSM, EventBus, ObjectPool
│   │   │       ├── GameEvents/   # Definiciones de eventos
│   │   │       └── Generated/    # Código auto-generado
│   │   ├── Settings/         # Configuración URP, Input Actions
│   │   └── TestScenes/       # Escenas de prueba
│   ├── Blocks/               # Bloques del template
│   ├── Platformer/           # Assets del template (adaptar/eliminar)
│   └── Shooter/              # Assets del template (adaptar/eliminar)
├── Packages/
├── ProjectSettings/
├── tareas.csv                # Backlog de tareas (Trello export)
└── Documento de Análisis de Requisitos.txt
```

---

## 4. Arquitectura del Software

### 4.1. Patrones de Diseño

| Patrón | Uso | Ubicación esperada |
|---|---|---|
| **Event Bus (Observer)** | Comunicación global desacoplada entre sistemas (ej. `OnPlayerDeath`, `OnZoneShrink`) | `Scripts/Runtime/Framework/EventBus.cs` |
| **FSM (Máquina de Estados Finita)** | Control de estados del jugador y bots: `Idle`, `Moving`, `Attacking`, `CC_Stunned` | `Scripts/Runtime/Framework/StateMachine.cs`, `State.cs` |
| **Object Pooling** | Reciclaje de proyectiles y partículas de impacto | `Scripts/Runtime/Framework/ObjectPool.cs` |
| **Strategy / Composición** | Sistema de efectos modulares en hechizos (`SpellEffect` abstracto → `DamageEffect`, `StunEffect`, etc.) | `Scripts/Runtime/Components/` |
| **ScriptableObjects** | Datos de hechizos (`SpellData`) con tier, daño, cooldown y lista de efectos | `Core/Data/` |

### 4.2. Comunicación entre Sistemas

```
GameManager ──(EventBus)──► UI (HUD)
     │                       │
     ├──► ZoneManager         ├──► HealthBar
     ├──► SpawnManager        ├──► SpellInventoryUI
     └──► AIController        └──► ZoneTimer

PlayerController ──(FSM)──► States (Idle, Move, Attack, Stunned)
BotController ────(FSM)──► States + BehaviorTree + FuzzyController
```

---

## 5. Requisitos Funcionales

### 5.1. Gestión de Escenas (mínimo 4)

| Escena | Descripción |
|---|---|
| `Scene_Presentacion` | Menú principal, créditos y acceso a otros menús |
| `Scene_Juego` | Arena de combate con sistema de cierre de zona |
| `Scene_Configuracion` | Ajustes de gameplay (vidas, tiempo, velocidad zona) y técnicos (volumen, gráficos) |
| `Scene_Finalizacion` | Tabla de records personales (Top 10) y registro de nombres |

### 5.2. Personajes y Habilidades Especiales

| Animal | Habilidad | Efecto | Cooldown |
|---|---|---|---|
| **Cerdo** | Barro Protector | Escudo que absorbe 50 de daño (5s) | 30s |
| **Gallo** | Kikirikí Aturdidor | AoE Stun en radio 5m (1.5s) | 50s |
| **Gallina** | Frenesí de Corral | +50% velocidad + daño doble al siguiente ataque (5s) | 45s |
| **Pato** | Vuelo de Emergencia | Impulso vertical + planeo (4s) | 60s |

Cada personaje tiene soporte para **skins** (cambio de color/texturas).

### 5.3. Inventario de Hechizos

- **Capacidad:** Palo Básico (fijo) + 2 hechizos adicionales (cualquier tier).
- **Inicio de partida:** Solo Palo Básico.
- **Descarte:** Al recoger un 3er hechizo, suelta el seleccionado actualmente.
- **Cooldowns:** Independientes por hechizo.

### 5.4. Tabla Completa de Hechizos

#### Tier Básico
| Hechizo | Daño | Cooldown | Efecto |
|---|---|---|---|
| Palo de Madera | 10 | 0.5s | Proyectil rápido, sin efecto especial |

#### Tier Hormiga (Común — Caja Verde)
| # | Hechizo | Daño | Cooldown | Efecto |
|---|---|---|---|---|
| 1 | Migas de Pan | 5 | 3s | Ralentiza 30% (3s) |
| 2 | Picadura 1:00 AM | 15 | 4s | Veneno: 2 daño/s (5s) |
| 3 | Antenas 5G | 0 | 8s | Invierte controles (3s) |
| 4 | Spray Limpieza | 10 | 5s | Knockback fuerte |
| 5 | Formación Fila | 3×8 | 6s | Triple proyectil |
| 6 | Azúcar Glass | 0 | 7s | Root (inmoviliza 1.5s) |
| 7 | Hormigón Armado | 0 | 10s | Crea cobertura destruible (50 HP) |
| 8 | Hoja Afilada | 25 | 2s | Melee (rango corto) |
| 9 | Canal Sur | 0 | 12s | Revela enemigos en radar (5s) |
| 10 | Ácido Fórmico | 12 | 5s | Charco DoT en suelo (5 daño/s) |
| 11 | Boli Bic | 12 | 1s | Proyectil preciso y rápido |
| 12 | Ticket Comedor | 0 | 15s | Cura 20 HP |
| 13 | Café Máquina | 0 | 10s | +25% cadencia de disparo |
| 14 | WiFi UHU | 5 | 8s | Teletransporta enemigo 2m atrás |
| 15 | Grapadora | 18 | 3s | Reduce velocidad de ataque enemigo |
| 16 | Clip de Papel | 8 | 0.8s | Rebota en paredes (×2) |
| 17 | Post-it | 5 | 4s | Se pega y explota a los 3s |
| 18 | Pendrive Virus | 10 | 10s | Desactiva habilidad especial enemiga |
| 19 | Casio Científica | 20 | 4s | Proyectil homing |
| 20 | Rotulador Seco | 5 | 2s | Nube de humo (ciega) |

#### Tier Ornitorrinco (Raro — Caja Azul)
| # | Hechizo | Daño | Cooldown | Efecto |
|---|---|---|---|---|
| 1 | Espolón Veneno | 20 | 6s | Parálisis total (1.5s) |
| 2 | Electro-localización | 0 | 15s | Wallhack: ver tras muros (6s) |
| 3 | Pico-Metralla | 5×5 | 5s | Escopeta (cono) |
| 4 | Agente P | 0 | 20s | Invisibilidad (8s / hasta atacar) |
| 5 | Huevo Sorpresa | 40 | 10s | Mina terrestre (radio 4m) |
| 6 | PDF No Editable | 0 | 12s | Bloquea magias al rival (4s) |
| 7 | Carga Portátil | 30 | 8s | Rayo eléctrico encadenado |
| 8 | Cable Ethernet | 15 | 7s | Atrapa enemigo y lo acerca |
| 9 | Ratón de Bola | 25 | 4s | Proyectil pesado (rueda por suelo) |
| 10 | Pantallazo Azul | 0 | 15s | Congela pantalla del rival (1s) |

#### Tier GOAT (Legendario — Caja Morada)
| # | Hechizo | Daño | Cooldown | Efecto |
|---|---|---|---|---|
| 1 | MADEJA | 10 | 20s | Ralentiza 80% (6s) |
| 2 | Tung Tung Sahur | 50 | 25s | Explosión masiva + Expulsión |
| 3 | Manifiesto Comunista | 35 | 15s | Roba 35 HP (Daño = Curación) |
| 4 | El Risitas | 0 | 30s | Stun total (1.5s) + suelta arma |
| 5 | Beca Erasmus | 20 | 25s | Intercambio de posición + Confusión (controles invertidos 3s) |

### 5.5. Cajas de Botín

| Tipo | Color | Contenido |
|---|---|---|
| Común | Verde | Hechizo del Tier Hormiga |
| Rara | Azul | Hechizo del Tier Ornitorrinco |
| Legendaria | Morada | Hechizo del Tier GOAT |

### 5.6. Sistema de Zona de Daño

- Círculo concéntrico que reduce su radio progresivamente por fases.
- Daño **incremental**: aumenta con el tiempo fuera de zona y con cada fase.
- Diseñado para impedir supervivencia indefinida mediante curaciones.

### 5.7. Persistencia y Configuración

| Sistema | Tecnología | Datos |
|---|---|---|
| Configuración | `PlayerPrefs` | Volumen, gráficos, controles |
| Records/Estadísticas | JSON + Hash | Nombre, bajas totales, bajas récord, victorias (Top 1), Top 10 histórico |

- Los archivos JSON incluyen verificación por **Hash** para evitar edición manual.

---

## 6. Inteligencia Artificial

### 6.1. Arquitectura Híbrida

```
BotController
├── Sistema Sensorial
│   ├── Cono de Visión (Raycasts, respeta obstáculos)
│   └── Radio de Audición (esfera)
├── Lógica Difusa (FuzzyController)
│   ├── Fuzzification: Vida, Distancia, Amenaza → etiquetas (Crítica, Media, Alta)
│   ├── Motor de Reglas Difusas
│   └── Defuzzification → valores de estado/acción
└── Behavior Tree
    ├── Selector
    │   ├── Secuencia: Huir a zona segura (si fuera de zona)
    │   ├── Secuencia: Perseguir y Atacar (si enemigo detectado)
    │   └── Secuencia: Recolectar botín (explorar cajas)
    └── Nodos: Selector, Secuencia, Acción, Condición
```

### 6.2. Inputs del Sistema Difuso

| Variable | Rango | Etiquetas |
|---|---|---|
| Vida | 0–100 | Crítica, Media, Alta |
| Distancia Enemigo | 0–50m | Cerca, Media, Lejos |
| Amenaza Percibida | 0–1 (normalizado) | Baja, Media, Alta |

---

## 7. Backlog de Desarrollo (Milestones)

### M1: Core MVP
| Tarea | Prioridad | Estado |
|---|---|---|
| Configuración inicial del proyecto (Unity 3D, .gitignore, paquetes, carpetas) | 🔴 Alta | Done |
| Implementación del Event Bus (Observer) | 🔴 Alta | Done |
| Máquina de Estados Finita (FSM) Base | 🟡 Normal | Done |
| Controlador base del Personaje y Cámara (Cinemachine + Input System) | 🔴 Alta | Done |

### M2: Combate y Hechizos
| Tarea | Prioridad | Estado |
|---|---|---|
| Arquitectura de Hechizos (SpellData SO + SpellEffect) | 🔴 Alta | To Do |
| Sistema de Object Pooling | 🟡 Normal | To Do |
| Inventario de Hechizos (3 slots, descarte, cooldowns) | 🟡 Normal | To Do |
| Sistema de Salud y Daño (HealthComponent, TakeDamage, Die) | 🔴 Alta | To Do |

### M3: Game Loop y Battle Royale
| Tarea | Prioridad | Estado |
|---|---|---|
| GameManager y Estados de la Partida (Waiting, Playing, GameOver) | 🔴 Alta | To Do |
| Sistema de Zona de Daño Incremental | 🔴 Alta | To Do |
| Spawners de Cajas de Botín (Común, Rara, Legendaria) | 🟡 Normal | To Do |
| Habilidades Especiales de Personajes (Cerdo, Gallo, Gallina, Pato) | 🟡 Normal | To Do |

### M4: Inteligencia Artificial
| Tarea | Prioridad | Estado |
|---|---|---|
| Sistema Sensorial de la IA (visión + audición) | 🟡 Normal | To Do |
| Controlador de Lógica Difusa (Fuzzy) | 🔴 Alta | To Do |
| Árbol de Comportamiento (Behavior Tree) | 🔴 Alta | To Do |

### M5: UI y Persistencia
| Tarea | Prioridad | Estado |
|---|---|---|
| Flujo de Escenas y UI de Menús (Presentación, Configuración, Finalización) | 🟡 Normal | To Do |
| UI In-Game / HUD (barra de vida, inventario con cooldown radial, timer zona) | 🟡 Normal | To Do |
| Persistencia de Datos (JSON + Hash) | 🔴 Alta | To Do |
| Pulido, Arte y Efectos VFX/SFX (modelos Low-Poly, partículas, audio) | 🟡 Normal | To Do |

---

## 8. Convenciones de Código

- **Idioma del código:** Inglés (nombres de clases, variables, métodos).
- **Idioma de comentarios/documentación:** Español.
- **Naming:**
  - Clases/Enums: `PascalCase` (ej. `SpellData`, `HealthComponent`)
  - Métodos: `PascalCase` (ej. `TakeDamage()`)
  - Variables privadas: `_camelCase` con prefijo `_`
  - Variables públicas/SerializeField: `camelCase`
  - Constantes: `UPPER_SNAKE_CASE`
- **Estructura de scripts:**
  1. Campos serializados
  2. Campos privados
  3. Propiedades públicas
  4. Unity lifecycle (`Awake`, `Start`, `Update`)
  5. Métodos públicos
  6. Métodos privados
- **Cada script incluye** un encabezado XML con `<summary>` describiendo su propósito.

---

## 9. Reglas para el Asistente IA

1. **Siempre consulta este archivo** antes de generar código para mantener coherencia arquitectónica.
2. **Respeta los patrones establecidos** (EventBus, FSM, Object Pool, SpellEffect composición).
3. **No introduzcas dependencias externas** sin justificación. Usa los paquetes ya importados.
4. **Todo script nuevo** debe ubicarse en la carpeta correcta según la estructura del punto 3.
5. **Prioriza las tareas** según su Milestone y Prioridad del backlog.
6. **Los hechizos se definen como ScriptableObjects** (`SpellData`) con listas de efectos modulares (`SpellEffect`).
7. **La IA usa arquitectura híbrida**: Behavior Tree + Lógica Difusa + Sistema Sensorial.
8. **El daño de zona es incremental**, no fijo.
9. **La persistencia** usa JSON + Hash para records, `PlayerPrefs` para configuración.
10. **Genera código limpio**, siguiendo las convenciones del punto 8, y con tests cuando sea posible.
