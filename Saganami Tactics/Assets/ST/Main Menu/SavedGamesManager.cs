using System;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Main_Menu
{
    [RequireComponent(typeof(MainMenuManager))]
    public class SavedGamesManager : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Transform listContent;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private GameObject noSavedGame;
        [SerializeField] private TMP_InputField playerNameField;
#pragma warning restore 649

        private void Start()
        {
            var manager = GetComponent<MainMenuManager>();
            var games = GameStateSaveSystem.ListGames();

            noSavedGame.SetActive(!games.Any());
            listContent.gameObject.SetActive(games.Any());
            
            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var gameInfo in games)
            {
                var item = Instantiate(listItemPrefab, listContent);
                item.transform.Find("Content/GameName").GetComponent<TMP_Text>().text = gameInfo.GameName;
                item.transform.Find("Content/Date").GetComponent<TMP_Text>().text =
                    gameInfo.Date.ToString(new CultureInfo("fr-fr"));
                item.transform.Find("Content/Turn").GetComponent<TMP_Text>().text = $"Turn {gameInfo.Turn}";
                item.transform.Find("Content/Ships").GetComponent<TMP_Text>().text = $"{gameInfo.NbShips} Ships";

                var button = item.GetComponent<Button>();
                button.interactable = playerNameField.text != string.Empty;
                button.onClick.AddListener(() => { manager.LoadGame(gameInfo, playerNameField.text); });
            }
            
            playerNameField.onValueChanged.AddListener(value =>
            {
                foreach (Transform child in listContent)
                {
                    child.GetComponent<Button>().interactable = playerNameField.text != string.Empty;
                }
            });
        }
    }
}