using FPS.Attribute;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class ModelContainer
{
    private static readonly Dictionary<Type, List<ModelBase>> modelDict = new();

    private static readonly Dictionary<Type, ModelBase> singleTonModelDict = new();

    public static void InitSingletonModel<T>() where T : ModelBase, new()
    {
        if (!singleTonModelDict.ContainsKey(typeof(T)))
        {
            singleTonModelDict[typeof(T)] = new T();
        }
    }

    public static void ReleaseModel(object target)
    {
        if (target == null) return;

        var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (Attribute.IsDefined(field, typeof(InjectAttribute)))
            {
                var modelType = field.FieldType;

                if (modelType.IsSubclassOf(typeof(ModelBase)))
                {
                    var model = field.GetValue(target) as ModelBase;

                    if (model != null && modelDict.ContainsKey(modelType))
                    {
                        modelDict[modelType].Remove(model);
                    }
                }
            }
        }
    }


    public static void RemoveSingletonModel<T>()
    {
        singleTonModelDict.Remove(typeof(T));
    }

    private static void AddToModelDict(ModelBase model)
    {
        var modelType = model.GetType();
        if (!modelDict.ContainsKey(modelType))
        {
            modelDict[modelType] = new List<ModelBase>();
        }

        modelDict[modelType].Add(model);
    }

    private static void AddToSingletonModelDict(ModelBase model)
    {
        var modelType = model.GetType();
        if (!singleTonModelDict.ContainsKey(modelType))
        {
            singleTonModelDict[modelType] = model;
        }
    }

    public static void InjectDependencies(object target)
    {
        var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (Attribute.IsDefined(field, typeof(InjectAttribute)))
            {
                var modelType = field.FieldType;
                if (modelType.IsSubclassOf(typeof(ModelBase)))
                {
                    var model = CreateModel(modelType);
                    if (model != null)
                    {
                        field.SetValue(target, model);
                        AddToModelDict(model);
                    }
                }
            }

            if (Attribute.IsDefined (field, typeof(SingletonInject)))
            {
                var modelType = field.FieldType;
                if (modelType.IsSubclassOf (typeof(ModelBase)))
                {
                    var model = GetSingletonModel(modelType);
                    if (model != null)
                    {
                        field.SetValue(target, model);
                    }
                    else
                    {
                        model = CreateModel(modelType);
                        if (model != null)
                        {
                            field.SetValue(target, model);
                            AddToSingletonModelDict(model);
                        }
                    }
                }
            }
        }
    }

    private static ModelBase GetSingletonModel(Type modelType)
    {
        if (singleTonModelDict.TryGetValue(modelType, out ModelBase model))
            return model;

        return null;
    }

    private static ModelBase CreateModel(Type modelType)
    {
        var model = Activator.CreateInstance(modelType) as ModelBase;
        return model;
    }
}