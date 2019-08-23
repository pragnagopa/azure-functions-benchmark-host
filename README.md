

## This repo contains Modified grpc samples from : https://github.com/grpc/grpc/tree/master/examples

### How to run locally:

#### C# Test Client
- Open project in visual studio
- Build
- Open command prompt and go to GrpcAspNet\TestClient
  - Run dotnet publish -c debug -r win-x86 to generate TestClient.exe
- Open debug options for GrpcAspNet project
  - Add Environement Variable GrpcClient with Value GrpcAspNet\TestClient\bin\debug\netcoreapp2.2\win-x86\publish\TestClient.exe

#### Node.js Test Client
- Open project in Visual Studio
- Build
- Open command prompt and go to GrpcAspNet\NodeTestClient
  - Run `./package.ps1`
- Open debug options for GrpcAspNet project
  - Add Environement Variable `GrpcClient` with Value `node`
  - Add Environement Variable `GrpcClientCode` with Value `..\NodeTestClient\pkg\nodejsWorker.js`

The contents for the Node.js client that need to be deployed are all in "pkg"

#### Java Test Client
- Open project in Visual Studio
- Build
- Open command prompt and go to GrpcAspNet\JavaTestClient\azurefunctionsjavaworker
  - Run mvn clean package
- Open debug options for GrpcAspNet project
  - Add Environement Variable `GrpcClient` with Value `java`
  - Add Environement Variable `GrpcClientCode` with Value `..\JavaTestClient\azurefunctionsjavaworker\target\azure-functions-java-worker-1.5.0.jar`


### How to run on Azure .Net Client:
- Open projec in visual studio
- Build
- Open command prompt and go to GrpcAspNet\TestClient
  - Run dotnet publish -c debug -r win-x86 to generate TestClient.exe
- Right click GrpcAspNet project and select Publish 
- Open Kudu 
 - Copy GrpcAspNet\TestClient\bin\debug\netcoreapp2.2\win-x86\publish folder to D:\Home\TestClient\publish
- Add AppSetting key GrpcClient value  D:\Home\TestClient\publish\TestClient.exe
- Restart the App.

### How to run on Azure Node Client:
- Open projec in visual studio
- Build
- Open command prompt and go to GrpcAspNet\NodeTestClient
  - Run `./package.ps1`
- Right click GrpcAspNet project and select Publish 
- Open Kudu 
 - Copy GrpcAspNet\TestClient\NodeTestClient\pkg\ folder to D:\Home\NodeClient\
- Add AppSetting key `GrpcClient` with Value `node`
- Add AppSetting Variable `GrpcClientCode` with Value `D:\Home\NodeClient\nodejsWorker.js` 
- Add AppSetting Variable 'WEBSITE_NODE_DEFAULT_VERSION' with value '10.15.2'
- Restart the App.

### How to run on Azure Java Client:
- Open projec in visual studio
- Build
- Open command prompt and go to GrpcAspNet\JavaTestClient\azurefunctionsjavaworker
  - Run mvn clean package
- Right click GrpcAspNet project and select Publish 
- Open Kudu 
 - Copy GrpcAspNet\JavaTestClient\azurefunctionsjavaworker\target\azure-functions-java-worker-1.5.0.jar to D:\Home\JavaClient\
 - Add AppSetting key `GrpcClient` with Value `java`
 - Add AppSetting Variable `GrpcClientCode` with Value `D:\Home\JavaClient\azure-functions-java-worker-1.5.0.jar`
- Restart the App.

