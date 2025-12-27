using UnityEngine;
using Unity.Services.Economy;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientEconomy : MonoBehaviour
    {

        private static ClientEconomy _instance = null; public static ClientEconomy Instance { get { GetInstance(); return _instance; } }
        private bool _initialized = false; public bool IsInitialized { get { return _initialized; } }
        private bool _initializing = false; public bool IsInitializing { get { return _initializing; } }
        public delegate void CallbackDelegate(bool response);
        private Dictionary<string, long> _balances = new Dictionary<string, long>();
        public delegate void AddDelegate(string id, bool isAdded, long added, long balance);
        public delegate void SpendDelegate(string id, bool isSpent, long spent, long balance);

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private static void GetInstance()
        {
            if (_instance != null) { return; }
            _instance = FindFirstObjectByType<ClientEconomy>();
            if (_instance == null)
            {
                _instance = new GameObject("ClientEconomy").AddComponent<ClientEconomy>();
            }
            DontDestroyOnLoad(_instance.gameObject);
        }

        public async void Initialize(CallbackDelegate callback)
        {
            while (_initializing && !_initialized)
            {
                await Task.Delay(100);
            }
            if (!_initialized)
            {
                try
                {
                    _initializing = true;
                    var options = new GetBalancesOptions();
                    options.ItemsPerFetch = 50;
                    var result = await EconomyService.Instance.PlayerBalances.GetBalancesAsync(options);
                    if (result.HasNext)
                    {
                        // ToDo
                    }
                    _balances.Clear();
                    foreach (var balance in result.Balances)
                    {
                        _balances.Add(balance.CurrencyId, balance.Balance);
                    }
                    _initialized = true;
                }
                catch (EconomyException e)
                {
                    Debug.Log(e);
                }
                _initializing = false;
            }
            if (callback != null)
            {
                callback.Invoke(_initialized);
            }
        }

        public long GetCurrencyBalance(string id)
        {
            if(_balances.TryGetValue(id, out var balance))
            {
                return balance;
            }
            return 0;
        }

        public async void AddCurrency(string id, long amount, AddDelegate callback = null)
        {
            if (string.IsNullOrEmpty(id) || !_balances.ContainsKey(id))
            {
                if(callback != null)
                {
                    callback.Invoke(id, false, 0, 0);
                }
                return;
            }
            try
            {
                var options = new IncrementBalanceOptions();
                var result = await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(id, (int)amount, options);
                _balances[id] = result.Balance;
                if (callback != null)
                {
                    callback.Invoke(id, true, amount, _balances[id]);
                }
            }
            catch (EconomyException e)
            {
                Debug.Log(e);
                if (callback != null)
                {
                    callback.Invoke(id, false, 0, _balances[id]);
                }
            }
        }

        public async void SpendCurrency(string id, long amount, SpendDelegate callback = null)
        {
            if (string.IsNullOrEmpty(id) || !_balances.ContainsKey(id) || _balances[id] < amount)
            {
                if (callback != null)
                {
                    callback.Invoke(id, false, 0, 0);
                }
                return;
            }
            try
            {
                var options = new DecrementBalanceOptions();
                var result = await EconomyService.Instance.PlayerBalances.DecrementBalanceAsync(id, (int)amount, options);
                _balances[id] = result.Balance;
                if (callback != null)
                {
                    callback.Invoke(id, true, amount, _balances[id]);
                }
            }
            catch (EconomyException e)
            {
                Debug.Log(e);
                if (callback != null)
                {
                    callback.Invoke(id, false, 0, _balances[id]);
                }
            }
        }

        public void Uninitialize()
        {
            _initialized = false;
        }

    }
}