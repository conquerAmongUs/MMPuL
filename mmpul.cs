using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;
using AmongUs.Data;
using AmongUs.GameOptions;
﻿using AmongUs.InnerNet.GameDataMessages;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Hazel;
using InnerNet;
using Il2CppInterop.Runtime.Injection;


namespace MultiModeMod
{
    [BepInPlugin("com.example.multimode", "MMPuL", "1.8.1")]
    public class MultiModePlugin : BasePlugin
    {
		/*
		
		МОД БЫЛ НАПИСАН С ИСПОЛЬЗОВАНИЕМ AI
		ПОЭТОМУ ТУТ МОЖЕТ БЫТЬ ОЧЕНЬ МНОГО НЕАКТУАЛЬНЫХ КОММЕНТАРИЕВ
		ТАК КАК МНЕ ЛЕНЬ ИХ УДАЛЯТЬ
		
		*/
        //public enum GameMode { Standard, MiniGames, Zombie, HotPotato, FreezeTag }
        public static int GameModeTab = 1;
		public static int SettingTab = 1;
		
        //public static GameMode CurrentMode = GameMode.Standard;
        public static bool ShowMenu = true;
		public static IGameOptions SavedLobbyOptionsBackup = null;
		public static bool NeedToResetLobby = false;
		public static float ResetTimer = 0f;
		public static bool endfromgame = false;
		public static int ChanceofDeath = 0;
		public static bool NoImpLadderDChance = false;
		public static bool AllowColorCommand = false;
		public static bool AllowColorFortegreen = false;
		// classic
		public static int impcount = 1;
		public static bool MoreImpsMode = false;
		
		public static bool IsChaosActive = false;
		public static bool ChaosMode = false;
		public static float ChaosInterval = 5f; // Интервал в секундах
		
		public static bool IsSSPartyActive = false;
		public static bool IsSSPartySwapActive = false;
		public static bool SSPartyMode = false;
		public static bool SSPartyEveryMode = false;
		public static bool SSPartySwapMode = false;
		public static PlayerControl TargetPlayer = null;
		public static int selectedPlayer = 0;
		// minigames
		private static float _miniGamesTimer = 0f;
		private static bool _miniGamesApplied = false;
		// zombie
        public static float InfectionDistance = 1.5f;
		public static float ZombieMatchDuration = 60f; // Выбранная длительность матча в меню (по умолчанию 60с)
		private static float _zombieMatchTimer = 0f;    // Секундомер текущего матча
		public static sbyte _savedImpostorId = -1;
		public static float ZombieSpeedSetting = 1.0f; // Дефолтная скорость зомби
		public static float ZombieVisionSetting = 0.25f; // Дефолтная скорость зомби

        private static bool _zombieGameActive = false;
        private static float _zombieTimer = 0f;
        private static bool _patientZeroSpawned = false;
		// hot potato
		// hot potato настройки
		public static float PotatoTransferDistance = 1.5f;   // Дистанция передачи (синхронизирована с InfectionDistance)
		public static float PotatoDetonationTime = 15f;     // Через сколько взрывается картошка (10-30s)
		public static float PotatoTransferCooldown = 1.0f;  // Кулдаун передачи (0.75-1.5s)
		public static float PotatoMatchDuration = 60f;      // Длительность матча (60-300s)

		// hot potato системные таймеры
		public static bool NeedToSnapHost = false;
		public static float HostSnapTimer = 0f;
		public static Vector2 HostTargetSnapPosition = Vector2.zero;
		private static float _potatoGlobalTimer = 0f;       // Общий таймер матча
		private static float _potatoSpawnTimer = 0f;        // Таймер до выдачи первой картошки (10с)
		private static float _potatoDetonationTimer = 0f;   // Внутренний таймер тика картошки у игрока
		private static float _potatoCooldownTimer = 0f;     // Таймер кулдауна после передачи
		private static byte _playerWithPotatoId = 255;      // ID игрока, у которого сейчас картошка (255 - ни у кого)
		private static bool _potatoGameActive = false;
		private static bool _firstPotatoSpawned = false;
		// freeze tag
		// --- FREEZE TAG НАСТРОЙКИ ---
		public static float FreezeDistance = 1.5f;          // Дистанция заморозки/разморозки
		public static float FreezeDeathTime = 15f;          // Время до смерти во льду (5-30s)
		public static float TaggerFreezeCooldown = 1.5f;    // Кулдаун заморозки для салки (0.5-3s)
		public static float CrewUnfreezeCooldown = 1.5f;    // Кулдаун разморозки для мирных (0.5-3s)
		public static float FreezeMatchDuration = 90f;      // Время раунда (60-300s)
		public static int TaggerCount = 1;                  // Количество салок (1-3)
		public static float TaggerSpeedMod = 1.25f;         // Множитель скорости салки (1.0-2.0x)

		// --- FREEZE TAG СИСТЕМНЫЕ ТАЙМЕРЫ И СОСТОЯНИЯ ---
		private static bool _freezeTagGameActive = false;
		private static float _freezeTagGlobalTimer = 0f;
		private static bool _freezeTagStarted = false;
		public static Dictionary<byte, float> FrozenTimers = new Dictionary<byte, float>();   // ID игрока -> сколько он уже во льду
		public static Dictionary<byte, float> PlayerCooldowns = new Dictionary<byte, float>(); // ID игрока -> его личный кулдаун действий
		public static List<byte> TaggerIds = new List<byte>();                                // Список ID выбранных салок
		
		// --- НАСТРОЙКИ СВЕТОФОРА ---
		public static float TrafficGreenDuration = 5f;      // Время зеленого света (3-15s)
		public static float TrafficYellowDuration = 0.5f;    // Время желтого света (0.25-1s)
		public static float TrafficRedDuration = 5f;        // Время красного света (3-15s)
		public static float TrafficRedDelay = 0.5f;         // Задержка перед наказанием (0.2-1s)
		public static float TrafficMatchDuration = 60f;   

		// --- СИСТЕМНЫЕ ПЕРЕМЕННЫЕ СВЕТОФОРА ---
		public enum TrafficState { Green, Yellow, Red }
		public static TrafficState CurrentTrafficState = TrafficState.Green;
		private static bool _trafficGameActive = false;
		private static float _trafficStateTimer = 0f;
		private static float _trafficGlobalTimer = 0f;
		private static float _trafficRedGraceTimer = 0f;
		public static bool IsRedLightActive = false;
		private static bool _trafficGraceActive = false;
		private static bool _trafficOnlyOne = false;

		// Позиции игроков в момент включения КРАСНОГО света
		public static Dictionary<byte, Vector2> TrafficRedPositions = new Dictionary<byte, Vector2>();
		// Список ID игроков, которые выполнили все задания и получили иммунитет
		public static List<byte> TrafficCompletedPlayers = new List<byte>();
		
		// FFA
		public static bool IsFFAActive = false;
		
		// --- НАСТРОЙКИ COPS & ROBBERS ---
		public static float CopsCatchDistance = 1.0f;
		public static int CopsCount = 1;
		public static float CopsMatchDuration = 180f;
		public static float CopsCatchCooldownDuration = 0.5f;

		// --- СИСТЕМНЫЕ ПЕРЕМЕННЫЕ ---
		private static bool _copsGameActive = false;
		private static bool _copsTeleported = false;
		private static bool _copsReady = false;
		private static float _copsGlobalTimer = 0f;
		private static Dictionary<byte, bool> IsInJail = new Dictionary<byte, bool>(); // ID -> в тюрьме ли
		// Словарь для отслеживания времени последней поимки: <PlayerId, Time>
		private static Dictionary<int, float> _copCatchCooldowns = new Dictionary<int, float>();
		
        // Наш костыль-база данных: Ключ - PlayerId, Значение - ID цвета (3 - голубой, 15 - зеленый)
        public static Dictionary<byte, int> TrackedColors = new Dictionary<byte, int>();

        public override void Load()
        {
            var harmony = new Harmony("com.example.multimode");
            harmony.PatchAll();
            AddComponent<MMPuLGUI>();
			
			ClassInjector.RegisterTypeInIl2Cpp<Coroutines>();
			var obj = new GameObject("Coroutines");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.hideFlags = HideFlags.HideAndDontSave;
			obj.AddComponent<Coroutines>();
        }
		
        public class MMPuLGUI : MonoBehaviour
        {
            // bool isGameStarted = AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started;
			private Rect _windowRect = new Rect(30, 30, 500, 350);
			public MMPuLGUI(IntPtr ptr) : base(ptr) { } 
			private Vector2 scrollPos;
			private Vector2 scrollPos2;
			private float alpha = 0.5f;
			private bool InSettings = false;
			
			
			GUIStyle windowStyle;
			
            private void OnGUI()
            {
                if (!ShowMenu) return;
				windowStyle = new GUIStyle(GUI.skin.window);

				// берём стандартный фон и делаем из него полупрозрачный
				Texture2D bg = MakeTex(1, 1, new Color(0.2f, 0.2f, 0.2f, alpha));

				windowStyle.normal.background = bg;
				windowStyle.focused.background = bg;
				windowStyle.active.background = bg;

				windowStyle.onNormal.background = bg;
				windowStyle.onFocused.background = bg;
				windowStyle.onActive.background = bg;
				
                _windowRect = GUI.Window(8001, _windowRect, (GUI.WindowFunction)DrawWindow, "MMPuL 1.8.1 by @hostmods", windowStyle);
            }
			
			private void DrawWindow(int id)
			{
				GUILayout.BeginVertical(GUI.skin.box);
				GUILayout.BeginHorizontal();
				if (!InSettings)
				{
					GUILayout.Label("\n    <b><size=16px>Выбор режима</size></b>");
				}
				else
				{
					GUILayout.Label("\n    <b><size=16px>Настройки</size></b>");
				}
				
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("️Z", GUILayout.Width(25), GUILayout.Height(25))) {InSettings = !InSettings;}
				if (GUILayout.Button("Х", GUILayout.Width(25), GUILayout.Height(25))) {ShowMenu = false;}
				GUILayout.EndHorizontal();
				GUILayout.Space(20);
				GUILayout.EndVertical();
				
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical(GUI.skin.box);
				scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(120));
				if (!InSettings)
				{
					if (GUILayout.Button("<size=10px>Стандартный</size>")) {GameModeTab = 1;}
					if (GUILayout.Button("<size=10px>Мини-игры</size>")) {GameModeTab = 2;}
					if (GUILayout.Button("<size=10px>Зомби</size>")) {GameModeTab = 3;}
					if (GUILayout.Button("<size=10px>Hot Картошка</size>")) {GameModeTab = 4;}
					if (GUILayout.Button("<size=10px>Заморозки</size>")) {GameModeTab = 5;}
					if (GUILayout.Button("<size=10px>Светофор</size>")) {GameModeTab = 6;}
					if (GUILayout.Button("<size=10px>ФФА</size>")) {GameModeTab = 7;}
					if (GUILayout.Button("<size=10px>Копы&Робберс</size>")) {GameModeTab = 8;}
				}
				else
				{
					if (GUILayout.Button("<size=10px>Окно</size>")) {SettingTab = 1;}
					if (GUILayout.Button("<size=10px>Игра</size>")) {SettingTab = 2;}
					if (GUILayout.Button("<size=10px>Прочее</size>")) {SettingTab = 3;}
				}
				GUILayout.EndScrollView();
				GUILayout.EndVertical();
				
				GUILayout.FlexibleSpace();
				
				GUILayout.BeginVertical(GUI.skin.box);
				scrollPos2 = GUILayout.BeginScrollView(scrollPos2, GUILayout.Width(340)); 
				if (!InSettings)
				{
					switch (GameModeTab)
					{
						case 1:
							GUILayout.Label("<b>Стандартный Режим</b>");
							GUILayout.Space(10);
							MoreImpsMode = GUILayout.Toggle(MoreImpsMode, "Кастомное количество предателей");
							if (MoreImpsMode)
							{
								GUILayout.Label($"Количество предателей: {impcount:F0}");
								impcount = Mathf.RoundToInt(GUILayout.HorizontalSlider(impcount, 1f, 15f));
							}
							ChaosMode = GUILayout.Toggle(ChaosMode, "Хаос режим");
							if (ChaosMode)
							{
								GUILayout.Label($"Между случайными событиями: {ChaosInterval:F1}s");
								ChaosInterval = Mathf.Round(GUILayout.HorizontalSlider(ChaosInterval, 0.5f, 15f) / 0.5f) * 0.5f;
							}
							SSPartyMode = GUILayout.Toggle(SSPartyMode, "Морф Вечеринка");
							if (SSPartyMode)
							{
								SSPartyEveryMode = GUILayout.Toggle(SSPartyEveryMode, "Менять каждый раунд");
								SSPartySwapMode = GUILayout.Toggle(SSPartySwapMode, "Свап режим (Все меняются обликом)");
								if (!SSPartySwapMode)
								{
									try
									{
										var AllPC = PlayerControl.AllPlayerControls;
										if (selectedPlayer == 0)
										{
											GUILayout.Label("Игрок: Случайный");
										}
										else
										{
											PlayerControl player = AllPC[selectedPlayer - 1];
											GUILayout.Label($"Игрок: {player.Data.PlayerName}");
										}
										selectedPlayer = Mathf.RoundToInt(GUILayout.HorizontalSlider(selectedPlayer, 0, AllPC.Count));
									}
									catch {selectedPlayer = 0;}
								}
							}
					
							break;

						case 2:
							GUILayout.Label("<b>Мини-игры</b>");
							GUILayout.Space(10);
							GUILayout.Label("На данный момент в этом режиме\nпредатели просто красятся в красный а мирные в голубой.");
							break;

						case 3:
							GUILayout.Label("<b>Зомби Режим</b>");
							GUILayout.Space(10);
							
							GUILayout.Label($"Дистанция заражения: {InfectionDistance:F1}m");
							InfectionDistance = GUILayout.HorizontalSlider(InfectionDistance, 0.5f, 2.0f);
							
							GUILayout.Label($"Авто-победа мирных: {ZombieMatchDuration:F0}s");
							ZombieMatchDuration = GUILayout.HorizontalSlider(ZombieMatchDuration, 30f, 120f);
							
							GUILayout.Label($"Скорость для Зомби: {ZombieSpeedSetting:F2}x");
							float rawSpeedValue = GUILayout.HorizontalSlider(ZombieSpeedSetting, 0.50f, 3.00f);
							ZombieSpeedSetting = Mathf.Round(rawSpeedValue / 0.05f) * 0.05f;
							
							GUILayout.Label($"Зрение для Зомби: {ZombieVisionSetting:F2}x");
							float rawVisionValue = GUILayout.HorizontalSlider(ZombieVisionSetting, 0.25f, 1.00f);
							ZombieVisionSetting = Mathf.Round(rawVisionValue / 0.05f) * 0.05f;
							break;
							
						case 4:
							GUILayout.Label("<b>Горячая картошка</b>");
							GUILayout.Space(10);
							
							GUILayout.Label($"Дистанция передачи: {InfectionDistance:F1}m");
							InfectionDistance = GUILayout.HorizontalSlider(InfectionDistance, 0.5f, 2.0f);
							PotatoTransferDistance = InfectionDistance; // Синхронизируем

							GUILayout.Label($"Время взрыва картошки: {PotatoDetonationTime:F0}s");
							PotatoDetonationTime = GUILayout.HorizontalSlider(PotatoDetonationTime, 10f, 30f);
							PotatoDetonationTime = Mathf.Round(PotatoDetonationTime);

							GUILayout.Label($"Задержка передачи: {PotatoTransferCooldown:F2}s");
							float rawCd = GUILayout.HorizontalSlider(PotatoTransferCooldown, 0.75f, 1.5f);
							PotatoTransferCooldown = Mathf.Round(rawCd / 0.05f) * 0.05f;

							GUILayout.Label($"Длительность матча: {PotatoMatchDuration:F0}s");
							PotatoMatchDuration = GUILayout.HorizontalSlider(PotatoMatchDuration, 60f, 300f);
							PotatoMatchDuration = Mathf.Round(PotatoMatchDuration / 10f) * 10f; // Шаг 10 секунд
							break;
							
						case 5:
							GUILayout.Label("<b>Заморозки</b>");
							GUILayout.Space(10);
							
							bool isHideNSeek = false;
							if (GameManager.Instance != null && GameManager.Instance.LogicOptions != null && GameManager.Instance.LogicOptions.currentGameOptions != null)
							{
								var gMode = GameManager.Instance.LogicOptions.currentGameOptions.GameMode;
								if (gMode == GameModes.HideNSeek || gMode == GameModes.SeekFools) isHideNSeek = true;
							}

							if (!isHideNSeek)
							{
								GUILayout.Label("<b>Режим Заморозки работает только\nв режиме ПРЯТОК (Hide & Seek)!</b>");
							}
							else
							{
								GUILayout.Label($"Дистанция контакта: {FreezeDistance:F1}m");
								FreezeDistance = GUILayout.HorizontalSlider(FreezeDistance, 0.5f, 2.0f);

								GUILayout.Label($"Время до смерти во льду: {FreezeDeathTime:F0}s");
								FreezeDeathTime = Mathf.Round(GUILayout.HorizontalSlider(FreezeDeathTime, 5f, 30f));

								GUILayout.Label($"Кд заморозки (Салка): {TaggerFreezeCooldown:F2}s");
								float rawTaggerCd = GUILayout.HorizontalSlider(TaggerFreezeCooldown, 0.5f, 3.0f);
								TaggerFreezeCooldown = Mathf.Round(rawTaggerCd / 0.05f) * 0.05f;

								GUILayout.Label($"Кд разморозки (Мирный): {CrewUnfreezeCooldown:F2}s");
								float rawCrewCd = GUILayout.HorizontalSlider(CrewUnfreezeCooldown, 0.5f, 3.0f);
								CrewUnfreezeCooldown = Mathf.Round(rawCrewCd / 0.05f) * 0.05f;

								GUILayout.Label($"Длительность матча: {FreezeMatchDuration:F0}s");
								FreezeMatchDuration = Mathf.Round(GUILayout.HorizontalSlider(FreezeMatchDuration, 60f, 300f) / 10f) * 10f;

								GUILayout.Label($"Количество Салок: {TaggerCount}");
								TaggerCount = Mathf.RoundToInt(GUILayout.HorizontalSlider(TaggerCount, 1f, 3f));

								GUILayout.Label($"Скорость Салки: {TaggerSpeedMod:F2}x");
								float rawSpeed = GUILayout.HorizontalSlider(TaggerSpeedMod, 1.00f, 2.00f);
								TaggerSpeedMod = Mathf.Round(rawSpeed / 0.05f) * 0.05f;
							}
							break;
							
						case 6:
							GUILayout.Label("<b>Светофор</b>");
							GUILayout.Space(10);
							// 1. Время зеленого света (Шаг: 1)
							GUILayout.Label($"Время зеленого света: {TrafficGreenDuration:F0}s");
							TrafficGreenDuration = Mathf.Round(GUILayout.HorizontalSlider(TrafficGreenDuration, 3f, 15f));

							// 2. Время желтого света (Шаг: 0.05)
							GUILayout.Label($"Время желтого света: {TrafficYellowDuration:F2}s");
							float rawYellow = GUILayout.HorizontalSlider(TrafficYellowDuration, 0.25f, 2.00f);
							TrafficYellowDuration = Mathf.Round(rawYellow / 0.05f) * 0.05f;
							
							_trafficOnlyOne = GUILayout.Toggle(_trafficOnlyOne, "Побеждает первый выполнивший задания");
							
							// 3. Время красного света (Шаг: 1)
							GUILayout.Label($"Время красного света: {TrafficRedDuration:F0}s");
							TrafficRedDuration = Mathf.Round(GUILayout.HorizontalSlider(TrafficRedDuration, 3f, 15f));

							// 4. Задержка красного света (Шаг: 0.1)
							GUILayout.Label($"Задержка активации красного: {TrafficRedDelay:F1}s");
							float rawDelay = GUILayout.HorizontalSlider(TrafficRedDelay, 0.2f, 1.2f);
							TrafficRedDelay = Mathf.Round(rawDelay / 0.1f) * 0.1f;
							
							GUILayout.Label($"Длительность матча: {TrafficMatchDuration:F0}s");
							TrafficMatchDuration = GUILayout.HorizontalSlider(TrafficMatchDuration, 60f, 300f);
							TrafficMatchDuration = Mathf.Round(TrafficMatchDuration / 10f) * 10f; // Шаг 10 секунд
							break;
							
						case 7:
							GUILayout.Label("<b>Все Против Всех</b>");
							GUILayout.Space(10);
							GUILayout.Label("В данном режиме настройки не прилагаются.");
							break;
							
						case 8:
							GUILayout.Label("<b>Копы И Приступники</b>");
							GUILayout.Space(10);
							bool isHideNSeek2 = false;
							if (GameManager.Instance != null && GameManager.Instance.LogicOptions != null && GameManager.Instance.LogicOptions.currentGameOptions != null)
							{
								var gMode2 = GameManager.Instance.LogicOptions.currentGameOptions.GameMode;
								if (gMode2 == GameModes.HideNSeek || gMode2 == GameModes.SeekFools) isHideNSeek2 = true;
							}

							if (!isHideNSeek2)
							{
								GUILayout.Label("<b>Режим Копы и Преступник работает только\nв режиме ПРЯТОК (Hide & Seek)!</b>");
							}
							else
							{
								GUILayout.Label($"Дистанция поимки: {CopsCatchDistance:F1}m");
								CopsCatchDistance = GUILayout.HorizontalSlider(CopsCatchDistance, 0.5f, 2.0f);
								
								GUILayout.Label($"Перезарядка поимки: {CopsCatchCooldownDuration:F1}s");
								CopsCatchCooldownDuration = Mathf.Round(GUILayout.HorizontalSlider(CopsCatchCooldownDuration, 0.5f, 1f) / 0.1f) * 0.1f;
								
								GUILayout.Label($"Количество копов: {CopsCount}");
								CopsCount = Mathf.RoundToInt(GUILayout.HorizontalSlider(CopsCount, 1f, 3f));
								
								GUILayout.Label($"Длительность матча: {CopsMatchDuration:F0}s");
								CopsMatchDuration = Mathf.Round(GUILayout.HorizontalSlider(CopsMatchDuration, 60f, 300f) / 10f) * 10f;
							}
							break;

						default:
							break;
					}
				}
				else 
				{
					switch (SettingTab)
					{
						case 1:
							GUILayout.Label("<b>Настройки окна</b>");
							GUILayout.Space(10);
							GUILayout.Label("Прозрачность окна: " + alpha);
							alpha = GUILayout.HorizontalSlider(alpha, 0f, 1f);
							break;

						case 2:
							GUILayout.Label("<b>Настройки игры</b>");
							GUILayout.Space(10);
							GUILayout.BeginHorizontal();
							if (GUILayout.Button("<b>Начать игру</b>"))
							{
								if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined)
								{
									AmongUsClient.Instance.SendStartGame();
								}
							}
							if (GUILayout.Button("<b>Закончить игру</b>"))
							{
								_zombieGameActive = false;
								_potatoGameActive = false;
								_freezeTagGameActive = false;
								_trafficGameActive = false;
								_copsGameActive = false;
								GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
							}
							GUILayout.EndHorizontal();
							GUILayout.Space(5);
							GUILayout.Label($"Шанс смерти при использовании лестницы: {ChanceofDeath}%");
							ChanceofDeath = Mathf.RoundToInt(GUILayout.HorizontalSlider(ChanceofDeath, 0, 100));
							NoImpLadderDChance = GUILayout.Toggle(NoImpLadderDChance, "Предатели тоже могут умереть от лестницы");
							GUILayout.Space(5);
							AllowColorCommand = GUILayout.Toggle(AllowColorCommand, "Разрешить всем использовать /color");
							AllowColorFortegreen = GUILayout.Toggle(AllowColorFortegreen, "Разрешить фортегрин в /color");
							break;

						case 3:
							GUILayout.Label("<b>Настройки прочее</b>");
							GUILayout.Space(10);
							break;

						default:
							
							break;
					}
				}
				GUILayout.EndScrollView();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				
				GUI.DragWindow();
			}
			
			Texture2D MakeTex(int width, int height, Color col)
			{
				Color[] pix = new Color[width * height];
				for (int i = 0; i < pix.Length; i++)
					pix[i] = col;

				Texture2D result = new Texture2D(width, height);
				result.SetPixels(pix);
				result.Apply();
				return result;
			}
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
        public static class ControlsPatch
        {
            public static void Postfix()
            {
                if (Input.GetKeyDown(KeyCode.F3)) ShowMenu = !ShowMenu;

				if (MultiModePlugin.NeedToSnapHost)
				{
					// Накапливаем время между кадрами
					MultiModePlugin.HostSnapTimer += Time.deltaTime;

					// Как только прошло 0.15 секунды (хватает, чтобы игра завершила рывок к трупу)
					if (MultiModePlugin.HostSnapTimer >= 0.15f)
					{
						if (PlayerControl.LocalPlayer != null)
						{
							// Возвращаем хоста обратно на исходную позицию
							PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(MultiModePlugin.HostTargetSnapPosition);
							UnityEngine.Debug.Log($"[HotPotato Timer] Хост возвращен на позицию: {MultiModePlugin.HostTargetSnapPosition}");
						}
						
						// Выключаем триггер
						MultiModePlugin.NeedToSnapHost = false;
						MultiModePlugin.HostSnapTimer = 0f;
					}
				}
				

                if (AmongUsClient.Instance.AmHost && GameData.Instance != null)
                {
                    bool isGameStarted = AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started;

                    if (isGameStarted)
					{
						if (GameModeTab == 3)
						{
							UpdateZombieMode();
						}
						else if (GameModeTab == 2)
						{
							UpdateMiniGamesMode(); // Вызываем новый метод с задержкой
						}
						else if (GameModeTab == 4)
						{
							UpdateHotPotatoMode(); // Вызываем логику картошки
						}
						else if (GameModeTab == 5)
						{
							UpdateFreezeTagMode(); // 
						}
						else if (GameModeTab == 6)
						{
							UpdateTrafficLightMode(); // Логика светофора
						}
						else if (GameModeTab == 8)
						{
							UpdateCopsAndRobbersMode();
						}
					}
					else
					{
						IsSSPartySwapActive = false;
						IsSSPartyActive = false;
						IsChaosActive = false;
						// Сброс всех состояний при выходе в лобби
						_zombieGameActive = false;
						_patientZeroSpawned = false;
						_zombieTimer = 0f;
						_miniGamesApplied = false;
						_miniGamesTimer = 0f;
						_zombieMatchTimer = 0f;
						
						// Сброс Hot Potato
						_potatoGameActive = false;
						_firstPotatoSpawned = false;
						_potatoGlobalTimer = 0f;
						_potatoSpawnTimer = 0f;
						_potatoDetonationTimer = 0f;
						_potatoCooldownTimer = 0f;
						_playerWithPotatoId = 255;
						
						// Сброс Freeze Tag состояний
						_freezeTagGameActive = false; // <- ДОБАВИТЬ СТРОКУ
						_freezeTagGlobalTimer = 0f;  // <- ДОБАВИТЬ СТРОКУ
						_freezeTagStarted = false;    // <- ДОБАВИТЬ СТРОКУ
						FrozenTimers.Clear();         // <- ДОБАВИТЬ СТРОКУ
						PlayerCooldowns.Clear();      // <- ДОБАВИТЬ СТРОКУ
						TaggerIds.Clear();            // <- ДОБАВИТЬ СТРОКУ
						
						_trafficGameActive = false;
						_trafficStateTimer = 0f;
						_trafficRedGraceTimer = 0f;
						_trafficGraceActive = false;
						_trafficGlobalTimer = 0f;
						TrafficRedPositions.Clear();
						TrafficCompletedPlayers.Clear();
						
						IsFFAActive = false;
						
						_copsGameActive = false;
						_copsTeleported = false;
						_copsReady = false;
						_copsGlobalTimer = 0f;
						IsInJail.Clear();
						_copCatchCooldowns.Clear();
						
						TrackedColors.Clear();
					}
                }
				if (!MultiModePlugin.NeedToResetLobby) return;
				// Используем твою проверку из Гидры: если корабль еще существует, значит мы еще не в лобби!
				if (ShipStatus.Instance != null) return;

				// Если мы уже в лобби, начинаем копить время (Time.deltaTime — время между кадрами)
				MultiModePlugin.ResetTimer += Time.deltaTime;

				// Ждем ровно 3 секунды, пока всё окончательно прогрузится
				if (MultiModePlugin.ResetTimer >= 3.0f)
				{
					MultiModePlugin.NeedToResetLobby = false; // Выключаем триггер, чтобы сброс сработал ТОЛЬКО ОДИН РАЗ
					MultiModePlugin.ResetTimer = 0f;
					
					// Наш старый добрый метод сброса из бэкапа
					if (endfromgame)
					{
						endfromgame = false;
						CustomNetworkHelper.ResetAllPlayersToLobbySettings();
					}
				}
            }
        }
		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
		public static class OnGameEndPatch
		{
			public static void Postfix()
			{
				endfromgame = true;
			}
		}
		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
		public static class OnDisconnectedPatch
		{
			public static void Postfix()
			{
				endfromgame = false;
			}
		}
		
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		public static class MurderPlayerPatch
		{
			[HarmonyPostfix]
			public static void Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
			{
				if (__instance == null || target == null) return;
				if (!IsFFAActive) return;
				if (!__instance.Data.IsDead)
				{
					target.RpcSetRole(RoleTypes.CrewmateGhost, true);
				}
			}
		}
		[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
		public static class GetAdjustedNumImpostorsPatch
		{
			static void Postfix(ref int __result)
			{
				if (GameModeTab == 1 && MoreImpsMode)
					__result = impcount;
			}
		}
		[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
        public static class DisableRoleAssignPatch
        {
            public static bool Prefix()
            {
				if (GameModeTab == 4 || GameModeTab == 5 || GameModeTab == 6 || GameModeTab == 8)
				{
					foreach (PlayerControl p in PlayerControl.AllPlayerControls)
					{
						p.RpcSetRole(RoleTypes.Crewmate, false);
					}
					return false;
				}
				/* if (GameModeTab == 1 && MoreImpsMode)
				{
					var players = PlayerControl.AllPlayerControls;

					System.Random random = new();

					for (int i = players.Count - 1; i > 0; i--)
					{
						int j = random.Next(i + 1);
						(players[i], players[j]) = (players[j], players[i]);
					}

					int impostorCount = Math.Min(impcount, players.Count);

					for (int i = 0; i < players.Count; i++)
					{
						players[i].RpcSetRole(
							i < impostorCount ? RoleTypes.Impostor : RoleTypes.Crewmate,
							false);
					}
					return false;
				} */
				if (GameModeTab == 7)
				{
					IsFFAActive = true;
					Coroutines.Instance.CoFFAModeStart();
					return false;
				}
				if (GameModeTab == 3)
				{
					var zombieImp = PlayerControl.AllPlayerControls[UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count)];
					_savedImpostorId = (sbyte)zombieImp.PlayerId;
					BatchedMessage batch = new BatchedMessage(zombieImp.Data.ClientId);
					batch.QueueSetRole(zombieImp, RoleTypes.Impostor, false);
					batch.FinishBatch();
					foreach (PlayerControl p in PlayerControl.AllPlayerControls)
					{
						p.RpcSetRole(RoleTypes.Crewmate, false);
					}
					return false;
				}
				return true;
			}
		}
		
		[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
		public static class LobbyStartResetPatch
		{
			[HarmonyPostfix]
			public static void Postfix()
			{
				// Проверяем, что мы хост комнаты, так как только хост имеет право раздавать настройки
				if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
				{
					MultiModePlugin.NeedToResetLobby = true;
					MultiModePlugin.ResetTimer = 0f;
				}
			}
		}
        // Ловим старт раунда для первичной выдачи цветов
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
		public static class GameStartPatch
		{
			public static void Postfix()
			{
				if (!AmongUsClient.Instance.AmHost || GameData.Instance == null) return;
				
				// Как только карта загрузилась и игра началась, намертво клонируем настройки лобби в бэкап
				if (GameManager.Instance != null && GameManager.Instance.LogicOptions != null)
				{
					MultiModePlugin.SavedLobbyOptionsBackup = CustomNetworkHelper.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				}

				TrackedColors.Clear(); // Очищаем кэш
				if (GameModeTab == 1)
				{
					if (ChaosMode) {IsChaosActive = true; Coroutines.Instance.CoChaosModeStart();}
					if (SSPartyMode) 
					{
						if (!SSPartySwapMode)
						{
							IsSSPartyActive = true;
							if (selectedPlayer == 0)
							{
								var players = PlayerControl.AllPlayerControls;
								TargetPlayer = players[UnityEngine.Random.Range(0, players.Count)];
							}
							else
							{
								TargetPlayer = PlayerControl.AllPlayerControls[selectedPlayer - 1];
							}
							Coroutines.Instance.CoSSPartyStart(TargetPlayer);
						}
						else
						{
							IsSSPartySwapActive = true;
							Coroutines.Instance.CoSSPartySwapStart();
						}
					}
				}
				else if (GameModeTab == 2)
				{
					_miniGamesTimer = 0f;
					_miniGamesApplied = false; // Разрешаем запуск таймера мини-игры
				}
				else if (GameModeTab == 3)
				{
					// Для зомби оставляем как есть — красим всех в голубой (10) на старте
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && p.Object != null) SetPlayerColorSecure(p.Object, 10);
					}
					_zombieTimer = 0f;
					_patientZeroSpawned = false;
					_zombieGameActive = true;
				}
				else if (GameModeTab == 4)
				{
					// На старте все игроки получают голубой цвет
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && p.Object != null) SetPlayerColorSecure(p.Object, 10);
					}
					_potatoGameActive = true;
					_firstPotatoSpawned = false;
					_potatoGlobalTimer = 0f;
					_potatoSpawnTimer = 0f;
					_playerWithPotatoId = 255;
				}
				else if (GameModeTab == 5) // <- ДОБАВИТЬ ВЕСЬ ЭТОТ БЛОК СЮДА
				{
					// Изначально красим всех в лаймовый (11), пока идет отсчет до выбора салок
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && p.Object != null) SetPlayerColorSecure(p.Object, 11);
					}
					
					_freezeTagGameActive = true;
					_freezeTagGlobalTimer = 0f;
					_freezeTagStarted = false;
					FrozenTimers.Clear();
					PlayerCooldowns.Clear();
					TaggerIds.Clear();
				}
				else if (GameModeTab == 6)
				{
					// На старте перекрашиваем всех игроков в Лаймовый (Зеленый свет)
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && p.Object != null) SetPlayerColorSecure(p.Object, 11);
					}
					_trafficGameActive = true;
					CurrentTrafficState = TrafficState.Green;
					_trafficStateTimer = 0f;
					_trafficGraceActive = false;
					TrafficRedPositions.Clear();
					TrafficCompletedPlayers.Clear();
				}
				else if (GameModeTab == 8)
				{
					// Очищаем тюрьму
					IsInJail.Clear();
					_copCatchCooldowns.Clear();
					_copsGlobalTimer = 0f;
					_copsGameActive = true;
					_copsTeleported = false;
					_copsReady = false;
					var players = PlayerControl.AllPlayerControls;

					System.Random random = new();

					for (int i = players.Count - 1; i > 0; i--)
					{
						int j = random.Next(i + 1);
						(players[i], players[j]) = (players[j], players[i]);
					}

					int copsCount = Math.Min(CopsCount, players.Count);

					for (int i = 0; i < players.Count; i++)
					{
						if (i < copsCount)
						{
							SetPlayerColorSecure(players[i], 1);
						}
						else
						{
							IsInJail[players[i].PlayerId] = false;
							SetPlayerColorSecure(players[i], 6);
						}
					}
				}
			}
		}

        // Логика зомби-режима
		private static void UpdateZombieMode()
		{
			if (!_zombieGameActive) return;

			// 1. Честный таймер 10 секунд на появление Нулевого Пациента
			// В начале файла или класса добавь переменную для хранения ID предателя (если её еще нет)
			// private sbyte _savedImpostorId = -1;

			if (!_patientZeroSpawned)
			{
				// 1. МГНОВЕННАЯ ЗАЩИТА: Забираем кнопку KILL в первую же секунду раунда
				/* if (_zombieTimer < 10f) 
				{
					foreach (var p in GameData.Instance.AllPlayers)
					{
						// Находим оригинального импостора, которого выбрала игра
						if (p != null && p.Role != null && p.Role.IsImpostor && p.Object != null)
						{
							_savedImpostorId = (sbyte)p.PlayerId; // Запоминаем его ID на будущее
							
							// Мгновенно превращаем его в мирного для сети, лишая кнопки убийства
							p.Object.RpcSetRole(RoleTypes.Crewmate, true);
							UnityEngine.Debug.Log($"[Mod] Кнопка KILL заблокирована! Предатель {p.PlayerName} временно стал мирным.");
							break;
						}
					}
				} */

				_zombieTimer += Time.deltaTime;

				// 2. АКТИВАЦИЯ ЗОМБИ: Через 25 секунд превращаем его в Нулевого Пациента
				if (_zombieTimer >= 25f)
				{
					// Если мы успешно запомнили импостора на старте
					if (_savedImpostorId != -1)
					{
						var p = GameData.Instance.GetPlayerById((byte)_savedImpostorId);
						if (p != null && p.Object != null)
						{
							// Он уже Crewmate благодаря коду выше, так что просто красим его
							SetPlayerColorSecure(p.Object, 2); // Нулевой пациент стал зеленым зомби
							_patientZeroSpawned = true;
							UnityEngine.Debug.Log($"[MMPuL] {p.PlayerName} стал Нулевым Пациентом.");
						}
					}
					else
					{
						// Резервный вариант: если на старте не успели поймать, ищем любого живого (хотя первый if должен отработать)
						foreach (var p in GameData.Instance.AllPlayers)
						{
							if (p != null && p.Object != null && !p.IsDead)
							{
								p.Object.RpcSetRole(RoleTypes.Crewmate, true);
								SetPlayerColorSecure(p.Object, 2);
								_patientZeroSpawned = true;
								break;
							}
						}
					}
				}
			}
			
			// 2. Проверка дистанций касания (только после того, как зомби появился)
			if (!_patientZeroSpawned) return; // Пока таймер не вышел, никто никого не заражает
			// Отсчет времени идет только после спавна зомби
			_zombieMatchTimer += Time.deltaTime;

			// Если время вышло — зомби побеждают
			if (_zombieMatchTimer >= ZombieMatchDuration)
			{
				_zombieGameActive = false;
				UnityEngine.Debug.Log("[MMPuL] Зомби проиграли по времени");
				Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask);
				return;
			}

			// 3. Проверка дистанций касания (с учетом вентиляций, лестниц и платформ)
			var players = GameData.Instance.AllPlayers;
			bool humansLeft = false;

			for (int i = 0; i < players.Count; i++)
			{
				var p1 = players[i];
				if (p1 == null || p1.Object == null || p1.IsDead) continue;

				int p1Color = GetPlayerColor(p1.Object);
				if (p1Color == 10) humansLeft = true; // Нашли выжившего человека (Голубой)

				for (int j = i + 1; j < players.Count; j++)
				{
					var p2 = players[j];
					if (p2 == null || p2.Object == null || p2.IsDead) continue;

					int p2Color = GetPlayerColor(p2.Object);

					// Проверяем контакт «Зомби (2) <-> Человек (10)»
					if ((p1Color == 2 && p2Color == 10) || (p1Color == 10 && p2Color == 2))
					{
						// Проверяем, находится ли кто-то из них в вентиляции, на лестнице или платформе
						// Если да — заражение невозможно, пропускаем эту пару игроков
						if (p1.Object.inVent || p1.Object.onLadder || p1.Object.inMovingPlat ||
						    p2.Object.inVent || p2.Object.onLadder || p2.Object.inMovingPlat)
						{
							continue; 
						}

						float dist = Vector2.Distance(p1.Object.transform.position, p2.Object.transform.position);
						if (dist <= InfectionDistance)
						{
							// Заражаем! Кто был голубым (10), становится зеленым (2)
							if (p1Color == 10) SetPlayerColorSecure(p1.Object, 2);
							if (p2Color == 10) SetPlayerColorSecure(p2.Object, 2);
						}
					}
				}
			}

			// 3. Конец игры (если людей больше нет)
			if (!humansLeft && players.Count > 1)
			{
				_zombieGameActive = false;
				UnityEngine.Debug.Log("[MMPuL] Зомби всех заразили");
				
				foreach (var p in players)
                {
                    if (p != null && p.Object != null && !p.IsDead && !p.Role.IsImpostor)
                    {
						Coroutines.Instance.CoEndGameStart(GameOverReason.ImpostorsByKill);
                    }
                }
			}
		}
		private static void UpdateMiniGamesMode()
		{
			if (_miniGamesApplied) return; // Если уже покрасили, ничего не делаем

			_miniGamesTimer += Time.deltaTime;
			if (_miniGamesTimer >= 8f) // Ждем ровно 2 секунды после старта карты
			{
				foreach (var p in GameData.Instance.AllPlayers)
				{
					if (p == null || p.Object == null) continue;

					// Теперь p.Role.IsImpostor сработает на 100%, так как пакеты уже пришли
					bool isImpostor = p.Role != null && p.Role.IsImpostor;
					byte colorId = isImpostor ? (byte)0 : (byte)10; // 0 - Красный, 10 - Голубой
					
					SetPlayerColorSecure(p.Object, colorId);
				}
				_miniGamesApplied = true; // Отключаем таймер, красим один раз за раунд
			}
		}
		private static void UpdateHotPotatoMode()
		{
			if (!_potatoGameActive) return;

			// 1. Таймер глобальной длительности игры
			_potatoGlobalTimer += Time.deltaTime;
			if (_potatoGlobalTimer >= PotatoMatchDuration)
			{
				_potatoGameActive = false;
				UnityEngine.Debug.Log("[MMPuL] Время вышло! Выжившие победили!");
				Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask);
				return;
			}

			// Ограничение кулдауна передачи между игроками
			if (_potatoCooldownTimer > 0f) _potatoCooldownTimer -= Time.deltaTime;

			// 2. Ожидание 10 секунд и спавн первой картошки
			if (!_firstPotatoSpawned)
			{
				_potatoSpawnTimer += Time.deltaTime;
				if (_potatoSpawnTimer >= 10f)
				{
					// Выбираем случайного живого игрока
					List<PlayerControl> alivePlayers = new List<PlayerControl>();
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && !p.IsDead && p.Object != null) alivePlayers.Add(p.Object);
					}

					if (alivePlayers.Count > 0)
					{
						int index = UnityEngine.Random.Range(0, alivePlayers.Count);
						PlayerControl target = alivePlayers[index];

						_playerWithPotatoId = target.PlayerId;
						_firstPotatoSpawned = true;
						_potatoDetonationTimer = 0f;

						SetPlayerColorSecure(target, 4); // Выдаем картошку — Оранжевый (4)
						UnityEngine.Debug.Log($"[MMPuL] Первая картошка выдана: {target.Data.PlayerName}");
					}
				}
				return; // Пока первая картошка не спавнилась, физику касаний не считаем
			}

			// 3. Логика удержания карточки и взрыва
			// 3. Логика удержания карточки и взрыва
			var currentPotatoPlayer = GameData.Instance.GetPlayerById(_playerWithPotatoId)?.Object;
			
			// --- ЗАЩИТА ОТ ВЫЛЕТА ИГРОКА С КАРТОШКОЙ ---
			// Если игрок с картошкой вышел, отключился или его объект стёрся из памяти игры
			if (currentPotatoPlayer == null || currentPotatoPlayer.Data == null || currentPotatoPlayer.Data.Disconnected)
			{
				UnityEngine.Debug.Log("[MMPuL] Игрок с картошкой вышел из игры! Ищем нового ведущего...");
				
				List<PlayerControl> remainingPlayers = new List<PlayerControl>();
				foreach (var p in GameData.Instance.AllPlayers)
				{
					// Берем только тех, кто реально в игре, жив и не отключен
					if (p != null && !p.IsDead && p.Object != null && !p.Disconnected && p.PlayerId != _playerWithPotatoId) 
						remainingPlayers.Add(p.Object);
				}

				if (remainingPlayers.Count > 1)
				{
					// Передаем картошку случайному выжившему
					int nextIndex = UnityEngine.Random.Range(0, remainingPlayers.Count);
					PlayerControl nextTarget = remainingPlayers[nextIndex];

					_playerWithPotatoId = nextTarget.PlayerId;
					_potatoDetonationTimer = 0f; // Сбрасываем таймер взрыва, чтобы у нового игрока был честный шанс убежать
					_potatoCooldownTimer = PotatoTransferCooldown; // Ставим задержку на передачу

					SetPlayerColorSecure(nextTarget, 4); // Красим в оранжевый
					UnityEngine.Debug.Log($"[MMPuL] Новая картошка выдана игроку: {nextTarget.Data.PlayerName} взамен вылетевшего.");
				}
				else
				{
					// Если людей больше вообще не осталось — завершаем игру
					_potatoGameActive = false;
					Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask);
				}
				return; // Выходим из этого кадра Update, чтобы не выполнять код ниже
			}
			// 
			// var currentPotatoPlayer = GameData.Instance.GetPlayerById(_playerWithPotatoId)?.Object;
			
			if (currentPotatoPlayer != null && !currentPotatoPlayer.Data.IsDead)
			{
				_potatoDetonationTimer += Time.deltaTime;

				// Взрыв игрока по истечении времени
				if (_potatoDetonationTimer >= PotatoDetonationTime)
				{
					UnityEngine.Debug.Log($"[MMPuL] Игрок {currentPotatoPlayer.Data.PlayerName} взорвался!");

					currentPotatoPlayer.RpcSetRole(RoleTypes.CrewmateGhost, true);
					// Ищем следующего живого игрока для передачи картошки
					List<PlayerControl> remainingPlayers = new List<PlayerControl>();
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && !p.IsDead && p.Object != null && p.PlayerId != _playerWithPotatoId) 
							remainingPlayers.Add(p.Object);
					}

					if (remainingPlayers.Count > 1)
					{
						int nextIndex = UnityEngine.Random.Range(0, remainingPlayers.Count);
						PlayerControl nextTarget = remainingPlayers[nextIndex];

						_playerWithPotatoId = nextTarget.PlayerId;
						_potatoDetonationTimer = 0f;
						_potatoCooldownTimer = PotatoTransferCooldown;

						SetPlayerColorSecure(nextTarget, 4);
					}
					else
					{
						_potatoGameActive = false;
						Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask);
					}
					return;
				}

				// 4. Передача картошки при касании (Оранжевый -> Голубой)
				// Если кулдаун передачи активен — касания временно заблокированы
				if (_potatoCooldownTimer <= 0f)
				{
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p == null || p.IsDead || p.Object == null || p.PlayerId == _playerWithPotatoId) continue;

						// Защита зон (вентиляции, лестницы)
						if (currentPotatoPlayer.inVent || currentPotatoPlayer.onLadder || currentPotatoPlayer.inMovingPlat ||
							p.Object.inVent || p.Object.onLadder || p.Object.inMovingPlat) continue;

						float dist = Vector2.Distance(currentPotatoPlayer.transform.position, p.Object.transform.position);
						if (dist <= PotatoTransferDistance)
						{
							// Меняем владельца картошки
							PlayerControl oldPotatoOwner = currentPotatoPlayer;
							PlayerControl newPotatoOwner = p.Object;

							_playerWithPotatoId = newPotatoOwner.PlayerId;
							_potatoCooldownTimer = PotatoTransferCooldown; // Включаем задержку
							// Таймер взрыва НЕ сбрасывается в ноль, а продолжает тикать дальше для азарта (или сбрось в 0f, если хочешь облегчить игру)
							
							SetPlayerColorSecure(oldPotatoOwner, 10); // Старый становится голубым
							SetPlayerColorSecure(newPotatoOwner, 4);  // Новый становится оранжевым

							UnityEngine.Debug.Log($"[MMPuL] Картошка передана от {oldPotatoOwner.Data.PlayerName} к {newPotatoOwner.Data.PlayerName}");
							break;
						}
					}
				}
			}
		}
        private static void UpdateFreezeTagMode()
		{
			if (!_freezeTagGameActive) return;

			// Защита: Если игра запущена не в Прятках, мгновенно выключаем режим во избежание киков античита
			var currentOptions = GameManager.Instance.LogicOptions.currentGameOptions;
			if (currentOptions.GameMode != GameModes.HideNSeek && currentOptions.GameMode != GameModes.SeekFools)
			{
				_freezeTagGameActive = false;
				return;
			}

			_freezeTagGlobalTimer += Time.deltaTime;

			// 1. СТАДИЯ НАЧАЛА: 10 секунд ожидания (разбежаться), затем выбор Салок
			if (!_freezeTagStarted)
			{
				if (_freezeTagGlobalTimer < 10f) {return;}

				// Прошло 10 секунд — выбираем случайных Салок
				List<PlayerControl> pool = new List<PlayerControl>();
				foreach (var p in GameData.Instance.AllPlayers)
				{
					if (p != null && !p.IsDead && p.Object != null) pool.Add(p.Object);
				}

				if (pool.Count > 0)
				{
					// Защита от переполнения (чтобы не сделать салок больше, чем игроков в лобби)
					int targetAmount = Math.Min(TaggerCount, pool.Count - 1);
					if (targetAmount <= 0) targetAmount = 1;

					for (int i = 0; i < targetAmount; i++)
					{
						int randIndex = UnityEngine.Random.Range(0, pool.Count);
						PlayerControl pickedTagger = pool[randIndex];
						
						TaggerIds.Add(pickedTagger.PlayerId);
						pool.RemoveAt(randIndex);

						SetPlayerColorSecure(pickedTagger, 0); // Красный цвет
						UnityEngine.Debug.Log($"[MMPuL] Игрок {pickedTagger.Data.PlayerName} стал Салкой.");
					}

					// Все остальные гарантированно остаются Лаймовыми
					foreach (var crew in pool)
					{
						SetPlayerColorSecure(crew, 11);
					}

					_freezeTagStarted = true;
				}
				return;
			}

			// Условие победы Мирных: Время вышло
			if (_freezeTagGlobalTimer >= (FreezeMatchDuration + 10f)) // Время матча + 10 секунд задержки старта
			{
				_freezeTagGameActive = false;
				UnityEngine.Debug.Log("[MMPuL] Время вышло! Мирные победили!");
				Coroutines.Instance.CoEndGameStart(GameOverReason.HideAndSeek_CrewmatesByTimer);
				return;
			}

			var allPlayers = GameData.Instance.AllPlayers;

			// 2. ТИК ТАЙМЕРОВ КУЛДАУНОВ ИГРОКОВ
			for (int i = 0; i < allPlayers.Count; i++)
			{
				var p = allPlayers[i];
				if (p == null || p.Object == null || p.IsDead) continue;

				byte pid = p.PlayerId;
				if (PlayerCooldowns.ContainsKey(pid) && PlayerCooldowns[pid] > 0f)
				{
					PlayerCooldowns[pid] -= Time.deltaTime;
				}
			}

			// 3. ТИК ТАЙМЕРОВ ЗАМЕРЗАНИЯ (Смерть во льду)
			bool activeCrewmatesLeft = false;

			for (int i = 0; i < allPlayers.Count; i++)
			{
				var p = allPlayers[i];
				if (p == null || p.Object == null || p.IsDead) continue;

				byte pid = p.PlayerId;
				int color = GetPlayerColor(p.Object);

				if (color == 10) // Игрок заморожен
				{
					if (!FrozenTimers.ContainsKey(pid)) FrozenTimers[pid] = 0f;
					FrozenTimers[pid] += Time.deltaTime;

					if (FrozenTimers[pid] >= FreezeDeathTime)
					{
						// Игрок окончательно замерз — уничтожаем его
						UnityEngine.Debug.Log($"[MMPuL] {p.PlayerName} замерз насмерть.");

						p.Object.RpcSetRole(RoleTypes.CrewmateGhost, true);
						FrozenTimers.Remove(pid);
						continue;
					}
				}
				else if (color == 11 && !TaggerIds.Contains(pid))
				{
					// Нашли хотя бы одного активного живого мирного игрока
					activeCrewmatesLeft = true;
				}
			}

			// Условие победы Салок: Активных мирных больше нет
			if (!activeCrewmatesLeft && _freezeTagStarted)
			{
				_freezeTagGameActive = false;
				UnityEngine.Debug.Log("[MMPuL] Все мирные заморожены! Салки победили!");
				Coroutines.Instance.CoEndGameStart(GameOverReason.HideAndSeek_CrewmatesByTimer);
				return;
			}

			// 4. ДИНАМИКА КАСАНИЙ (Заморозка и Разморозка)
			for (int i = 0; i < allPlayers.Count; i++)
			{
				var p1 = allPlayers[i];
				if (p1 == null || p1.Object == null || p1.IsDead) continue;

				// Если игрок на лестнице, платформе или в венте — он недосягаем
				if (p1.Object.inVent || p1.Object.onLadder || p1.Object.inMovingPlat) continue;

				bool isP1Tagger = TaggerIds.Contains(p1.PlayerId);
				int p1Color = GetPlayerColor(p1.Object);

				for (int j = i + 1; j < allPlayers.Count; j++)
				{
					var p2 = allPlayers[j];
					if (p2 == null || p2.Object == null || p2.IsDead) continue;
					if (p2.Object.inVent || p2.Object.onLadder || p2.Object.inMovingPlat) continue;

					bool isP2Tagger = TaggerIds.Contains(p2.PlayerId);
					int p2Color = GetPlayerColor(p2.Object);

					float dist = Vector2.Distance(p1.Object.transform.position, p2.Object.transform.position);
					if (dist > FreezeDistance) continue;

					// СИТУАЦИЯ А: Салка касается активного мирного (11) -> ЗАМОРОЗКА
					if ((isP1Tagger && !isP2Tagger && p2Color == 11) || (isP2Tagger && !isP1Tagger && p1Color == 11))
					{
						var tagger = isP1Tagger ? p1 : p2;
						var crew = isP1Tagger ? p2 : p1;

						float cd = 0f;
						PlayerCooldowns.TryGetValue(tagger.PlayerId, out cd);
						
						if (cd <= 0f)
						{
							SetPlayerColorSecure(crew.Object, 10); // Превращаем в ледышку (Голубой)
							PlayerCooldowns[tagger.PlayerId] = TaggerFreezeCooldown; // Вешаем кулдаун на салку
							FrozenTimers[crew.PlayerId] = 0f; // Сбрасываем таймер жизни во льду
							UnityEngine.Debug.Log($"[MMPuL] {tagger.PlayerName} заморозил {crew.PlayerName}");
						}
					}

					// СИТУАЦИЯ Б: Активный мирный (11) касается Замороженного (10) -> РАЗМОРОЗКА
					if ((p1Color == 11 && p2Color == 10) || (p2Color == 11 && p1Color == 10))
					{
						var helper = (p1Color == 11) ? p1 : p2;
						var frozen = (p1Color == 10) ? p1 : p2;

						// Салки не могут размораживать
						if (TaggerIds.Contains(helper.PlayerId)) continue; 

						float cd = 0f;
						PlayerCooldowns.TryGetValue(helper.PlayerId, out cd);

						if (cd <= 0f)
						{
							SetPlayerColorSecure(frozen.Object, 11); // Размораживаем (обратно в Лайм)
							PlayerCooldowns[helper.PlayerId] = CrewUnfreezeCooldown; // Вешаем кулдаун спасителю
							FrozenTimers.Remove(frozen.PlayerId); // Удаляем из списка замерзающих
							UnityEngine.Debug.Log($"[MMPuL] {helper.PlayerName} разморозил {frozen.PlayerName}");
						}
					}
				}
			}
		}
		private static void UpdateTrafficLightMode()
		{
			if (!_trafficGameActive) return;

			_trafficStateTimer += Time.deltaTime;
			_trafficGlobalTimer += Time.deltaTime;
			if (_trafficGlobalTimer >= TrafficMatchDuration)
			{
				_trafficGameActive = false;
				UnityEngine.Debug.Log("[MMPuL] Время вышло!");
				Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask);
				return;
			}
			// --- 1. АВТОМАТ СВЕТОФОРА (ПЕРЕКЛЮЧЕНИЕ СОСТОЯНИЙ) ---
			if (CurrentTrafficState == TrafficState.Green)
			{
				if (_trafficStateTimer >= TrafficGreenDuration)
				{
					CurrentTrafficState = TrafficState.Yellow;
					_trafficStateTimer = 0f;

					// Перекрашиваем всех активных игроков без выполненных задач в Желтый (5)
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && p.Object != null && !p.IsDead && p.RoleType != RoleTypes.CrewmateGhost && !TrafficCompletedPlayers.Contains(p.PlayerId))
						{
							SetPlayerColorSecure(p.Object, 5);
						}
					}
				}
			}
			else if (CurrentTrafficState == TrafficState.Yellow)
			{
				if (_trafficStateTimer >= TrafficYellowDuration)
				{
					CurrentTrafficState = TrafficState.Red;
					_trafficStateTimer = 0f;
					_trafficRedGraceTimer = 0f;
					_trafficGraceActive = true; // Включаем временный буфер защиты от рассинхрона
					TrafficRedPositions.Clear();

					// Перекрашиваем в Красный (0) и фиксируем их текущие координаты
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && p.Object != null && !p.IsDead && p.RoleType != RoleTypes.CrewmateGhost && !TrafficCompletedPlayers.Contains(p.PlayerId))
						{
							SetPlayerColorSecure(p.Object, 0);
							TrafficRedPositions[p.PlayerId] = p.Object.transform.position;
						}
					}
				}
			}
			else if (CurrentTrafficState == TrafficState.Red)
			{
				// Если мы только что перешли в красный свет, запускаем таймер задержки
				if (!IsRedLightActive)
				{
					_trafficRedGraceTimer += Time.deltaTime;

					// Если задержка прошла, фиксируем позицию и включаем "режим убийства"
					if (_trafficRedGraceTimer >= TrafficRedDelay)
					{
						IsRedLightActive = true;
						TrafficRedPositions.Clear();
						
						foreach (var p in GameData.Instance.AllPlayers)
						{
							if (p != null && p.Object != null && !p.IsDead && !TrafficCompletedPlayers.Contains(p.PlayerId))
							{
								TrafficRedPositions[p.PlayerId] = p.Object.transform.position;
							}
						}
					}
				}
				else
				{
					// РЕЖИМ УБИЙСТВА: Проверяем движение только если IsRedLightActive == true
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p == null || p.Object == null || p.IsDead || TrafficCompletedPlayers.Contains(p.PlayerId)) continue;

						if (TrafficRedPositions.ContainsKey(p.PlayerId))
						{
							// Проверка дистанции от ЗАФИКСИРОВАННОЙ позиции
							if (Vector2.Distance(TrafficRedPositions[p.PlayerId], p.Object.transform.position) > 1.0f)
							{
								p.Object.RpcSetRole(RoleTypes.CrewmateGhost, true);
							}
						}
					}
				}

				// Переключение обратно на зеленый (если время вышло)
				if (_trafficStateTimer >= TrafficRedDuration)
				{
					CurrentTrafficState = TrafficState.Green;
					_trafficStateTimer = 0f;
					IsRedLightActive = false; // Отключаем проверку движения
					_trafficRedGraceTimer = 0f;
					TrafficRedPositions.Clear();

					// Возвращаем всем лаймовый (11)
					foreach (var p in GameData.Instance.AllPlayers)
					{
						if (p != null && p.Object != null && !p.IsDead && !TrafficCompletedPlayers.Contains(p.PlayerId))
						{
							SetPlayerColorSecure(p.Object, 11);
						}
					}
				}
			}

			// --- 2. ПРОВЕРКА ДВИЖЕНИЯ И ВЫПОЛНЕНИЯ ЗАДАНИЙ ---
			int alivePlayersCount = 0;
			int completedPlayersCount = 0;

			foreach (var p in GameData.Instance.AllPlayers)
			{
				if (p == null || p.Object == null || p.Disconnected) continue;

				// Игнорируем тех, кто уже мертв или является призраком
				bool isGhost = p.IsDead || p.RoleType == RoleTypes.CrewmateGhost;
				if (isGhost) continue;

				alivePlayersCount++;
				// Проверяем, выполнил ли игрок задания, учитывая специфику "Светофора"
				bool HasCompletedTasks(PlayerControl p)
				{
					// Если список задач пуст или null - считаем, что задания еще не "выполнены" для режима
					if (p.Data.Tasks == null || p.Data.Tasks.Count == 0) return false;
					
					// Используем твой метод, но с защитой от пустого списка
					return p.AllTasksCompleted(); 
				}

				// ... и в цикле проверки игроков:
				bool isDone = HasCompletedTasks(p.Object);
				// Проверяем выполнение всех тасков с помощью встроенного метода игры
				if (isDone)
				{
					completedPlayersCount++;
					if (!TrafficCompletedPlayers.Contains(p.PlayerId))
					{
						TrafficCompletedPlayers.Add(p.PlayerId);
						SetPlayerColorSecure(p.Object, 10); // Становится голубым и получает иммунитет
						UnityEngine.Debug.Log($"[MMPuL] {p.PlayerName} выполнил все задания и спасся!");
						// ЕСЛИ ВКЛЮЧЕН РЕЖИМ ТОЛЬКО ОДНОГО ПОБЕДИТЕЛЯ
						if (_trafficOnlyOne)
						{
							_trafficGameActive = false;
							UnityEngine.Debug.Log($"[MMPuL] {p.PlayerName} победил первым!");
							Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask, 0.5f);
							return;
						}
					}
					continue; 
				}

				// Если горит Красный свет и буфер задержки рассинхрона истек — проверяем на движение
				if (CurrentTrafficState == TrafficState.Red && !_trafficGraceActive)
				{
					if (TrafficRedPositions.ContainsKey(p.PlayerId))
					{
						Vector2 originalPos = TrafficRedPositions[p.PlayerId];
						Vector2 currentPos = p.Object.transform.position;

						// Если игрок вышел за пределы радиуса в 1 единицу — превращаем в призрака
						if (Vector2.Distance(originalPos, currentPos) > 1.0f)
						{
							UnityEngine.Debug.Log($"[MMPuL] Игрок {p.PlayerName} двигался на красный свет!");
							p.Object.RpcSetRole(RoleTypes.CrewmateGhost, false);
						}
					}
				}
			}

			// --- 3. УСЛОВИЯ ЗАВЕРШЕНИЯ МАТЧА ---
			if (alivePlayersCount == 0) // Никто не выжил
			{
				_trafficGameActive = false;
				Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask);
			}
			else if (!_trafficOnlyOne && completedPlayersCount == alivePlayersCount) // Все выжившие выполнили квесты
			{
				_trafficGameActive = false;
				Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask);
			}
		}
		private static void UpdateCopsAndRobbersMode()
		{
			if (!_copsGameActive) return;
			bool allJailed = true;
			bool anyRobberFree = false;
			bool allTasksDone = true;

			var players = GameData.Instance.AllPlayers;
			_copsGlobalTimer += Time.deltaTime;
			
			// 1. Условие победы: Время вышло -> Копы победили
			if (_copsGlobalTimer >= CopsMatchDuration)
			{
				_copsGameActive = false;
				Coroutines.Instance.CoEndGameStart(GameOverReason.HideAndSeek_CrewmatesByTimer);
				UnityEngine.Debug.Log("[MMPuL] Время вышло!");
				return;
			}
			if (_copsGlobalTimer >= 10f && !_copsTeleported)
			{
				_copsTeleported = true;
				
				foreach (var p in GameData.Instance.AllPlayers)
				{
					// Проверка на валидность игрока
					if (p == null || p.Object == null || p.Disconnected) continue;

					// Игнорируем мертвых или призраков
					if (p.IsDead || p.RoleType == RoleTypes.CrewmateGhost) continue;

					PlayerControl pc = p.Object;
					int color = GetPlayerColor(pc);
					
					if (color == 1)
					{
						int mapId = GameManager.Instance.LogicOptions.MapId;
						int maxVents = mapId switch { 0 => 13, 1 => 11, 2 => 11, 3 => 11, 4 => 9, _ => 11 };
						int ventId = UnityEngine.Random.Range(0, maxVents);
						
						pc.MyPhysics.RpcBootFromVent(ventId);
					}
				}
			}
			if (_copsGlobalTimer >= 11f && !_copsReady)
			{
				_copsReady = true;
			}

			// 2. Условие победы: Все преступники в тюрьме -> Копы победили
			// 3. Условие победы: Все задачи выполнены -> Преступники победили
			

			// Вспомогательная функция для проверки задач (как в Светофоре)
			bool HasCompletedTasks(PlayerControl p)
			{
				// Если список задач пуст или null - считаем, что задания еще не "выполнены" 
				// (это защищает от преждевременного завершения игры при старте)
				if (p.Data.Tasks == null || p.Data.Tasks.Count == 0) return false;
				
				// Используем встроенный метод игры
				return p.AllTasksCompleted(); 
			}

			// Основной цикл проверки
			foreach (var p in GameData.Instance.AllPlayers)
			{
				// Проверка на валидность игрока
				if (p == null || p.Object == null || p.Disconnected) continue;

				// Игнорируем мертвых или призраков
				if (p.IsDead || p.RoleType == RoleTypes.CrewmateGhost) continue;

				PlayerControl pc = p.Object;
				int color = GetPlayerColor(pc);
				
				if (color == 6) // Преступник
				{
					allJailed = false;
					anyRobberFree = true;

					// Если хотя бы один преступник НЕ выполнил задачи, то allTasksDone становится false
					if (!HasCompletedTasks(pc))
					{
						allTasksDone = false;
					}
				}
			}

			if (allTasksDone && anyRobberFree)
			{
				_copsGameActive = false;
				Coroutines.Instance.CoEndGameStart(GameOverReason.HideAndSeek_CrewmatesByTimer);
				UnityEngine.Debug.Log("[MMPuL] Все задания выполнены");
				return;
			}
			if (allJailed && players.Count > 1)
			{
				_copsGameActive = false;
				Coroutines.Instance.CoEndGameStart(GameOverReason.HideAndSeek_CrewmatesByTimer);
				UnityEngine.Debug.Log("[MMPuL] Все пойманы");
				return;
			}

			// 4. Логика поимки и спасения
			foreach (var p1 in players)
			{
				if (!_copsReady) continue;
				if (p1 == null || p1.Object == null || p1.IsDead) continue;
				int color1 = GetPlayerColor(p1.Object);

				foreach (var p2 in players)
				{
					if (p1 == p2 || p2 == null || p2.Object == null || p2.IsDead) continue;
					int color2 = GetPlayerColor(p2.Object);
					float dist = Vector2.Distance(p1.Object.transform.position, p2.Object.transform.position);

					// Коп (1) ловит Преступника (6)
					if (color1 == 1 && color2 == 6 && dist <= CopsCatchDistance)
					{
						// Проверка задержки
						float currentTime = Time.time;
						float lastCatchTime = 0f;
						
						// Получаем время последней поимки этого копа
						if (_copCatchCooldowns.ContainsKey(p1.PlayerId))
						{
							lastCatchTime = _copCatchCooldowns[p1.PlayerId];
						}

						// Если прошло больше времени, чем CopsCatchCooldownDuration
						if (currentTime - lastCatchTime >= CopsCatchCooldownDuration)
						{
							JailPlayer(p2.Object); // Ловим
							_copCatchCooldowns[p1.PlayerId] = currentTime; // Обновляем время поимки
						}
					}
					// Преступник (6) спасает Преступника в тюрьме (5)
					else if (color1 == 6 && color2 == 5 && dist <= 0.5f)
					{
						UnjailPlayer(p2.Object);
					}
				}
			}
		}

		private static void JailPlayer(PlayerControl p)
		{
			if (IsInJail.ContainsKey(p.PlayerId) && IsInJail[p.PlayerId]) return;
			
			IsInJail[p.PlayerId] = true;
			SetPlayerColorSecure(p, 5); // Желтый
			
			// Телепорт на случайный люк
			int mapId = GameManager.Instance.LogicOptions.MapId;
			int maxVents = mapId switch { 0 => 13, 1 => 11, 2 => 11, 3 => 11, 4 => 9, _ => 11 };
			int ventId = UnityEngine.Random.Range(0, maxVents);
			
			p.MyPhysics.RpcBootFromVent(ventId);
			Coroutines.Instance.CoVentDesyncFixStart(p, ventId);
		}

		private static void UnjailPlayer(PlayerControl p)
		{
			if (!IsInJail.ContainsKey(p.PlayerId) || !IsInJail[p.PlayerId]) return;
			
			IsInJail[p.PlayerId] = false;
			SetPlayerColorSecure(p, 6); // Черный
		}
		
		// Безопасное получение цвета игрока из нашего личного словаря хоста
        private static int GetPlayerColor(PlayerControl player)
        {
            if (player == null) return -1;

            if (TrackedColors.ContainsKey(player.PlayerId))
            {
                return TrackedColors[player.PlayerId];
            }

            return 10; // Если данных нет, по умолчанию считаем голубым
        }

        // Метод, который меняет цвет в сети и обновляет наш словарь
        private static void SetPlayerColorSecure(PlayerControl player, byte colorId)
        {
            if (player == null) return;

            try
            {
				bool isMe = player.PlayerId == PlayerControl.LocalPlayer.PlayerId;
				if (colorId == 2)
				{
					if (isMe)
					{
						GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.CrewLightMod, MultiModePlugin.ZombieVisionSetting);
						GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, MultiModePlugin.ZombieSpeedSetting);
					}
					else
					{
						IGameOptions privateOptions = CustomNetworkHelper.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);

						privateOptions.SetFloat(FloatOptionNames.CrewLightMod, MultiModePlugin.ZombieVisionSetting); // Ослепление
						privateOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, MultiModePlugin.ZombieSpeedSetting); // Динамическая скорость из GUI!

						CustomNetworkHelper.SendPrivateOptions(privateOptions, player.OwnerId);
					}
				}
				// --- ЛОГИКА СКОРОСТИ ДЛЯ HOT POTATO ---
				if (GameModeTab == 4)
				{
					// Получаем базовую скорость лобби из бэкапа (если его нет, берем текущую)
					float baseSpeed = MultiModePlugin.SavedLobbyOptionsBackup != null 
						? MultiModePlugin.SavedLobbyOptionsBackup.GetFloat(FloatOptionNames.PlayerSpeedMod) 
						: GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod);

					if (colorId == 4) // СТАЛ КАРТОШКОНОСИТЕЛЕМ (+20% к скорости)
					{
						float targetSpeed = Math.Min(baseSpeed * 1.25f, 3.0f);; 

						if (isMe)
						{
							// Меняем себе локально без сети
							GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, targetSpeed);
						}
						else
						{
							// Шлем приватный пакет другому игроку
							IGameOptions privateOptions = CustomNetworkHelper.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
							privateOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, targetSpeed);
							CustomNetworkHelper.SendPrivateOptions(privateOptions, player.OwnerId);
						}
					}
					else if (colorId == 10) // СБРОСИЛ КАРТОШКУ (Возвращаем обычную скорость)
					{
						if (isMe)
						{
							// Возвращаем себе базовую скорость локально
							GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, baseSpeed);
						}
						else
						{
							// Возвращаем базовую скорость другому игроку через приватный пакет
							IGameOptions privateOptions = CustomNetworkHelper.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
							privateOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, baseSpeed);
							CustomNetworkHelper.SendPrivateOptions(privateOptions, player.OwnerId);
						}
					}
				}
				// --- ЛОГИКА СКОРОСТИ ДЛЯ FREEZE TAG ---
				if (GameModeTab == 5)
				{
					float baseSpeed = MultiModePlugin.SavedLobbyOptionsBackup != null 
						? MultiModePlugin.SavedLobbyOptionsBackup.GetFloat(FloatOptionNames.PlayerSpeedMod) 
						: GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod);

					float targetSpeed = baseSpeed;

					if (colorId == 0) // Салка (Красный)
					{
						targetSpeed = Math.Min(baseSpeed * TaggerSpeedMod, 3.0f);
					}
					else if (colorId == 10) // Заморожен (Голубой) -> Ставим скорость в 0.0
					{
						targetSpeed = 0.0f;
					}
					else if (colorId == 11) // Обычный активный мирный (Лаймовый)
					{
						targetSpeed = baseSpeed;
					}

					if (isMe)
					{
						GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, targetSpeed);
					}
					else
					{
						IGameOptions privateOptions = CustomNetworkHelper.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
						privateOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, targetSpeed);
						CustomNetworkHelper.SendPrivateOptions(privateOptions, player.OwnerId);
					}
				}
				if (GameModeTab == 8)
				{
					float baseSpeed = MultiModePlugin.SavedLobbyOptionsBackup != null 
						? MultiModePlugin.SavedLobbyOptionsBackup.GetFloat(FloatOptionNames.PlayerSpeedMod) 
						: GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod);

					float targetSpeed = baseSpeed;

					if (colorId == 1) // Салка (Красный)
					{
						targetSpeed = Math.Min(baseSpeed * 1.2f, 3.0f);
					}
					else if (colorId == 5)
					{
						targetSpeed = 0.0f;
					}
					else if (colorId == 6)
					{
						targetSpeed = baseSpeed;
					}

					if (isMe)
					{
						GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, targetSpeed);
					}
					else
					{
						IGameOptions privateOptions = CustomNetworkHelper.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
						privateOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, targetSpeed);
						CustomNetworkHelper.SendPrivateOptions(privateOptions, player.OwnerId);
					}
				}
				player.RpcSetColor(colorId); // Отправляем RPC пакет всем игрокам
				UnityEngine.Debug.Log($"[MMPuL] {player.Data.PlayerName} окрашен в {colorId}");
                TrackedColors[player.PlayerId] = colorId; // Записываем в память мода
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[MMPuL] SCS Error: " + e.Message);
            }
        }
		// Патч 1: Запрещаем игре автоматически завершаться по ванильным правилам (например, когда предателей нет)
		[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
		public static class EndCriteriaPatch
		{
			[HarmonyPrefix]
			public static bool Prefix()
			{
				// Если запущен наш кастомный режим — полностью отключаем ванильный подсчет побед
				if (GameModeTab == 3 || GameModeTab == 4 || GameModeTab == 5 || GameModeTab == 6 || GameModeTab == 8)
				{
					return false;     // Пропускаем оригинальный метод
				}
				if (GameModeTab == 7)
				{
					int alivePlayers = 0;

					foreach (PlayerControl player in PlayerControl.AllPlayerControls)
					{
						if (!player.Data.IsDead)
						{
							alivePlayers++;

							if (alivePlayers > 1)
								return false;
						}
					}
					if (alivePlayers <= 1)
					{
						Coroutines.Instance.CoEndGameStart(GameOverReason.CrewmatesByTask);
						return false;
					}
				}
				return true;
			}
		}
		[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
		public static class HnSEndCriteriaPatch
		{
			[HarmonyPrefix]
			public static bool Prefix()
			{
				// Если запущен наш кастомный режим — полностью отключаем ванильный подсчет побед
				if (GameModeTab == 3 || GameModeTab == 4 || GameModeTab == 5 || GameModeTab == 6 || GameModeTab == 8)
				{
					return false;     // Пропускаем оригинальный метод
				}
				if (GameModeTab == 7)
				{
					int alivePlayers = 0;

					foreach (PlayerControl player in PlayerControl.AllPlayerControls)
					{
						if (!player.Data.IsDead)
						{
							alivePlayers++;

							if (alivePlayers > 1)
								return false;
						}
					}
					if (alivePlayers <= 1)
					{
						Coroutines.Instance.CoEndGameStart(GameOverReason.HideAndSeek_ImpostorsByKills);
						return false;
					}
				}
				return true;
			}
		}

		// Патч 2: Запрещаем репортить трупы в наших аркадных режимах
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
		public static class DisableReportPatch
		{
			[HarmonyPrefix]
			public static bool Prefix()
			{
				if (SSPartyEveryMode)
				{
					if (IsSSPartyActive) {Coroutines.Instance.CoSSPartyStart(PlayerControl.AllPlayerControls[UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count)]);}
					if (IsSSPartySwapActive) {Coroutines.Instance.CoSSPartySwapStart();}
				}
				// Если идет кастомная мини-игра, нажатие на Report просто игнорируется
				if (GameModeTab == 3 || GameModeTab == 4 || GameModeTab == 5 || GameModeTab == 6 || GameModeTab == 7 || GameModeTab == 8)
				{
					return false; // Запретить репорт
				}
				return true;
			}
		}
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
		public static class DisableCloseDoors
		{
			static bool Prefix()
			{
				if (GameModeTab != 1 && GameModeTab != 2) {return false;}
				return true;
			}
		}
		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
		public static class DisableSabotages
		{
			static bool Prefix()
			{
				if (GameModeTab != 1 && GameModeTab != 2) {return false;}
				return true;
			}
		}
		[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
		public static class EnterVentPatch
		{
			static void Postfix(Vent __instance, PlayerControl pc)
			{
				if (GameModeTab != 1 && GameModeTab != 2)
				{
					pc.MyPhysics.RpcBootFromVent(__instance.Id);
				}
			}
		}
		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
		public class ClimbLadderPatch
		{
			static void Prefix(PlayerPhysics __instance)
			{
				if (!AmongUsClient.Instance.AmHost)
					return;
				
				PlayerControl player = __instance.myPlayer;

				if (player == null)
					return;
				if (!NoImpLadderDChance && player.Data.Role.IsImpostor) return;
				if (UnityEngine.Random.value < ChanceofDeath / 100f)
				{
					UnityEngine.Debug.Log($"[MMPuL] {player.Data.PlayerName} сдох от лестницы");
					player.RpcSetRole(RoleTypes.CrewmateGhost, true);
				}
			}
		}
		[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
		public static class IncreasePatch
		{
			public static bool Prefix(NumberOption __instance)
			{
				// if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.HideNSeek &&
					// __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed) return true;
				
				if (!Input.GetKey(KeyCode.LeftShift) &&
					!Input.GetKey(KeyCode.RightShift) &&
					!Input.GetKey(KeyCode.LeftControl) &&
					!Input.GetKey(KeyCode.RightControl)) return true;
				float step = 0.05f;
				if (__instance.Increment >= 1f) step = 1f;
				__instance.Value += step;
				__instance.Value = Mathf.Min(__instance.Value, __instance.ValidRange.max);
				
				__instance.UpdateValue();
				__instance.OnValueChanged.Invoke(__instance);
				__instance.AdjustButtonsActiveState();
				return false;
			}
		}
		[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
		public static class DecreasePatch
		{
			public static bool Prefix(NumberOption __instance)
			{
				// if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.HideNSeek &&
					// __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed) return true;

				if (!Input.GetKey(KeyCode.LeftShift) &&
					!Input.GetKey(KeyCode.RightShift) &&
					!Input.GetKey(KeyCode.LeftControl) &&
					!Input.GetKey(KeyCode.RightControl)) return true;
				float step = 0.05f;
				if (__instance.Increment >= 1f && __instance.Title != StringNames.GameKillCooldown) step = 1f;
				__instance.Value -= step;
				__instance.Value = Mathf.Max(__instance.Value, __instance.ValidRange.min);
				
				__instance.UpdateValue();
				__instance.OnValueChanged.Invoke(__instance);
				__instance.AdjustButtonsActiveState();
				return false;
			}
		}
		[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
		public static class InitializePatch
		{
			public static void Postfix(NumberOption __instance)
			{
				// if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.HideNSeek && __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed) return;
				
				if (__instance.Title == StringNames.GameKillCooldown)
					__instance.ValidRange = new FloatRange(0.05f,__instance.ValidRange.max);
				
				if (__instance.Title == StringNames.ViperDissolveTime ||
					__instance.Title == StringNames.ScientistCooldown ||
					__instance.Title == StringNames.ScientistBatteryCharge ||
					__instance.Title == StringNames.PhantomDuration
					) __instance.ValidRange = new FloatRange(1f,__instance.ValidRange.max);
				
				if (__instance.Title == StringNames.GuardianAngelCooldown ||
					__instance.Title == StringNames.EngineerCooldown ||
					__instance.Title == StringNames.PhantomCooldown ||
					__instance.Title == StringNames.ShapeshifterCooldown ||
					__instance.Title == StringNames.TrackerCooldown ||
					__instance.Title == StringNames.TrackerDuration
					) __instance.ValidRange = new FloatRange(0f,__instance.ValidRange.max);
				
				if (__instance.Title == StringNames.GuardianAngelDuration)
					__instance.ValidRange = new FloatRange(1f,60f);
				
				__instance.AdjustButtonsActiveState();
			}
		}
		[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
		public static class AddChatPatch
		{
			public static void Prefix(PlayerControl sourcePlayer, string chatText)
			{
				if (!AmongUsClient.Instance.AmHost) return;
				if (string.IsNullOrWhiteSpace(chatText)) return;
				if (!chatText.StartsWith("/color ", StringComparison.OrdinalIgnoreCase)) return;
				string[] args = chatText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (args.Length < 2) return;
				if (!ColorHelper.TryGetColor(args[1], out byte colorId)) return;
				if (!LobbyBehaviour.Instance) return;
				if (!AllowColorCommand) {if (sourcePlayer != PlayerControl.LocalPlayer) return;}
				if (!AllowColorFortegreen && colorId == 18) {if (sourcePlayer != PlayerControl.LocalPlayer) return;}
				sourcePlayer.RpcSetColor(colorId);
				UnityEngine.Debug.Log($"[MMPuL] {sourcePlayer.Data.PlayerName} новый цвет {colorId}");
			}
		}
		
		internal class CustomNetworkHelper
		{
			// Метод создания независимой копии настроек
			public static IGameOptions CreateCloneOptions(IGameOptions options)
			{
				var factory = GameManager.Instance.LogicOptions.gameOptionsFactory;
				
				// Пакуем текущие опции в байты
				// Флаг первоапрельского режима берем из менеджера игры
				byte[] byteArray = factory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn);
				
				// Распаковываем обратно в НОВЫЙ изолированный объект
				return factory.FromBytes(byteArray);
			}

			// Отправка персональных настроек конкретному игроку по его OwnerId (ClientId)
			public static void SendPrivateOptions(IGameOptions options, int targetClientId)
			{
				// Создаем надежный сетевой пакет (Reliable)
				MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
				
				// Находим индекс компонента LogicOptions в игре
				int logicIndex = FindLogicOptionsIndex();
				if (logicIndex == -1) return;

				writer.StartMessage((byte)logicIndex);
				
				// Пакуем измененные настройки и пишем их в пакет
				var factory = GameManager.Instance.LogicOptions.gameOptionsFactory;
				writer.WriteBytesAndSize(factory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
				writer.EndMessage();

				// Самая сочная строка: шлем пакет напрямую по NetId компонента GameManager конкретному клиенту!
				SendDataFlag(GameManager.Instance.NetId, writer, targetClientId);
			}

			private static int FindLogicOptionsIndex()
			{
				for (int i = 0; i < GameManager.Instance.LogicComponents.Count; i++)
				{
					var component = GameManager.Instance.LogicComponents[i];
					if (component.GetType() == typeof(LogicOptions))
					{
						return i;
					}
				}
				return -1;
			}
			
			public static void SendDataFlag(uint netId, MessageWriter msg, int targetClientId = -1)
			{
				MessageWriter writer = MessageWriter.Get(SendOption.Reliable);

				if(targetClientId == -1)
				{
					writer.StartMessage(InnerNet.Tags.GameData);
					writer.Write(AmongUsClient.Instance.GameId);
				}
				else
				{
					writer.StartMessage(InnerNet.Tags.GameDataTo);
					writer.Write(AmongUsClient.Instance.GameId);
					writer.WritePacked(targetClientId);
				}

				writer.StartMessage((byte)GameDataTypes.DataFlag);
				writer.WritePacked(netId);
				writer.Write(msg, false);
				writer.EndMessage();

				writer.EndMessage();
				AmongUsClient.Instance.SendOrDisconnect(writer);
				writer.Recycle();
			}
			
			public static void ResetAllPlayersToLobbySettings()
			{
				try
				{
					if (GameManager.Instance == null || GameManager.Instance.LogicOptions == null) return;

					// Берем чистые, оригинальные настройки лобби с хоста
					IGameOptions normalOptions = MultiModePlugin.SavedLobbyOptionsBackup ?? GameManager.Instance?.LogicOptions?.currentGameOptions;

					if (PlayerControl.AllPlayerControls == null) return;

					// Пробегаемся по всем игрокам в комнате
					foreach (var player in PlayerControl.AllPlayerControls)
					{
						if (player == null || player.Data == null) continue;

						// Отправляем каждому игроку оригинальный пакет настроек лобби по его OwnerId
						SendPrivateOptions(normalOptions, player.OwnerId);
					} 
					
					UnityEngine.Debug.Log("[MMPuL] All players successfully reset to original lobby options.");
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log("[MMPuL] Reset Error: " + e.Message);
				}
			}
		}
    }
	
	public class Coroutines : MonoBehaviour
	{
		public Coroutines(System.IntPtr ptr) : base(ptr) {}
		
		public static Coroutines Instance;

		private void Awake()
		{
			Instance = this;
		}
		
		public void CoVentDesyncFixStart(PlayerControl p, int ventId)
		{
			StartCoroutine(CoVentDesyncFix(p, ventId).WrapToIl2Cpp());
		}
		private IEnumerator CoVentDesyncFix(PlayerControl p, int ventId)
		{
			yield return new WaitForSeconds(1f);
			p.MyPhysics.RpcBootFromVent(ventId);
		}
		
		public void CoEndGameStart(GameOverReason reason, float waitsec = 1f)
		{
			StartCoroutine(CoEndGame(reason, waitsec).WrapToIl2Cpp());
		}
		private IEnumerator CoEndGame(GameOverReason reason, float waitsec = 1f)
		{
			yield return new WaitForSeconds(waitsec);
			GameManager.Instance.RpcEndGame(reason, false);
		}
		
		public void CoChaosModeStart()
		{
			StartCoroutine(CoChaosMode().WrapToIl2Cpp());
		}
		private IEnumerator CoChaosMode()
		{
			// Ждем 10 секунд пока идет сцена с ролями
			yield return new WaitForSeconds(10f);

			// Условие: цикл работает, пока игра идет (Started) И режим Хаос активен
			// GameManager.Instance.GameState — это встроенное состояние игры в Among Us
			while (GameManager.Instance != null && 
				   AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && 
				   MultiModePlugin.IsChaosActive)
			{
				// 1. Выбор случайного живого игрока
				var livePlayers = new List<PlayerControl>();
				foreach (var p in GameData.Instance.AllPlayers)
				{
					if (p != null && p.Object != null && !p.IsDead)
					{
						livePlayers.Add(p.Object);
					}
				}

				// 2. Логика Хаоса
				if (livePlayers.Count > 0)
				{
					PlayerControl target = livePlayers[UnityEngine.Random.Range(0, livePlayers.Count)];
					int eventType = UnityEngine.Random.Range(0, 3);
					if (eventType >= 2 && !target.Data.Role.IsImpostor) {eventType = UnityEngine.Random.Range(0, 2);}
					UnityEngine.Debug.Log($"[MMPuL] Для {target.Data.PlayerName} хаос ивент {eventType}");
					switch (eventType)
					{
						case 0:
							float newSpeed = UnityEngine.Random.Range(0.5f, 3.0f);
							ApplyChaosSetting(target, FloatOptionNames.PlayerSpeedMod, newSpeed);
							UnityEngine.Debug.Log($"[MMPuL] {target.Data.PlayerName} новая скорость {newSpeed}");
							break;
						case 1:
							float newVision = UnityEngine.Random.Range(0.35f, 1.5f);
							if (target.Data.Role.IsImpostor) {ApplyChaosSetting(target, FloatOptionNames.ImpostorLightMod, newVision);}
							else {ApplyChaosSetting(target, FloatOptionNames.CrewLightMod, newVision);}
							UnityEngine.Debug.Log($"[MMPuL] {target.Data.PlayerName} новое зрение {newVision}");
							break;
						case 2:
							float newKill = UnityEngine.Random.Range(1f, 25f);
							ApplyChaosSetting(target, FloatOptionNames.KillCooldown, newKill);
							RoleTypes currentRole = target.Data.RoleType;
							target.RpcSetRole(currentRole, true);
							UnityEngine.Debug.Log($"[MMPuL] {target.Data.PlayerName} новая перезарядка убийства {newKill}");
							break;
					}
				}
				
				// Ждем время, настроенное в GUI
				yield return new WaitForSeconds(MultiModePlugin.ChaosInterval);
			}

			// Здесь корутина сама завершается, когда условие while становится false
			MultiModePlugin.IsChaosActive = false; 
			UnityEngine.Debug.Log("[MMPuL] Режим Хаос завершен.");
		}
		private static void ApplyChaosSetting(PlayerControl player, FloatOptionNames optionName, float value)
		{
			// Если это МЫ (Локальный игрок)
			if (player == PlayerControl.LocalPlayer)
			{
				GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(optionName, value);
			}
			// Если это ДРУГОЙ игрок (отправляем пакет)
			else
			{
				IGameOptions privateOptions = MultiModePlugin.CustomNetworkHelper.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				privateOptions.SetFloat(optionName, value);
				MultiModePlugin.CustomNetworkHelper.SendPrivateOptions(privateOptions, player.OwnerId);
			}
		}
		
		
		public void CoSSPartyStart(PlayerControl target)
		{
			StartCoroutine(CoSSParty(target).WrapToIl2Cpp());
		}
		private IEnumerator CoSSParty(PlayerControl target)
		{
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if (target == null) {target = PlayerControl.AllPlayerControls[UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count)];}
				// if(player == target || player.shapeshiftTargetPlayerId == target.PlayerId) continue;
				if (player == null || player.Data == null || player.Data.IsDead) continue;
				BatchedMessage batch = new BatchedMessage();
				RoleTypes currentRole = player.Data.RoleType;

				batch.QueueSetRole(player, RoleTypes.Shapeshifter, true);
				batch.QueueShapeshift(player, target, false);
				batch.QueueSetRole(player, currentRole, true);
				
				batch.FinishBatch();
				UnityEngine.Debug.Log($"[MMPuL] {player.Data.PlayerName} превращен в {target.Data.PlayerName}");
				// This function can send up to 42 reliable messages at once, so we need to implement a delay to avoid getting disconnected
				yield return Effects.Wait(0.05f);
			}
		}
		
		public void CoSSPartySwapStart()
		{
			StartCoroutine(CoSSPartySwap().WrapToIl2Cpp());
		}
		private IEnumerator CoSSPartySwap()
		{
			List<PlayerControl> players = new List<PlayerControl>();

			foreach (PlayerControl p in PlayerControl.AllPlayerControls)
			{
				players.Add(p);
			}

			if (players.Count < 2)
				yield break;

			List<PlayerControl> targets = new List<PlayerControl>();

			foreach (PlayerControl p in players)
			{
				targets.Add(p);
			}

			// Генерируем перестановку, пока никто не попадает в самого себя.
			do
			{
				targets = new List<PlayerControl>(players);

				// Fisher-Yates Shuffle
				for (int i = targets.Count - 1; i > 0; i--)
				{
					int j = UnityEngine.Random.Range(0, i + 1);
					(targets[i], targets[j]) = (targets[j], targets[i]);
				}

			} while (HasSelfTarget(players, targets));

			for (int i = 0; i < players.Count; i++)
			{
				PlayerControl player = players[i];
				PlayerControl target = targets[i];
				
				if (player == null || player.Data == null || player.Data.IsDead) continue;
				if (target == null || target.Data == null) continue;
				BatchedMessage batch = new BatchedMessage();
				RoleTypes currentRole = player.Data.RoleType;

				batch.QueueSetRole(player, RoleTypes.Shapeshifter, true);
				batch.QueueShapeshift(player, target, false);
				batch.QueueSetRole(player, currentRole, true);

				batch.FinishBatch();
				UnityEngine.Debug.Log($"[MMPuL] {player.Data.PlayerName} превращен в {target.Data.PlayerName}");
				yield return Effects.Wait(0.05f);
			}
		}

		private bool HasSelfTarget(List<PlayerControl> players, List<PlayerControl> targets)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i] == targets[i])
					return true;
			}

			return false;
		}
		
		public void CoFFAModeStart()
		{
			StartCoroutine(CoFFAMode().WrapToIl2Cpp());
		}
		private IEnumerator CoFFAMode()
		{
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				BatchedMessage batch = new BatchedMessage(player.Data.ClientId);

				batch.QueueSetRole(player, RoleTypes.Impostor, false);
				UnityEngine.Debug.Log($"[MMPuL] {player.Data.PlayerName} назачен предателем для {player.Data.PlayerName}");
				foreach(PlayerControl p in PlayerControl.AllPlayerControls)
				{
					if (p == player) continue;
					batch.QueueSetRole(p, RoleTypes.Crewmate, false);
					UnityEngine.Debug.Log($"[MMPuL] {p.Data.PlayerName} назначен мирным для {player.Data.PlayerName}");
				}
				batch.FinishBatch();

				yield return Effects.Wait(0.05f);
			}
		}
	}
	public class BatchedMessage
	{
		public MessageWriter writer;

		public BatchedMessage(int targetClientId = -1)
		{
			writer = MessageWriter.Get(SendOption.Reliable);

			if(targetClientId == -1)
			{
				writer.StartMessage(InnerNet.Tags.GameData);
				writer.Write(AmongUsClient.Instance.GameId);
			}
			else
			{
				writer.StartMessage(InnerNet.Tags.GameDataTo);
				writer.Write(AmongUsClient.Instance.GameId);
				writer.WritePacked(targetClientId);
			}
		}

		public void QueueSetRole(PlayerControl source, RoleTypes role, bool canOverride = false)
		{
			source.StartCoroutine(source.CoSetRole(role, canOverride));

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetRole);
			writer.Write((ushort)role);
			writer.Write(canOverride);
			writer.EndMessage();
		}

		public void QueueShapeshift(PlayerControl source, PlayerControl target, bool shouldAnimate)
		{
			source.Shapeshift(target, shouldAnimate);

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.Shapeshift);
			writer.WriteNetObject(target);
			writer.Write(shouldAnimate);
			writer.EndMessage();
		}

		public void QueueSpawn(InnerNetObject netObject, int ownerId = -2, SpawnFlags flags = SpawnFlags.None)
		{
			SpawnGameDataMessage spawn = AmongUsClient.Instance.CreateSpawnMessage(netObject, ownerId, flags);
			spawn.Serialize(writer);
		}
		
		public void QueueVotingComplete(MeetingHud.VoterState[] voteStates, NetworkedPlayerInfo ejectedPlayer, bool isTie)
		{
			MeetingHud.Instance.VotingComplete(voteStates, ejectedPlayer, isTie);

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(MeetingHud.Instance.NetId);
			writer.Write((byte)RpcCalls.VotingComplete);

			writer.WritePacked(voteStates.Length);

			foreach(MeetingHud.VoterState state in voteStates)
			{
				state.Serialize(writer);
			}

			writer.Write(ejectedPlayer.PlayerId);
			writer.Write(isTie);

			writer.EndMessage();
		}
		
		public void QueueCloseMeeting()
		{
			MeetingHud.Instance.Close();

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(MeetingHud.Instance.NetId);
			writer.Write((byte)RpcCalls.CloseMeeting);
			writer.EndMessage();
		}
		
		public void FinishBatch()
		{
			writer.EndMessage();
			AmongUsClient.Instance.SendOrDisconnect(writer);
			writer.Recycle();
		}
	}
	
	public record ColorInfo(byte Id, params string[] Aliases);
	public static class ColorHelper
	{
		public static readonly ColorInfo[] Colors =
		{
			new(0, "0", "red", "красн", "красный", "помидор"),
			new(1, "1", "blue", "син", "синий"),
			new(2, "2", "green", "зел", "зеленый", "зелёный"),
			new(3, "3", "pink", "роз", "розовый", "шлюха"),
			new(4, "4", "orange", "оранж", "оранжевый", "трамп"),
			new(5, "5", "yellow", "желт", "жёлт", "желтый", "жёлтый"),
			new(6, "6", "black", "черн", "чёрн", "черный", "чёрный"),
			new(7, "7", "white", "бел", "белый"),
			new(8, "8", "purple", "фиол", "фиолет", "фиолетовый"),
			new(9, "9", "brown", "корич", "коричн", "коричневый", "говно", "какашка"),
			new(10, "10", "cyan", "голуб", "голубой", "бирюзовый", "гей"),
			new(11, "11", "lime", "лайм", "лаймовый", "салат", "салатовый"),
			new(12, "12", "maroon", "бордо", "бордовый", "борода"),
			new(13, "13", "rose", "сирень", "сиреневый"),
			new(14, "14", "banana", "банан", "банановый"),
			new(15, "15", "gray", "сер", "серый"),
			new(16, "16", "tan", "беж", "бежевый"),
			new(17, "17", "coral", "корал", "коралл", "кораловый", "коралловый"),
			new(18, "18", "fortegreen", "forte", "форте", "фортегрин", "???"),
		};

		private static readonly Dictionary<string, byte> AliasMap =
			Colors
				.SelectMany(c => c.Aliases.Select(alias => new KeyValuePair<string, byte>(alias, c.Id)))
				.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

		public static bool TryGetColor(string input, out byte colorId)
		{
			return AliasMap.TryGetValue(input.Trim(), out colorId);
		}
	}
}