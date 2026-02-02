# SPRINT - Galeria de Tiro VR 🎯

Galeria de tiro em VR com **3 Armas, 3 Alvos e 3 Modos de Jogo**

---

## 📦 O que tem no projeto?

### 🔫 Armas
- **Pistola** - Aperte o gatilho para atirar
- **Arco** - Puxe a corda e solte para disparar
- **Dardo** - Arremesse no alvo

### 🎯 Alvos
- **Estático** - Parado (10 pontos)
- **Móvel** - Se move (20 pontos)
- **Resistente** - Precisa de vários tiros (30 pontos)

### 🎮 Modos
- **Livre** - Treino infinito
- **Tempo** - Corrida contra o relógio
- **Precisão** - Sistema de combos

---

## 🚀 Como Usar (Passo a Passo Simples)

### 1️⃣ Configurar o Gerenciador

1. **Clique com botão direito** na Hierarchy → `Create Empty`
2. **Renomeie** para "GameManager"
3. **Arraste** o script `GerenciadorJogo.cs` nele
4. No Inspector, **arraste os textos da UI** para os campos

---

### 2️⃣ Configurar um Modo de Jogo

1. **Selecione** o objeto "Modo De Jogo" na Hierarchy
2. **Arraste** um dos scripts de modo (ModoLivre, ModoTempo ou ModoPrecisao)
3. No Inspector:
   - **Prefabs Alvos**: Arraste os alvos da pasta `Assets/Criar Jogo/Alvos/`
   - **Pontos Spawn**: Arraste os objetos Spawn1(1) até Spawn1(10) que estão em "Posicao Alvos"
   - **Textos UI**: Arraste os textos do Canvas em "UI"
   - **Rotação Alvos**: Ajuste se necessário (ex: Y = 180 se estiverem de costas)

---

### 3️⃣ Configurar Armas Existentes

**As armas já estão na cena!** (Dardo, Arco e Flecha, Nerf)

Se precisar ajustar:
1. **Selecione** a arma na Hierarchy (em "Armas")
2. No Inspector, **verifique**:
   - ✅ Componente `XR Grab Interactable` (para pegar no VR)
   - ✅ Script da arma (Atirador, Arco ou Lancavel)
   - ✅ Collider (para poder pegar)
3. **Configure se necessário**:
   - `Ponto Disparo`: Empty na ponta da arma
   - `Prefab Projetil`: Bala/Flecha da pasta `Assets/Criar Jogo/Armas/`
   - `Rotação/Editar Alvos

**Para usar alvos existentes:**
1. Vá em `Assets/Criar Jogo/Alvos/`
2. **Arraste** o prefab do alvo para "Modo De Jogo" → "Prefabs Alvos"

**Para criar novo alvo:**
1. **Arraste** o modelo 3D para a cena
2. **Adicione componente**: Script do alvo (AlvoEstatico, AlvoMovel ou AlvoResistente)
3. **Adicione componente**: Collider
4. **Adicione componente**: Rigidbody
5. **Configure Rigidbody**:
   - ✅ Marque `Is Kinematic` (importante!)
   - `Collision Detection`: Continuous
6. **Salve como Prefab**: Arraste para `Assets/Criar Jogo/Alvo
5. **Configure Rigidbody**:
   - ✅ Marque `Is Kinematic` (importante!)
   - `Collision Detection`: Continuous
6. **Salve como Prefab**: Arraste da Hierarchy para a pasta `Prefabs/`

---

## 🎮 Controles VR (Meta Quest 2)

- **Gatilho (Trigger)** → Atirar / Puxar arco
- **Botão B** → Iniciar/Reiniciar jogo
- **Joystick** → Andar

---

## 🔧 Ajustes Rápidos

### Alvo está virado ao contrário?
→ No modo de jogo, ajuste `Rotação Alvos` (ex: Y = 180)

### Bala está virada ao contrário?
→ Na arma, ajuste `Rotação Projetil` (ex: Y = 180)

### Alvos voando quando atingidos?
→ No Rigidbody do alvo, marque `Is Kinematic`

### Pontos não aparecem?
→ Verifique se o `GerenciadorJogo` está na cena

---

## 📝 Estrutura de Pastas

```
Assets/
├── Criar Jogo/
│   ├── Alvos/          # Prefabs dos alvos (Estático, Móvel, Resistente)
│   ├── Armas/          # Prefabs das armas (Nerf, Arco, Dardo)
│ 
├── Scripts/
│   ├── Armas/          # Atirador.cs, Arco.cs, Lancavel.cs
│   ├── Alvos/          # AlvoEstatico.cs, AlvoMovel.cs, AlvoResistente.cs
│   └── Nucleo/         # GerenciadorJogo.cs, Modos
└── Scenes/             # Suas cenas
```

**Hierarquia da Cena:**
```
- XR Origin Hands (XR Rig)
- EventSystem
- Hands Permissions Manager
- Ambiente
- Posicao Alvos
  └── Spawn1 (1) até Spawn1 (10)  ← Pontos onde alvos aparecem
- Gerenciador                      ← GerenciadorJogo aqui
- Modo De Jogo                     ← ModoLivre/Tempo/Precisao aqui
- UI
  └── Canvas                       ← Textos de pontos e UI
- Armas
  ├── Dardo
  ├── Arco e Flecha
  └── Nerf (Pistola)
```

---

## 💡 Dicas para Iniciantes

1. **Prefabs** são como templates - crie um alvo perfeito, salve como prefab, reutilize
2. **Inspector** mostra todas as configurações - se tem um campo vazio, precisa arrastar algo
3. **Hierarchy** é a lista de objetos na sua cena
4. **Teste no VR** desde cedo para ver se está funcionando

---

**Meta Quest 2 | Unity 6 | XR Interaction Toolkit**
