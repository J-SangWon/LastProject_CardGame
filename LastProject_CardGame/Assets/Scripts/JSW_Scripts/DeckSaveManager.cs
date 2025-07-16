using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public class DeckSaveData
{
    public string deckName;
    public List<DeckCardEntry> mainDeck = new List<DeckCardEntry>();
    public List<DeckCardEntry> extraDeck = new List<DeckCardEntry>();
}

[System.Serializable]
public class DeckListData
{
    public List<string> deckNames = new List<string>();
    public string lastSelectedDeck = "";
}

public class DeckSaveManager : MonoBehaviour
{
    private const string DECK_LIST_FILE = "deck_list.json";
    private const string LAST_DECK_KEY = "LastSelectedDeck";
    
    private DeckListData deckListData;
    
    void Awake()
    {
        if (deckListData == null)
            LoadDeckList();
    }
    
    // 덱 리스트 불러오기
    private void LoadDeckList()
    {
        string path = Path.Combine(Application.persistentDataPath, DECK_LIST_FILE);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            deckListData = JsonUtility.FromJson<DeckListData>(json);
        }
        else
        {
            deckListData = new DeckListData();
        }
    }
    
    // 덱 리스트 저장
    private void SaveDeckList()
    {
        string json = JsonUtility.ToJson(deckListData, true);
        string path = Path.Combine(Application.persistentDataPath, DECK_LIST_FILE);
        File.WriteAllText(path, json);
    }
    
    // 덱 저장
    public void SaveDeck(DeckData deckData, string fileName)
    {
        // BaseCardData 참조를 ID로 변환
        var saveData = ConvertToSaveFormat(deckData);
        string json = JsonUtility.ToJson(saveData, true);
        
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        File.WriteAllText(path, json);
        
        // 덱 리스트에 추가 (중복 방지)
        if (!deckListData.deckNames.Contains(fileName))
        {
            deckListData.deckNames.Add(fileName);
            SaveDeckList();
        }
    }
    
    // 덱 불러오기
    public DeckData LoadDeck(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var saveData = JsonUtility.FromJson<DeckSaveData>(json);
            return ConvertFromSaveFormat(saveData);
        }
        return null;
    }
    
    // 마지막 선택된 덱 불러오기
    public DeckData LoadLastSelectedDeck()
    {
        if (!string.IsNullOrEmpty(deckListData.lastSelectedDeck))
        {
            return LoadDeck(deckListData.lastSelectedDeck);
        }
        return null;
    }
    
    // 덱 선택 (마지막 선택 덱 업데이트)
    public void SelectDeck(string deckName)
    {
        deckListData.lastSelectedDeck = deckName;
        SaveDeckList();
    }
    
    // 덱 삭제
    public void DeleteDeck(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        deckListData.deckNames.Remove(fileName);
        
        // 삭제된 덱이 마지막 선택 덱이었다면 초기화
        if (deckListData.lastSelectedDeck == fileName)
        {
            deckListData.lastSelectedDeck = "";
        }
        
        SaveDeckList();
    }
    
    // 모든 덱 이름 가져오기
    public List<string> GetAllDeckNames()
    {
        if (deckListData == null)
            LoadDeckList();
        if (deckListData == null)
            return new List<string>();
        return new List<string>(deckListData.deckNames);
    }
    
    // 마지막 선택된 덱 이름 가져오기
    public string GetLastSelectedDeckName()
    {
        return deckListData.lastSelectedDeck;
    }
    
    // BaseCardData를 ID로 변환하여 저장용 데이터 생성
    private DeckSaveData ConvertToSaveFormat(DeckData deckData)
    {
        var saveData = new DeckSaveData();
        saveData.deckName = deckData.deckName;
        
        // 메인 덱 변환
        foreach (var entry in deckData.mainDeck)
        {
            var saveEntry = new DeckCardEntry();
            saveEntry.card = entry.card;
            saveEntry.count = entry.count;
            saveData.mainDeck.Add(saveEntry);
        }
        
        // 엑스트라 덱 변환
        foreach (var entry in deckData.extraDeck)
        {
            var saveEntry = new DeckCardEntry();
            saveEntry.card = entry.card;
            saveEntry.count = entry.count;
            saveData.extraDeck.Add(saveEntry);
        }
        
        return saveData;
    }
    
    // 저장된 데이터를 DeckData로 변환
    private DeckData ConvertFromSaveFormat(DeckSaveData saveData)
    {
        var deckData = new DeckData();
        deckData.deckName = saveData.deckName;
        
        // 메인 덱 변환
        foreach (var entry in saveData.mainDeck)
        {
            var deckEntry = new DeckCardEntry();
            deckEntry.card = entry.card;
            deckEntry.count = entry.count;
            deckData.mainDeck.Add(deckEntry);
        }
        
        // 엑스트라 덱 변환
        foreach (var entry in saveData.extraDeck)
        {
            var deckEntry = new DeckCardEntry();
            deckEntry.card = entry.card;
            deckEntry.count = entry.count;
            deckData.extraDeck.Add(deckEntry);
        }
        
        return deckData;
    }
}
