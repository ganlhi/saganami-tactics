using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Michsky.UI.Shift;
using Photon.Pun;
using Photon.Realtime;
using ST.Common;
using ST.Common.UI;
using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ST.Setup
{
    public class SetupManager : MonoBehaviourPunCallbacks
    {
#pragma warning disable 649
        [SerializeField] private ModalWindowManager messageModal;
        [SerializeField] private BlurManager blurManager;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button exitButton;

        [SerializeField] private HorizontalSelector factionSelector;
        [SerializeField] private HorizontalSelector categorySelector;
        [SerializeField] private HorizontalSelector ssdSelector;

        [SerializeField] private SsdPanel ssdPanel;

        [SerializeField] private SetupShipListItem shipListPrefab;
        [SerializeField] private Transform shipListContent;

        [SerializeField] private UIManagerText totalCostLabelManager;
        [SerializeField] private UIManagerText totalCostValueManager;
        [SerializeField] private TextMeshProUGUI totalCostValueText;
        [SerializeField] private List<OtherPlayerCostAndReadiness> otherPlayersCostAndReadiness;

        [SerializeField] private Button readyButton;
        [SerializeField] private GameObject waitingFoOtherPlayers;

        [SerializeField] private Button addButton;
#pragma warning restore 649

        private bool _initOnNextUpdate;

        private List<Faction> _factions;
        private List<ShipCategory> _categories;
        private List<Ssd> _ssds;

        private readonly UnityEvent _factionChanged = new UnityEvent();
        private readonly UnityEvent _categoryChanged = new UnityEvent();
        private readonly UnityEvent _ssdChanged = new UnityEvent();

        private Ssd _selectedSsd;
        private Ssd _displayedSsd;

        private void Start()
        {
            _ssds = SsdHelper.AvailableSsds.Values.ToList();
            _categories = SsdHelper.AvailableCategories;
            _factions = SsdHelper.AvailableFactions;

            _factionChanged.AddListener(OnFactionChanged);
            _categoryChanged.AddListener(OnCategoryChanged);
            _ssdChanged.AddListener(OnSsdChanged);

            foreach (Transform child in shipListContent)
            {
                Destroy(child.gameObject);
            }

            if (PhotonNetwork.InRoom)
            {
                _initOnNextUpdate = true;
            }
        }

        private void Update()
        {
            if (_initOnNextUpdate)
            {
                _initOnNextUpdate = false;
                Init();
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Init();
        }

        private void Init()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            exitButton.onClick.AddListener(() =>
            {
                PhotonNetwork.LeaveRoom();
                SceneManager.LoadScene(GameSettings.Default.SceneMainMenu);
            });
            readyButton.onClick.AddListener(() => PhotonNetwork.LocalPlayer.SetReady());

            InitOtherPlayersCostAndReadiness();
            UpdateTitle();

            factionSelector.itemList = _factions.Select(f => new HorizontalSelector.Item
            {
                itemTitle = f.Name,
                onValueChanged = _factionChanged
            }).ToList();

            factionSelector.index = 0;
            factionSelector.UpdateUI();

            OnFactionChanged();
        }

        private void InitOtherPlayersCostAndReadiness()
        {
            var allTeams = new Team[]
            {
                Team.Blue,
                Team.Yellow,
                Team.Green,
                Team.Magenta
            };

            var i = 0;
            foreach (var team in allTeams)
            {
                otherPlayersCostAndReadiness[i].Team = team;
                otherPlayersCostAndReadiness[i].SetCost(0, false);
                otherPlayersCostAndReadiness[i].SetReady(false);
                otherPlayersCostAndReadiness[i].gameObject.SetActive(false);
                i++;
            }
        }

        private void OnFactionChanged()
        {
            var selectedFaction =
                _factions.First(f => f.Name == factionSelector.itemList[factionSelector.index].itemTitle);

            var ssdsInFaction = _ssds.Where(s => s.faction == selectedFaction);
            var availableCategories = _categories.Where(cat => ssdsInFaction.Any(ssd => ssd.category == cat));

            categorySelector.itemList = availableCategories.Select(c => new HorizontalSelector.Item
            {
                itemTitle = c.Name,
                onValueChanged = _categoryChanged
            }).ToList();

            categorySelector.index = 0;
            categorySelector.UpdateUI();

            OnCategoryChanged();
        }

        private void OnCategoryChanged()
        {
            var selectedFaction =
                _factions.First(f => f.Name == factionSelector.itemList[factionSelector.index].itemTitle);
            var selectedCategory =
                _categories.First(c => c.Name == categorySelector.itemList[categorySelector.index].itemTitle);

            var ssdsInFactionAndCategory =
                _ssds.Where(s => s.faction == selectedFaction && s.category == selectedCategory).ToList();

            ssdsInFactionAndCategory.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

            ssdSelector.itemList = ssdsInFactionAndCategory.Select(s => new HorizontalSelector.Item
            {
                itemTitle = s.className,
                onValueChanged = _ssdChanged
            }).ToList();

            ssdSelector.index = 0;
            ssdSelector.UpdateUI();

            OnSsdChanged();
        }

        private void OnSsdChanged()
        {
            _selectedSsd = _ssds.First(s => s.className == ssdSelector.itemList[ssdSelector.index].itemTitle);

            if (_displayedSsd == null)
            {
                ShowSelectedSsd();
            }
        }

        public void ShowSelectedSsd()
        {
            ShowSsd(_selectedSsd);
        }

        public void ShowSsd(Ssd ssd, string shipName = "")
        {
            _displayedSsd = ssd;
            ssdPanel.Ssd = ssd;
            ssdPanel.ShipName = shipName.Length > 0 ? shipName : null;
        }

        public void AddSelectedSsd()
        {
            if (_selectedSsd != null)
            {
                photonView.RPC("RPC_AddSsd",
                    RpcTarget.MasterClient,
                    _selectedSsd.className,
                    PhotonNetwork.LocalPlayer.GetTeam()
                );
            }
        }

        [PunRPC]
        private void RPC_AddedShip(string ssdName, Team team, string uid)
        {
            if (team != PhotonNetwork.LocalPlayer.GetTeam()) return;

            var listItem = Instantiate(shipListPrefab, shipListContent).GetComponent<SetupShipListItem>();
            listItem.Ship = new Tuple<Ssd, string>(SsdHelper.AvailableSsds[ssdName], "");
            listItem.OnShowSsd += (sender, args) => ShowSsd(listItem.Ship.Item1, listItem.Ship.Item2);
            listItem.OnDelete += (sender, args) =>
            {
                photonView.RPC("RPC_RemoveShip", RpcTarget.MasterClient, team, uid);
                StartCoroutine(Utils.DelayedAction(() => { Destroy(listItem.gameObject); }, .5f));
            };
            listItem.OnNameChange += (sender, newName) =>
            {
                photonView.RPC("RPC_SetName", RpcTarget.MasterClient, team, uid, newName);
                listItem.Ship = new Tuple<Ssd, string>(listItem.Ship.Item1, newName);
            };
        }

        [PunRPC]
        private void RPC_UpdateTotalCost(Team team, int totalCost, bool costOverflow, bool allShipsNamed)
        {
            if (team == PhotonNetwork.LocalPlayer.GetTeam())
            {
                totalCostLabelManager.colorType =
                    costOverflow ? UIManagerText.ColorType.NEGATIVE : UIManagerText.ColorType.PRIMARY;
                totalCostValueManager.colorType =
                    costOverflow ? UIManagerText.ColorType.NEGATIVE : UIManagerText.ColorType.PRIMARY;
                totalCostValueText.text = totalCost.ToString();

                readyButton.interactable = totalCost > 0 && !costOverflow && allShipsNamed;
            }
            else
            {
                var otherPlayerCostAndReadiness = otherPlayersCostAndReadiness.First(op => op.Team == team);
                if (otherPlayerCostAndReadiness != null)
                {
                    otherPlayerCostAndReadiness.gameObject.SetActive(true);
                    otherPlayerCostAndReadiness.SetCost(totalCost, costOverflow);
                }
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (changedProps.ContainsKey(GameSettings.Default.ReadyProp))
            {
                if (targetPlayer.IsLocal)
                {
                    readyButton.gameObject.SetActive(!targetPlayer.IsReady());
                    waitingFoOtherPlayers.SetActive(targetPlayer.IsReady());

                    addButton.interactable = !targetPlayer.IsReady();
                    foreach (Transform shipListItem in shipListContent)
                    {
                        shipListItem.Find("DeleteButton").gameObject.SetActive(!targetPlayer.IsReady());
                        shipListItem.Find("RandomNameButton").gameObject.SetActive(!targetPlayer.IsReady());
                        shipListItem.Find("UnitName").GetComponent<TMP_InputField>().interactable =
                            !targetPlayer.IsReady();
                    }
                }
                else
                {
                    var otherPlayerCostAndReadiness = otherPlayersCostAndReadiness.First(op =>
                    {
                        var team = targetPlayer.GetTeam();
                        return team.HasValue && op.Team == team.Value;
                    });

                    if (otherPlayerCostAndReadiness != null)
                    {
                        otherPlayerCostAndReadiness.SetReady(targetPlayer.IsReady());
                    }
                }
            }

            // Only for master
            if (PhotonNetwork.IsMasterClient &&
                PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers &&
                PhotonNetwork.CurrentRoom.Players.Values.All(p => p.IsReady()))
            {
                CreateGameStateAndContinue();
            }
        }

        private void UpdateTitle()
        {
            titleText.text = $"Setup game: {PhotonNetwork.CurrentRoom.Name}";
        }

        #region Main client

        private readonly Dictionary<Team, Dictionary<string, Tuple<Ssd, string>>> _teamShips =
            new Dictionary<Team, Dictionary<string, Tuple<Ssd, string>>>();

        [PunRPC]
        private void RPC_AddSsd(string ssdName, Team team)
        {
            var uid = Utils.GenerateId();
            var item = new Tuple<Ssd, string>(SsdHelper.AvailableSsds[ssdName], "");

            if (!_teamShips.ContainsKey(team))
            {
                _teamShips.Add(team, new Dictionary<string, Tuple<Ssd, string>>()
                {
                    {uid, item},
                });
            }
            else
            {
                _teamShips[team].Add(uid, item);
            }

            photonView.RPC("RPC_AddedShip", RpcTarget.All, ssdName, team, uid);

            SendUpdatedTotalCost(team);
        }

        [PunRPC]
        private void RPC_RemoveShip(Team team, string uid)
        {
            if (!_teamShips.ContainsKey(team)) return;
            _teamShips[team].Remove(uid);
            SendUpdatedTotalCost(team);
        }

        [PunRPC]
        private void RPC_SetName(Team team, string uid, string newName)
        {
            if (!_teamShips.ContainsKey(team)) return;
            var cur = _teamShips[team][uid];
            _teamShips[team][uid] = new Tuple<Ssd, string>(cur.Item1, newName);
            SendUpdatedTotalCost(team);
        }

        private void SendUpdatedTotalCost(Team team)
        {
            if (!_teamShips.ContainsKey(team)) return;
            var teamShips = _teamShips[team].Values.ToList();
            var totalCost = teamShips.Sum(t => t.Item1.baseCost);
            photonView.RPC("RPC_UpdateTotalCost", RpcTarget.All, team, totalCost,
                totalCost > PhotonNetwork.CurrentRoom.GetMaxPoints(),
                teamShips.TrueForAll(t => !string.IsNullOrEmpty(t.Item2)));
        }

        private void CreateGameStateAndContinue()
        {
            PhotonNetwork.CurrentRoom.ResetPlayersReadiness();

            var allShips = new List<ShipState>();
            var allTeams = new Team[]
            {
                Team.Blue,
                Team.Yellow,
                Team.Green,
                Team.Magenta
            };

            foreach (var team in allTeams)
            {
                if (!_teamShips.ContainsKey(team)) continue;

                var teamShipStates = new List<ShipState>();

                foreach (var (ssd, shipName) in _teamShips[team].Values)
                {
                    var shipState = new ShipState()
                    {
                        ssdName = ssd.className,
                        name = shipName ?? ssd.className,
                        status = ShipStatus.Ok,
                        uid = Utils.GenerateId(),
                        team = team,
                        position = Vector3.zero,
                        rotation = Quaternion.identity,
                        velocity = Vector3.zero,
                    };

                    teamShipStates.Add(shipState);
                }

                allShips.AddRange(Game.PrePlaceTeamShips(team, teamShipStates));
            }

            var gameState = new GameState()
            {
                turn = 1,
                step = TurnStep.Plotting,
                setup = new GameSetup()
                {
                    maxCost = PhotonNetwork.CurrentRoom.GetMaxPoints(),
                    nbPlayers = PhotonNetwork.CurrentRoom.PlayerCount
                },
                ships = allShips
            };

            var gameStateGo = new GameObject {name = "_GameState"};
            DontDestroyOnLoad(gameStateGo);
            var container = gameStateGo.AddComponent<HasGameState>();
            container.gameState = gameState;

            PhotonNetwork.LoadLevel(GameSettings.Default.SceneDeploy);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            SendUpdatedTotalCost(Team.Blue);
            SendUpdatedTotalCost(Team.Yellow);
            SendUpdatedTotalCost(Team.Green);
            SendUpdatedTotalCost(Team.Magenta);
        }

        #endregion
    }
}