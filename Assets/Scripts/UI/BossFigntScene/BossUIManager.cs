using UnityEngine;
using UnityEngine.Rendering;
public enum BossPage
{
   InBattle,
   End
}
public class BossUIManager: MonoBehaviour
{
    public static BossUIManager Instance;

    [Header("Panels")]
    [SerializeField]
    private GameObject _inBattlePage;
    [SerializeField]
    private GameObject _endBattlePage;
    [Header("Reference")]
    [SerializeField]
    private FinalInfoPanel _info;
    private void Awake()
    {
        if (Instance == null) { Instance = this; } 
        else { Destroy(this); }
    }
    public void ShowPage(BossPage page)
    {
        _inBattlePage.SetActive(page == BossPage.InBattle);
        _endBattlePage.SetActive(page == BossPage.End);
    }
    public void ShowInBattle(){ShowPage(BossPage.InBattle); }
    public void ShowEndBattle(bool isSuccess,PlayerBattleStats[] stats){
        ShowPage(BossPage.End);
       _info.UpdateBattleEndInfo(isSuccess,stats);
    }
}
