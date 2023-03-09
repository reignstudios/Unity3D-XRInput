# Using Pico transmission layers

## Transmission layers explained

Transmission layers, sometimes described as Compositor layers or a Synthesis layer, is a different way of rendering to VR headsets.

Normal rendering will have an applications content rendered onto an 'eye buffer', and then the 'eye buffer' will be sampled by ATW (Asynchronous TimeWarp) to find out what content needs to be re-rendered onto a screen, and then finally it will actually be rendered onto a VR screen.

Using transmission layers a different method can be used. Content rendered in this way does not need to use the 'eye buffer' at all, and instead renders directly to ATW for sampling. As a step is skipped both the speed of rendering as well as the overall sharpness of the result can be improved. The downside is that certain steps must be taken by the developer to enable this behavior.

## Transmission layer supported textures

The transmission layer supports two transparent texture types:
1.  Standard Texture : standard 2D texture  
2.  Equirectangular Texture: panorama texture (360°) 

## Enabling transmission eye overlays

Eye overlays are added using the **'Pvr_UnitySDKEyeOverlay.cs'** script. This script should be attached to the users **'LeftEye'** and **'RightEye'** objects inside the 'Pvr_UnitySDK' prefab. 

<p align="center">
  <img alt="Splash Screen Menu" width="500px" src="/docs/assets/EyeOverlay.png">
</p>

The Pvr_UnitySDKEyeOverlay script has some configurable settings:
1.  Eye Side : Left or Right. Should be configured for whichever eye it has been placed on.
2.  Image Type: Specify standard or equirectangular texture.
3.  Image Texture: The actual texture that will be shown.
4.  Image Transform: Optional. For standard textures specify the transform (position, rotation, zoom) of the texture.

### Standard 2D textures

There is an example scene showing the concept of 2D transmission textures included with the Pico SDK under: **Assets -> PicoMobileSDK -> Pvr_UnitySDK -> Scene -> Examples -> 2DOverlay.unity** 

To use 2d Transmission textures fist set up the scene so that it is compatible for Pico, including the use of a Pvr_UnitySDK prefab. Then ensure that the **Pvr_UnitySDKEyeOverlay.cs** scripts have been attached correctly to each of the Pico players eye game objects, and set up correctly including assigning a valid texture to the 'Image Texture' value.

Then create a Quad object in the scene that you wish to be used to render a transparent 2d texture via transmission. For the object uncheck and deactivate the 'Mesh Renderer' component, and then set the 'Image Transform' value in Pvr_UnitySDKEyeOverlay to be the mesh.

The mesh itself does not need a texture assigned to it, and instead will use the texture specified in Pvr_UnitySDKEyeOverlay.

<p align="center">
  <img alt="2D" width="500px" src="/docs/assets/2DOverlay.png">
</p>

### Equirectangular Textures

There is an example scene showing the concept of 3D transmission textures included with the Pico SDK under: **Assets -> PicoMobileSDK -> Pvr_UnitySDK -> Scene -> Examples -> 360Overlay.unity** 

To set up a 360 degree vision texture many of the steps are similar to creating a 2D texture. Set the components 'Image Type' value to Equirectangular, and assign a panoramic texture to 'Image Texture'. Any panoramic texture will work, but an example one has been provided with the Pico SDK under: **Assets -> PicoMobileSDK -> Pvr_UnitySDK -> Resources -> 360.jpg** 

<p align="center">
  <img alt="360" width="500px" src="/docs/assets/360Overlay.png">
</p>
