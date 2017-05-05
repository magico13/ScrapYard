
import urllib
import ast
import os

#mmJenkins = "https://ksp.sarbian.com/jenkins/job/ModuleManager/api/python"

#tree = ast.literal_eval(urllib.urlopen(mmJenkins).read())
#lastBuildNum = tree["lastCompletedBuild"]["number"]

#print("Last completed build is #"+str(lastBuildNum))

#get the latest artifact name
mmJenkinsBuild = "https://ksp.sarbian.com/jenkins/job/ModuleManager/lastSuccessfulBuild/api/python"
tree = ast.literal_eval(urllib.urlopen(mmJenkinsBuild).read())

artifactName = ""

for artifact in tree["artifacts"]:
  if (".dll" in artifact["fileName"]):
    artifactName = artifact["fileName"]
    break

print("Latest .dll is "+artifactName)

#get the file
finalFile = "https://ksp.sarbian.com/jenkins/job/ModuleManager/lastSuccessfulBuild/artifact/"+artifactName
os.chdir("GameData")
os.system("wget "+finalFile)