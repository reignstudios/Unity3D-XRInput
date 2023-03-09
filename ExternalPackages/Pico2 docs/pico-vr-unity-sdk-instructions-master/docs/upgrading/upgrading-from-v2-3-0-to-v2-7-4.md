# Upgrading from v2.3.0 to v2.7.4

### Remove old files

Delete `Assets/Plugins/Android/Pico_PaymentSDK_Unity_V1.0.16.jar` if it appears in your project - this is replaced by `Pico_PaymentSDK_Unity_V1.0.19.jar`.

Delete `Assets/Plugins/Android/hummingbirdservicecontroller.jar` if it appears in your project - this is replaced by `hbcserviceclient.jar`.

Delete folders containing old SDK assets, to ensure that anything that isn't merged over dosn't remain to cause conflcits. During the upgrade new versions of these files will be imported into the asssets folder `PicoMobileSdk/`.

<p align="center">
  <img alt="Remove these folders when upgrading to ensure no old code or assets remain" width="500px" src="/docs/assets/PVR Folders.png">
</p>

### Install the new unitypackage

<a href="https://users.wearvr.com/developers/devices/pico-goblin/resources/vr-unity-package/versions/v2-7-4" target="_blank">Download v2.7.4 of the Pico VR Unity SDK</a> from WEARVR and import it into your Unity project

<p align="center">
  <img alt="Import the .unitypackage as custom package" width="500px" src="/docs/assets/ImportUnityPackageImage.png">
</p>

## API changes

### Add controller indexes

Update all references to the following methods by adding an extra first parameter value of `0` (see [Controller indexes](/docs/pico-goblin-and-neo-controllers.md#controller-indexes) for more information):

* `UPvr_GetKeyDown`
* `UPvr_GetKeyUp`
* `UPvr_GetKey`
* `UPvr_GetKeyLongPressed`
* `UPvr_IsTouching`
* `UPvr_GetControllerQUA`

For example:

```cs
using Pvr_UnitySDKAPI;

public class MyClass : MonoBehaviour {

    private void Update()
    {
        if (Controller.UPvr_GetKeyUp(Pvr_KeyCode.TOUCHPAD))
        {
            // Touchpad was just released
        }
    }
}
```

Becomes:

```cs
using Pvr_UnitySDKAPI;

public class MyClass : MonoBehaviour {

    private void Update()
    {
        if (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.TOUCHPAD))
        {
            // Touchpad was just released
        }
    }
}
```

### Update UPvr_GetSlipDirection references

Update all references to `UPvr_GetSlipDirection`:

* Rename the method call to `UPvr_GetSwipeDirection`
* Add a controller index parameter of `0`
* Remove the `Pvr_SlipDirection` parameter
* Branch on the response of the method, using the corresponding `SwipeDirection` (not the old `Pvr_SlipDirection`)

For example:

```cs
using Pvr_UnitySDKAPI;

public class MyClass : MonoBehaviour {

    private void Update()
    {
        if (Controller.UPvr_GetSlipDirection(Pvr_SlipDirection.SlideDown))
        {
            // Touchpad received a touch gesture downwards since the last frame
        }
    }
}
```

becomes:

```cs
using Pvr_UnitySDKAPI;

public class MyClass : MonoBehaviour {

    private void Update()
    {
        SwipeDirection swipeDirection = Controller.UPvr_GetSwipeDirection(0);

        if (swipeDirection == SwipeDirection.SwipeDown)
        {
            // Touchpad received swipe down gesture since the last frame
        }
    }
}
```


### Update UPvr_GetTouchPadPosition references

Update all references to `UPvr_GetTouchPadPosition`:

* Remove the `axis` parameter and replace it a controller index of `0`
* Change the return type to a `Vector2`

For example:

```cs
using Pvr_UnitySDKAPI;

public class MyClass : MonoBehaviour {

    private void Update()
    {
	    float yCoordinate = Controller.UPvr_GetTouchPadPosition(0);
	    float xCoordinate = Controller.UPvr_GetTouchPadPosition(1);
    }
}
```

becomes:

```cs
using Pvr_UnitySDKAPI;

public class MyClass : MonoBehaviour {

    private void Update()
    {
	    Vector2 touchPosition = Controller.UPvr_GetTouchPadPosition(0);

	    float yCoordinate = touchPosition.y;
	    float xCoordinate = touchPosition.x;
    }
}
```

## Use new prefabs

Delete the existing `Pvr_UnitySDK` prefab from your scene and drag in the new one from `Pvr_UnitySDK/Prefabs/Pvr_UnitySDK.prefab`.

[Set up your camera for UI and GameObject interactions](/docs/pico-vr-unity-sdk-instructions.md).

Then [set up the new `Pvr_Controller`](/docs/pico-goblin-and-neo-controllers.md#integrating-with-headset-and-controller-input).

## Updating payment API

If your app does not integrate the Pico's user or payment API, then you can skip this section.

### Update AndroidManifest.xml

After updating the Pico SDK the apps AndroidManifest.xml may be rebuilt. It is reccomended that the android permissions and the values 'pico_merchant_id', 'pico_app_id', 'pico_app_key' as well at [the other values that were changed when the Pico payment SDK was first integrated](/docs/pico-payment-sdk-user-management.md) be checked.

### Update user methods

The JSON object passed to the `LoginCallback` callback method [has changed](/docs/pico-payment-sdk-user-management.md#logging-the-user-in) to have two attributes: `"isSuccess"` and `"msg"`.

For example, this:

```cs
public class Callback : MonoBehaviour{
    public void LoginCallback(string result) {
        JsonData response = JsonMapper.ToObject(result);

        if (response["cancel"] != null ) {
            // User closed sign in UI
        } else if (response["exception"] != null ) {
            // Sign in error occurred
        } else {
            // Access token and openID must be saved in CommonDic for user details
            // request to work later
            CommonDic.getInstance().access_token = response["access_token"].toString();
            CommonDic.getInstance().open_id = response["open_id"].toString();
        }
    }
}
```

Becomes this:

```cs
public class Callback : MonoBehaviour{
    public void LoginCallback(string result) {
        JsonData response = JsonMapper.ToObject(result);

        if (response["isSuccess"] == "true" ) {
            PicoPaymentSDK.GetUserAPI();
        } else {
            if response["msg"] == "Network Exception,please check the network connection" {
                // Display dialog with message ("Connection error, please check your connection and try again."):
                // 网络链接错误，请检查网络稍后再试。
            } else {
                // Another error has occurred - typically handled by the operating system.
                // Display a dialog with message ("An unknown error has occurred. Please try again later."):
                // 未知错误，请稍后再试。
            }
        }
    }
}
```

### Update payment codes

Status codes for `QueryOrPayCallback(string result)` have been updated in v2.7.4 to be more concise and consistent. (You can [see here](/docs/pico-payment-sdk-in-app-purchases.md#app-purchases-error-handling) for full details of the new error codes.).

#### Removed status codes

If your app references any of the error codes, they have now been removed and your app will not need to handle them.

| Old code(s) | Comments |
| :---: | :--- |
| 11002 | |
| 12001 | |
| 13001 | |
| 13002 | |
| 15003 | This code has been re-purposed (see [Altered status codes](#altered-status-codes)). |

#### Altered status codes

If your code references any of the old codes, you need to substitute the new ones:

| Old code(s) | New code | Description |
| :---: | :---: | :--- |
| 10001 | 11001 | The request to check the current user’s available balance failed because the user was not signed in. |
| 10002 | 12002 | A purchase was attempted with a non-positive price. |
| 15000 | 12006 | `PicoPaymentSDK.Pay()` was called without a serialised JSON object, or one that is missing required parameters. |
| ORDER_EXIST | 12007 | A purchase with this `"order_id"` already exists. |
| PAY_CODE_EXIST | 12009 | The user has already purchased this non-consumable item. |
| PAY_CODE_NOT_EXIST | 12008 | The `pay_code` parameter did not match any pre-registered items. |
| 00000 | 14004 | A network error occurred. |
| SYSTEMERROR | 15001 | An system error occurred on the server. |
| APP_ID_NOT_EXIST, MCHID_NOT_EXIST, NOAUTH | 15003 | The purchase failed because `pico_merchant_id`, `pico_app_id` or `pico_pay_key` from the AndroidManifest file is invalid. (Note that this error code used to represent a different error - see [Removed status codes](#removed-status-codes).) |

#### New status codes

Your app should now reference these new codes:

| New code(s) | Comments |
| :---: | :--- |
| 14001 | Occurs in the event that one of the operations the SDK performs fails. Your app should re-attempt the purchase and then display an error message if it fails again, asking the user to retry later. You can use the following copy in your message to the user: 未知错误，请稍后再试 |
