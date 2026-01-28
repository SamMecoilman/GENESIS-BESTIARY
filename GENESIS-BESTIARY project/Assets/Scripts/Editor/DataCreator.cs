using UnityEngine;
using UnityEditor;
using GenesisBestiary.Player;
using GenesisBestiary.Combat;
using GenesisBestiary.Monster;
using GenesisBestiary.Quest;

public static class DataCreator
{
    [MenuItem("GENESIS/Create/Player Data (Default)")]
    public static void CreatePlayerData()
    {
        EnsureFolderExists("Assets/ScriptableObjects/Players");
        var asset = ScriptableObject.CreateInstance<PlayerData>();
        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Players/PlayerData_Default.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = asset;
        Debug.Log("PlayerData_Default created!");
    }
    
    [MenuItem("GENESIS/Create/Weapon Data (Greatsword)")]
    public static void CreateWeaponData()
    {
        EnsureFolderExists("Assets/ScriptableObjects/Weapons");
        var asset = ScriptableObject.CreateInstance<WeaponData>();
        asset.weaponName = "Greatsword";
        asset.attack = 100;
        asset.affinity = 0;
        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Weapons/Greatsword_Default.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = asset;
        Debug.Log("Greatsword_Default created!");
    }
    
    [MenuItem("GENESIS/Create/Attack Data Set (Greatsword)")]
    public static void CreateAttackDataSet()
    {
        EnsureFolderExists("Assets/ScriptableObjects/Weapons");
        
        // Vertical Slash
        var attack1 = ScriptableObject.CreateInstance<AttackData>();
        attack1.attackName = "Vertical Slash";
        attack1.motionValue = 48;
        attack1.startup = 0.5f;
        attack1.active = 0.1f;
        attack1.recovery = 0.8f;
        attack1.forwardMovement = 1f;
        attack1.canCombo = true;
        attack1.comboWindow = 0.6f;
        AssetDatabase.CreateAsset(attack1, "Assets/ScriptableObjects/Weapons/Attack_VerticalSlash.asset");
        
        // Horizontal Slash
        var attack2 = ScriptableObject.CreateInstance<AttackData>();
        attack2.attackName = "Horizontal Slash";
        attack2.motionValue = 26;
        attack2.startup = 0.3f;
        attack2.active = 0.1f;
        attack2.recovery = 0.5f;
        attack2.forwardMovement = 0.5f;
        attack2.canCombo = true;
        attack2.comboWindow = 0.4f;
        AssetDatabase.CreateAsset(attack2, "Assets/ScriptableObjects/Weapons/Attack_HorizontalSlash.asset");
        
        // Rising Slash
        var attack3 = ScriptableObject.CreateInstance<AttackData>();
        attack3.attackName = "Rising Slash";
        attack3.motionValue = 38;
        attack3.startup = 0.4f;
        attack3.active = 0.1f;
        attack3.recovery = 0.7f;
        attack3.forwardMovement = 0.3f;
        attack3.canCombo = false;
        AssetDatabase.CreateAsset(attack3, "Assets/ScriptableObjects/Weapons/Attack_RisingSlash.asset");
        
        AssetDatabase.SaveAssets();
        Debug.Log("Attack Data Set created!");
    }
    
    [MenuItem("GENESIS/Create/Monster Data (Test Monster)")]
    public static void CreateMonsterData()
    {
        EnsureFolderExists("Assets/ScriptableObjects/Monsters");
        var asset = ScriptableObject.CreateInstance<MonsterData>();
        asset.monsterName = "Test Monster";
        asset.maxHealth = 1000;
        asset.moveSpeed = 4f;
        asset.chaseSpeed = 6f;
        asset.detectionRange = 20f;
        asset.attackRange = 3f;
        asset.flinchThreshold = 100;
        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Monsters/TestMonster_Data.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = asset;
        Debug.Log("TestMonster_Data created!");
    }
    
    [MenuItem("GENESIS/Create/Quest Data (Test Quest)")]
    public static void CreateQuestData()
    {
        EnsureFolderExists("Assets/ScriptableObjects/Quests");
        var asset = ScriptableObject.CreateInstance<QuestData>();
        asset.questName = "Test Hunt";
        asset.description = "Hunt the Test Monster";
        asset.timeLimit = 50f * 60f;
        asset.maxDeaths = 3;
        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Quests/Quest_TestHunt.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = asset;
        Debug.Log("Quest_TestHunt created!");
    }
    
    [MenuItem("GENESIS/Create All Default Data")]
    public static void CreateAllDefaultData()
    {
        CreatePlayerData();
        CreateWeaponData();
        CreateAttackDataSet();
        CreateMonsterData();
        CreateQuestData();
        Debug.Log("All default data created!");
    }
    
    private static void EnsureFolderExists(string path)
    {
        string[] folders = path.Split('/');
        string currentPath = folders[0];
        
        for (int i = 1; i < folders.Length; i++)
        {
            string newPath = currentPath + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(newPath))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }
            currentPath = newPath;
        }
    }
}
