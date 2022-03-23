Remove-Item -Recurse -Force bld
$options = @('--configuration', 'Release', '-p:DebugType=embedded')
dotnet publish ./BezierInterpolator $options --framework net6.0 -o ./bld
dotnet publish ./RadialFollow $options --framework net6.0 -o ./bld
dotnet publish ./TouchTapping $options --framework net6.0 -o ./bld


Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
