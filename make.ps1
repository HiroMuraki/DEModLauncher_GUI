dotnet publish -c release -r win10-x64 --self-contained=false /p:PublishSingleFile=True
$hasPath = Test-Path "OutputProgram/DEModLauncher.exe"
if ($hasPath) {
    Remove-Item "OutputProgram/DEModLauncher.exe"
}
Copy-Item "C:\Users\11717\source\repos\DEModLauncher_GUI\DEModLauncher_GUI\bin\Release\net6.0-windows\win10-x64\publish\DEModLauncher_GUI.exe" "OutputProgram/DEModLauncher.exe"