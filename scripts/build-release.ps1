# KeyOverlayFPS リリースビルドスクリプト
# このスクリプトはローカルでリリースパッケージを作成するためのものです

param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "1.0.0",
    
    [Parameter(Mandatory=$false)]
    [string[]]$Architectures = @("x64", "x86"),
    
    [Parameter(Mandatory=$false)]
    [string]$OutputDirectory = "dist"
)

# エラー時に停止
$ErrorActionPreference = "Stop"

Write-Host "KeyOverlayFPS リリースビルド開始" -ForegroundColor Green
Write-Host "バージョン: $Version" -ForegroundColor Yellow
Write-Host "アーキテクチャ: $($Architectures -join ', ')" -ForegroundColor Yellow

# 出力ディレクトリのクリーンアップ
if (Test-Path $OutputDirectory) {
    Remove-Item $OutputDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null

# プロジェクトルートに移動
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectRoot = Split-Path -Parent $ScriptPath
Set-Location $ProjectRoot

Write-Host "プロジェクトルート: $ProjectRoot" -ForegroundColor Cyan

foreach ($arch in $Architectures) {
    Write-Host "`n=== $arch ビルド開始 ===" -ForegroundColor Blue
    
    # 単一実行ファイル版
    Write-Host "単一実行ファイル版をビルド中..." -ForegroundColor Yellow
    $singleOutputPath = Join-Path $OutputDirectory "single-$arch"
    
    dotnet publish src/KeyOverlayFPS.csproj `
        --configuration Release `
        --runtime "win-$arch" `
        --self-contained true `
        --output $singleOutputPath `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:Version=$Version `
        -p:AssemblyVersion="$Version.0" `
        -p:FileVersion="$Version.0"
    
    if ($LASTEXITCODE -ne 0) {
        throw "単一実行ファイル版のビルドに失敗しました (アーキテクチャ: $arch)"
    }
    
    # 単一実行ファイルをコピー
    $singleExePath = Join-Path $singleOutputPath "KeyOverlayFPS.exe"
    $finalSinglePath = Join-Path $OutputDirectory "KeyOverlayFPS-win-$arch.exe"
    Copy-Item $singleExePath $finalSinglePath
    Write-Host "作成完了: $finalSinglePath" -ForegroundColor Green
    
    # フォルダ版（Zip用）
    Write-Host "フォルダ版をビルド中..." -ForegroundColor Yellow
    $folderOutputPath = Join-Path $OutputDirectory "folder-$arch"
    
    dotnet publish src/KeyOverlayFPS.csproj `
        --configuration Release `
        --runtime "win-$arch" `
        --self-contained true `
        --output $folderOutputPath `
        -p:PublishSingleFile=false `
        -p:Version=$Version `
        -p:AssemblyVersion="$Version.0" `
        -p:FileVersion="$Version.0"
    
    if ($LASTEXITCODE -ne 0) {
        throw "フォルダ版のビルドに失敗しました (アーキテクチャ: $arch)"
    }
    
    # レイアウトファイルをコピー
    $layoutsSource = "layouts"
    $layoutsDestination = Join-Path $folderOutputPath "layouts"
    if (Test-Path $layoutsSource) {
        Copy-Item $layoutsSource $layoutsDestination -Recurse
        Write-Host "レイアウトファイルをコピーしました" -ForegroundColor Cyan
    }
    
    # README等をコピー（存在する場合）
    $readmeFiles = @("README.md", "LICENSE", "CHANGELOG.md")
    foreach ($readme in $readmeFiles) {
        if (Test-Path $readme) {
            Copy-Item $readme $folderOutputPath
            Write-Host "ドキュメントをコピー: $readme" -ForegroundColor Cyan
        }
    }
    
    # Zipファイルを作成
    $zipPath = Join-Path $OutputDirectory "KeyOverlayFPS-win-$arch.zip"
    Compress-Archive -Path "$folderOutputPath/*" -DestinationPath $zipPath -Force
    Write-Host "作成完了: $zipPath" -ForegroundColor Green
    
    # 一時フォルダを削除
    Remove-Item $singleOutputPath -Recurse -Force
    Remove-Item $folderOutputPath -Recurse -Force
}

Write-Host "`n=== ビルド完了 ===" -ForegroundColor Green
Write-Host "出力ディレクトリ: $OutputDirectory" -ForegroundColor Yellow

# 作成されたファイルを表示
Get-ChildItem $OutputDirectory | ForEach-Object {
    $size = if ($_.Length -gt 1MB) { "{0:N1} MB" -f ($_.Length / 1MB) } else { "{0:N0} KB" -f ($_.Length / 1KB) }
    Write-Host "  $($_.Name) ($size)" -ForegroundColor White
}

Write-Host "`nリリース手順:" -ForegroundColor Yellow
Write-Host "1. git tag v$Version" -ForegroundColor White
Write-Host "2. git push origin v$Version" -ForegroundColor White
Write-Host "3. GitHub Actionsでリリースが自動作成されます" -ForegroundColor White