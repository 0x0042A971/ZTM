# ZTM
Simple, clean UI for ZeroTier network.

**Why:** Let's say you have some friends, who aren't tech savvy at all. The most "hacker" thing they did was to change the stone texture in minecraft.jar to a transparent rectangle. You want to play LAN games with them and for whatever reason decided to use ZeroTier. While network administrators can use ZeroTier Central's user-friendly interface, common folk has to suffer and constantly ask for the IP address of network members - both others and their own.

# UI
UI provides some basic info about each member in network
- Username
- IP address
- Online/Offline indicator
- Profile picture

The link to the profile picture should be stored in the "Description" in ZeroTier network, otherwise UI will use my paint drawing.

Dark theme included.

![zt](https://user-images.githubusercontent.com/97065300/150563603-e922389f-fecd-44ac-8db6-1e1e671d9f3c.png)

# Security
This UI uses network administator API key in http request. Since whole code is open sourced it is not very secure.

If you need security, pack your own encryption and decriptions alogorithms and http request to some c++ library.
