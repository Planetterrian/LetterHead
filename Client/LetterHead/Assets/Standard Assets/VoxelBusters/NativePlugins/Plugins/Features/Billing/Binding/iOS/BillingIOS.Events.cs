﻿using UnityEngine;
using System.Collections;

#if USES_BILLING && UNITY_IOS
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins
{	
	using Internal;

	public partial class BillingIOS : Billing
	{
		#region Constants

		private		const 	string		kProductsKey		= "products";
		private		const 	string		kTransactionsKey	= "transactions";
		private 	const 	string		kErrorKey			= "error";

		#endregion

		#region Callback Methods

		protected override void DidReceiveBillingProducts (string _dataStr)
		{
			IDictionary		_dataDict	= (IDictionary)JSONUtility.FromJSON(_dataStr);
			string			_error		= _dataDict.GetIfAvailable<string>(kErrorKey);
			
			if (_error != null)
			{
				DidReceiveBillingProducts(null, _error);
				return;
			}
			else
			{
				IList				_regProductsJSONList	= _dataDict.GetIfAvailable<IList>(kProductsKey);
				BillingProduct[]	_regProductsList		= null;

				if (_regProductsJSONList != null)
				{
					_regProductsList	= new iOSBillingProduct[_regProductsJSONList.Count];
					int		_iter		= 0;
					
					foreach (IDictionary _productInfoDict in _regProductsJSONList)
					{
						_regProductsList[_iter++]			= new iOSBillingProduct(_productInfoDict);
					}
				}
				
				DidReceiveBillingProducts(_regProductsList, null);
				return;
			}
		}
		
		protected override void ExtractTransactionResponseData (string _dataStr, out BillingTransaction[] _transactions, out string _error)
		{
			// Set default values
			_transactions	= null;
			_error			= null;

			// Parse and fetch properties from JSON object
			IDictionary		_dataDict	= (IDictionary)JSONUtility.FromJSON(_dataStr);
			_error						= _dataDict.GetIfAvailable<string>(kErrorKey);

			if (_error == null)
			{
				IList		_transactionsJSONList	= _dataDict.GetIfAvailable<IList>(kTransactionsKey);

				if (_transactionsJSONList != null)
				{
					int		_count		= _transactionsJSONList.Count;
					_transactions		= new iOSBillingTransaction[_count];

					for (int _iter = 0; _iter < _count; _iter++)
						_transactions[_iter]	= new iOSBillingTransaction((IDictionary)_transactionsJSONList[_iter]);
				}
			}
		}

		#endregion
	}
}
#endif