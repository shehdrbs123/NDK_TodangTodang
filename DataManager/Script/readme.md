# 개요
DataManager 관련된 코드 모음입니다.


# 클래스 목록
|||
|---|---|
|[DataManager.cs](DataManager.cs)| 기본 데이터인 SO를 Resources로 부터 가져와 보관하고<br> Savable 데이터를 파일로 저장/로드하는 Class |
|[Savable.cs](Savable.cs)| 저장관련된 기본 클래스로 DataManager에 구독을 등록하고, <br> DataManager로 부터 저장 요청을 받는 Base 클래스 |
[NewsSystem.cs](NewsSystem.cs)| Savable 상속을 받은 저장 클래스 중 하나로, 성취형 뉴스를 취득했는지 여부를 저장 |
|[PlayerData.cs](PlayerData.cs)| Player의 돈, 평점, inventory의 내용물을 저장하는 데이터 클래스 |
|[MarketData.cs](MarketData.cs)| Market내 실시간 재료의 가격을 보관하고, <br>판매 가능한 재료인지 판단하는 정보를 저장하는 데이터 클래스|
|[EffectsData.cs](EffectsData.cs)| 현재 EffectManager 내 적용중인 Effect와 다음날 적용할 Effect의 큐를 저장하는 데이터 클래스|
|[DecoStoreData.cs](DecoStoreData.cs)| 현재 DecoStore 내 구매된 Deco 오브젝트를 저장하는 데이터 클래스|