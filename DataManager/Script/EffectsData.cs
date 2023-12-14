[Serializable]
public class EffectsData : Savable
{
    [field:SerializeField]public List<EffectIngredientSO> IngredientEffects { get; private set; }
    [field:SerializeField]public List<EffectCustomerSO> CustomerEffects { get; private set; }
    [field:SerializeField]public List<int> IngredientEffectRemainDays{ get; private set; }
    [field:SerializeField]public List<int> CustomerEffectRemainDays{ get; private set; }
    [field:SerializeField]public List<SoAndInt> _effectQueue { get; private set; }

    public EffectsData()
    {
        IngredientEffects = new List<EffectIngredientSO>();
        CustomerEffects = new List<EffectCustomerSO>();
        IngredientEffectRemainDays = new List<int>();
        CustomerEffectRemainDays = new List<int>();
        _effectQueue = new List<SoAndInt>();
    }
    public void AddEffect(EffectIngredientSO effectIngredientSo, int durationDay)
    {
        IngredientEffects.Add(effectIngredientSo);
        IngredientEffectRemainDays.Add(durationDay);
    }
    public void AddEffect(EffectCustomerSO effectCustomerSo, int durationDay)
    {
        CustomerEffects.Add(effectCustomerSo);
        CustomerEffectRemainDays.Add(durationDay);
    }
    
    public void EraseAllData()
    {
        IngredientEffects.Clear();
        CustomerEffects.Clear();
        IngredientEffectRemainDays.Clear();
        CustomerEffectRemainDays.Clear();
        _effectQueue.Clear();
    }
    
    public void SpendDay()
    {
        for (int i = 0; i < CustomerEffects.Count; ++i)
        {
            CustomerEffectRemainDays[i] -= 1;
            if (CustomerEffectRemainDays[i] == 0)
            {
                Debug.Log($"{CustomerEffects[i].EffectType} {CustomerEffectRemainDays[i]}");
                CustomerEffectRemainDays.RemoveAt(i);
                CustomerEffects.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < IngredientEffects.Count; ++i)
        {
            IngredientEffectRemainDays[i] -= 1;
            if (IngredientEffectRemainDays[i] == 0)
            {
                MarketData data = GameManager.Instance.GetMarketData();
                int AddPrice = (int)(IngredientEffects[i].Food.BasePrice * IngredientEffects[i].EffectRate);
                int currentPrice = data.GetCurrentIngredientPrice(IngredientEffects[i].Food.name);
        
                data.UpdateIngredientPrice(IngredientEffects[i].Food.name,currentPrice - AddPrice);
                IngredientEffects.RemoveAt(i);
                IngredientEffectRemainDays.RemoveAt(i);
                i--;
            }           
        }
        
        Debug.Log($"남아있는 효과{CustomerEffectRemainDays.Count}");
    }



    public override void Init(string json, Param saveParam = null)
    {
        EffectSaveData effectSaveData = JsonUtility.FromJson<EffectSaveData>(json);
        EffectsDataParam effectsDataParam = saveParam as EffectsDataParam;
        
        SOCheckUtil.StringToSO(effectSaveData.IngredientEffects,IngredientEffects);
        SOCheckUtil.StringToSO(effectSaveData.CustomerEffects,CustomerEffects);
        IngredientEffectRemainDays = effectSaveData.IngredientEffectRemainDays;
        CustomerEffectRemainDays = effectSaveData.CustomerEffectRemainDays;
            
        for (int i = 0; i < effectSaveData.EffectQueueSOString.Count; ++i)
        {
            EffectSO so = effectsDataParam.dataManager.GetDefaultData<EffectSO>(effectSaveData.EffectQueueSOString[i]);
            _effectQueue.Add(new SoAndInt(so,effectSaveData.EffectQueueValue[i]));
        }
    }

    public override string GetJsonData()
    {
        EffectSaveData effectData = new EffectSaveData();
        effectData.IngredientEffects = new List<string>();
        for (int i = 0; i < IngredientEffects.Count; i++)
        {
            effectData.IngredientEffects.Add(IngredientEffects[i].name);
        }

        effectData.CustomerEffects = new List<string>();
        for (int i = 0; i < CustomerEffects.Count; i++)
        {
            effectData.CustomerEffects.Add(CustomerEffects[i].name);
        }

        effectData.IngredientEffectRemainDays = IngredientEffectRemainDays;
        effectData.CustomerEffectRemainDays = CustomerEffectRemainDays;

        effectData.EffectQueueSOString = new List<string>();
        effectData.EffectQueueValue = new List<int>();
        foreach (var queued in _effectQueue)
        {
            effectData.EffectQueueSOString.Add(queued.effectSo.name);
            effectData.EffectQueueValue.Add(queued.value);
        }

        string jsonData = JsonUtility.ToJson(effectData);
        return jsonData;
    }

    public class EffectsDataParam : Param
    {
        public DataManager dataManager;
    }
}