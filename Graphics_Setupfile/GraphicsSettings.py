import os
import glob
import time

class Globals:
    def __init__(self):
        # type: () -> object
        pass

files = glob.glob('C:/Ardeagames/**/*.gamedef')

G = Globals()
G.Gamedef = ""
G.Gamename = ""
G.options =""


def filesettings(path):
	data2 = path.split("\\")
	G.Gamedef = data2[-1]
	data3 = G.Gamedef.split(".")
	G.Gamename=data3[0]
	if answer == '1':
		G.options = 'args=GAMEDEF='+G.Gamedef+' BUILD=\unity\\'+G.Gamename+'.exe GRAPHICS=\"-popupwindow -force-d3d9 -adapter2\"\n'
	elif answer == '2':
		G.options ='args=GAMEDEF='+G.Gamedef+' BUILD=\unity\\'+G.Gamename+'.exe GRAPHICS=\"-popupwindow -adapter2\"\n'
	elif answer == '3':
		G.options ='args=GAMEDEF='+G.Gamedef+' BUILD=\unity\\'+G.Gamename+'.exe GRAPHICS=\"-popupwindow -force-gcore -adapter2\"\n'
	else:
		print "Wrong choice please restart application"
		exit()


print "Choose from the following graphics options :"
print "1. Directx9"
print "2. Directx11"
print "3. OpenGL"
print "Enter your choice : "

answer = raw_input()




for file in files:
	with open(file,'r') as newfile:
		filesettings(file)
		data = newfile.readlines()
		data[10] = G.options
		newfile.close()
	with open(file,'w') as newfile:
		newfile.writelines(data)
		newfile.close()

print "Successfully updated the graphics settings"
time.sleep(2)

exit()
