# [DataManager] 데이터 저장 과정의 개선

## 개요

- TodangTodang 구현사항 중 DataManager에 대한 구현/개선 사항에 대한 내용입니다.
- 사용 기술
    - 옵저버 패턴
    - 제네릭 메소드

<br>
<br>

## 개선 배경

![Untitled](Image/Untitled.png)
- 저장해야 할 데이터가 추가될 때 마다 DataManager에 데이터 저장/로드 메소드를 만듬
- 데이터가 새로 생성될 때마다 DataManager를 수정해야함
- 비슷한 구조의 코드의 반복이 일어남

<br>
<br>

## 해결 과정

- 여러 개의 코드가 아닌 한 개의 코드로 관리할 수 있는 방안은 제네릭 메소드가 적합하다고 판단.
- 제네릭 메소드를 이용하면, 하나의 메소드로 단일화 시킬 수 있다고 판단, 제네릭 메소드를 이용
- 추가적으로 옵저버 패턴을 활용하게 된다면, 저장 대상에 대해 저장 시 따로따로 요청해서 하는 것이 아닌<br>
  하나의 클래스를 통해 데이터를 전부 저장할 수 있는 형태로 구성되므로, 옵저버 패턴 적용.

<br>
<br>

## 구현 사항

![Untitled](Image/Untitled%201.png)

### Save과정

- Savable 라는 Base 클래스를 두고, Base클래스는 옵저버의 역할을 담당,
- Savable 를 상속하는 클래스는 자동으로 DataManager에 구독요청
- DataManager는 외부의 저장 요청이 있을 때 구독된 Savable HashSet에 JsonData를 요청
- 각각의 Data에서 JsonData를 추출해 DataManager로 반환
- 가져온 JsonData를 바탕으로 데이터를 저장(암호화, 무결성 검증 데이터 삽입)

### 관련 코드

Savable.cs

```csharp
public abstract class Savable
{
    private DataManager _dataManager;

    public Savable()
    {
        _dataManager = DataManager.Instance;
#if UNITY_EDITOR
        Debug.Assert(_dataManager, $"dataManager {Strings.DebugLog.INIT_PROBLEM} ");
#endif

        _dataManager.RegistSaveData(this);
    }

    public abstract void Init(string json, Param saveParam = null);
    public abstract string GetJsonData();

    ~Savable()
    {
        _dataManager.CancelRegistSaveData(this);
    }
}
```

DataManager.cs 중 Save 관련내용

```csharp
private void Save(string data, string path)
{
    using (StreamWriter sr = new StreamWriter(path))
    {
        string encryptingData = EncryptData(data);
        string versionAddedData = $"{Strings.SaveData.SAVE_VERSION}|{encryptingData}";
        string hashString = versionAddedData.GetHashCode().ToString();
        sr.Write($"{Strings.SaveData.SAVE_VERSION}|{encryptingData}|{hashString}");
    }
}

public void SaveAllData()
{
    foreach (Savable data in _saveDatas)
    {
        Type type = data.GetType();
        if (_filePathDic.TryGetValue(type, out string filePath))
        {
            string jsonData = data.GetJsonData();
            Save(jsonData,filePath);
        }
        else
        {   
#if UNITY_EDITOR
            Debug.LogWarning(Strings.Path.NONE_PATH);
#endif
        }       
    }
}
```

MarketData.cs 중 JsonData 반환 코드
```csharp
public override string GetJsonData()
{
    MarketSaveData marketSaveData = new MarketSaveData();
    marketSaveData.IngredientPriceStrs = new List<string>();
    marketSaveData.IngredientPriceValues= new List<int>();
    marketSaveData.IsSellableStrs = new List<string>();
    marketSaveData.IsSellableValues = new List<bool>();
    foreach (var pricesKeyValue in GetIngredientPrices())
    {
        marketSaveData.IngredientPriceStrs.Add(pricesKeyValue.Key);
        marketSaveData.IngredientPriceValues.Add(pricesKeyValue.Value);
    }
    
    foreach (var sellableKeyValue in GetIsSellableDatas())
    {
        marketSaveData.IsSellableStrs.Add(sellableKeyValue.Key);
        marketSaveData.IsSellableValues.Add(sellableKeyValue.Value);
    }
    
    string jsonData = JsonUtility.ToJson(marketSaveData);
    return jsonData;
}
```
<br>
<br>

---

<br>
<br>

### Load과정

- Load과정은 JsonData를 통하고 싶었지만, 기본 데이터를 DataManager에서 관리하고 있어, 각각의 data 자식 클래스에서 참조하는 것은 코드의 결합도를 높이는 결과라고 판단,
- Load과정에서는 각각의 Data를 직접 반환해주는 것으로 결정
- 이때 제네릭 메소드를 이용, 코드를 하나로 묶어서 관리할 수 있도록 만들어
코드 수정을 국소적으로 해도 되도록 수정함.

### 관련 코드

- DataManager.cs의 Load 과정
MarketData.cs(로드되는 데이터 중 하나)<br>
Init : JsonData데이터가 있을 때 로드 시 사용<br>
GetJson : 저장 시 JsonData 반환

```csharp
public override void Init(string json, Param saveParam = null)
{
    MarketDataParam marketDataParam = saveParam as MarketDataParam;
    MarketSaveData data = JsonUtility.FromJson<MarketSaveData>(json);
    _ingredientInfoList = marketDataParam.IngredientInfoSos;

    if (data != null)
    {
        for (int i = 0; i < data.IngredientPriceStrs.Count; ++i)
        {
            _ingredientPrices.Add(data.IngredientPriceStrs[i],data.IngredientPriceValues[i]);
            _isSellable.Add(data.IsSellableStrs[i],data.IsSellableValues[i]);
        }
    }
}

public class MarketDataParam : Param
{
    public IngredientInfoSO[] IngredientInfoSos;
}
```

<br>
<br>

## 개선 더 필요한 사항

### DataManager의 과도한 역할
  - 현재 구조는 DataManager가 SO와 같은 기본 데이터들을 저장하는 기능과 Save/Load 기능을 포함하고 있는 클래스임
  - 두 개를 분리해서 DataManager는 기본데이터의 관리만, SaveAndLoad는  Save와 Load기능만 관리하게 하고, 각각이 따로 BaseClass나 Interface에서 접근하도록 설계를 해 놓는다면, 기본 데이터와 SaveAndLoad 부분 간 결합도를 낮출 수 있어, 구조적으로 좋았을 것임.

### Savable와 DataManager의 관계
  - Savable에서 Save와 Load를 string 데이터만을 이용해 구현하게 한다면, 지금보다 더 간단한 구조의 Save/Load 시스템이 완성되었을 것임
  - string만을 사용하기에 지금 구조와 다르게 제네릭 메소드 없이 구현이 가능하게 됨
  - 이에 따라 DataManager에서는 수정 없이 저장 데이터를 계속해서 포함할 수 있게됨.
  - 이를 통해 간단하면서도, 코드 가독성 증대 및, 저장 데이터의 추가/삭제에 영향 전혀 없는 DataManager가 되었을 것임.