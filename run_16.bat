csc -out:node.exe /r:Nancy.dll /r:Nancy.Hosting.Self.dll  node.cs
csc -out:net.exe /r:Nancy.dll /r:Nancy.Hosting.Self.dll /r:System.Net.Http.dll net.cs

set config=config411.txt
start "node1" cmd /k node.exe %config% 1 2 4 7 ^> node1.log
start "node2" cmd /k node.exe %config% 2 1 3 4 7 ^> node2.log
start "node3" cmd /k node.exe %config% 3 2 4 5 6 8 9 ^> node3.log
start "node4" cmd /k node.exe %config% 4 1 2 3 5 ^> node4.log
start "node5" cmd /k node.exe %config% 5 3 4 6 8 9 ^> node5.log
start "node6" cmd /k node.exe %config% 6 3 5 7 16 ^> node6.log
start "node7" cmd /k node.exe %config% 7 1 2 6 8 10 ^> node7.log
start "node8" cmd /k node.exe %config% 8 3 5 7 ^> node8.log
start "node9" cmd /k node.exe %config% 9 3 5 11 14 ^> node9.log
start "node10" cmd /k node.exe %config% 10 7 11 13 15 ^> node10.log
start "node11" cmd /k node.exe %config% 11 9 10 ^> node11.log
start "node12" cmd /k node.exe %config% 12 13 16 ^> node12.log
start "node13" cmd /k node.exe %config% 13 10 12 15 16 ^> node13.log
start "node14" cmd /k node.exe %config% 14 9 15 ^> node14.log
start "node15" cmd /k node.exe %config% 15 10 13 14 16 ^> node15.log
start "node16" cmd /k node.exe %config% 16 6 12 13 15 ^> node16.log
 
rem 2>&1
net.exe %config% > net.log
#start "net" cmd /k net.exe config.txt