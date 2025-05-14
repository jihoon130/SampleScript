using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
public static class DataContainer
{
	public static Dictionary<int, EnemyData> EnemyData = new();
	public static Dictionary<string, PlayerData> PlayerData = new();
	public static Dictionary<int, RoundData> RoundData = new();
	public static Dictionary<string, ShopData> ShopData = new();
	public static Dictionary<string, WeaponData> WeaponData = new();
    
    public static void InitializeData()
    {
        var types = FindAllTypesImplementingIGameData();

        foreach (var type in types)
        {
            string jsonFilePath = Path.Combine(Application.dataPath, "Data", type.Name + ".json");

            if (File.Exists(jsonFilePath))
            {
                string jsonContent = File.ReadAllText(jsonFilePath);

                var gameDataList = DeserializeGameDataList(jsonContent, type);

                AddToDataDictionary(type, gameDataList);
            }
        }
    }

    public static Type[] FindAllTypesImplementingIGameData()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var gameDataTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IGameData).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .ToArray();

        return gameDataTypes;
    }

    private static IList<IGameData> DeserializeGameDataList(string jsonContent, Type type)
    {
        var listType = typeof(List<>).MakeGenericType(type);

        var result = JsonConvert.DeserializeObject(jsonContent, listType);

        if (result is IEnumerable<IGameData> enumerable)
        {
            return enumerable.ToList();
        }

        return null;
    }

    private static void AddToDataDictionary(Type type, IList<IGameData> gameDataList)
    {
        var fields = typeof(DataContainer).GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Where(f => f.FieldType.IsGenericType &&
                                        f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                            .ToList();

        var findField = fields.Find(f => f.Name == type.Name);
        if (findField != null)
        {
            foreach (var gameData in gameDataList)
            {
                var firstField = gameData.GetType().GetFields().FirstOrDefault();

                var key = firstField.GetValue(gameData);

                var dictionary = (IDictionary)findField.GetValue(null);
                var method = dictionary.GetType().GetMethod("Add");
                method.Invoke(dictionary, new object[] { key, gameData });
        }
    }
}
}