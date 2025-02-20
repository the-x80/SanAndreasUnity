﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public static class SingletonConstants
    {
        public static readonly IReadOnlyList<string> nonAllowedMethodNames = new string[]
        {
            "Awake",
            "OnEnable",
            "OnDisable",
            "Start",
        };
    }

    public class SingletonComponent<T> : MonoBehaviour
        where T : SingletonComponent<T>
    {
#if !UNITY_EDITOR
        public static T Singleton { get; private set; }
#else
        private static T s_cachedSingleton;
        public static T Singleton
        {
            get
            {
                if (!F.IsAppInEditMode)
                {
                    return s_cachedSingleton;
                }

                if (s_cachedSingleton != null)
                    return s_cachedSingleton;

                T[] objects = FindObjectsOfType<T>();

                if (objects.Length == 0)
                    return null;

                if (objects.Length > 1)
                    throw new Exception($"Found multiple singleton objects of type {typeof(T).Name}. Make sure there is only 1 singleton object created per type.");

                s_cachedSingleton = objects[0];
                return s_cachedSingleton;
            }
            private set
            {
                s_cachedSingleton = value;
            }
        }
#endif



        protected SingletonComponent()
        {
            Type type = this.GetType();
            var bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            for (int i = 0; i < SingletonConstants.nonAllowedMethodNames.Count; i++)
            {
                string methodName = SingletonConstants.nonAllowedMethodNames[i];
                var methodInfo = type.GetMethod(methodName, bindingFlags);
                if (methodInfo != null)
                    throw new Exception($"{type.Name} is using non-allowed method {methodName}. Singletons should not have any of following methods: {string.Join(", ", SingletonConstants.nonAllowedMethodNames)}.");
            }
        }

        private void Awake()
        {
            if (Singleton != null)
            {
                throw new Exception($"Awake() method called twice for singleton of type {this.GetType().Name}");
            }

            this.OnSingletonAwakeValidate();

            Singleton = (T)this;

            this.OnSingletonAwake();
        }

        protected virtual void OnSingletonAwake()
        {
        }

        protected virtual void OnSingletonAwakeValidate()
        {
        }

        private void OnDisable()
        {
            if (Singleton != this)
                return;

            this.OnSingletonDisable();
        }

        protected virtual void OnSingletonDisable()
        {
        }

        private void Start()
        {
            if (this != Singleton)
                return;

            this.OnSingletonStart();
        }

        protected virtual void OnSingletonStart()
        {
        }
    }
}
