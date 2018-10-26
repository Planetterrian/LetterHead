using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VoxelBusters.NativePlugins;

public class IapManager : Singleton<IapManager>
{
    public UnityEvent OnItemsUpdated;

    void Start()
    {
        NPBinding.Billing.RequestForBillingProducts(NPSettings.Billing.Products);
    }

    private void OnEnable()
    {
        Billing.DidFinishProductPurchaseEvent += OnDidFinishTransaction;
        Billing.DidFinishRestoringPurchasesEvent += OnDidFinishRestoringPurchases;
        Billing.DidFinishRequestForBillingProductsEvent += OnDidFinishRequestForBillingProducts;
    }

    private void OnDidFinishRequestForBillingProducts(BillingProduct[] _products, string _error)
    {
        if (!string.IsNullOrEmpty(_error))
        {
            Debug.LogError(_error);
        }
        else
        {
            Debug.Log(_products.Length + " IAPs Loaded");
        }
    }

    private void OnDisable()
    {
        Billing.DidFinishProductPurchaseEvent -= OnDidFinishTransaction;
        Billing.DidFinishRestoringPurchasesEvent -= OnDidFinishRestoringPurchases;
    }

    private void OnDidFinishRestoringPurchases(BillingTransaction[] _transactions, string _error)
    {
        if (_transactions.Length > 0)
        {
            OnPurchaseMade();
        }

        DialogWindowTM.Instance.Show("Restore Purchases", "Purchases have been restored.", () => { });
    }

    private void OnPurchaseMade()
    {
        PlayerPrefs.SetInt("PurchaseMade", 1);
        AdManager.Instance.DisableAds();

        if (OnItemsUpdated != null)
            OnItemsUpdated.Invoke();
    }

    private void OnDidFinishTransaction(BillingTransaction _transaction)
    {
        if (_transaction != null)
        {
            if (_transaction.TransactionState == eBillingTransactionState.PURCHASED)
            {
                if (_transaction.VerificationState == eBillingTransactionVerificationState.NOT_CHECKED)
                {
                    _transaction.OnCustomVerificationFinished(eBillingTransactionVerificationState.SUCCESS);
                }
                else if (_transaction.VerificationState == eBillingTransactionVerificationState.SUCCESS)
                {
                    AwardProduct(_transaction);
                }
            }
            else
            {
                DialogWindowTM.Instance.Hide();
            }
        }
        else
        {
            DialogWindowTM.Instance.Hide();
        }
    }

    private void AwardProduct(BillingTransaction transaction)
    {
        OnPurchaseMade();

        DialogWindowTM.Instance.Show("Awarding", "Please wait while we award you your items.", () => { }, () => { }, "");

        Srv.Instance.POST("User/IapPurchase", new Dictionary<string, string>() {{"productId", transaction.ProductIdentifier}, {"receipt", transaction.TransactionReceipt}}, s =>
        {
            OnPurchaseMade();
            DialogWindowTM.Instance.Hide();
            //DialogWindowTM.Instance.Show("Purchase Completed", "Thank you for your purchase!", () => { });
        }, DialogWindowTM.Instance.Error);
    }


    public void RequestPurchase(string identifier)
    {
        identifier = identifier.Trim();

        if (!NPBinding.Billing.IsAvailable())
        {
            DialogWindowTM.Instance.Error("Unable to access the store at this time");
            return;
        }

        var product = NPBinding.Billing.GetStoreProduct(identifier);
        if (product == null)
        {
            DialogWindowTM.Instance.Error("Unable to access the store at this time.");
            return;
        }

        DialogWindowTM.Instance.Show("Purchase", "One moment, processing your request...", () => { }, () => { }, "");


        Debug.Log("Trying to purchase: " + product.Name);
        NPBinding.Billing.BuyProduct(product);
    }

}