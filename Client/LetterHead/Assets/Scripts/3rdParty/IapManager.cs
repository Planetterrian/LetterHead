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
            if (_transaction.VerificationState == eBillingTransactionVerificationState.SUCCESS || _transaction.VerificationState == eBillingTransactionVerificationState.NOT_CHECKED)
            {
                if (_transaction.TransactionState == eBillingTransactionState.PURCHASED)
                {
                    // Your code to handle purchased products
                    AwardProduct(_transaction);
                    _transaction.OnCustomVerificationFinished(eBillingTransactionVerificationState.SUCCESS);
                }
                else
                {
                    //DialogWindowTM.Instance.Error(_transaction.Error);
                }
            }
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
        DialogWindowTM.Instance.Show("Purchase", "One moment, processing your request...", () => { }, () => { }, "");
        var product = NPBinding.Billing.GetStoreProduct(identifier);
        NPBinding.Billing.BuyProduct(product);
    }

}