

## This repo contains Modified grpc samples from : https://github.com/grpc/grpc/tree/master/examples

### How to run locally:
- Open projec in visual studio
- Build
- Open command prompt and go to GrpcAspNet\TestClient
  - Run dotnet publish -c debug -r win-x86 to generate TestClient.exe
- Open debug options for GrpcAspNet project
  - Add Environement Variable GrpcClient with Value GrpcAspNet\TestClient\bin\debug\netcoreapp2.2\win-x86\publish\TestClient.exe

### How to run on Azure:
- Open projec in visual studio
- Build
- Open command prompt and go to GrpcAspNet\TestClient
  - Run dotnet publish -c debug -r win-x86 to generate TestClient.exe
- Right click GrpcAspNet project and select Publish 
- Open Kudu 
 - Copy GrpcAspNet\TestClient\bin\debug\netcoreapp2.2\win-x86\publish folder to D:\Home\TestClient\publish
- Add AppSetting key GrpcClient value  D:\Home\TestClient\publish\TestClient.exe
- Restart the App.
