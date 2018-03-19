
#sets the version in the VersionInfo.cs file and the .version file

import sys
import json

VersionInfo = "./ScrapYard/Properties/VersionInfo.cs"
VersionFile = "./GameData/ScrapYard/ScrapYard.version"


with open(VersionFile, 'r') as v:
    fileJSON = json.load(v)

#get the various parts of the version from the passed in version
#version = sys.argv[1] #"1.2.3.4"  or "major.minor.patch.build"
#versionSplit = version.split('.')

#major = int(versionSplit[0])
#minor = int(versionSplit[1])
#patch = int(versionSplit[2])
#build = int(versionSplit[3])

build = sys.argv[1]

major = fileJSON["VERSION"]["MAJOR"]
minor = fileJSON["VERSION"]["MINOR"]
patch = fileJSON["VERSION"]["PATCH"]

version = '{0}.{1}.{2}.{3}'.format(major, minor, patch, build)

print('Version is '+version+'\n')

#update the VersionInfo.cs file
print("Setting .dll version")
with open(VersionInfo, 'w') as v:
    #v.write('[assembly: System.Reflection.AssemblyVersion("' + version + '")] //Added by build\n')
    v.write('[assembly: System.Reflection.AssemblyFileVersion("' + version + '")] //Added by build\n')
    v.write('[assembly: System.Reflection.AssemblyInformationalVersion("' + version + '")] //Added by build\n')
  
#Update the .version file
#don't erase everything, just the version stuff
#it's json, so lets do it that way
print("Editing .version file")

fileJSON["VERSION"]["BUILD"] = build

with open(VersionFile, 'w') as v:
    json.dump(fileJSON, v, sort_keys=True, indent=4)
  

print("Writing version.txt")
with open('version.txt', 'w') as f:
    f.write(version+'\n')

print("Done!")