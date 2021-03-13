// Decompiled with JetBrains decompiler
// Type: GalaSoft.MvvmLight.Ioc.SimpleIoc
// Assembly: GalaSoft.MvvmLight, Version=5.4.1.0, Culture=neutral, PublicKeyToken=0ffbc31322e9d308
// MVID: 4BFE803B-4723-4F56-B6C3-0E8BC3E0A4B9
// Assembly location: C:\Users\ckara\.nuget\packages\mvvmlightlibsstd10\5.4.1.1\lib\uap10.0\GalaSoft.MvvmLight.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SpotifyLibV2.Attributes;

namespace SpotifyLibV2.Helpers
{
    /// <summary>
    /// A very simple IOC container with basic functionality needed to register and resolve
    /// instances. If needed, this class can be replaced by another more elaborate
    /// IOC container implementing the IServiceLocator interface.
    /// The inspiration for this class is at https://gist.github.com/716137 but it has
    /// been extended with additional features.
    /// </summary>
    public class ServiceLocator : IServiceLocator
    {
        private readonly Dictionary<Type, ConstructorInfo> _constructorInfos = new();
        private readonly string _defaultKey = Guid.NewGuid().ToString();
        private readonly object[] _emptyArguments = new object[0];
        private readonly Dictionary<Type, Dictionary<string, Delegate>> _factories = new();
        private readonly Dictionary<Type, Dictionary<string, object>> _instancesRegistry = new();
        private readonly Dictionary<Type, Type> _interfaceToClassMap = new();
        private readonly object _syncLock = new();
        private static readonly object _instanceLock = new();
        private static ServiceLocator _default;

        /// <summary>This class' default instance.</summary>
        public static ServiceLocator Default
        {
            get
            {
                if (ServiceLocator._default == null)
                {
                    lock (ServiceLocator._instanceLock)
                    {
                        if (ServiceLocator._default == null)
                            ServiceLocator._default = new ServiceLocator();
                    }
                }
                return ServiceLocator._default;
            }
        }

        /// <summary>
        /// Checks whether at least one instance of a given class is already created in the container.
        /// </summary>
        /// <typeparam name="TClass">The class that is queried.</typeparam>
        /// <returns>True if at least on instance of the class is already created, false otherwise.</returns>
        public bool ContainsCreated<TClass>() => this.ContainsCreated<TClass>((string)null);

        /// <summary>
        /// Checks whether the instance with the given key is already created for a given class
        /// in the container.
        /// </summary>
        /// <typeparam name="TClass">The class that is queried.</typeparam>
        /// <param name="key">The key that is queried.</param>
        /// <returns>True if the instance with the given key is already registered for the given class,
        /// false otherwise.</returns>
        public bool ContainsCreated<TClass>(string key)
        {
            Type key1 = typeof(TClass);
            if (!this._instancesRegistry.ContainsKey(key1))
                return false;
            return string.IsNullOrEmpty(key) ? this._instancesRegistry[key1].Count > 0 : this._instancesRegistry[key1].ContainsKey(key);
        }

        /// <summary>
        /// Gets a value indicating whether a given type T is already registered.
        /// </summary>
        /// <typeparam name="T">The type that the method checks for.</typeparam>
        /// <returns>True if the type is registered, false otherwise.</returns>
        public bool IsRegistered<T>() => this._interfaceToClassMap.ContainsKey(typeof(T));

        /// <summary>
        /// Gets a value indicating whether a given type T and a give key
        /// are already registered.
        /// </summary>
        /// <typeparam name="T">The type that the method checks for.</typeparam>
        /// <param name="key">The key that the method checks for.</param>
        /// <returns>True if the type and key are registered, false otherwise.</returns>
        public bool IsRegistered<T>(string key)
        {
            Type key1 = typeof(T);
            return this._interfaceToClassMap.ContainsKey(key1) && this._factories.ContainsKey(key1) && this._factories[key1].ContainsKey(key);
        }

        /// <summary>Registers a given type for a given interface.</summary>
        /// <typeparam name="TInterface">The interface for which instances will be resolved.</typeparam>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        public void Register<TInterface, TClass>()
          where TInterface : class
          where TClass : class, TInterface => this.Register<TInterface, TClass>(false);

        /// <summary>
        /// Registers a given type for a given interface with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface for which instances will be resolved.</typeparam>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        public void Register<TInterface, TClass>(bool createInstanceImmediately)
          where TInterface : class
          where TClass : class, TInterface
        {
            lock (this._syncLock)
            {
                Type type1 = typeof(TInterface);
                Type type2 = typeof(TClass);
                if (this._interfaceToClassMap.ContainsKey(type1))
                {
                    if ((object)this._interfaceToClassMap[type1] != (object)type2)
                        throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "There is already a class registered for {0}.", new object[1]
                        {
              (object) type1.FullName
                        }));
                }
                else
                {
                    this._interfaceToClassMap.Add(type1, type2);
                    this._constructorInfos.Add(type2, this.GetConstructorInfo(type2));
                }
                Func<TInterface> factory = new Func<TInterface>(this.MakeInstance<TInterface>);
                this.DoRegister<TInterface>(type1, factory, this._defaultKey);
                if (!createInstanceImmediately)
                    return;
                this.GetInstance<TInterface>();
            }
        }

        /// <summary>Registers a given type.</summary>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        public void Register<TClass>() where TClass : class => this.Register<TClass>(false);

        /// <summary>
        /// Registers a given type with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        public void Register<TClass>(bool createInstanceImmediately) where TClass : class
        {
            Type type = typeof(TClass);
            if (type.GetTypeInfo().IsInterface)
                throw new ArgumentException("An interface cannot be registered alone.");
            lock (this._syncLock)
            {
                if (this._factories.ContainsKey(type) && this._factories[type].ContainsKey(this._defaultKey))
                {
                    if (!this._constructorInfos.ContainsKey(type))
                        throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Class {0} is already registered.", new object[1]
                        {
              (object) type
                        }));
                }
                else
                {
                    if (!this._interfaceToClassMap.ContainsKey(type))
                        this._interfaceToClassMap.Add(type, (Type)null);
                    this._constructorInfos.Add(type, this.GetConstructorInfo(type));
                    Func<TClass> factory = new Func<TClass>(this.MakeInstance<TClass>);
                    this.DoRegister<TClass>(type, factory, this._defaultKey);
                    if (!createInstanceImmediately)
                        return;
                    this.GetInstance<TClass>();
                }
            }
        }

        /// <summary>Registers a given instance for a given type.</summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        public void Register<TClass>(Func<TClass> factory) where TClass : class => this.Register<TClass>(factory, false);

        /// <summary>
        /// Registers a given instance for a given type with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        public void Register<TClass>(Func<TClass> factory, bool createInstanceImmediately) where TClass : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            lock (this._syncLock)
            {
                Type type = typeof(TClass);
                if (this._factories.ContainsKey(type) && this._factories[type].ContainsKey(this._defaultKey))
                    throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "There is already a factory registered for {0}.", new object[1]
                    {
            (object) type.FullName
                    }));
                if (!this._interfaceToClassMap.ContainsKey(type))
                    this._interfaceToClassMap.Add(type, (Type)null);
                this.DoRegister<TClass>(type, factory, this._defaultKey);
                if (!createInstanceImmediately)
                    return;
                this.GetInstance<TClass>();
            }
        }

        /// <summary>
        /// Registers a given instance for a given type and a given key.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="key">The key for which the given instance is registered.</param>
        public void Register<TClass>(Func<TClass> factory, string key) where TClass : class => this.Register<TClass>(factory, key, false);

        /// <summary>
        /// Registers a given instance for a given type and a given key with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="key">The key for which the given instance is registered.</param>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        public void Register<TClass>(Func<TClass> factory, string key, bool createInstanceImmediately) where TClass : class
        {
            lock (this._syncLock)
            {
                Type type = typeof(TClass);
                if (this._factories.ContainsKey(type) && this._factories[type].ContainsKey(key))
                    throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "There is already a factory registered for {0} with key {1}.", new object[2]
                    {
            (object) type.FullName,
            (object) key
                    }));
                if (!this._interfaceToClassMap.ContainsKey(type))
                    this._interfaceToClassMap.Add(type, (Type)null);
                this.DoRegister<TClass>(type, factory, key);
                if (!createInstanceImmediately)
                    return;
                this.GetInstance<TClass>(key);
            }
        }

        /// <summary>
        /// Resets the instance in its original states. This deletes all the
        /// registrations.
        /// </summary>
        public void Reset()
        {
            this._interfaceToClassMap.Clear();
            this._instancesRegistry.Clear();
            this._constructorInfos.Clear();
            this._factories.Clear();
        }

        /// <summary>
        /// Unregisters a class from the cache and removes all the previously
        /// created instances.
        /// </summary>
        /// <typeparam name="TClass">The class that must be removed.</typeparam>
        public void Unregister<TClass>() where TClass : class
        {
            lock (this._syncLock)
            {
                Type key1 = typeof(TClass);
                Type key2;
                if (this._interfaceToClassMap.ContainsKey(key1))
                {
                    Type type = this._interfaceToClassMap[key1];
                    if ((object)type == null)
                        type = key1;
                    key2 = type;
                }
                else
                    key2 = key1;
                if (this._instancesRegistry.ContainsKey(key1))
                    this._instancesRegistry.Remove(key1);
                if (this._interfaceToClassMap.ContainsKey(key1))
                    this._interfaceToClassMap.Remove(key1);
                if (this._factories.ContainsKey(key1))
                    this._factories.Remove(key1);
                if (!this._constructorInfos.ContainsKey(key2))
                    return;
                this._constructorInfos.Remove(key2);
            }
        }

        /// <summary>
        /// Removes the given instance from the cache. The class itself remains
        /// registered and can be used to create other instances.
        /// </summary>
        /// <typeparam name="TClass">The type of the instance to be removed.</typeparam>
        /// <param name="instance">The instance that must be removed.</param>
        public void Unregister<TClass>(TClass instance) where TClass : class
        {
            lock (this._syncLock)
            {
                Type key1 = typeof(TClass);
                if (!this._instancesRegistry.ContainsKey(key1))
                    return;
                Dictionary<string, object> source = this._instancesRegistry[key1];
                List<KeyValuePair<string, object>> list = source.Where<KeyValuePair<string, object>>((Func<KeyValuePair<string, object>, bool>)(pair => pair.Value == (object)(TClass)instance)).ToList<KeyValuePair<string, object>>();
                for (int index = 0; index < list.Count<KeyValuePair<string, object>>(); ++index)
                {
                    string key2 = list[index].Key;
                    source.Remove(key2);
                }
            }
        }

        /// <summary>
        /// Removes the instance corresponding to the given key from the cache. The class itself remains
        /// registered and can be used to create other instances.
        /// </summary>
        /// <typeparam name="TClass">The type of the instance to be removed.</typeparam>
        /// <param name="key">The key corresponding to the instance that must be removed.</param>
        public void Unregister<TClass>(string key) where TClass : class
        {
            lock (this._syncLock)
            {
                Type key1 = typeof(TClass);
                if (this._instancesRegistry.ContainsKey(key1))
                {
                    Dictionary<string, object> source = this._instancesRegistry[key1];
                    List<KeyValuePair<string, object>> list = source.Where<KeyValuePair<string, object>>((Func<KeyValuePair<string, object>, bool>)(pair => pair.Key == key)).ToList<KeyValuePair<string, object>>();
                    for (int index = 0; index < list.Count<KeyValuePair<string, object>>(); ++index)
                        source.Remove(list[index].Key);
                }
                if (!this._factories.ContainsKey(key1) || !this._factories[key1].ContainsKey(key))
                    return;
                this._factories[key1].Remove(key);
            }
        }

        private object DoGetService(Type serviceType, string key, bool cache = true)
        {
            lock (this._syncLock)
            {
                if (string.IsNullOrEmpty(key))
                    key = this._defaultKey;
                Dictionary<string, object> dictionary = (Dictionary<string, object>)null;
                if (!this._instancesRegistry.ContainsKey(serviceType))
                {
                    if (!this._interfaceToClassMap.ContainsKey(serviceType))
                        throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Type not found in cache: {0}.", new object[1]
                        {
              (object) serviceType.FullName
                        }));
                    if (cache)
                    {
                        dictionary = new Dictionary<string, object>();
                        this._instancesRegistry.Add(serviceType, dictionary);
                    }
                }
                else
                    dictionary = this._instancesRegistry[serviceType];
                if (dictionary != null && dictionary.ContainsKey(key))
                    return dictionary[key];
                object obj = (object)null;
                if (this._factories.ContainsKey(serviceType))
                {
                    if (this._factories[serviceType].ContainsKey(key))
                        obj = this._factories[serviceType][key].DynamicInvoke((object[])null);
                    else if (this._factories[serviceType].ContainsKey(this._defaultKey))
                        obj = this._factories[serviceType][this._defaultKey].DynamicInvoke((object[])null);
                    else
                        throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Type not found in cache without a key: {0}", new object[1]
                        {
              (object) serviceType.FullName
                        }));
                }
                if (cache && dictionary != null)
                    dictionary.Add(key, obj);
                return obj;
            }
        }

        private void DoRegister<TClass>(Type classType, Func<TClass> factory, string key)
        {
            if (this._factories.ContainsKey(classType))
            {
                if (this._factories[classType].ContainsKey(key))
                    return;
                this._factories[classType].Add(key, (Delegate)factory);
            }
            else
            {
                Dictionary<string, Delegate> dictionary = new Dictionary<string, Delegate>()
        {
          {
            key,
            (Delegate) factory
          }
        };
                this._factories.Add(classType, dictionary);
            }
        }

        private ConstructorInfo GetConstructorInfo(Type serviceType)
        {
            Type type1;
            if (this._interfaceToClassMap.ContainsKey(serviceType))
            {
                Type type2 = this._interfaceToClassMap[serviceType];
                if ((object)type2 == null)
                    type2 = serviceType;
                type1 = type2;
            }
            else
                type1 = serviceType;
            ConstructorInfo[] array = type1.GetTypeInfo().DeclaredConstructors.Where<ConstructorInfo>((Func<ConstructorInfo, bool>)(c => c.IsPublic)).ToArray<ConstructorInfo>();
            if (array.Length > 1)
            {
                if (array.Length > 2)
                    return ServiceLocator.GetPreferredConstructorInfo((IEnumerable<ConstructorInfo>)array, type1);
                if ((object)((IEnumerable<ConstructorInfo>)array).FirstOrDefault<ConstructorInfo>((Func<ConstructorInfo, bool>)(i => i.Name == ".cctor")) == null)
                    return ServiceLocator.GetPreferredConstructorInfo((IEnumerable<ConstructorInfo>)array, type1);
                ConstructorInfo constructorInfo = ((IEnumerable<ConstructorInfo>)array).FirstOrDefault<ConstructorInfo>((Func<ConstructorInfo, bool>)(i => i.Name != ".cctor"));
                return (object)constructorInfo != null && constructorInfo.IsPublic ? constructorInfo : throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Cannot register: No public constructor found in {0}.", new object[1]
                {
          (object) type1.Name
                }));
            }
            if (array.Length == 0 || array.Length == 1 && !array[0].IsPublic)
                throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Cannot register: No public constructor found in {0}.", new object[1]
                {
          (object) type1.Name
                }));
            return array[0];
        }

        private static ConstructorInfo GetPreferredConstructorInfo(
          IEnumerable<ConstructorInfo> constructorInfos,
          Type resolveTo)
        {
            ConstructorInfo constructorInfo = constructorInfos.Select(t => new
            {
                t = t,
                attribute = t.GetCustomAttribute(typeof(PreferredConstructorAttribute))
            }).Where(_param1 => _param1.attribute != null).Select(_param1 => _param1.t).FirstOrDefault<ConstructorInfo>();
            return (object)constructorInfo != null ? constructorInfo : throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Cannot register: Multiple constructors found in {0} but none marked with PreferredConstructor.", new object[1]
            {
        (object) resolveTo.Name
            }));
        }

        private TClass MakeInstance<TClass>()
        {
            Type type = typeof(TClass);
            ConstructorInfo constructorInfo = this._constructorInfos.ContainsKey(type) ? this._constructorInfos[type] : this.GetConstructorInfo(type);
            ParameterInfo[] parameters1 = constructorInfo.GetParameters();
            if (parameters1.Length == 0)
                return (TClass)constructorInfo.Invoke(this._emptyArguments);
            object[] parameters2 = new object[parameters1.Length];
            foreach (ParameterInfo parameterInfo in parameters1)
                parameters2[parameterInfo.Position] = this.GetService(parameterInfo.ParameterType);
            return (TClass)constructorInfo.Invoke(parameters2);
        }

        /// <summary>
        /// Provides a way to get all the created instances of a given type available in the
        /// cache. Registering a class or a factory does not automatically
        /// create the corresponding instance! To create an instance, either register
        /// the class or the factory with createInstanceImmediately set to true,
        /// or call the GetInstance method before calling GetAllCreatedInstances.
        /// Alternatively, use the GetAllInstances method, which auto-creates default
        /// instances for all registered classes.
        /// </summary>
        /// <param name="serviceType">The class of which all instances
        /// must be returned.</param>
        /// <returns>All the already created instances of the given type.</returns>
        public IEnumerable<object> GetAllCreatedInstances(Type serviceType) => this._instancesRegistry.ContainsKey(serviceType) ? (IEnumerable<object>)this._instancesRegistry[serviceType].Values : (IEnumerable<object>)new List<object>();

        /// <summary>
        /// Provides a way to get all the created instances of a given type available in the
        /// cache. Registering a class or a factory does not automatically
        /// create the corresponding instance! To create an instance, either register
        /// the class or the factory with createInstanceImmediately set to true,
        /// or call the GetInstance method before calling GetAllCreatedInstances.
        /// Alternatively, use the GetAllInstances method, which auto-creates default
        /// instances for all registered classes.
        /// </summary>
        /// <typeparam name="TService">The class of which all instances
        /// must be returned.</typeparam>
        /// <returns>All the already created instances of the given type.</returns>
        public IEnumerable<TService> GetAllCreatedInstances<TService>() => this.GetAllCreatedInstances(typeof(TService)).Select<object, TService>((Func<object, TService>)(instance => (TService)instance));

        /// <summary>Gets the service object of the specified type.</summary>
        /// <exception cref="T:System.InvalidOperationException">If the type serviceType has not
        /// been registered before calling this method.</exception>
        /// <returns>
        /// A service object of type <paramref name="serviceType" />.
        /// </returns>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        public object GetService(Type serviceType) => this.DoGetService(serviceType, this._defaultKey);

        /// <summary>
        /// Provides a way to get all the created instances of a given type available in the
        /// cache. Calling this method auto-creates default
        /// instances for all registered classes.
        /// </summary>
        /// <param name="serviceType">The class of which all instances
        /// must be returned.</param>
        /// <returns>All the instances of the given type.</returns>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            lock (this._factories)
            {
                if (this._factories.ContainsKey(serviceType))
                {
                    foreach (KeyValuePair<string, Delegate> keyValuePair in this._factories[serviceType])
                        this.GetInstance(serviceType, keyValuePair.Key);
                }
            }
            return this._instancesRegistry.ContainsKey(serviceType) ? (IEnumerable<object>)this._instancesRegistry[serviceType].Values : (IEnumerable<object>)new List<object>();
        }

        /// <summary>
        /// Provides a way to get all the created instances of a given type available in the
        /// cache. Calling this method auto-creates default
        /// instances for all registered classes.
        /// </summary>
        /// <typeparam name="TService">The class of which all instances
        /// must be returned.</typeparam>
        /// <returns>All the instances of the given type.</returns>
        public IEnumerable<TService> GetAllInstances<TService>() => this.GetAllInstances(typeof(TService)).Select<object, TService>((Func<object, TService>)(instance => (TService)instance));

        /// <summary>
        /// Provides a way to get an instance of a given type. If no instance had been instantiated
        /// before, a new instance will be created. If an instance had already
        /// been created, that same instance will be returned.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If the type serviceType has not
        /// been registered before calling this method.</exception>
        /// <param name="serviceType">The class of which an instance
        /// must be returned.</param>
        /// <returns>An instance of the given type.</returns>
        public object GetInstance(Type serviceType) => this.DoGetService(serviceType, this._defaultKey);

        /// <summary>
        /// Provides a way to get an instance of a given type. This method
        /// always returns a new instance and doesn't cache it in the IOC container.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If the type serviceType has not
        /// been registered before calling this method.</exception>
        /// <param name="serviceType">The class of which an instance
        /// must be returned.</param>
        /// <returns>An instance of the given type.</returns>
        public object GetInstanceWithoutCaching(Type serviceType) => this.DoGetService(serviceType, this._defaultKey, false);

        /// <summary>
        /// Provides a way to get an instance of a given type corresponding
        /// to a given key. If no instance had been instantiated with this
        /// key before, a new instance will be created. If an instance had already
        /// been created with the same key, that same instance will be returned.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If the type serviceType has not
        /// been registered before calling this method.</exception>
        /// <param name="serviceType">The class of which an instance must be returned.</param>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance corresponding to the given type and key.</returns>
        public object GetInstance(Type serviceType, string key) => this.DoGetService(serviceType, key);

        /// <summary>
        /// Provides a way to get an instance of a given type. This method
        /// always returns a new instance and doesn't cache it in the IOC container.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If the type serviceType has not
        /// been registered before calling this method.</exception>
        /// <param name="serviceType">The class of which an instance must be returned.</param>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance corresponding to the given type and key.</returns>
        public object GetInstanceWithoutCaching(Type serviceType, string key) => this.DoGetService(serviceType, key, false);

        /// <summary>
        /// Provides a way to get an instance of a given type. If no instance had been instantiated
        /// before, a new instance will be created. If an instance had already
        /// been created, that same instance will be returned.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If the type TService has not
        /// been registered before calling this method.</exception>
        /// <typeparam name="TService">The class of which an instance
        /// must be returned.</typeparam>
        /// <returns>An instance of the given type.</returns>
        public TService GetInstance<TService>() => (TService)this.DoGetService(typeof(TService), this._defaultKey);

        /// <summary>
        /// Provides a way to get an instance of a given type. This method
        /// always returns a new instance and doesn't cache it in the IOC container.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If the type TService has not
        /// been registered before calling this method.</exception>
        /// <typeparam name="TService">The class of which an instance
        /// must be returned.</typeparam>
        /// <returns>An instance of the given type.</returns>
        public TService GetInstanceWithoutCaching<TService>() => (TService)this.DoGetService(typeof(TService), this._defaultKey, false);

        /// <summary>
        /// Provides a way to get an instance of a given type corresponding
        /// to a given key. If no instance had been instantiated with this
        /// key before, a new instance will be created. If an instance had already
        /// been created with the same key, that same instance will be returned.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If the type TService has not
        /// been registered before calling this method.</exception>
        /// <typeparam name="TService">The class of which an instance must be returned.</typeparam>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance corresponding to the given type and key.</returns>
        public TService GetInstance<TService>(string key) => (TService)this.DoGetService(typeof(TService), key);

        /// <summary>
        /// Provides a way to get an instance of a given type. This method
        /// always returns a new instance and doesn't cache it in the IOC container.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If the type TService has not
        /// been registered before calling this method.</exception>
        /// <typeparam name="TService">The class of which an instance must be returned.</typeparam>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance corresponding to the given type and key.</returns>
        public TService GetInstanceWithoutCaching<TService>(string key) => (TService)this.DoGetService(typeof(TService), key, false);
    }
}
