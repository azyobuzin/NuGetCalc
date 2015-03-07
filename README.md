# NuGetCalc #
Finds which assembly NuGet adds to your project.

# Example #
```
PS> Find-MostCompatibleReferenceGroup CoreTweet portable-net40+win81

TargetFramework                                                                                           Items
---------------                                                                                           -----
.NETPortable, Version=v0.0, Profile=net4+wp8+win81+wpa81+monoandroid+monotouch                            {lib/portable-net4+wp8+win81+wpa81+MonoAndroid+MonoTouch/CoreTweet.dll}
```
This means when you install CoreTweet package to the ```portable-net40+win81``` project, NuGet uses ```net4+wp8+win81+wpa81+MonoAndroid+MonoTouch``` assembly.

# License #
Do What The Fuck You Want To Public License
