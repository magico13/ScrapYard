
#sets the version in the VersionInfo.cs file and the .version file

import sys
import json

VersionInfo = "./ScrapYard/Properties/VersionInfo.cs"
VersionFile = "./GameData/ScrapYard/ScrapYard.version"


#get the various parts of the version from the passed in version
version = sys.args[1] #"1.2.3.4"  or "major.minor.patch.build"
versionSplit = version.split('.')

major = versionSplit[0]
minor = versionSplit[1]
patch = versionSplit[2]
build = versionSplit[3]


#update the VersionInfo.cs file
print("Setting .dll version")
with open(VersionInfo, 'w') as v:
  v.write('[assembly: System.Reflection.AssemblyVersion("' + version + '")] //Added by build\n')
  v.write('[assembly: System.Reflection.AssemblyFileVersion("' + version + '")] //Added by build\n')
  
#Update the .version file
#don't erase everything, just the version stuff
#it's json, so lets do it that way
print("Editing .version file")

with open(VersionFile, 'r') as v:
  fileJSON = json.load(v)

fileJSON["VERSION"]["MAJOR"] = major
fileJSON["VERSION"]["MINOR"] = minor
fileJSON["VERSION"]["PATCH"] = patch
fileJSON["VERSION"]["BUILD"] = build

with open(VersionFile, 'w') as v:
  json.dump(fileJSON, v)
  
print("Done!")