## DEMO


https://github.com/AXVIII3/unity-app-launcher/assets/76608488/2f47e7b8-bae1-44ae-a363-b53057507a41


This video shows the loading of remote config variables and the auto-loading of background and logo images. This also shows the automatic setting up, downloading and checking of game files.

#### Tested
2021.3.20f1 version tested on Windows, Mac and Linux<br>
2022.3.16f1 version tested only on Windows

#### What is this?
This is a simple launcher (installer and updater) made in Unity based on [Tom Weiland](https://www.youtube.com/@tomweiland)'s infamous [launcher tutorial](https://www.youtube.com/watch?v=JIjZQo03YdA) for which was originally a .NET WPF application and I converted into a Unity application.

#### What is the purpose?
I originally made this in March of 2023 in Unity 2021.3.20f1 as I was about to send a game I was working on to a few people for testing. The game was in the works and I needed a way for people to be able to play new versions of the game without needing to go to some online host and download the latest files. I didn't really use this anymore but felt like someone might. So I ported it to 2022.3.16f1 and currently am uploading this to GitHub after refactoring some stuff. I would actually use and recommend something like [Patch](https://patch.mhlab.tech/) for a production launcher as obviously.. it supports patching. However this launcher is fine for small applications which doesn't really require patching or high-end functionality of professional patchers.

#### Why use this?
As discussed previously, this can be useful for auto-updating application files so that users always stay in the latest version of the application. I also used a Unity Cloud service called Remote Config so that you can remotely control various aspects of the launcher! Another big thing is that this is cross-platform. This was my biggest motivation for converting Tom Weiland's WPF launcher to a Unity app, so that I can distribute on multiple platforms Unity natively supports. I probably should have used something like [Electron](https://www.electronjs.org/) or Flutter to make the launcher but[](https://flutter.dev/)I already knew Unity and wasn't planning on keeping this launcher long term... so I didn't bother.

#### Pros:
* Free and Open Source!
* Easy to edit and develop as Unity provides a great UI framework
* Cross-Platform
* Remote Controllable
* Can be used with any kind of application which builds to a native executable file (e.g. .exe for windows,  .86_64 for linux or .app for Mac) though supporting new executables shouldn't be very
  
#### Cons:
* Depends on Unity and Unity Products (Do I really need to elaborate?)
* Its technically a game, made in a game engine and hence has many files bloating the final export while the original .NET WPF app was a single executable
* Similar to above, using a game engine for a launcher is overkill and it uses more resources than a launcher should (though not very noticeable)
* The code in one file... y-y-y-e-e-e-ah... my bad :)
* Deletes and downloads the complete directory the game resides in.. Wouldn't want to store any persistent data there
* Users can directly use the downloaded application and avoid the launcher to stay on a older version (though there are many easy fixes)

#### Some Tips:
1. If you have any issues with the de-compression make sure you zipped your files using the default Windows method, not with a third party application such as WinRAR, WinZip, 7-zip, etc.
2. For testing/development host the files on your computer and just use the file path as the download link. Obviously this won't work if the launcher isn't running on your computer, but it'll do the trick for testing/development purposes. You'll have to find a proper file hosting solution at some point anyways, as Google Drive, OneDrive, Dropbox, etc. all have limits on how frequently a file can be downloaded, so all of those are really only usable during development.
3. Try not to keep any of the Remote Config links empty or broken... make sure there are no trailing white spaces too... I probably should have accounted for this in code and I might... but I haven't yet

#### Documentation:
Please read everything if you are doing this for the first time.<br>
[Setup](./Setup.md)<br>
[Hierarchy](Hierarchy.md)<br>
[Files](./Files.md)<br>
  
Cheers,<br>
Smile :)  

### Special thanks to [@snoweuph](https://www.snoweuph.net/) for his help in testing and finalizing
