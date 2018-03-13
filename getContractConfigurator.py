import urllib
import json
import os

#get the latest artifact name
latestBuild = "https://api.github.com/repos/jrossignol/ContractConfigurator/releases/latest"
tree = json.loads(urllib.urlopen(latestBuild).read())

downloadLink = ""
name = ""

for asset in tree["assets"]:
  if (".zip" in asset["name"]):
    name = asset["name"]
    downloadLink = asset["browser_download_url"]
    break

print("Latest CC .zip is "+name)

#get the file
os.system("wget "+downloadLink)

#then unzip and copy
