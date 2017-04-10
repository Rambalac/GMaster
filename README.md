# GMaster
Application can remotly control Panasonic Lumix Cameras from Windows 10 devices (PC, Mobile, ... Xbox, Hololens!?)

Application can do no more than original app from Panasonic for Android/iOS. 
While for now app does not support all remote features but Supports features work in a better and faster way. 
And if something is wrong or not enough I can fix it much much faster than Panasonic in their app.

App is free and has no ad. You can install it from [MS Store](https://www.microsoft.com/store/apps/9NC2W8KC526F) or from [Project Release page](https://github.com/Rambalac/GMaster/releases/latest)

## Features
* Fullscreen camera liveview. Split screen and new windows for multiple cameras.
* LUT preview. Per camera setting.
* Photo capture and video recording start/stop.
* Change basic parameters like ISO, Aperture, and Shutter. Changes on camera get reflected on the screen in real time.
* Move/resize Autofocus point by mouse or gesture. Real AF area is displayed in real time for Point and Following AF.
* Power Zoom lens zoom and change focus in Manual Focus mode.
* Anamorphic desqueezing (1.33x, 1.5x, 1.75x, 2x). Per camera setting with option for Anamorphinc Video mode only.
* WiFi manager to autoconnect to camera WiFi access point.
* Multiple cameras can be controlled. You can connect cameras to common access point.

![Screenshot](/images/screenshots/PC-4.jpg)

## Cameras
### Fully supported
* GH4
* GH3

### Other
App may work with other Panasonic Lumix cameras, but I have no way to test it.

* GX85 - As some people reported App cannot connect. Hopefull v 1.6.0 fixes that.

## News
※ It can take from 12 hours to several days from pushing to MS Store til update appears for all users .
#### 2017-04-10
Pushed version 1.7.0 to MS Store

* More fixes and workarounds for other cameras connectivity.
* Added split screen modes and new window.
* Added WiFi management to remember and autoconnect to access points.
* Minor fixes.
#### 2017-04-07
Pushed version 1.6.0 to MS Store

* Hopefully fixed issue with modern camera models like GX85. Thanks to Lufthummel from DPreview
* Added Cube LUT support. Don't have grading experience so could be wrong.
* Added Anamorphic desqueezing support (1.33x, 1.5x, 1.75x, 2x).
* Added hardware acceleration as for LUT as for general performance improvement.
#### 2017-04-04
Pushed version 1.5.0 to MS Store

* Added Power Zoom
* Added Manual Focus
* Fixed Focus point rendering for different aspects
* Fixes for reconnect problems in specific networks
* Added better Enum conversion from int
* Moved Tools to separate project
* Added test for IntToEnum
* Fixed focus point pinch resize
* Fixed initial ISO, Sh and Ap selection

