# Pico payment SDK and in-app purchases

This is an optional step that is only required if your app has in-app purchases.

## Warning

**Currently the Payment API (required for in-app purchases) is not supported outside of China.**

> If you wish to release your app in China with in-app purchases, you will need to submit different release builds: one with in-app purchases to be released in China and one without, for the sale outside of China.

## Currency 

All purchases through the Pico ecosystem are done in Pico’s own virtual currency as an integer value of P-Coins.

Buyers in the Pico ecosystems add P-Coins to their account through Pico’s website, so it does not need to be handled by your app.

| P-Coins | Yuan | USD |
| :---: | :---: | :---: |
| 10 | ¥ 1 | $0.15 (approx - check the current exchange rate) |

## Displaying a confirmation dialog

The Pico Unity SDK and the Pico operating system do not provide a confirmation dialog to the user before deducting funds from the users account. It's up to you to build messaging into your app to ensure the user understands that they are about to make a purchase, and the cost of that purchase.

Please see [Pico localisation](/docs/pico-unity-localization.md) for guidance on how to display this information to the user.

## Creating in-app purchases

> Before your app can create any in-app purchases, you must first have [requested credentials for your app](https://users.wearvr.com/developers/devices/pico-goblin/store-listings/) and [signed the current user in](/docs/pico-payment-sdk-user-management.md).

To make an in-app purchase, you need to use the `PicoPaymentSDK.Pay()` method. You use it in one of two ways, depending on how you wish to register purchases.

You must choose only one of the two methods and use it for all of your in-app purchases. The Pico platform does not support using a combination of the two.

### Option 1: Creating purchases for pre-registered items

Use this option when you have pre-registered a list of items that users can purchase from within your app, that have fixed prices.

```cs
void PicoPaymentSDK.Pay(string purchaseDetails);
```

Where `purchaseDetails` is a string containing a serialized JSON object with the following attributes:

| Attribute | Type | Description |
| :---: | :--- | :--- |
| `"subject"` | String | A short description of the purchase |
| `"body"` | String | A longer description of the purchase |
| `"order_id"` | String | A unique identifier for the purchase (no two purchase transactions within the same game can have the same order_id). It must be less than 64 characters - see [Generating an order id](#generating-an-order-id). |
| `"pay_code"` | String | Product code - this must match the one you have provided when registering your purchase item. |
| `"goods_tag"` | String | An optional tag to attach to the purchase. |

> It's important the purchaseDetails *not* have a "total" attribute at all (with any value), if you use this option.

#### Generating an order id

To allow querying whether a user has already purchased a particular non-consumable item, you will need to make the `order_id` depend on the user's `openid` and the item's `pay_code`. You can use a function like the following to generate a MD5 hash 32 characters long:

```cs
public string PurchaseId(string userOpenId, string itemPayCode)

{
    string purchaseString = userOpenId + '-' + itemPayCode;
    byte[] purchaseStringBytes = System.Text.Encoding.ASCII.GetBytes(purchaseString);

    MD5 md5 = System.Security.Cryptography.MD5.Create();
    byte[] purchaseHash = md5.ComputeHash(purchaseStringBytes);

    StringBuilder stringBuilder = new StringBuilder();

    for (int i = 0; i < purchaseHash.Length; i++)

    {

        stringBuilder.Append(purchaseHash[i].ToString(“X2”));

    }

    return stringBuilder.ToString();
}
```

#### Dynamic or updating item pricing

Although Pico allows developers to update the price of thier items after launch, the Pico payment API does not currently provide a way for apps to query the current price of items. This unfortunately means if you plan to alter prices after release, you will need to record the current value of each item on your own game server and query them that way in your app.

### Option 2: Creating purchases with arbitrary amounts

Use this option when you have in-app purchases that have not been pre-registered and do not have fixed prices.

```cs
void PicoPaymentSDK.Pay(string purchaseDetails);
```

Where `purchaseDetails` is a string containing a serialized JSON object with the following attributes:

| Attribute | Type | Description |
| :---: | :--- | :--- |
| `"subject"` | String | A short description of the purchase |
| `"body"` | String | A longer description of the purchase |
| `"order_id"` | String | A unique identifier for the purchase (no two purchase transactions within the same game can have the same order_id). It's up to you how you generate this, but it must be less than 64 characters. |
| `"total"` | Integer | The total price of the purchase in P-coins. |
| `"goods_tag"` | String | An optional tag to attach to the purchase. |

> It's important that the purchaseDetails not have a "pay_code" attribute at all (with any value), if you use this option.

## Processing the purchase response

To receive the purchase response, define a `QueryOrPayCallback()` method on the PicoPayment GameObject script you added earlier.

```cs
void QueryOrPayCallback(string result);
```

Where `result` is a string containing a serialized JSON object representing the result of the purchase request, with the following attributes:

| Attribute | Type | Description |
| :---: | :--- | :--- |
| `"code"` | String | A code indicating the status of the request. |
| `"msg"` | String | A description of the response status. Use"code" to check the status of the request rather than this attribute. |

> The response does not reference what was purchased, so your app will need to record this information before making the request to the Pico servers. When the purchase response arrives, if the purchase was successful, your app can then determine which item to unlock.

#### Successful purchase response

| Response (after being parsed as JSON) | Description |
| :---: | :--- |
| `"code": "12000", "msg": "PAY_SUCCESS"` | Payment succeeded. Your app can now give access to the purchased item. |

#### App purchases error handling

The following responses should be handled by your app:

| Response (after being parsed as JSON) | Description | Suggested app behaviour |
| :---: | :--- | :--- |
| `"code": "11001", "msg": "USER_NOT_LOGIN_OR_EXPIRED"` | The request to check the current user’s available balance failed because the user was not signed in. | Call `PicoPaymentSDK.Login()` to log the user in. |
| `"code": "12007", "msg": "PAY_ORDER_EXIST"` | A purchase with this `"order_id"` already exists. | Retry the purchase with a different purchase order ID. |
| `"code": "12009", "msg": "PAY_CODE_ALERADY_CONSUMED"` | The user has already purchased this non-consumable item. | Give the user access to the purchased content as if they had just completed the purchase. |
| `"code": "12003", "msg": "PCOIN_NOT_ENOUGH"` | The user did not have enough P coins. | Display a dialog with 你的 Pico 账号里没有足够的P币来完成这次购买。你可以在 Pico 网站上充值。(“Your Pico account does not have enough P-coins to make this purchase. You can top up your balance on the Pico website.”) |
| `"code": "15001", "msg": "SYSTEM_ERROR"` | An system error occurred on the server. | Automatically re-attempt the purchase. If it fails again, display a dialog with message: 未知错误，请稍后再试。("An unknown error has occurred. Please try again later.") |
| `"code": "14001", "msg": "SDK_LOCAL_ERROR"` | A local error in the Pico SDK has occurred. | Same as above. |
| `"code": "14004", "msg": "NETWORK_ERROR"` | A network error occurred. | Re-attempt the purchase. If it also fails, display a dialog saying 网络链接错误，请检查网络稍后再试。("Connection error, please check your connection and try again."). |

#### Troubleshooting in-app purchases

The following error conditions should not normally occur. If they do, they may indicate that the SDK has not been correctly integrated into your project.

| Response (after being parsed as JSON) | Description | Suggested app behaviour |
| :---: | :--- | :--- |
| `"code": "11004", "msg": "MISSING_APP_PARAMETERS"` | The purchase failed because `pico_merchant_id`, `pico_app_id` or `pico_pay_key` is missing from the AndroidManifest file. | Check your AndroidManifest. |
| `"code": "15003", "msg": "SERVICE_APP_PARAMETER_NOT_MATCH"` | The purchase failed because `pico_merchant_id`, `pico_app_id` or `pico_pay_key` from the AndroidManifest file is invalid. | Check your AndroidManifest. |
| `"code": "12002", "msg": "ENTER_AMOUNT_ERROR"` | A purchase was attempted with a non-positive price. | Check the value of the `total` attribute you are using to create the purchase. |
| `"code": "12008", "msg": "PAY_CODE_NOT_EXIST"` | The `pay_code` parameter did not match any pre-registered items. | Check that you are using the correct value of this parameter and you have already registered the item.<br/>If you are not purchasing a pre-registered item, check that you are not submitting any value for pay_code. |
| `"code": "12006", "msg": "NOT_ENTER_ORDER_INFO"` | `PicoPaymentSDK.Pay()` was called without a serialised JSON object, or one that is missing required parameters. | Check the serialised JSON object you are passing to `PicoPaymentSDK.Pay()`. |

## Determining whether a user has already purchased an (non-consumable) item

Once the Pico servers has confirmed the purchase was successful, it’s preferreable for your application to record this information on your own game server. It's ok (and encouraged) to cache this information on the device - but on-devie storage *should not* be the only method of recording whether a user has purchased a particular item before - if the user switches Pico devices, the user's purchase must still be honoured.

If you this option is not available to you (you do not have a game server or are concerned about its availability world-wide), you can query the Pico servers to establish whether a particular item has been purchased providing the following conditions:

* The item is non-consumable (the user cannot purchase it more than once and therefore is unique)
* You are using a unique, `order_id` that depends on the current user's `openid` and the item's `pay_code` (see [Generating an order id](#generating-an-order-id)).

You do this by querying whether a particular `order_id` already exists using:

```cs
void QueryOrder(string orderId)
```

This method accepts an orderId as its only parameter and calls the [QueryOrPayCallback()](#processing-the-purchase-response) function when the Pico server responds. If the purchase already exists on the server, you will get the details of that purchase, otherwise the response will be as follows:

```
{"code ":"13006","msg":"Order does not exist"}
```

## Next: Testing your in-app purchases

See [Testing in-app purchases](/docs/testing-in-app-purchases.md).
