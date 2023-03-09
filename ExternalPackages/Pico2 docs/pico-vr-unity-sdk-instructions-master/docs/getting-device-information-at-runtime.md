# Getting device information at runtime

If you would like your app to get device information at runtime, you can use the following method:

```cs
Pvr_UnitySDKAPI.System.UPvr_GetDeviceModel()
```

It will return one of the following values:

| Device | `UPvr_GetDeviceModel()` |
| :---: | :---: |
| Pico Goblin | `"Pico Pico Goblin"` |
| Pico Neo | `"Pico Pico Neo"` |
